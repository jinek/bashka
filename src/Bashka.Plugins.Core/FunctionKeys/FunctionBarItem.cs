using Bashka.Plugins.Core.Scoping;

namespace Bashka.Plugins.Core.FunctionKeys
{
    public class FunctionBarItem : BaseObject
    {
        private static readonly FunctionBarBinding EmptyWrapper = new()
        {
            Command = null,
            Name = string.Empty,
            IsDelete = false
        };
    
        public int Number { get; init; }
    
        public required IReadOnlyList<FunctionBarBinding> CommandWrappers { get; init; }
        public FunctionBarBinding CurrentCommandWrapper { get; private set; } = EmptyWrapper;

        public async void Execute()
        {
            await CurrentCommandWrapper.Command?.ExecuteAsync(CurrentCommandWrapper.Context!)!;
        }

        public void ChooseCommand()
        {
            if (CommandWrappers.Count == 0)
            {
                CurrentCommandWrapper = EmptyWrapper;
                return;
            }

            FunctionBarBinding finalCommand = CommandWrappers[^1];
            finalCommand = CommandWrappers.LastOrDefault(c => c.CanExecute) ?? finalCommand;
            CurrentCommandWrapper = finalCommand;
        }
    }
}
