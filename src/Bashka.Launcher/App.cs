using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Bashka.Plugins.Core.Abstract;
using Consolonia.Controls.DataGrid;
using Consolonia.Modal;
using Consolonia.Themes;
using PropertyChanged;

namespace Bashka.Launcher;

[DoNotNotify]
public partial class App : Application
{
    /// <summary>
    ///     todo:
    ///     0) popup shell run
    ///     1) shell residential run
    ///     2) -1 size converter
    ///     3) ini serialization
    ///     4) ini strategy
    ///     -1) file type detection: https://www.darwinsys.com/file/
    ///     1) root catalog should show "show more", because can be too many roots
    ///     13) needs faster file type detection
    ///     124) List of catalogs title
    ///     !!!!!56) introduce Fslot.CatalogAllowed - will return if command is allowed for a catalog or two, it will mean to
    ///     show disabled command
    ///     27) Check in ssh all the commands
    /// </summary>
    public App()
    {
        HandleDispatcherExceptions();
        Program.ConfigureContainer();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        //Styles.Add(new TurboVisionTheme(baseUri));
        //Styles.Add(new TurboVisionDarkTheme(baseUri));
        //Styles.Add(new TurboVisionBlackTheme(baseUri));

        Styles.Add(new TurboVisionCompatibleTheme());
        Styles.Add(new ModalTheme());
        Styles.Add(new AutoDataGridStyles());
        //everything else visual is brought by plugins as VisualExtension and is registered in the application
        //new ResourceDictionary()
        //Resources.MergedDictionaries.Add(new ResourceInclude { Source = new Uri(@"/Domain/Templates/VolkovCommander.axaml") });

        /*Styles.Add(new StyleInclude(baseUri)
            { Source = new Uri(@"Styles/SpecialStyles.axaml", UriKind.Relative) });*/

        this.RegisterVisualExtensions(MainWindow.Scope);

        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}