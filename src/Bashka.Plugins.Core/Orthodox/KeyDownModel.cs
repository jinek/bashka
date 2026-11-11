using Avalonia.Input;

namespace Bashka.Plugins.Core.Orthodox
{
    public class KeyDownModel
    {
        private readonly KeyEventArgs _keyEventArgs;

        public KeyDownModel(KeyEventArgs keyEventArgs)
        {
            _keyEventArgs = keyEventArgs;
            KeyCombination = new KeyCombination(keyEventArgs);
        }

        public KeyCombination KeyCombination { get; }

        public void SetHandled() => _keyEventArgs.Handled = true;

        public bool Handled
        {
            get => _keyEventArgs.Handled;
            set => _keyEventArgs.Handled = value;
        }
    }
}