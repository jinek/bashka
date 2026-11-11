using PropertyChanged;

namespace Bashka.Plugins.Core.Abstract.Data;

public enum SortKey
{
    /// <summary>
    /// Preserves the order provided by the catalog
    /// </summary>
    Unsorted,
    Name,
    Extension,
    Size,
    TimeStamp
}

[DoNotNotify]
public record SortMode
{
    public SortMode()
    {
        
    }
    
    public SortMode(SortKey key, bool descending)
    {
        Key = key;
        Descending = descending;
    }

    /// <summary>
    /// Switches to <param name="key"/>, or flips the direction when <param name="key"/> is already active
    /// </summary>
    public SortMode Toggle(SortKey key) =>
        key == Key ? this with { Descending = !Descending } : new SortMode(key, false);

    public SortKey Key { get; set; }
    public bool Descending { get; set; }

    public void Deconstruct(out SortKey key, out bool descending)
    {
        key = Key;
        descending = Descending;
    }

    public string Printable => Key == SortKey.Unsorted ? "Unsorted" : $"{Key} {(Descending ? "▼" : "▲")}";
}