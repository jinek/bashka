namespace Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

[ShellScript(typeof(BashExecutor),
    @"df --output=target | tail -n +2",
    @"(?<Target>.+)$")]
public record GetLogicalDrivesShellMultiScript
    : AttributedShellMultiScript<RawLogicalDrive>;