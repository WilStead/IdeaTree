using System.Windows.Controls;
using System.Windows.Input;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for TimelineNoteControl.xaml
    /// </summary>
    public partial class TimelineNoteControl : UserControl
    {
        public static readonly RoutedCommand DeleteEvent = new RoutedCommand();
        public static readonly RoutedCommand AddEvent = new RoutedCommand();

        public TimelineNoteControl() { InitializeComponent(); }

        private void DeleteEvent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((TimelineNote)DataContext).Events.Remove((TimelineEvent)e.Parameter);
        }

        private void AddEvent_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((TimelineNote)DataContext).Events.Add(new TimelineEvent() { Date = ((TimelineNote)DataContext).Now });
            listBox_Events.Items.MoveCurrentToLast();
            listBox_Events.ScrollIntoView(listBox_Events.Items.CurrentItem);
        }
    }
}
