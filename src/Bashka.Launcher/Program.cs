// See https://aka.ms/new-console-template for more information

using Avalonia;
using Consolonia;
using Consolonia.Fonts;
using Bashka.Launcher;

//ExpressionObserver.PropertyAccessors[2] = new FixedInpcPropertyAccessorPlugin();

AppBuilder.Configure<App>()
    .LogToException()
    .UseSkia()
    .UseConsolonia()
    .UseAutoDetectedConsole()
    .WithConsoleFonts()
    .ThrowOnErrors()
    .StartWithConsoleLifetime([]);
