using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for FileNoteControl.xaml
    /// </summary>
    public partial class FileNoteControl : UserControl
    {
        public FileNoteControl() { InitializeComponent(); }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!File.Exists(((FileNote)DataContext).FileName))
                ((FileNote)DataContext).TryRootingPath();
        }

        private void button_Open_Click(object sender, RoutedEventArgs e)
        {
            if (((FileNote)DataContext).IsURL || File.Exists(((FileNote)DataContext).FileName) || Directory.Exists(((FileNote)DataContext).FileName))
                System.Diagnostics.Process.Start(((FileNote)DataContext).FileName);
            else MessageBox.Show("Unable to identify this link. Please check the file name or URL.", "Invalid Link", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void SetFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.openAnyDialog.ShowDialog() == true)
            {
                if (ImageNote.IsImage(MainViewModel.openAnyDialog.FileName) &&
                    MessageBox.Show("This type of file can be displayed within IdeaTree.\n\nDo you want to show the file in an Image Note instead?",
                    "Add as Image Note?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ImageNote newNote = new ImageNote() { FileName = MainViewModel.openAnyDialog.FileName };
                    MainViewModel.ReplaceNote((FileNote)DataContext, newNote);
                    newNote.IsSelected = true;
                }
                else if (MediaNote.IsMedia(MainViewModel.openAnyDialog.FileName) &&
                    MessageBox.Show("This type of file can be displayed within IdeaTree.\n\nDo you want to show the file in a Media Note instead?",
                    "Add as Media Note?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    MediaNote newNote = new MediaNote() { FileName = MainViewModel.openAnyDialog.FileName };
                    MainViewModel.ReplaceNote((FileNote)DataContext, newNote);
                    newNote.IsSelected = true;
                }
                else ((FileNote)DataContext).FileName = MainViewModel.openAnyDialog.FileName;
            }
        }

        private void SetFolderButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok) ((FileNote)DataContext).FileName = folderDialog.FileName;
        }
    }
}
