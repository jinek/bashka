using System.Text.RegularExpressions;

namespace Bashka.Plugins.Core.Abstract;

public interface IRegexCommand : ICommand
{
    string? Regexp { get; }
    bool DoesRegexMatch(string entryPath) => Regex.IsMatch(entryPath, Regexp, RegexOptions.IgnoreCase);
}