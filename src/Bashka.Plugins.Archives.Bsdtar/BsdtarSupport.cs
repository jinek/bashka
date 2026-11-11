namespace Bashka.Plugins.Archives.Bsdtar;

internal static class BsdtarSupport
{
    public const string SupportedExtensions =
        // containers
        @"\.(7z|zip|zipx|jar|nupkg|rar|cb7|cbr|cbt|cbz|cab|lha|lzh|iso|cpio|xar|pkg|warc|mtree|ar|a|deb|rpm|gem|crate|whl|war|ear|apk|xpi)$" +
        // tar and compressed tars
        @"|\.tar$" +
        @"|\.tar\.(gz|bz2|xz|lzma|lz|lz4|lzo|zst|Z)$" +
        @"|\.(tgz|tbz|tbz2|tb2|txz|tlz|tlzma|tzst|taz)$" +
        // single file compression
        @"|\.(gz|bz2|xz|lzma|lz|lz4|lzo|zst|Z)$";

    public static string Quote(string path) => $"\"{path}\"";

    public static string QuoteJoin(IEnumerable<string> paths) => string.Join(' ', paths.Select(Quote));
}
