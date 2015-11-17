using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for MediaNoteControl.xaml
    /// </summary>
    public partial class MediaNoteControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool hasMedia;
        public bool HasMedia
        {
            get { return hasMedia; }
            set
            {
                if (hasMedia != value)
                {
                    hasMedia = value;

                    var handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(nameof(HasMedia)));
                }
            }
        }

        private bool isPlaying;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;

                    var handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
                }
            }
        }

        private bool sliderUpdate;

        public MediaNoteControl()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (HasMedia && mediaElement_Main.NaturalDuration.HasTimeSpan && !sliderUpdate)
            {
                slider_Progress.Minimum = 0;
                slider_Progress.Maximum = mediaElement_Main.NaturalDuration.TimeSpan.TotalMilliseconds;
                slider_Progress.Value = mediaElement_Main.Position.TotalMilliseconds;
            }
        }

        private void mediaNoteControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!File.Exists(((MediaNote)DataContext).FileName))
                ((MediaNote)DataContext).TryRootingPath();
            if (File.Exists(((MediaNote)DataContext).FileName)) HasMedia = true;
        }

        private void PlayPause()
        {
            if (IsPlaying)
            {
                mediaElement_Main.Pause();
                IsPlaying = false;
            }
            else if (HasMedia)
            {
                mediaElement_Main.Play();
                IsPlaying = true;
            }
        }

        private void Loader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) PlayPause();
        }

        private void Loader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MainViewModel.openMediaDialog.ShowDialog() == true)
            {
                mediaElement_Main.Stop();
                IsPlaying = false;
                ((MediaNote)DataContext).FileName = MainViewModel.openMediaDialog.FileName;
            }
        }

        private void mediaElement_Main_MediaEnded(object sender, RoutedEventArgs e) => IsPlaying = false;

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) => PlayPause();

        private void mediaElement_Main_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (File.Exists(((MediaNote)DataContext).FileName) &&
                MessageBox.Show($"IdeaTree was unable to open this media file. Do you want to launch it with the default player instead?\n\n(Error: {e.ErrorException})",
                "Unable to Open Media", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                Process.Start(((MediaNote)DataContext).FileName);
        }

        private void slider_Progress_DragStarted(object sender, RoutedEventArgs e)
        {
            sliderUpdate = true;
        }

        private void slider_Progress_DragCompleted(object sender, RoutedEventArgs e)
        {
            mediaElement_Main.Position = TimeSpan.FromMilliseconds(slider_Progress.Value);
            sliderUpdate = false;
        }

        private void slider_Progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!sliderUpdate) mediaElement_Main.Position = TimeSpan.FromMilliseconds(slider_Progress.Value);
            label_Position.Content = TimeSpan.FromMilliseconds(slider_Progress.Value).ToString(@"hh\:mm\:ss");
            label_Duration.Content = TimeSpan.FromMilliseconds(slider_Progress.Maximum).ToString(@"hh\:mm\:ss");
        }
    }
}
