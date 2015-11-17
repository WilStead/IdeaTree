using System;
using System.Windows;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for PromptDialog.xaml
    /// </summary>
    public partial class PromptDialog : Window
    {
        public string Answer => (textBox_Password.Visibility == Visibility.Visible ? textBox_Password.Password : textBox_Answer.Text);

        /// <summary>
        /// Creates a new PromptDialog window.
        /// </summary>
        /// <param name="instructions">The instructions to show to the user.</param>
        /// <param name="title">The text to set as the window's title.</param>
        /// <param name="defaultAnswer">An optional default response (ignored for password dialogs).</param>
        /// <param name="isPassword">If true, the dialog will mask the response field.</param>
        public PromptDialog(string instructions, string title, bool isPassword = false, string defaultAnswer = "")
        {
            InitializeComponent();

            Title = title;
            label_Instructions.Content = instructions;
            if (isPassword)
            {
                textBox_Answer.Visibility = Visibility.Collapsed;
                textBox_Password.Visibility = Visibility.Visible;
            }
            else textBox_Answer.Text = defaultAnswer;
        }

        /// <summary>
        /// Displays a PromptDialog window, waits for a response, then returns the response.
        /// </summary>
        /// <param name="instructions">The instructions to show to the user.</param>
        /// <param name="title">The text to set as the window's title.</param>
        /// <param name="defaultAnswer">An optional default response (ignored for password dialogs).</param>
        /// <param name="isPassword">If true, the dialog will mask the response field.</param>
        /// <returns>The text entered in the response field; null if the dialog was cancelled.</returns>
        public static string Prompt(string instructions, string title, bool isPassword = false, string defaultAnswer = "")
        {
            PromptDialog dialog = new PromptDialog(instructions, title, isPassword, defaultAnswer);
            if (dialog.ShowDialog() == true) return dialog.Answer;
            return null;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (textBox_Password.Visibility == Visibility.Visible)
            {
                textBox_Password.SelectAll();
                textBox_Password.Focus();
            }
            else
            {
                textBox_Answer.SelectAll();
                textBox_Answer.Focus();
            }
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
