using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Features.AttributeFilters;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.FunctionBar;

namespace Bashka.Plugins.Core.Scoping;

public static class RegistrationHelper
{
    public static IRegistrationBuilder<TImplementer, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        FullRegister<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementer,
            TInterface>(this ContainerBuilder builder) where TImplementer : TInterface where TInterface : notnull
    {
        return builder.RegisterType<TImplementer>()
            .As<TInterface>()
            .Keyed<TInterface>(typeof(TImplementer).GetTypeKey())
            .WithAttributeFiltering()
            .PropertiesAutowired((property, _) => property.GetCustomAttribute(typeof(AutoWireAttribute), true) != null);
    }

    /// <summary>
    /// Registers a command which can be executed for the selected entry (by Enter).
    /// The latest registered one overrides the previously registered.
    /// </summary>
    public static IRegistrationBuilder<TCommand, ConcreteReflectionActivatorData, SingleRegistrationStyle>
        RegisterEnterCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(
            this ContainerBuilder builder) where TCommand : ICommand
    {
        return builder.FullRegister<TCommand, ICommand>()
            .AsSelf()
            .Keyed<ICommand>(ICommand.EnterCommandKey);
    }

    public static readonly object Hierarchy = new();

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
            .As<IFunctionBarCommand<TCatalog>>().WithMetadata((string)nameof(FunctionPage), (object?)functionPage).WithMetadata((string)nameof(FunctionKey), (object?)functionKey)
            .WithMetadata(IFunctionBarCommand<ICatalog>.DefaultTitleMetadataKey, defaultTitle)
            .WithMetadata(IFunctionBarCommand<ICatalog>.IsDeleteMetadataKey, isDelete);
    }
}