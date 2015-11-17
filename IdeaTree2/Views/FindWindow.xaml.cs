using System.Windows;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for FindWindow.xaml
    /// </summary>
    public partial class FindWindow : Window
    {
        private bool tree;

        public FindWindow(bool tree)
        {
            this.tree = tree;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MaxHeight = ActualHeight;
            if (tree)
            {
                dockPanel_ReplaceText.Visibility = Visibility.Collapsed;
                checkBox_MatchCase.Visibility = Visibility.Collapsed;
                button_Replace.Visibility = Visibility.Collapsed;
                button_ReplaceAll.Visibility = Visibility.Collapsed;
            }
            textBox_FindText.Focus();
        }

        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = Owner as MainWindow;
            if (tree) main?.FindTextInTree(textBox_FindText.Text, 1);
            else main?.FindText(textBox_FindText.Text, 0, checkBox_MatchCase.IsChecked ?? false);
        }

        private void FindPrev_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = Owner as MainWindow;
            if (tree) main?.FindTextInTree(textBox_FindText.Text, -1);
            else main?.FindText(textBox_FindText.Text, -1, checkBox_MatchCase.IsChecked ?? false);
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = Owner as MainWindow;
            main?.ReplaceText(textBox_FindText.Text, textBox_ReplaceText.Text, checkBox_MatchCase.IsChecked ?? false, false);
        }

        private void ReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = Owner as MainWindow;
            main?.ReplaceText(textBox_FindText.Text, textBox_ReplaceText.Text, checkBox_MatchCase.IsChecked ?? false, true);
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
