using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.Configuration;
using Bashka.Plugins.Core.Dialog.Abstract;
using Bashka.Plugins.Core.Dialog.Basic;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Core.Worker.Schedulers;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Foundation.Configuration;

/// <summary>
///     Copies read-only defaults of a plugin into a new user settings file
/// </summary>
public class ClonePluginSettingsCommand(IDialogProvider dialogProvider, WorkerTaskFactory workerTaskFactory)
    : CatalogCommand<SettingsCatalog, PluginSettingsEntry>(null, false, true), IFunctionBarCatalogCommand
{
    public override async Task ExecuteAsync(CatalogCommandContext<SettingsCatalog, PluginSettingsEntry> context)
    {
        const string iniExtension = ".ini";
        
        PluginSettingsEntry entry = context.SingleEntry;
        string fileName = await dialogProvider.ShowOkCancelDialogAsync(
            new StringInputDialog("Enter user settings file name", entry.File.ResourceName));
        if (!fileName.EndsWith(iniExtension, StringComparison.OrdinalIgnoreCase))
            fileName += iniExtension;

        string path = Path.Combine(Settings.UserDirectory, fileName);
        
        await workerTaskFactory.StartNewWorkerTask(async (cancellationToken, _, _) =>
        {
            if (System.IO.File.Exists(path))
                throw new UserErrorException($"{fileName} already exists");
            System.IO.Directory.CreateDirectory(Settings.UserDirectory);
            await System.IO.File.WriteAllTextAsync(path, entry.File.Content, cancellationToken);
        },"Copying...");
        

        var catalog = (SettingsCatalog)entry.ParentCatalog;
        await catalog.NavigateAsync(string.Empty);

        IPanel panel = context.CommandExecutionContext.Panel!;
        if (panel.CurrentCatalog != catalog)
            throw new InvalidProgramException("Must not happen");
        panel.IsFocused = true;
        panel.SelectedEntry = catalog.Children.OfType<FileEntry>().SingleOrDefault(file => file.FullPath == path);
    }
}
