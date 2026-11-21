using Autofac.Features.Indexed;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Abstract.Surrounding;
using Bashka.Plugins.Core.FunctionKeys;
using Bashka.Plugins.Foundation.FileSystem;

namespace Bashka.Plugins.Foundation.Configuration;

/// <summary>
///     Opens a temporary copy in the editor, the copy is discarded afterward
/// </summary>
public class EditPluginSettingsCommand(IIndex<string, ICommand> commands)
    : CatalogCommand<SettingsCatalog, PluginSettingsEntry>(null, false, true), IFunctionBarCatalogCommand
{
    public override async Task ExecuteAsync(CatalogCommandContext<SettingsCatalog, PluginSettingsEntry> context)
    {
        PluginSettingsEntry entry = context.SingleEntry;
        DirectoryInfo tempDirectory = System.IO.Directory.CreateTempSubdirectory("Bashka.");
        try
        {
            string path = Path.Combine(tempDirectory.FullName, entry.File.ResourceName);
            await System.IO.File.WriteAllTextAsync(path,
                $"""
                 ; Read-only defaults of {entry.File.PluginName}: changes made here are discarded.
                 ; To change a value, copy it with its [section] into a user settings file of the Settings catalog
                 ; and restart Bashka.

                 """ + entry.File.Content);
            var copy = new FileEntry
            {
                Path = entry.File.ResourceName,
                Name = entry.File.ResourceName,
                FullPath = path,
                ParentCatalog = entry.ParentCatalog
            };
            await commands[ICommand.EditCommandKey]
                .ExecuteAsync(context.CommandExecutionContext with { Entries = [copy] });
        }
        finally
        {
            tempDirectory.Delete(true);
        }
    }
}