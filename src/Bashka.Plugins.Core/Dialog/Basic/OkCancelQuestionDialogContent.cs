using JetBrains.Annotations;

namespace Bashka.Plugins.Core.Dialog.Basic;

public class OkCancelQuestionDialogContent(string title, string question)
    : OkCancelDialogContentBase(title, new QuestionDialog(question))
{
    [UsedImplicitly]
    public string Question { [UsedImplicitly] get; } = question;
}