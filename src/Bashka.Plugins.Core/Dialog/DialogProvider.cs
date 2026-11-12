using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Bashka.Plugins.Core.Dialog.Abstract;
using Consolonia.Modal;

namespace Bashka.Plugins.Core.Dialog;

public class DialogProvider(Control parentControl) : IDialogProvider
{
    public async Task<TResult?> ShowDialogAsync<TResult>(IDialogContent<TResult> dialogContent)
    {
        object? findResource = parentControl.FindResource(dialogContent.TemplateKey);
        if (findResource is UnsetValueType)
            throw new KeyNotFoundException($"DialogContent {dialogContent.TemplateKey} template not found");

        var dialogWindow = new ModalWindow
        {
            Content = dialogContent,
            ContentTemplate = (IDataTemplate)findResource!,
            Title = dialogContent.Title
        };

        IReadOnlyList<string> classes = dialogContent.Classes;
        dialogWindow.Classes.AddRange(classes);
        
        TResult? dialogResult = default;
        dialogContent.OnClose += OnClose;
        
        /*_mainWindow.FindDescendantOfType<Control>().Focus();*/
        await dialogWindow.ShowModalAsync(parentControl).ContinueWith(_ => dialogContent.OnClose -= OnClose);

        return dialogResult! /*This must not execute if exception happened and dialogResult has not been initialized*/;

        void OnClose(TResult result)
        {
            dialogResult = result;
            dialogWindow.CloseModal();
        }
    }
}