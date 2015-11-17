using System.Windows;
using System.Windows.Input;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for TitlesAndSuffixesTemplateEditor.xaml
    /// </summary>
    public partial class TitlesAndSuffixesTemplateEditor : Window
    {
        public static readonly RoutedCommand DeleteTitle = new RoutedCommand();
        public static readonly RoutedCommand AddTitle = new RoutedCommand();

        public static readonly RoutedCommand DeleteSuffix = new RoutedCommand();
        public static readonly RoutedCommand AddSuffix = new RoutedCommand();

        public TitlesAndSuffixesTemplateEditor() { InitializeComponent(); }

        private void DeleteTitle_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((CharacterTemplate)DataContext).Titles.Remove((string)e.Parameter);
        }

        private void AddTitle_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter the new title:", "New Title", false);
            if (!string.IsNullOrWhiteSpace(response)) ((CharacterTemplate)DataContext).Titles.Add(response);
        }

        private void DeleteSuffix_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((CharacterTemplate)DataContext).Suffixes.Remove((string)e.Parameter);
        }

        private void AddSuffix_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter the new suffix:", "New Suffix", false);
            if (!string.IsNullOrWhiteSpace(response)) ((CharacterTemplate)DataContext).Suffixes.Add(response);
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
