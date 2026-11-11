namespace Bashka.Plugins.Core.Dialog.Basic;

public class OkCancelQuestionDialogContent : OkCancelDialogContentBase
{
    public string Question { get; }

    public OkCancelQuestionDialogContent(string title, string question) : base(title, new QuestionDialog(question))
    {
        Question = question;
    }
}