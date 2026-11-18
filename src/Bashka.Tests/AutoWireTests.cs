using Autofac;
using Autofac.Core;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Tests;

[TestFixture]
public class AutoWireTests
{
    [Test]
    public void OverrideSelectsKeyedServiceWithoutChangingOtherPropertiesOrScope()
    {
        var defaultService = new Service();
        var keyedService = new Service();
        var remoteService = new Service();
        var builder = new ContainerBuilder();
        builder.RegisterInstance(defaultService).As<IService>();
        builder.RegisterInstance(keyedService).Keyed<IService>("local");
        builder.FullRegister<KeyedConsumer, KeyedConsumer>();
        using IContainer container = builder.Build();
        using ILifetimeScope scope = container.BeginLifetimeScope(registrations =>
            registrations.RegisterInstance(remoteService).As<IService>());

        KeyedConsumer consumer = scope.Resolve<KeyedConsumer>();
        Assert.That(consumer.Service, Is.SameAs(keyedService));
        Assert.That(((Consumer)consumer).Service, Is.SameAs(keyedService));
        Assert.That(consumer.OtherService, Is.SameAs(remoteService));
        Assert.That(consumer.Assignments, Is.EqualTo(1));
        Assert.That(scope.Resolve<IService>(), Is.SameAs(remoteService));
        Assert.That(container.Resolve<IService>(), Is.SameAs(defaultService));
    }

    [Test]
    public void MissingKeyedServiceFailsInsteadOfUsingDefaultService()
    {
        var builder = new ContainerBuilder();
        builder.RegisterInstance(new Service()).As<IService>();
        builder.FullRegister<KeyedConsumer, KeyedConsumer>();
        using IContainer container = builder.Build();
        Assert.Throws<DependencyResolutionException>(() => container.Resolve<KeyedConsumer>());
    }

    public interface IService;
    public sealed class Service : IService;

    public class Consumer
    {
        [AutoWire] public virtual IService Service { get; internal set; } = null!;
        [AutoWire] public IService OtherService { get; internal set; } = null!;
    }

    public sealed class KeyedConsumer : Consumer
    {
        private IService _service = null!;
        public int Assignments { get; private set; }

        [AutoWire(Key = "local")]
        public override IService Service
        {
            get => _service;
            internal set
            {
                Assignments++;
                _service = value;
            }
        }
    }
}
