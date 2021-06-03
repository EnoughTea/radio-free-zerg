using NStack;
using Terminal.Gui;

namespace RadioFreeZerg.Gui
{
    public class InputPrompt : Dialog
    {
        public const string DefaultConfirmationText = "Ok";
        public const string DefaultCancelText = "Cancel";
        
        private readonly Button cancelButton;
        private readonly Label inputLabel;
        private readonly TextField inputTextField;
        private readonly Label messageLabel;
        private readonly Button okButton;
        private bool allowEmpty = true;

        public InputPrompt()
            : this(string.Empty, string.Empty, string.Empty, string.Empty) { }

        /// <summary>
        ///     Initializes a new instance of <see cref="T:Terminal.Gui.FileDialog" />
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="confirmationButtonText">OK button text.</param>
        /// <param name="cancelButtonText">Cancel button text.</param>
        /// <param name="message">The message.</param>
        public InputPrompt(ustring title, ustring confirmationButtonText, ustring cancelButtonText, ustring message)
            : this(title, confirmationButtonText, cancelButtonText, ustring.Empty, message) { }

        /// <summary>
        ///     Initializes a new instance of <see cref="T:Terminal.Gui.FileDialog" />
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="confirmationButtonText">OK button text.</param>
        /// <param name="cancelButtonText">Cancel button text.</param>
        /// <param name="inputLabelText">The text of the input field label.</param>
        /// <param name="message">The message.</param>
        /// <param name="allowEmptyInput">if set to true, empty input is allowed; false otherwise.</param>
        public InputPrompt(ustring title,
                           ustring confirmationButtonText,
                           ustring cancelButtonText,
                           ustring inputLabelText,
                           ustring message,
                           bool allowEmptyInput = true)
            : base(title) {
            allowEmpty = allowEmptyInput;
            messageLabel = new Label(message) {X = 1, Y = 0};
            Add(messageLabel);

            var messageHeight = TextFormatter.MaxLines(message, Driver.Cols - 20);
            inputLabel = new Label(inputLabelText.IsEmpty ? "" : $"{inputLabelText}: ") {
                X = 1,
                Y = 1 + messageHeight,
                AutoSize = true
            };

            inputTextField = new TextField("") {
                X = Pos.Right(inputLabel),
                Y = 1 + messageHeight,
                Width = Dim.Fill() - 1
            };
            inputTextField.TextChanged += _ => {
                if (okButton != null) okButton.CanFocus = allowEmpty || !inputTextField.Text.IsEmpty;
            };
            Add(inputLabel, inputTextField);

            cancelButton = new Button(cancelButtonText.IsEmpty ? DefaultCancelText : cancelButtonText);
            cancelButton.Clicked += () => {
                Canceled = true;
                Application.RequestStop();
            };
            AddButton(cancelButton);

            okButton = new Button(confirmationButtonText.IsEmpty ? DefaultConfirmationText : confirmationButtonText) {
                IsDefault = true,
                CanFocus = allowEmpty || !inputTextField.Text.IsEmpty
            };
            okButton.Clicked += () => {
                Canceled = false;
                Application.RequestStop();
            };
            AddButton(okButton);

            Width = Dim.Percent(80f);
            Height = messageHeight + 6;
            Canceled = true;
        }

        /// <summary>
        ///     Gets or sets the prompt label for the <see cref="T:Terminal.Gui.Button" /> displayed to the user
        /// </summary>
        public ustring Prompt {
            get => okButton.Text;
            set => okButton.Text = value;
        }

        /// <summary>Gets or sets the text of the input field label.</summary>
        public ustring InputLabel {
            get => inputLabel.Text;
            set => inputLabel.Text = $"{value}: ";
        }

        /// <summary>Gets or sets the message displayed to the user, defaults to nothing.</summary>
        public ustring Message {
            get => messageLabel.Text;
            set => messageLabel.Text = value;
        }

        /// <summary>Gets or sets the input for this dialog.</summary>
        public ustring Input {
            get => inputTextField.Text;
            set => inputTextField.Text = value;
        }

        /// <summary>Check if the dialog was or not canceled.</summary>
        public bool Canceled { get; private set; }

        public static (string, bool) Display(string title,
                                             string confirmationButtonText = DefaultConfirmationText,
                                             string cancelButtonText = DefaultCancelText,
                                             string inputLabelText = "",
                                             string message = "") {
            var prompt = new InputPrompt(title, confirmationButtonText, inputLabelText, message);
            Application.Run(prompt);
            return (prompt.Input.IsEmpty ? "" : prompt.Input.ToString() ?? "", prompt.Canceled);
        }
    }
}