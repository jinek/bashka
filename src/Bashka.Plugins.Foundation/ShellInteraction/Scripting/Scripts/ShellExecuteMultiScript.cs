namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor), "xdg-open '{0}'")]
public record ShellExecuteMultiScript : AttributedShellMultiScript
{
    // ReSharper disable once ConvertToPrimaryConstructor // stupid situation
    public ShellExecuteMultiScript(string fileNameWithExtension) : base(fileNameWithExtension)
    {
    }
}