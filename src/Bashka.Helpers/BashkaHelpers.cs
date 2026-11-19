using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace Bashka.Helpers;

public static class BashkaHelpers
{
    public static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bashka");

    public static T GetElement<T>(this ILogical visual, [CallerMemberName] string? elementName = null)
        where T : class, ILogical
    {
        return visual.Get<T>(elementName!);
    }
}