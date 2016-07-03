using Microsoft.Practices.Prism.Mvvm;
using MonitoredUndo;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;

namespace IdeaTree2
{
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 2)]
    [ProtoInclude(100, typeof(TextNote))]
    [ProtoInclude(200, typeof(FileNote))]
    [ProtoInclude(300, typeof(TimelineNote))]
    [ProtoInclude(400, typeof(StoryNote))]
    [ProtoInclude(500, typeof(CharacterNote))]
    public class IdeaNote : BindableBase, ICloneable, ISupportsUndo
    {
        [ProtoIgnore]
        public static string IdeaNoteExt = ".itn";
        
        [ProtoIgnore]
        public virtual string IdeaNoteType => nameof(IdeaNote);

        private string storedIdeaNoteType;
        public string StoredIdeaNoteType
        {
            get
            {
                if (string.IsNullOrEmpty(storedIdeaNoteType))
                    storedIdeaNoteType = IdeaNoteType;
                return storedIdeaNoteType;
            }
            set { storedIdeaNoteType = value; }
        }

        private string explicitName;
        public string ExplicitName
        {
            get { return explicitName; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(ExplicitName), explicitName, value);
                SetProperty(ref explicitName, value);
                OnPropertyChanged(nameof(Name));
            }
        }
        
        [ProtoIgnore]
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(ExplicitName)) return ExplicitName;
                else return GetDefaultName();
            }
            set { ExplicitName = value; }
        }

        private bool isSelected;
        [ProtoIgnore]
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty(ref isSelected, value);
                if (value) ShowInTree();
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        private Visibility visibility;
        [ProtoIgnore]
        public Visibility Visibility
        {
            get { return visibility; }
            set { SetProperty(ref visibility, value); }
        }

        private IdeaTreeSaveFile rootSaveFile;
        [ProtoIgnore]
        public IdeaTreeSaveFile RootSaveFile
        {
            get
            {
                if (rootSaveFile != null) return rootSaveFile;
                else if (Parent != null) return Parent.RootSaveFile;
                else return null;
            }
            set { SetProperty(ref rootSaveFile, value); }
        }

        private IdeaNote parent;
        [ProtoIgnore]
        public IdeaNote Parent
        {
            get { return parent; }
            set { SetProperty(ref parent, value); }
        }
        
        [ProtoIgnore]
        public ObservableCollection<IdeaNote> Ideas { get; set; } = new ObservableCollection<IdeaNote>();
        [ProtoMember(1, OverwriteList = true)]
        private IList<IdeaNote> IdeaList
        {
            get { return Ideas; }
            set
            {
                Ideas = new ObservableCollection<IdeaNote>(value);
                Ideas.CollectionChanged += Ideas_CollectionChanged;
                foreach (IdeaNote item in Ideas)
                    item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public IdeaNote() { Ideas.CollectionChanged += Ideas_CollectionChanged; }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            foreach (var child in Ideas) child.Parent = this;
        }

        private void Ideas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IdeaNote item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IdeaNote item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
            }
            if (RootSaveFile != null)
            {
                RootSaveFile.ChangedSinceLastSave = true;
                RootSaveFile.ChangedSinceLastSaveExceptExpansion = true;
            }
            DefaultChangeFactory.Current.OnCollectionChanged(this, nameof(Ideas), Ideas, e);
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (RootSaveFile != null && e.PropertyName != nameof(IsSelected))
            {
                RootSaveFile.ChangedSinceLastSave = true;
                if (e.PropertyName != nameof(IsExpanded))
                    RootSaveFile.ChangedSinceLastSaveExceptExpansion = true;
            }
        }

        public static IdeaNote FromFile(string path)
        {
            IdeaNote noteFile = null;

            using (Stream file = File.OpenRead(path))
            {
                using (var gzip = new GZipStream(file, CompressionMode.Decompress, true))
                {
                    noteFile = Serializer.Deserialize<IdeaNote>(gzip);
                }
            }

            return noteFile;
        }

        public void Export(string path)
        {
            using (Stream file = File.Create(path))
            {
                using (var gzip = new GZipStream(file, CompressionMode.Compress, true))
                {
                    Serializer.Serialize(gzip, this);
                }
            }
        }

        public static IdeaNote FromData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<IdeaNote>(stream);
            }
        }

        public byte[] GetData()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public virtual object Clone() => MemberwiseClone();

        public IdeaNote GetCopy()
        {
            IdeaNote copy = (IdeaNote)Clone();
            copy.IsSelected = false;
            copy.IsExpanded = false;
            return copy;
        }

        public IdeaNote GetChildlessCopy()
        {
            IdeaNote copy = GetCopy();
            copy.Ideas = new ObservableCollection<IdeaNote>();
            return copy;
        }

        public virtual string GetDefaultName() => "[Idea Note]";

        public IdeaTreeSaveFile GetRootSaveFile()
        {
            if (RootSaveFile != null) return RootSaveFile;
            else if (Parent != null) return Parent.GetRootSaveFile();
            else return null;
        }

        public IdeaNote GetTopLevelNote()
        {
            if (Parent != null) return Parent.GetTopLevelNote();
            else return this;
        }

        public void AddChild(IdeaNote idea)
        {
            if (idea == this) throw new Exception("Cannot add an IdeaNote as its own child.");

            if (idea.RootSaveFile != null)
            {
                idea.RootSaveFile.Ideas.Remove(idea);
                idea.RootSaveFile = null;
            }
            if (idea.Parent != null) idea.Parent.Ideas.Remove(idea);
            idea.Parent = this;
            Ideas.Add(idea);
        }

        public void InsertChild(int index, IdeaNote idea)
        {
            if (idea.RootSaveFile != null)
            {
                idea.RootSaveFile.Ideas.Remove(idea);
                idea.RootSaveFile = null;
            }
            if (idea.Parent != null) idea.Parent.Ideas.Remove(idea);
            idea.Parent = this;
            Ideas.Insert(index, idea);
        }

        public void ShowInTree()
        {
            if (Parent == null) return;
            Parent.IsExpanded = true;
            Parent.ShowInTree();
        }

        public object GetUndoRoot() => GetRootSaveFile()?.GetUndoRoot();

        public IdeaNote GetNextNoteInTree(bool skipChildren = false)
        {
            if (Ideas.Count > 0 && !skipChildren) return Ideas[0];
            else if (Parent != null)
            {
                int index = Parent.Ideas.IndexOf(this);
                if (index != (Parent.Ideas.Count - 1)) return Parent.Ideas[index + 1];
                else return Parent.GetNextNoteInTree(true);
            }
            else
            {
                IdeaTreeSaveFile save = GetRootSaveFile();
                if (save == null) return null;
                int index = save.Ideas.IndexOf(this);
                if (index != (save.Ideas.Count - 1)) return save.Ideas[index + 1];
                else return null;
            }
        }

        private IdeaNote GetLastChild()
        {
            if (Ideas.Count > 0) return Ideas[Ideas.Count - 1].GetLastChild();
            else return this;
        }

        public string GetPath()
        {
            if (Parent != null) return $"{Parent.GetPath()} > {Name}";
            else return Name;
        }

        public Stack<IdeaNote> GetRootToChildStack(Stack<IdeaNote> childStack = null)
        {
            if (childStack == null) childStack = new Stack<IdeaNote>();
            childStack.Push(this);
            if (Parent != null) return Parent.GetRootToChildStack(childStack);
            else return childStack;
        }

        public IdeaNote GetPrevNoteInTree()
        {
            if (Parent != null)
            {
                int index = Parent.Ideas.IndexOf(this);
                if (index != 0) return Parent.Ideas[index - 1].GetLastChild();
                else return Parent;
            }
            else
            {
                IdeaTreeSaveFile save = GetRootSaveFile();
                if (save == null) return null;
                int index = save.Ideas.IndexOf(this);
                if (index != 0) return save.Ideas[index - 1].GetLastChild();
                else return null;
            }
        }

        public int GetNewSiblingIndex()
        {
            if (Parent == null) return (RootSaveFile?.Ideas.IndexOf(this) ?? -2) + 1;
            else return Parent.Ideas.IndexOf(this) + 1;
        }

        public virtual TextNote ConvertToTextNote() => new TextNote();

        public void RefreshTemplate() => OnPropertyChanged(nameof(RootSaveFile));

        public virtual bool ShowOnlyMatchingGenres(ObservableCollection<NoteOption> requiredGenres, ObservableCollection<NoteOption> possibleGenres,
            ObservableCollection<NoteOption> excludedGenres, bool showNoGenres, bool force = false)
        {
            bool show = force;
            foreach (var child in Ideas)
                show = show || child.ShowOnlyMatchingGenres(requiredGenres, possibleGenres, excludedGenres, showNoGenres, force);
            if (show) Visibility = Visibility.Visible;
            else Visibility = Visibility.Collapsed;
            return show;
        }

        public void MakeVisible()
        {
            Visibility = Visibility.Visible;
            foreach (var child in Ideas) child.MakeVisible();
        }

        public virtual bool ContainsText(string text) => Name.ToLowerInvariant().Contains(text);
    }
    
    [ProtoContract(AsReferenceDefault = true)]
    public class TextNote : IdeaNote
    {
        public override string IdeaNoteType => nameof(TextNote);

        private string rtf;
        [ProtoMember(1)]
        public string Rtf
        {
            get { return rtf; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(Rtf), rtf, value);
                SetProperty(ref rtf, value);
            }
        }

        private static readonly string textExtension = ".txt";

        public static bool IsText(string path) => Path.GetExtension(path).Equals(textExtension, StringComparison.InvariantCultureIgnoreCase);

        public TextNote() : base() { }

        public override string GetDefaultName() => "[Text Note]";

        public void AddParagraph(string text)
        {
            Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
            rtb.Text = Rtf;
            rtb.Document.Blocks.Add(new Paragraph(new Run(text)));
            Rtf = rtb.Text;
        }

        public void PasteToRtf()
        {
            Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
            rtb.Text = Rtf;
            rtb.Selection.Select(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
            rtb.Paste();
            Rtf = rtb.Text;
        }

        public void SetRtfFromPlainText(string text)
        {
            Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
            rtb.Document.Blocks.Add(new Paragraph(new Run(text)));
            Rtf = rtb.Text;
        }

        public override TextNote ConvertToTextNote() => this;

        public override bool ContainsText(string text)
        {
            string matchText = text.ToLowerInvariant();
            return (Name.ToLowerInvariant().Contains(matchText) || Rtf.ToLowerInvariant().Contains(matchText));
        }
    }
    
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    [ProtoInclude(100, typeof(ImageNote))]
    [ProtoInclude(200, typeof(MediaNote))]
    public class FileNote : IdeaNote
    {
        [ProtoIgnore]
        public override string IdeaNoteType => nameof(FileNote);

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(FileName), fileName, value);
                SetProperty(ref fileName, value);
                if (string.IsNullOrEmpty(ExplicitName)) OnPropertyChanged(nameof(Name));

                if (!string.IsNullOrEmpty(fileName))
                {
                    Uri uri = new Uri(fileName);
                    Uri appUri = new Uri(AppDomain.CurrentDomain.BaseDirectory);
                    Uri relativeUri = appUri.MakeRelativeUri(uri);

                    if (relativeUri == uri) RelativePath = null; // paths aren't relative; fully-qualified path is required
                    else RelativePath = relativeUri.ToString();
                }
                else RelativePath = fileName;
            }
        }

        private string relativePath;
        public string RelativePath
        {
            get { return relativePath; }
            set { SetProperty(ref relativePath, value); }
        }

        [ProtoIgnore]
        public bool IsURL => !string.IsNullOrEmpty(FileName) && StringIsURL(FileName);

        public FileNote() : base() { }

        public override string GetDefaultName()
        {
            if (!string.IsNullOrEmpty(FileName)) return string.Format("[{0}]", FileName);
            else return "[File Note]";
        }

        /// <summary>
        /// If the absolute path is out of date, but the relative path still works
        /// (e.g. the IdeaTree file and the reference file were part of a subdirectory which was moved as a unit),
        /// then re-root the absolute path.
        /// </summary>
        public void TryRootingPath()
        {
            if (!File.Exists(FileName) && File.Exists(RelativePath))
                FileName = Path.GetFullPath(RelativePath);
        }

        public static string GetPathAsLink(string path) => path.Replace('\\', '/').Replace(" ", "%20").Insert(0, "file:///");

        public static Regex urlRegex =
            new Regex(@"^(((ht|f)tp(s?))\://)?((([a-zA-Z0-9_\-]{2,}\.)+[a-zA-Z]{2,})|((?:(?:25[0-5]|2[0-4]\d|[01]\d\d|\d?\d)(?(\.?\d)\.)){4}))(:[a-zA-Z0-9]+)?(/[a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~]*)?$",
                RegexOptions.Compiled);

        public static bool StringIsURL(string path) => urlRegex.IsMatch(path);

        public string GetFileNameAsLink() => GetPathAsLink(FileName);

        public override TextNote ConvertToTextNote()
        {
            TextNote newNote = new TextNote()
            {
                ExplicitName = this.ExplicitName,
                IsExpanded = this.IsExpanded,
                IsSelected = this.IsSelected
            };
            newNote.Ideas = Ideas;

            Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
            string text = GetFileNameAsLink();
            Hyperlink link = new Hyperlink(new Run(text), rtb.CaretPosition.GetInsertionPosition(LogicalDirection.Forward));
            link.NavigateUri = new Uri(text);
            link.RequestNavigate += (s, args) => Process.Start(args.Uri.ToString());
            newNote.Rtf = rtb.Text;

            return newNote;
        }

        public override bool ContainsText(string text) => (Name.ToLowerInvariant().Contains(text) || FileName.ToLowerInvariant().Contains(text));
    }
    
    [ProtoContract(AsReferenceDefault = true)]
    public class ImageNote : FileNote
    {
        public override string IdeaNoteType => nameof(ImageNote);

        private static ReadOnlyCollection<string> imageExtensions;
        public static ReadOnlyCollection<string> ImageExtensions => imageExtensions;

        public static string ImageExtensionsString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (string ext in ImageExtensions)
                {
                    if (sb.Length > 0) sb.Append(";");
                    sb.AppendFormat("*{0}", ext);
                }
                return sb.ToString();
            }
        }

        public static bool IsImage(string path) => ImageExtensions.Contains($"*{Path.GetExtension(path).ToLowerInvariant()}");

        static ImageNote()
        {
            List<string> imageExts = new List<string>();
            foreach (var codec in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
            {
                imageExts.AddRange(codec.FilenameExtension.ToLowerInvariant().Split(";".ToCharArray()));
            }
            imageExtensions = imageExts.AsReadOnly();
        }

        public ImageNote() : base() { }

        public override string GetDefaultName()
        {
            if (!string.IsNullOrEmpty(FileName)) return string.Format("[{0}]", FileName);
            else return "[Image Note]";
        }
    }
    
    [ProtoContract(AsReferenceDefault = true)]
    public class MediaNote : FileNote
    {
        public override string IdeaNoteType => nameof(MediaNote);

        public static ReadOnlyCollection<string> MediaExtensions = new ReadOnlyCollection<string>(new List<string>()
        {
            ".m4a",
            ".m4v",
            ".mp4v",
            ".mp4",
            ".mov",
            ".m2ts",
            ".asf",
            ".wm",
            ".wmv",
            ".wma",
            ".aac",
            ".adt",
            ".adts",
            ".mp3",
            ".wav",
            ".avi",
            ".ac3",
            ".ec3"
        });

        public static string MediaExtensionsString = "*.m4a;*.m4v;*.mp4v;*.mp4;*.mov;*.m2ts;*.asf;*.wm;*.wmv;*.wma;*.aac;*.adt;*.adts;*.mp3;*.wav;*.avi;*.ac3;*.ec3";

        public static bool IsMedia(string path) => MediaExtensions.Contains($"*{Path.GetExtension(path).ToLowerInvariant()}");

        public MediaNote() : base() { }

        public override string GetDefaultName()
        {
            if (!string.IsNullOrEmpty(FileName)) return string.Format("[{0}]", FileName);
            else return "[Media Note]";
        }
    }
}
