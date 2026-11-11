using Avalonia.Input;

namespace Bashka.Plugins.Core.Orthodox
{
    public record KeyCombination(bool Ctrl, bool Shift, bool Alt, string MainKey)
    {
        public KeyCombination(KeyEventArgs keyEventArgs) : this(keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Control),
            keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Shift),
            keyEventArgs.KeyModifiers.HasFlag(KeyModifiers.Alt),
            keyEventArgs.Key.ToString().ToUpper())
        {
        }

        public KeyCombination(string keyCombinationString) : this(false, false, false, "")
        {
            string[] parts = keyCombinationString.Split('+');
            foreach (string part in parts)
            {
                string trimmedPart = part.Trim().ToUpper();
                switch (trimmedPart)
                {
                    case "CTRL":
                        Ctrl = true;
                        break;
                    case "SHIFT":
                        Shift = true;
                        break;
                    case "ALT":
                        Alt = true;
                        break;
                    default:
                        if (MainKey != "")
                            throw new ArgumentException($"Key combination '{keyCombinationString}' is invalid");
                        MainKey = trimmedPart;
                        break;
                }
            }
            if (MainKey == "")
                throw new ArgumentException($"Key combination {keyCombinationString} is invalid");
        }
    }
}