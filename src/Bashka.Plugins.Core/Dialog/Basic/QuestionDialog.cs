using Bashka.Plugins.Core.Dialog.Abstract;

namespace Bashka.Plugins.Core.Dialog.Basic
{
    public class QuestionDialog : DialogWithResultBase<object>
    {
        public QuestionDialog(string question)
        {
            Question = question;
            SetResult(null!);
        }

        public string Question { get; }
    }
}