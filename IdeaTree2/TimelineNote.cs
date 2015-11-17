using Microsoft.Practices.Prism.Mvvm;
using MonitoredUndo;
using ProtoBuf;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace IdeaTree2
{
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class TimelineEvent : BindableBase, ICloneable
    {
        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { SetProperty(ref date, value); }
        }

        private bool useYear;
        public bool UseYear
        {
            get { return useYear; }
            set { SetProperty(ref useYear, value); }
        }

        private bool useTime;
        public bool UseTime
        {
            get { return useTime; }
            set { SetProperty(ref useTime, value); }
        }

        private string rtf;
        public string Rtf
        {
            get { return rtf; }
            set
            {
                SetProperty(ref rtf, value);
                OnPropertyChanged(nameof(RtfFirstLine));
            }
        }

        private const int firstLineLength = 50;
        [ProtoIgnore]
        public string RtfFirstLine
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Rtf)) return Rtf;
                Xceed.Wpf.Toolkit.RichTextBox richTextConverter = new Xceed.Wpf.Toolkit.RichTextBox();
                richTextConverter.Text = Rtf;
                richTextConverter.TextFormatter = new Xceed.Wpf.Toolkit.PlainTextFormatter();
                if (richTextConverter.Text.Length > firstLineLength)
                    return $"{richTextConverter.Text.Substring(0, firstLineLength - 3)}...";
                else if (richTextConverter.Text.Length == firstLineLength)
                    return richTextConverter.Text.Substring(0, firstLineLength);
                else return richTextConverter.Text;
            }
        }

        public TimelineEvent() { }

        public object Clone() => MemberwiseClone();
    }
    
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TimelineNote : IdeaNote
    {
        [ProtoIgnore]
        public override string IdeaNoteType => nameof(TimelineNote);

        public DateTime Now { get; set; }

        public bool NowYear { get; set; }

        public bool NowTime { get; set; }

        public ObservableCollection<TimelineEvent> Events { get; set; } = new ObservableCollection<TimelineEvent>();

        public TimelineNote() : base()
        {
            Now = DateTime.Now;
            Events.CollectionChanged += Events_CollectionChanged;
        }

        private void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DefaultChangeFactory.Current.OnCollectionChanged(this, nameof(Events), Events, e);
        }

        public override string GetDefaultName() => "[Timeline Note]";

        public override bool ContainsText(string text)
        {
            string matchText = text.ToLowerInvariant();
            return (Name.ToLowerInvariant().Contains(matchText) || Events.Any(e => e.Rtf.ToLowerInvariant().Contains(matchText)));
        }

        public override TextNote ConvertToTextNote()
        {
            TextNote newNote = new TextNote()
            {
                ExplicitName = this.ExplicitName,
                IsExpanded = this.IsExpanded,
                IsSelected = this.IsSelected
            };
            newNote.Ideas = Ideas;
            
            newNote.AddParagraph(Now.ToString($"Now: dddd, MMMM d{(NowYear ? ", yyy" : string.Empty)}{(NowTime ? " h:mm tt" : string.Empty)}"));
            newNote.AddParagraph(string.Empty);
            newNote.AddParagraph("Events:");
            foreach (var timelineEvent in Events)
            {
                newNote.AddParagraph(timelineEvent.Date.ToString($"dddd, MMMM dd{(timelineEvent.UseYear ? ", yyy" : string.Empty)}{(timelineEvent.UseTime ? " h:mm tt" : string.Empty)}"));
                newNote.AddParagraph(string.Empty);
                Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
                rtb.Text = timelineEvent.Rtf;
                rtb.SelectAll();
                rtb.Copy();
                newNote.PasteToRtf();
            }

            return newNote;
        }
    }
}
