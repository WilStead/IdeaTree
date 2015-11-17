using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for ImageNoteControl.xaml
    /// </summary>
    public partial class ImageNoteControl : UserControl
    {
        public ImageNoteControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!File.Exists(((ImageNote)DataContext).FileName))
                ((ImageNote)DataContext).TryRootingPath();
        }

        private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (File.Exists(((ImageNote)DataContext).FileName))
                Process.Start(((ImageNote)DataContext).FileName);
            else if (MainViewModel.openImageDialog.ShowDialog() == true)
                ((ImageNote)DataContext).FileName = MainViewModel.openImageDialog.FileName;
        }
    }
}
