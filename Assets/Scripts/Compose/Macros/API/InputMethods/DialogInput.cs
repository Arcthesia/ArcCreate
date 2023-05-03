using EmmySharp;
using MoonSharp.Interpreter;

namespace ArcCreate.Compose.Macros
{
    [MoonSharpUserData]
    [EmmyDoc("Class for requesting input from user through a dialog.")]
    [EmmyGroup("Macros")]
    public class DialogInput
    {
        public string DialogTitle { get; private set; }

        public DialogField[] DialogFields { get; private set; }

        [EmmyDoc("Create a dialog with the provided title.")]
        public static DialogInput WithTitle(string message)
        {
            DialogInput newDialog = new DialogInput
            {
                DialogTitle = message,
            };
            return newDialog;
        }

        [EmmyDoc("Start the request. Returned value of each field is access through the key of the field.")]
        public MacroRequest RequestInput(DialogField[] fields)
        {
            DialogFields = fields;
            MacroRequest request = new MacroRequest();
            Services.Macros.CreateDialog(DialogTitle, fields, request);
            return request;
        }
    }
}