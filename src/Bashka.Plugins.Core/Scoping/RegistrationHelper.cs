using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Core.FunctionKeys;

namespace Bashka.Plugins.Core.Scoping;

public static class RegistrationHelper
{
    public static readonly object Hierarchy = new();

    public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        FullRegister<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementer,
            TInterface>(this ContainerBuilder builder) where TImplementer : TInterface where TInterface : notnull
    {
        Parameter[] keyedProperties =
        [
            .. typeof(TImplementer)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(property =>
                    (Property: property, Attribute: property.GetCustomAttribute<AutoWireAttribute>(true)))
                .Where(item => item.Attribute?.Key != null)
                .Select(item => (Parameter)new ResolvedParameter(
                    (parameter, _) => parameter.Member == item.Property.SetMethod,
                    (_, context) => context.ResolveKeyed(item.Attribute!.Key!, item.Property.PropertyType)))
        ];

        return builder.RegisterType<TImplementer>()
            .AsSelf()
            .As<TInterface>()
            .Keyed<TInterface>(typeof(TImplementer).GetTypeKey())
            .WithAttributeFiltering()
            .WithProperties(keyedProperties)
            .PropertiesAutowired((property, _) => property.GetCustomAttribute<AutoWireAttribute>(true) is
                { Key: null });
    }

    /// <summary>
    ///     Registers a catalog listed in roots, see <see cref="ICatalog.FavoritesCountMetadataKey" />
    /// </summary>
    public static IRegistrationBuilder<TCatalog, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        RegisterCatalog<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCatalog>(
            this ContainerBuilder builder, string name, int favoritesCount = 0) where TCatalog : ICatalog
    {
        return builder.FullRegister<TCatalog, ICatalog>()
            .WithMetadata(ICatalog.NameMetadataKey, name)
            .WithMetadata(ICatalog.FavoritesCountMetadataKey, favoritesCount);
    }

    /// <summary>
    ///     Registers a command which can be executed for the selected entry (by Enter).
    ///     The latest registered one overrides the previously registered.
    /// </summary>
    public static IRegistrationBuilder<TCommand, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        RegisterEnterCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(
            this ContainerBuilder builder) where TCommand : ICommand
    {
        return builder.FullRegister<TCommand, ICommand>()
            .AsSelf()
            .Keyed<ICommand>(ICommand.EnterCommandKey);
    }

    public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
        InstancePerHierarchy<TLimit, TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
    {
        return builder.InstancePerMatchingLifetimeScope(Hierarchy);
    }

    public static IRegistrationBuilder<TCommand, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        RegisterFCommand<TCommand, TCatalog>(this ContainerBuilder containerBuilder, FunctionPage functionPage,
            FunctionKey functionKey,
            string defaultTitle,
            bool isDelete = false) where TCatalog : ICatalog where TCommand : ICommand
    {
        return containerBuilder.FullRegister<TCommand, ICommand>()
            .AsSelf()
            .As<IFunctionBarCommand<TCatalog>>().WithMetadata((string)nameof(FunctionPage), functionPage)
            .WithMetadata((string)nameof(FunctionKey), functionKey)
            .WithMetadata(IFunctionBarCommand<ICatalog>.DefaultTitleMetadataKey, defaultTitle)
            .WithMetadata(IFunctionBarCommand<ICatalog>.IsDeleteMetadataKey, isDelete);
    }

    /// <summary>
    ///     Adds the ini file embedded in <paramref name="assembly" /> to <see cref="Settings" />
    /// </summary>
    public static void RegisterBuiltinSettingsFile(this ContainerBuilder builder, Assembly assembly,
        string resourceName)
    {
        using Stream stream = assembly.GetManifestResourceStream(resourceName)
                              ?? throw new InvalidOperationException(
                                  $"Resource {resourceName} is not found in {assembly.FullName}.");
        using var reader = new StreamReader(stream);
        builder.RegisterInstance(new PluginSettingsFile(assembly, resourceName, reader.ReadToEnd()));
    }

    /// <summary>
    ///     Registers <typeparamref name="T" /> filled from its section of <see cref="Settings" />.
    ///     An inheritor registered with <c>.As&lt;Parent&gt;()</c> replaces the parent
    /// </summary>
    public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterSettings<T>(
        this ContainerBuilder builder) where T : class, new()
    {
        return builder.Register(context => Settings.Bind<T>(context.Resolve<Settings>()))
            .AsSelf().SingleInstance();
    }
}