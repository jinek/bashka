using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.FunctionBar
{
    public class FunctionBarItem : BaseObject, INotifyPropertyChanged
    {
        private static readonly FunctionBarBinding EmptyWrapper = new()
        {
            Command = null,
            Name = string.Empty,
            IsDelete = false
        };
    
        public int Number { get; init; }
    
        public FunctionBarBinding[] CommandWrappers { get; init; }
        public FunctionBarBinding CurrentCommandWrapper { get; private set; } = EmptyWrapper;

        public async void Execute()
        {
            await CurrentCommandWrapper.Command?.ExecuteAsync(CurrentCommandWrapper.Context!)!;
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        public void ChooseCommand()
        {
            if (!CommandWrappers.Any())
            {
                CurrentCommandWrapper = EmptyWrapper;
                return;
            }

            FunctionBarBinding finalCommand = CommandWrappers.Last();
            finalCommand = CommandWrappers.LastOrDefault(c => c.CanExecute) ?? finalCommand;
            CurrentCommandWrapper = finalCommand;
        }
    }
}
