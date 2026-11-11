namespace Bashka.Plugins.Core.Dialog.Abstract
{
    public class DialogWithResultBase<TResult> : DialogBase
    {
        public TResult? Result { get; private set; }

        public void SetResult(TResult subresult)
        {
            Result = subresult;
            IsOkEnabled = true;
        }

        public void ResetResult()
        {
            Result = default!;
            IsOkEnabled = false;
        }
    }
}