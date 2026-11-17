using Avalonia;
using Bashka.Launcher;
using Consolonia.Core.Drawing.PixelBufferImplementation;
using Consolonia.NUnit;

namespace Bashka.Tests.Helpers;

public abstract class BashkaAppTestBase() : ConsoloniaAppTestBase<App>(new PixelBufferSize(140, 40))
{
    protected override AppBuilder CreateAppBuilder()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        return base.CreateAppBuilder();
    }
}
