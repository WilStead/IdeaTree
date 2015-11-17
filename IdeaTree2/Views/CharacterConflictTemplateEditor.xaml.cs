using System.Windows;
using System.Windows.Input;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for CharacterConflictTemplateEditor.xaml
    /// </summary>
    public partial class CharacterConflictTemplateEditor : Window
    {
        public static readonly RoutedCommand DeleteProtagonist = new RoutedCommand();
        public static readonly RoutedCommand AddProtagonist = new RoutedCommand();

        public static readonly RoutedCommand DeleteSupportingCharacter = new RoutedCommand();
        public static readonly RoutedCommand AddSupportingCharacter = new RoutedCommand();

        public static readonly RoutedCommand DeleteConflict = new RoutedCommand();
        public static readonly RoutedCommand AddConflict = new RoutedCommand();

        public CharacterConflictTemplateEditor() { InitializeComponent(); }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void DeleteProtagonist_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((StoryTemplate)DataContext).Protagonists.Remove((string)e.Parameter);
        }

        private void AddProtagonist_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter a term for the new protagonist:", "New Protagonist", false);
            if (!string.IsNullOrWhiteSpace(response)) ((StoryTemplate)DataContext).Protagonists.Add(response);
        }

        private void DeleteSupportingCharacter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((StoryTemplate)DataContext).SupportingCharacters.Remove((string)e.Parameter);
        }

        private void AddSupportingCharacter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter a term for the new supporting character:", "New Supporting Character", false);
            if (!string.IsNullOrWhiteSpace(response)) ((StoryTemplate)DataContext).SupportingCharacters.Add(response);
        }

        private void DeleteConflict_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((StoryTemplate)DataContext).Conflicts.Remove((string)e.Parameter);
        }

        private void AddConflict_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter a term for the new conflict:", "New Conflict", false);
            if (!string.IsNullOrWhiteSpace(response)) ((StoryTemplate)DataContext).Conflicts.Add(response);
        }
    }
}
