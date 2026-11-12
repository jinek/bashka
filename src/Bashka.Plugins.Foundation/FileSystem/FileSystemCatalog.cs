using System.Diagnostics;
using System.Globalization;
using Bashka.Plugins.Core.Abstract;
using Bashka.Plugins.Core.Abstract.Data;
using Bashka.Plugins.Core.Worker.ProgressUI;
using Bashka.Plugins.Core.Worker.Schedulers;
using Bashka.Plugins.Foundation.ShellInteraction;
using Bashka.Plugins.Foundation.ShellInteraction.Scripting.Scripts;

namespace Bashka.Plugins.Foundation.FileSystem;

public class FileSystemCatalog : ShellCatalog, IReadStreamCatalog, IWriteStreamCatalog, IPtyExecutor
{
    public const string LinuxRootCatalog = "/";

    /// <summary>
    ///     Amount of raw bytes sent in one shell command. Encoded as base64 it takes 4/3 of that size,
    ///     which must stay well below the shell command line limit.
    /// </summary>
    private const int WriteChunkSize = 24 * 1024;

    private readonly ICommandSelector _commandSelector;


    public FileSystemCatalog(ICommandSelector commandSelector)
    {
        _commandSelector = commandSelector;
        NotifyDependentProperties(nameof(CurrentPath), nameof(Title));
    }

    public Task ExecuteAsResidentAsync(string workingDir, string applicationName, string arguments,
        CancellationToken cancellationToken)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = applicationName,
            Arguments = arguments,
            UseShellExecute = false,
            WorkingDirectory = workingDir
        };

        return Task.Run(() =>
        {
            using Process process = Process.Start(processStartInfo)!;
            //WaitForExitAsync probably can be used, but seems we can not continue on initial thread
            process.WaitForExit();
        }, cancellationToken);
    }

    public override string Title => CurrentPath ?? "File System";

    //public override string Title => Path.GetPathRoot(CurrentPath) ?? " ";

    public Task<string> ResolveCurrentPathAsync()
    {
        CurrentDirectoryLine currentDirectory = ShellSession.ExecuteScalar(new GetCurrentDirectoryShellMultiScript());
        return Task.FromResult(currentDirectory.Line);
    }

    public Stream ReadStream(string fullPath)
    {
        string base64 = string.Concat(ShellSession
            .PrepareQuery(new ReadBase64ShellMultiScript(fullPath))
            .Select(line => line.Line));

        return new MemoryStream(Convert.FromBase64String(base64), false);
    }

    public void WriteStream(string fullPath, Stream stream)
    {
        ShellSession.ExecuteNonQuery(new TruncateFileShellMultiScript(fullPath));

        byte[] buffer = new byte[WriteChunkSize];
        int read;
        while ((read = stream.ReadAtLeast(buffer, buffer.Length, false)) > 0)
            ShellSession.ExecuteNonQuery(
                new AppendBase64ShellMultiScript(fullPath, Convert.ToBase64String(buffer, 0, read)));
    }

    private IEnumerable<Entry> GetDrives()
    {
        var logicalDrivesShellMultiScript = new GetLogicalDrivesShellMultiScript();

        Entry[] result = ShellSession.PrepareQuery(logicalDrivesShellMultiScript).Select(rawDrive =>
                new Entry((rawDrive.Target, rawDrive.Target, 0, DateTime.Now, EntryCategory.Default), true, this))
            .ToArray();

        Entry? linuxRoot = result.SingleOrDefault(entry => entry.Path == LinuxRootCatalog);
        return linuxRoot != null
            ? new[] { linuxRoot }.Union(result.Where(entry => entry != linuxRoot)).ToArray()
            : result;
    }

    private IEnumerable<FileEntry> GetEntries(string path)
    {
        var dirShellMultiScript = new DirShellMultiScript(path);
        IEnumerable<RawFile> query = ShellSession.PrepareQuery(dirShellMultiScript);
        IProgress<ProgressStatus> progress = WorkerTaskFactory.CurrentProgress;
        long totalBytes = 0;
        long processedBytes = 0;
        //todo: add automapper
        foreach ((string type, string permission, string owner, string group, string size, string timeStamp,
                     string name) in query)
        {
            if (type == string.Empty)
            {
                totalBytes = long.Parse(size) * 1024; //'total' line of ls -l is in 1KiB blocks
                continue;
            }

            long sizeBytes = long.Parse(size);
            processedBytes += sizeBytes;
            progress.ReportProgress($"Listing {name}",
                totalBytes > 0 ? (byte)Math.Min(100, processedBytes * 100 / totalBytes) : (byte)0);
            var category = EntryCategory.Default;
            string fullPath = PathJoin(path, name);

            if (name.StartsWith(".", StringComparison.Ordinal))
            {
                category = EntryCategory.Hidden;
            }
            else
            {
                //todo: this is very slow for us
                ICommand[] commands = _commandSelector.SelectCommands(new CommandExecutionContext([
                    new FileEntry(
                        (name, name, fullPath, 0, DateTime.Now, category),
                        false, this)
                ], this, null, null, null));
                if (commands.OfType<IRegexCommand>()
                    .Any(command => command.Regexp != null && command.DoesRegexMatch(fullPath)))
                    category = EntryCategory.Important;
                else if (new[] { ".sh", ".exe", ".dll", "*.msi", "*.dev" }.Any(ending =>
                             name.EndsWith(ending, StringComparison.OrdinalIgnoreCase)))
                    category = EntryCategory.Executable;
            }

            yield return new FileEntry(
                (name, name, fullPath, sizeBytes, ParseTimeStamp(timeStamp), category),
                type == "d",
                this)
            {
                Details = [$"{owner}:{group}", type + permission]
            };
        }
    }

    private static DateTime ParseTimeStamp(string timeStamp)
    {
        return DateTimeOffset.TryParse(timeStamp, CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces, out DateTimeOffset parsed)
            ? parsed.LocalDateTime
            : DateTime.Parse(timeStamp, CultureInfo.InvariantCulture);
    }

    protected override Task<IEnumerable<Entry>> GetChildrenInternal()
    {
        string? currentPath = CurrentPath;
        if (!string.IsNullOrEmpty(currentPath))
            ShellSession.ExecuteNonQuery(new ChangeDirectoryShellMultiScript(currentPath));

        return Task.FromResult(currentPath == string.Empty ? GetDrives() : GetEntries(currentPath!));
    }
}