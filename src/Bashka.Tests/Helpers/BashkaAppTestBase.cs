using Avalonia;
using Bashka.Launcher;
using Bashka.Plugins.Core.Configuration;
using Consolonia.Core.Drawing.PixelBufferImplementation;
using Consolonia.NUnit;

namespace Bashka.Tests.Helpers;

public abstract class BashkaAppTestBase() : ConsoloniaAppTestBase<App>(new PixelBufferSize(140, 40))
{
    protected override AppBuilder CreateAppBuilder()
    {
        // panel locations are restored from the state, every test starts from the default ones
        if (File.Exists(Settings.StateFilePath)) File.Delete(Settings.StateFilePath);
        return base.CreateAppBuilder();
    }
}