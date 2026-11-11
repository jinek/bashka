using Bashka.Plugins.Core.Dialog.Abstract;
using JetBrains.Annotations;

namespace Bashka.Plugins.Core.Dialog.Basic;

public class StringInputDialog : DialogWithResultBase<string>
{
    private readonly InputValidationMode _validationMode;
    private string _input;

    public StringInputDialog(string question,
        string defaultValue = "",
        InputValidationMode validationMode = InputValidationMode.FileName)
    {
        _validationMode = validationMode;
        Question = question;
        Input = defaultValue;
        if (validationMode == InputValidationMode.None)
            SetResult(string.Empty);
    }

    public string Question { get; }
    public string ErrorText { get; private set; } = string.Empty;

    public string Input
    {
        get => _input;
        set => SetField(ref _input, value);
    }

    [UsedImplicitly]
    public void OnInputChanged(string _, string newValue)
    {
        SetError(string.Empty);
        // validating
        switch (_validationMode)
        {
            case InputValidationMode.None:
                return;
            case InputValidationMode.NotEmpty:
                if (string.IsNullOrWhiteSpace(newValue))
                    SetError("Input must not be empty");
                return;
            case InputValidationMode.FileName:
                if (string.IsNullOrWhiteSpace(newValue))
                {
                    SetError("Input must not be empty");
                    return;
                }

                char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
                if (newValue.IndexOfAny(invalidFileNameChars) >= 0)
                {
                    SetError("Input contains invalid characters: " + string.Join(", ", invalidFileNameChars));
                    return;
                }

                break;
            default:
                throw new NotSupportedException();
        }

        SetResult(Input);
    }

    private void SetError(string errorText)
    {
        ErrorText = errorText;
        ResetResult();
    }
}