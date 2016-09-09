using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using ICSharpCode.SharpZipLib.Zip;
using IdeaTree2.Properties;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Win32;
using MonitoredUndo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace IdeaTree2
{
    public class MainViewModel : BindableBase, IDropTarget
    {
        private string currentSavePath;

        private static string autoSavePath = $"autosave{IdeaTreeSaveFile.IdeaTreeSaveFileExt}";

        private IdeaTreeSaveFile saveFile;
        public IdeaTreeSaveFile SaveFile
        {
            get { return saveFile; }
            set
            {
                SetProperty(ref saveFile, value);
                OnPropertyChanged(nameof(Title));
            }
        }

        private IdeaTreeTemplate template;
        public IdeaTreeTemplate Template
        {
            get { return template; }
            set
            {
                SetProperty(ref template, value);

                if (SaveFile != null)
                {
                    SaveFile.TemplatePath = template.CurrentPath;
                    SaveFile.Template = value;
                }

                OnPropertyChanged(nameof(Title));
            }
        }

        private string defaultTemplatePath = $"current_template{IdeaTreeTemplate.IdeaTreeTemplateExt}";

        private IdeaNote currentIdeaNote;
        public IdeaNote CurrentIdeaNote
        {
            get { return currentIdeaNote; }
            set
            {
                SetProperty(ref currentIdeaNote, value);
                if (value != null) currentIdeaNote.IsSelected = true;
                OnPropertyChanged(nameof(Title));
            }
        }

        public TextNote CurrentTextNote => CurrentIdeaNote as TextNote;

        public FileNote CurrentFileNote => CurrentIdeaNote as FileNote;

        public CharacterNote CurrentCharacterNote => CurrentIdeaNote as CharacterNote;

        public string Title
        {
            get
            {
                StringBuilder sb = new StringBuilder("IdeaTree - ");
                if (CurrentIdeaNote == null) return sb.ToString();
                if (!string.IsNullOrEmpty(currentSavePath))
                {
                    sb.Append(currentSavePath);
                    sb.Append(" ");
                }
                if (!string.IsNullOrEmpty(Template?.CurrentPath) && Template.CurrentPath != defaultTemplatePath)
                {
                    sb.Append("(");
                    sb.Append(Path.GetFileNameWithoutExtension(Template.CurrentPath));
                    sb.Append(") ");
                }
                sb.Append(": ");
                sb.Append(CurrentIdeaNote.GetPath());
                if (SaveFile.ChangedSinceLastSaveExceptExpansion)
                    sb.Append('*');
                return sb.ToString();
            }
        }

        private bool showNoGenres = true;
        public bool ShowNoGenres
        {
            get { return showNoGenres; }
            set
            {
                SetProperty(ref showNoGenres, value);
                ApplyGenreFilter();
            }
        }

        private bool showGenres;
        public bool ShowGenres
        {
            get { return showGenres; }
            set { SetProperty(ref showGenres, value); }
        }

        private ObservableCollection<NoteOption> genres;
        public ObservableCollection<NoteOption> Genres
        {
            get { return genres; }
            set { SetProperty(ref genres, value); }
        }

        private ObservableCollection<NoteOption> possibleGenres;
        public ObservableCollection<NoteOption> PossibleGenres
        {
            get { return possibleGenres; }
            set { SetProperty(ref possibleGenres, value); }
        }

        private ObservableCollection<NoteOption> excludedGenres;
        public ObservableCollection<NoteOption> ExcludedGenres
        {
            get { return excludedGenres; }
            set { SetProperty(ref excludedGenres, value); }
        }

        private bool copyShowingGenres;
        public bool CopyShowingGenres
        {
            get { return copyShowingGenres; }
            set { SetProperty(ref copyShowingGenres, value); }
        }

        private string searchText;

        public static OpenFileDialog openImageDialog = new OpenFileDialog() { Filter = $"Image Files|{ImageNote.ImageExtensionsString}|All Files|*.*" };
        public static OpenFileDialog openMediaDialog = new OpenFileDialog() { Filter = $"Media Files|{MediaNote.MediaExtensionsString}|All Files|*.*" };
        public static OpenFileDialog openAnyDialog = new OpenFileDialog() { Filter = "All Files|*.*" };

        public MainViewModel()
        {
            ParseArguments();

            // Template may be already set be loading a file which specified one, if that Template was found and loaded successfully;
            // otherwise, load one.
            if (Template == null) SetStartingTemplate();

            Timer autoSaveTimer = new Timer(AutoSave, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        #region Templates

        private bool LoadTemplate(string path, bool silent)
        {
            try
            {
                Template = IdeaTreeTemplate.FromFile(path);
                Settings.Default.MostRecentTemplate = path;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                if (!silent) MessageBox.Show(Application.Current.MainWindow, string.Format("There was a problem loading the Template file: \"{0}\"\n\n(Error:\n{1})",
                    path, ex.Message), "Unable to Load Template", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        public void LoadTemplate()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = $"IdeaTree Templates|*{IdeaTreeTemplate.IdeaTreeTemplateExt}|All Files|*.*";
            if (openDialog.ShowDialog() == true) LoadTemplate(openDialog.FileName, false);
        }

        public void NewTemplate()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = IdeaTreeTemplate.IdeaTreeTemplateExt;
            saveDialog.Filter = $"IdeaTree Templates|*{IdeaTreeTemplate.IdeaTreeTemplateExt}|All Files|*.*";
            saveDialog.OverwritePrompt = true;
            saveDialog.AddExtension = true;
            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    Template.Save(saveDialog.FileName);
                    Settings.Default.MostRecentTemplate = saveDialog.FileName;
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, string.Format("Unable to save file \"{0}\"\n\n(Error: {1})", saveDialog.FileName, ex.Message),
                        "Error Saving File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetStartingTemplate()
        {
            if (Settings.Default.UseMostRecentTemplate &&
                !string.IsNullOrWhiteSpace(Settings.Default.MostRecentTemplate) &&
                File.Exists(Settings.Default.MostRecentTemplate))
            {
                try { Template = IdeaTreeTemplate.FromFile(Settings.Default.MostRecentTemplate); }
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow,
                        string.Format("The most recently used Template file could not be loaded.\nThe default Template will be used instead.\n\n(Error:\n{0})",
                        ex.Message), "Unable to Load Template", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            if (Template == null) UseDefaultTemplate();

            // Setting the initial template doesn't count as a change for the purposes of confirming closing without saving.
            SaveFile.ChangedSinceLastSave = false;
            SaveFile.ChangedSinceLastSaveExceptExpansion = false;
        }

        public void UseDefaultTemplate()
        {
            Template = IdeaTreeTemplate.DefaultTemplate;
            Template.CurrentPath = defaultTemplatePath;
            Settings.Default.MostRecentTemplate = null;
            Settings.Default.Save();
        }

        #endregion Templates

        #region Opening

        private string GetFreeFileName(string fileName)
        {
            string freeFileName = fileName;
            int counter = 1;
            while (File.Exists(Path.Combine(Path.GetTempPath(), freeFileName)))
            {
                freeFileName = string.Format("{0}_{1}", fileName, counter);
                counter++;
            }
            return freeFileName;
        }

        private IdeaTreeSaveFile LoadSaveFileBase(string path)
        {
            IdeaTreeSaveFile newSaveFile;
            try { newSaveFile = IdeaTreeSaveFile.FromFile(path); }
            catch (Exception ex)
            {
                if (Template == null) SetStartingTemplate();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(Application.Current.MainWindow, string.Format("There was a problem loading the file: \"{0}\"\n\n(Error:\n{1})",
                        path, ex.Message), "Unable to Load File", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
                return null;
            }

            if (!string.IsNullOrEmpty(newSaveFile.Password))
            {
                string response = null;
                do
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        response = PromptDialog.Prompt("Please enter this file's password:", "This File is Password-Protected", true);
                    });
                } while (!string.IsNullOrEmpty(response) && response != newSaveFile.Password);

                if (string.IsNullOrEmpty(response)) return null;
            }

            return newSaveFile;
        }

        private bool LoadSaveFile(string path)
        {
            IdeaTreeSaveFile newSaveFile = LoadSaveFileBase(path);
            if (newSaveFile == null) return false;

            if (!string.IsNullOrEmpty(newSaveFile.TemplatePath) && File.Exists(newSaveFile.TemplatePath))
            {
                LoadTemplate(newSaveFile.TemplatePath, true);
                newSaveFile.Template = Template;
                newSaveFile.TemplatePath = Template.CurrentPath;
            }
            else newSaveFile.Template = Template;

            SaveFile = newSaveFile;
            currentSavePath = path;

            if (SaveFile.Ideas.Count > 0) CurrentIdeaNote = SaveFile.Ideas[0];

            SaveFile.ChangedSinceLastSave = false;
            SaveFile.ChangedSinceLastSaveExceptExpansion = false;

            return true;
        }

        private void LoadSaveFile(IdeaTreeSaveFile saveFile, bool replace, IdeaNote parent, int index)
        {
            if (replace)
            {
                SaveFile = saveFile;

                if (!string.IsNullOrEmpty(saveFile.TemplatePath) && File.Exists(saveFile.TemplatePath))
                    LoadTemplate(saveFile.TemplatePath, true);

                if (SaveFile.Ideas.Count > 0) CurrentIdeaNote = SaveFile.Ideas[0];

                SaveFile.ChangedSinceLastSave = false;
                SaveFile.ChangedSinceLastSaveExceptExpansion = false;
            }
            else if (parent == null)
            {
                IdeaNote original = CurrentIdeaNote;
                if (index == -1)
                    SaveFile.Ideas = new ObservableCollection<IdeaNote>(SaveFile.Ideas.Concat(saveFile.Ideas));
                else SaveFile.Ideas = new ObservableCollection<IdeaNote>(SaveFile.Ideas.Take(index).Concat(saveFile.Ideas).Concat(SaveFile.Ideas.Skip(index)));
                CurrentIdeaNote = original;
            }
            else
            {
                IdeaNote original = CurrentIdeaNote;
                if (index == -1) parent.Ideas = new ObservableCollection<IdeaNote>(parent.Ideas.Concat(saveFile.Ideas));
                else parent.Ideas = new ObservableCollection<IdeaNote>(parent.Ideas.Take(index).Concat(saveFile.Ideas).Concat(parent.Ideas.Skip(index)));
                foreach (IdeaNote note in parent.Ideas) note.Parent = parent;
                CurrentIdeaNote = original;
            }
        }

        private bool ImportNote(byte[] data, IdeaNote parent, int index)
        {
            IdeaNote note = IdeaNote.FromData(data);
            if (note == null) return false;

            AddNote(note, parent, index);
            return true;
        }

        private bool ImportNote(string path, bool silent, IdeaNote parent, int index)
        {
            try
            {
                IdeaNote note = IdeaNote.FromFile(path);
                if (note == null) return false;

                AddNote(note, parent, index);
                return true;
            }
            catch (Exception ex)
            {
                if (!silent) MessageBox.Show(string.Format("There was a problem importing the file: \"{0}\"\n\n(Error:\n{1})",
                    path, ex.Message), "Unable to Import File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        /// <summary>
        /// Loads a file, and interprets how it should be added.
        /// </summary>
        /// <param name="path">The path of the file to load.</param>
        /// <param name="replace">Indicates whether loading this file will replace the current SaveFile, or be added as a new Note within it.</param>
        /// <param name="parent">Target parent Note for the new Note; may be null.</param>
        /// <param name="index">Index in teh child collection at which to add the new Note; -1 indicates that it should be added to the end of the list.</param>
        private void AddFile(string path, bool replace, IdeaNote parent, int index)
        {
            if (Path.GetExtension(path).Equals(IdeaTreeSaveFile.IdeaTreeSaveFileExt, StringComparison.InvariantCultureIgnoreCase))
            {
                if (replace)
                {
                    if (CheckClosing()) LoadSaveFile(path);
                }
                else
                {
                    IdeaTreeSaveFile newSaveFile = LoadSaveFileBase(path);
                    if (newSaveFile != null) LoadSaveFile(newSaveFile, false, parent, index);
                }
                return;
            }
            else if (Path.GetExtension(path).Equals(IdeaNote.IdeaNoteExt, StringComparison.InvariantCultureIgnoreCase))
            {
                ImportNote(path, false, parent, index);
                return;
            }
            else if (Path.GetExtension(path).Equals(IdeaTreeTemplate.IdeaTreeTemplateExt, StringComparison.InvariantCultureIgnoreCase))
            {
                LoadTemplate(path, false);
                return;
            }
            else if (MediaNote.IsMedia(path)) LoadMediaNote(path, parent, index);
            else if (ImageNote.IsImage(path)) LoadImageNote(path, parent, index);
            else if (TextNote.IsText(path)) LoadTextNote(path, parent, index);
            else if (string.IsNullOrEmpty(Path.GetExtension(path)) && Directory.Exists(path))
            {
                AddFolder(path, replace, parent, index);
                return;
            }
            else LoadFileNote(path, parent, index);
        }

        private void AddFolder(string path, bool replace, IdeaNote parent, int index)
        {
            if (replace)
            {
                if (!CheckClosing()) return;
                SaveFile = new IdeaTreeSaveFile();
            }

            TextNote newNote = NewTextNote(parent, index);
            DirectoryInfo dInfo = new DirectoryInfo(path);
            newNote.ExplicitName = dInfo.Name;

            for (int i = 0; i < dInfo.GetFileSystemInfos().Count(); i++)
            {
                AddFile(dInfo.GetFileSystemInfos()[i].FullName, false, newNote, -1);
            }
        }

        /// <summary>
        /// Opens the specified file. IdeaTree files are loaded; other filetypes are added as Notes.
        /// </summary>
        /// <param name="path">The path of the file to open or add.</param>
        /// <returns>True if the file was opened successfully.</returns>
        private bool OpenFile(string path)
        {
            if (Path.GetExtension(path) == IdeaTreeSaveFile.IdeaTreeSaveFileExt)
            {
                if (!CheckClosing()) return false;
                return LoadSaveFile(path);
            }
            else
            {
                if (SaveFile == null) SaveFile = new IdeaTreeSaveFile();
                AddFile(path, true, null, -1);
            }
            return true;
        }

        /// <summary>
        /// If the specified file is an IdeaTree file, it is opened in a separate instance of the program.
        /// If it is any other type of file, it is added to the current file as a Note.
        /// </summary>
        /// <param name="path">The path of the file to open or add.</param>
        /// <returns>
        /// True if the file was opened successfully in this instance ONLY.
        /// False if a new process was started to open it, or if it could not be opened at all for any reason.
        /// </returns>
        public bool RelaunchOrAddFile(string path, IdeaNote parent, int index)
        {
            if (Path.GetExtension(path) == IdeaTreeSaveFile.IdeaTreeSaveFileExt)
            {
                Process.Start(Environment.GetCommandLineArgs()[0], path);
                return false;
            }
            else
            {
                if (SaveFile == null) SaveFile = new IdeaTreeSaveFile();
                AddFile(path, false, parent, index);
            }
            return true;
        }

        public bool LoadFiles(IList<string> paths, bool replace, IdeaNote parent, int index)
        {
            if (paths.Count == 0) return false;

            bool batch = false;
            if (SaveFile != null)
            {
                batch = true;
                UndoService.Current[SaveFile].BeginChangeSetBatch("Loading Files", false);
            }

            bool loaded = false;
            int startIndex = 0;
            if (replace && File.Exists(paths[0]))
            {
                loaded = OpenFile(paths[0]);
                startIndex++;
                if (index != -1) index++;
            }
            for (int i = startIndex; i < paths.Count; i++)
            {
                if (File.Exists(paths[i]) || Directory.Exists(paths[i]))
                {
                    loaded = loaded || RelaunchOrAddFile(paths[i], parent, index);
                    if (index != -1) index++;
                }
            }

            if (batch) UndoService.Current[SaveFile].EndChangeSetBatch();

            return loaded;
        }

        private void ParseArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            bool loaded = false;
            if (args.Length > 1) // first arg is name of executable
                loaded = LoadFiles(args.Skip(1).ToList(), true, null, -1);
            if (!loaded) NewFile();

            // Check for an autosave.
            if (File.Exists(autoSavePath) &&
                MessageBox.Show(Application.Current.MainWindow, "An automatic backup of an unsaved file was detected from your last session. Recover the file?",
                "Recover Autosave?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                LoadFiles(new List<string>() { autoSavePath }, false, null, -1);
        }

        public void Open(bool replace, IdeaNote parent, int index)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = string.Format($"IdeaTree Files|*{IdeaTreeSaveFile.IdeaTreeSaveFileExt}|Text Files|*.txt;*.rtf|Image Files|{0}|Media Files|{1}|IdeaNote Files|*{IdeaNote.IdeaNoteExt}|All Files|*.*",
                ImageNote.ImageExtensionsString, MediaNote.MediaExtensionsString);
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == true) LoadFiles(openDialog.FileNames, replace, parent, index);
        }

        public void Revert()
        {
            bool abort = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (MessageBox.Show(Application.Current.MainWindow, "Are you sure you want to discard all changes to this file?\nWARNING: This operation cannot be undone!",
                "Are You Sure?", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                    abort = true;
            });
            if (abort) return;
            if (string.IsNullOrEmpty(currentSavePath)) NewFile(true);
            else LoadSaveFile(currentSavePath);
        }

        #endregion Opening

        #region Saving

        public void AutoSave(object state)
        {
            if (SaveFile != null && SaveFile.ChangedSinceLastSave)
            {
                string savedPath = currentSavePath;
                try { SaveAs(autoSavePath); }
                catch (Exception) { } // Failures are allowed to pass silently; this is only a background auto-save.
                finally { currentSavePath = savedPath; }
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(currentSavePath)) SaveAs();
            else
            {
                try { SaveAs(currentSavePath); }
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, string.Format("Unable to save file \"{0}\"\n\n(Error: {1})", currentSavePath, ex.Message), "Error Saving File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void SaveAs()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = IdeaTreeSaveFile.IdeaTreeSaveFileExt;
            saveDialog.Filter = $"IdeaTree Files|*{IdeaTreeSaveFile.IdeaTreeSaveFileExt}|All Files|*.*";
            saveDialog.OverwritePrompt = true;
            saveDialog.AddExtension = true;
            if (saveDialog.ShowDialog() == true)
            {
                try { SaveAs(saveDialog.FileName); }
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, string.Format("Unable to save file \"{0}\"\n\n(Error: {1})", saveDialog.FileName, ex.Message), "Error Saving File", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveAs(string path)
        {
            if (SaveFile?.ChangedSinceLastSave == false && path == currentSavePath) return;
            SaveFile.Save(path);
            currentSavePath = path;
            SaveFile.ChangedSinceLastSave = false;
            SaveFile.ChangedSinceLastSaveExceptExpansion = false;
            OnPropertyChanged(nameof(Title));

            // Remove the autosave in favor of the real thing.
            if (File.Exists(autoSavePath))
            {
                try { File.Delete(autoSavePath); }
                catch (Exception) { } // Failure is allowed to pass silently; this is only the autosave.
            }

            CommandManager.InvalidateRequerySuggested();
        }

        public bool CheckClosing()
        {
            if (SaveFile?.ChangedSinceLastSave == true)
            {
                MessageBoxResult result = MessageBoxResult.Cancel;
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    result = MessageBox.Show(Application.Current.MainWindow, "Do you want to save the changes to the current file before closing it?",
                        "Save?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        result = MessageBox.Show(Application.Current.MainWindow, "Do you want to save the changes to the current file before closing it?",
                            "Save?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);
                    }));
                }
                if (result == MessageBoxResult.Cancel) return false;
                else if (result == MessageBoxResult.Yes) Save();
            }
            return true;
        }

        #region Exporting

        public void ExportNote(bool includeChildren)
        {
            if (CurrentIdeaNote == null) return;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = Path.Combine(saveDialog.InitialDirectory, $"{CurrentIdeaNote.Name}.itn");
            saveDialog.DefaultExt = IdeaNote.IdeaNoteExt;
            saveDialog.Filter = $"Idea Tree Note Files|*{IdeaNote.IdeaNoteExt}|All Files|*.*";
            saveDialog.OverwritePrompt = true;
            saveDialog.AddExtension = true;
            if (saveDialog.ShowDialog() == true)
            {
                if (includeChildren) CurrentIdeaNote.Export(saveDialog.FileName);
                else CurrentIdeaNote.GetChildlessCopy().Export(saveDialog.FileName);
            }
        }

        public void ExportTextNote(string extension, string dataType)
        {
            if (CurrentIdeaNote == null || CurrentIdeaNote.IdeaNoteType != nameof(TextNote)) return;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = Path.Combine(saveDialog.InitialDirectory, $"{CurrentIdeaNote.Name}.{extension}");
            saveDialog.DefaultExt = $".{extension}";
            saveDialog.Filter = $"{(extension == "rtf" ? "Rich " : string.Empty)}Text Files|*.{extension}|All Files|*.*";
            saveDialog.OverwritePrompt = true;
            saveDialog.AddExtension = true;
            if (saveDialog.ShowDialog() == true)
            {
                using (FileStream stream = new FileStream(saveDialog.FileName, FileMode.OpenOrCreate))
                {
                    Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
                    rtb.Text = ((TextNote)CurrentIdeaNote).Rtf;
                    TextRange text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                    text.Save(stream, dataType);
                }
            }
        }

        public void ExportNoteAsRtf() => ExportTextNote("rtf", DataFormats.Rtf);

        public void ExportNoteAsTxt() => ExportTextNote("txt", DataFormats.Text);

        #endregion Exporting

        #endregion Saving

        public void SetPassword()
        {
            string response = PromptDialog.Prompt("Select a password for this file:", "Select a Password", true);
            if (!string.IsNullOrWhiteSpace(response)) SaveFile.Password = response;
        }

        public void NewFile(bool silent = false)
        {
            if (SaveFile?.ChangedSinceLastSave == true && !silent && !CheckClosing()) return;

            SaveFile = new IdeaTreeSaveFile();
            currentSavePath = null;

            SetStartingTemplate();
            
            NewTextNote(null, -1);

            SaveFile.ChangedSinceLastSave = false;
            SaveFile.ChangedSinceLastSaveExceptExpansion = false;
        }

        public bool CanAcceptData(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo != null || (dropInfo.Data is DataObject && ((DataObject)dropInfo.Data).ContainsFileDropList()))
                return true;

            else return DefaultDropHandler.CanAcceptData(dropInfo);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (CanAcceptData(dropInfo))
            {
                // when source is the same as the target set the move effect otherwise set the copy effect
                var moveData = dropInfo.DragInfo?.VisualSource == dropInfo.VisualTarget
                               || (dropInfo.DragInfo?.DragDropCopyKeyState != null && !dropInfo.KeyStates.HasFlag(dropInfo.DragInfo.DragDropCopyKeyState))
                               || dropInfo.DragInfo?.VisualSourceItem is TabItem
                               || dropInfo.DragInfo?.VisualSourceItem is TreeViewItem
                               || dropInfo.DragInfo?.VisualSourceItem is MenuItem
                               || dropInfo.DragInfo?.VisualSourceItem is ListBoxItem;
                dropInfo.Effects = moveData ? DragDropEffects.Move : DragDropEffects.Copy;
                var isTreeViewItem = dropInfo.InsertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter) && dropInfo.VisualTargetItem is TreeViewItem;
                dropInfo.DropTargetAdorner = isTreeViewItem ? DropTargetAdorners.Highlight : DropTargetAdorners.Insert;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo == null || (dropInfo.DragInfo == null && !((DataObject)dropInfo.Data).ContainsFileDropList()))
            {
                return;
            }

            var insertIndex = dropInfo.InsertIndex != dropInfo.UnfilteredInsertIndex ? dropInfo.UnfilteredInsertIndex : dropInfo.InsertIndex;
            var destinationList = dropInfo.TargetCollection.TryGetList();
            var data = DefaultDropHandler.ExtractData(dropInfo.Data);

            object targetParent = dropInfo.TargetItem;
            if (targetParent is IdeaNote)
            {
                if (dropInfo.TargetCollection != ((IdeaNote)targetParent).Ideas)
                {
                    if (((IdeaNote)targetParent).Parent == null) targetParent = ((IdeaNote)targetParent).RootSaveFile;
                    else targetParent = ((IdeaNote)targetParent).Parent;
                }
            }

            if (dropInfo.Data is DataObject && ((DataObject)dropInfo.Data).ContainsFileDropList())
            {
                IEnumerable<string> files = ((DataObject)dropInfo.Data).GetFileDropList().TryGetList().Cast<string>();
                LoadFiles(files.Where(f => File.Exists(f) || Directory.Exists(f)).ToList(), false, targetParent as IdeaNote, insertIndex);
                return;
            }

            IdeaNote original = CurrentIdeaNote;

            // when source is the same as the target remove the data from source and fix the insertion index
            var moveData = dropInfo.DragInfo.VisualSource == dropInfo.VisualTarget
                           || !dropInfo.KeyStates.HasFlag(dropInfo.DragInfo.DragDropCopyKeyState)
                           || dropInfo.DragInfo.VisualSourceItem is TabItem
                           || dropInfo.DragInfo.VisualSourceItem is TreeViewItem
                           || dropInfo.DragInfo.VisualSourceItem is MenuItem
                           || dropInfo.DragInfo.VisualSourceItem is ListBoxItem;
            var sourceList = dropInfo.DragInfo.SourceCollection.TryGetList();

            // check for cloning
            var cloneData = dropInfo.Effects.HasFlag(DragDropEffects.Copy)
                            || dropInfo.Effects.HasFlag(DragDropEffects.Link);
            foreach (var o in data)
            {
                var obj2Insert = o;
                if (cloneData)
                {
                    var cloneable = o as ICloneable;
                    if (cloneable != null) obj2Insert = cloneable.Clone();
                }

                if (obj2Insert is IdeaNote)
                {
                    // Fail silently; an error messgae would be more intrusive than necessary: the problem should be immediately apparent.
                    if (targetParent is IdeaNote && obj2Insert == targetParent) continue;

                    if (moveData)
                    {
                        var index = sourceList.IndexOf(o);

                        if (index != -1)
                        {
                            sourceList.RemoveAt(index);
                            // so, is the source list the destination list too ?
                            if (Equals(sourceList, destinationList) && index < insertIndex) --insertIndex;
                            else
                            {
                                // clear Relationship for moved Characters, if not being moved within the same child collection
                                if (obj2Insert is CharacterNote && ((CharacterNote)obj2Insert).Relationship != null)
                                    ((CharacterNote)obj2Insert).Relationship = null;
                            }
                        }
                    }

                    if (targetParent is IdeaNote)
                    {
                        // If both Notes are CharacterNotes, and the moved note is either being moved to a new Parent,
                        // or else had no Relationship to an existing Parent, ask if the user wants to create a Relationship now.
                        bool addRelationship = false;
                        if (((IdeaNote)targetParent).IdeaNoteType == nameof(CharacterNote) &&
                            ((IdeaNote)o).IdeaNoteType == nameof(CharacterNote) &&
                            (((IdeaNote)o).Parent != targetParent || ((CharacterNote)o).Relationship == null) &&
                            MessageBox.Show("Do you want to create a relationship between these two Characters?",
                            "Add Relationship?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            addRelationship = true;
                        ((IdeaNote)targetParent).InsertChild(insertIndex++, (IdeaNote)obj2Insert);
                        if (addRelationship) AddOrChangeRelationship((CharacterNote)obj2Insert);
                    }
                    else if (targetParent is IdeaTreeSaveFile)
                        ((IdeaTreeSaveFile)targetParent).InsertIdea(insertIndex++, (IdeaNote)obj2Insert);
                    else destinationList.Insert(insertIndex++, obj2Insert);
                }
                else destinationList.Insert(insertIndex++, obj2Insert);
            }

            CurrentIdeaNote = original;
            CurrentIdeaNote.ShowInTree();
        }

        #region Notes

        #region Tree Operations

        public static void ReplaceNote(IdeaNote original, IdeaNote newNote)
        {
            int index = -1;
            if (original.Parent != null)
            {
                index = original.Parent.Ideas.IndexOf(original);
                original.Parent.Ideas.Remove(original);
                original.Parent.InsertChild(index, newNote);
            }
            else
            {
                index = original.RootSaveFile.Ideas.IndexOf(original);
                original.RootSaveFile.Ideas.Remove(original);
                original.RootSaveFile.InsertIdea(index, newNote);
            }
        }

        #region Adding Notes

        private void AddNote(IdeaNote note, IdeaNote parent, int index)
        {
            if (parent == null)
            {
                if (index == -1) SaveFile.AddIdea(note);
                else SaveFile.InsertIdea(index, note);
            }
            else if (index == -1) parent.AddChild(note);
            else parent.InsertChild(index, note);
            CurrentIdeaNote = note;
        }

        public void NewMediaNote(IdeaNote parent, int index)
        {
            if (openMediaDialog.ShowDialog() == true)
                AddNote(new MediaNote() { FileName = openMediaDialog.FileName }, parent, index);
        }

        private void LoadMediaNote(string path, IdeaNote parent, int index)
        {
            MediaNote newNote = new MediaNote();
            newNote.FileName = path;
            AddNote(newNote, parent, index);
        }

        public void NewImageNote(IdeaNote parent, int index)
        {
            if (openImageDialog.ShowDialog() == true)
                AddNote(new ImageNote() { FileName = openImageDialog.FileName }, parent, index);
        }

        private void LoadImageNote(string path, IdeaNote parent, int index)
        {
            ImageNote newNote = new ImageNote();
            newNote.FileName = path;
            AddNote(newNote, parent, index);
        }

        public TextNote NewTextNote(IdeaNote parent, int index)
        {
            TextNote newNote = new TextNote();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
                rtb.SelectAll();
                if (Settings.Default.UseDefaultFont && Settings.Default.DefaultFont != null)
                {
                    rtb.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, (FontFamily)Settings.Default.DefaultFont);
                    if (Settings.Default.DefaultFontSize > 0)
                        rtb.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, Settings.Default.DefaultFontSize);
                }
                else if (Settings.Default.MostRecentFont != null)
                {
                    rtb.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, (FontFamily)Settings.Default.MostRecentFont);
                    rtb.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, Settings.Default.MostRecentFontSize);
                }
                newNote.Rtf = rtb.Text;
            });
            AddNote(newNote, parent, index);
            return newNote;
        }

        private TextNote LoadTextNote(string path, IdeaNote parent, int index)
        {
            TextNote newNote = new TextNote();
            AddNote(newNote, parent, index);
            string text;
            try { text = File.ReadAllText(path); }
            catch (Exception) { return newNote; } // failures allowed to pass without mention; expected to be obvious without intrusive dialog warning
            if (Path.GetExtension(path).ToLowerInvariant() == ".rtf") newNote.Rtf = text;
            else newNote.SetRtfFromPlainText(text);
            return newNote;
        }

        public void NewFileNote(IdeaNote parent, int index) => AddNote(new FileNote(), parent, index);

        private void LoadFileNote(string path, IdeaNote parent, int index)
        {
            FileNote newNote = new FileNote();
            newNote.FileName = path;
            AddNote(newNote, parent, index);
        }

        public void NewTimelineNote(IdeaNote parent, int index) => AddNote(new TimelineNote(), parent, index);

        public void NewStoryNote(IdeaNote parent, int index) => AddNote(new StoryNote(), parent, index);

        public void NewCharacterNote(IdeaNote parent, int index) => AddNote(new CharacterNote(), parent, index);

        #endregion Adding Notes

        #region Moving Notes

        public void MoveNoteUp()
        {
            if (CurrentIdeaNote == null) return;
            if (CurrentIdeaNote.Parent == null)
            {
                int index = SaveFile.Ideas.IndexOf(CurrentIdeaNote);
                if (index > 0) SaveFile.Ideas.Move(index, index - 1);
            }
            else
            {
                int index = CurrentIdeaNote.Parent.Ideas.IndexOf(CurrentIdeaNote);
                if (index > 0) CurrentIdeaNote.Parent.Ideas.Move(index, index - 1);
            }
        }

        public void MakeNoteFirst()
        {
            if (CurrentIdeaNote == null) return;
            if (CurrentIdeaNote.Parent == null)
            {
                int index = SaveFile.Ideas.IndexOf(CurrentIdeaNote);
                if (index != -1) SaveFile.Ideas.Move(index, 0);
            }
            else
            {
                int index = CurrentIdeaNote.Parent.Ideas.IndexOf(CurrentIdeaNote);
                if (index != -1) CurrentIdeaNote.Parent.Ideas.Move(index, 0);
            }
        }

        public void MoveNoteDown()
        {
            if (CurrentIdeaNote == null) return;
            if (CurrentIdeaNote.Parent == null)
            {
                int index = SaveFile.Ideas.IndexOf(CurrentIdeaNote);
                if (index != -1 && index < SaveFile.Ideas.Count - 1)
                    SaveFile.Ideas.Move(index, index + 1);
            }
            else
            {
                int index = CurrentIdeaNote.Parent.Ideas.IndexOf(CurrentIdeaNote);
                if (index != -1 && index < CurrentIdeaNote.Parent.Ideas.Count - 1)
                    CurrentIdeaNote.Parent.Ideas.Move(index, index + 1);
            }
        }

        public void MakeNoteLast()
        {
            if (CurrentIdeaNote == null) return;
            if (CurrentIdeaNote.Parent == null)
            {
                int index = SaveFile.Ideas.IndexOf(CurrentIdeaNote);
                if (index != -1) SaveFile.Ideas.Move(index, SaveFile.Ideas.Count - 1);
            }
            else
            {
                int index = CurrentIdeaNote.Parent.Ideas.IndexOf(CurrentIdeaNote);
                if (index != -1) CurrentIdeaNote.Parent.Ideas.Move(index, CurrentIdeaNote.Parent.Ideas.Count - 1);
            }
        }

        public void PromoteNote(IdeaNote note)
        {
            if (note?.Parent != null)
            {
                note.Parent.Ideas.Remove(note);
                if (note.Parent.Parent != null)
                    note.Parent.Parent.InsertChild(note.Parent.Parent.Ideas.IndexOf(note.Parent) + 1, note);
                else SaveFile.InsertIdea(SaveFile.Ideas.IndexOf(note.Parent) + 1, note);
                CurrentIdeaNote = note;
                CurrentIdeaNote.ShowInTree();
            }
        }

        public void PromoteNote() => PromoteNote(CurrentIdeaNote);

        public void MakeNoteTop()
        {
            if (CurrentIdeaNote?.Parent != null)
            {
                IdeaNote original = CurrentIdeaNote;
                SaveFile.InsertIdea(SaveFile.Ideas.IndexOf(original.Parent.GetTopLevelNote()) + 1, original);
                CurrentIdeaNote = original;
            }
        }

        public void DemoteNote()
        {
            if (CurrentIdeaNote == null) return;
            IdeaNote original = CurrentIdeaNote;
            if (CurrentIdeaNote.Parent == null)
            {
                int index = SaveFile.Ideas.IndexOf(CurrentIdeaNote);
                if (index == 0)
                {
                    if (SaveFile.Ideas.Count > 1)
                        SaveFile.Ideas[1].AddChild(CurrentIdeaNote);
                }
                else SaveFile.Ideas[index - 1].AddChild(CurrentIdeaNote);
            }
            else
            {
                int index = CurrentIdeaNote.Parent.Ideas.IndexOf(CurrentIdeaNote);
                if (index == 0)
                {
                    if (CurrentIdeaNote.Parent.Ideas.Count > 1)
                        CurrentIdeaNote.Parent.Ideas[1].AddChild(CurrentIdeaNote);
                }
                else CurrentIdeaNote.Parent.Ideas[index - 1].AddChild(CurrentIdeaNote);
            }
            CurrentIdeaNote = original;
            CurrentIdeaNote.ShowInTree();
        }

        #endregion Moving Notes

        public void DeleteNote(bool branch, bool explicitSingle = false)
        {
            if (CurrentIdeaNote == null) return;

            UndoService.Current[SaveFile].BeginChangeSetBatch("Delete Note", false);

            IdeaNote target = CurrentIdeaNote;

            // Promote children if appropriate
            if (!branch && target.Ideas.Count > 0)
            {
                bool promoteChildren = explicitSingle;
                if (!explicitSingle)
                {
                    MessageBoxResult result = MessageBox.Show("Deleting this Note would also delete its child Notes.\n\nDo you want to promote the child Notes instead to preserve them?",
                        "Promote Children?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Cancel) return;
                    else if (result == MessageBoxResult.Yes) promoteChildren = true;
                }
                if (promoteChildren)
                {
                    List<IdeaNote> formerChildren = new List<IdeaNote>(target.Ideas.Reverse());
                    foreach (var child in formerChildren) PromoteNote(child);
                }
            }

            if (target.Parent != null)
            {
                int index = target.Parent.Ideas.IndexOf(target);
                IdeaNote parent = target.Parent;
                parent.Ideas.Remove(target);
                if (index > 0) parent.Ideas[index - 1].IsSelected = true;
            }
            else
            {
                int index = SaveFile.Ideas.IndexOf(target);
                SaveFile.Ideas.Remove(target);
                if (index > 0) SaveFile.Ideas[index - 1].IsSelected = true;
            }

            // Add default text note if there are none left.
            if (SaveFile.Ideas.Count == 0) NewTextNote(null, -1);

            UndoService.Current[SaveFile].EndChangeSetBatch();
        }

        private void ExpandOrCollapseNote(IdeaNote note, bool expand)
        {
            note.IsExpanded = expand;
            foreach (var child in note.Ideas) ExpandOrCollapseNote(child, expand);
        }

        public void ExpandNote() => ExpandOrCollapseNote(CurrentIdeaNote, true);

        public void ExpandAll()
        {
            foreach (IdeaNote note in SaveFile.Ideas) ExpandOrCollapseNote(note, true);
        }

        public void CollapseNote() => ExpandOrCollapseNote(CurrentIdeaNote, false);

        public void CollapseAll()
        {
            foreach (IdeaNote note in SaveFile.Ideas) ExpandOrCollapseNote(note, false);
        }

        #endregion Tree Operations

        public void RenameNote()
        {
            if (CurrentIdeaNote == null) return;
            string defaultAnswer = CurrentIdeaNote.ExplicitName;
            if (string.IsNullOrEmpty(defaultAnswer))
            {
                if (CurrentIdeaNote is FileNote)
                    defaultAnswer = Path.GetFileNameWithoutExtension(CurrentFileNote.FileName);
                else if (CurrentIdeaNote is CharacterNote)
                    defaultAnswer = CurrentCharacterNote.GetFullName();
            }
            string response = PromptDialog.Prompt("Enter a new name for this note:", "Rename Note", false, defaultAnswer);
            if (response != null) CurrentIdeaNote.ExplicitName = response; // allow setting to an empty string (clear the explicit name); vs. null, indicates cancel
        }

        #region Copying Names

        private void CopyNoteNames(IdeaNote note, StringBuilder sb, int indentLevel)
        {
            if (sb.Length > 0) sb.AppendLine();
            for (int i = 0; i < indentLevel; i++) sb.Append('\t');
            if (!CopyShowingGenres || note.Visibility == Visibility.Visible)
                sb.Append(note.Name);
            foreach (var child in note.Ideas) CopyNoteNames(child, sb, indentLevel + 1);
        }

        public void CopyNoteNames(bool children)
        {
            if (!children) Clipboard.SetText(CurrentIdeaNote.Name);

            StringBuilder sb = new StringBuilder();
            CopyNoteNames(CurrentIdeaNote, sb, 0);
            Clipboard.SetText(sb.ToString());
        }

        private void CopyNoteNames(ObservableCollection<IdeaNote> notes, bool children)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var note in notes)
            {
                if (CopyShowingGenres && note.Visibility != Visibility.Visible) continue;

                if (children) CopyNoteNames(note, sb, 0);
                else
                {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.Append(note.Name);
                }
            }
            Clipboard.SetText(sb.ToString());
        }

        public void CopyTopLevelNoteNames(bool children) => CopyNoteNames(SaveFile.Ideas, children);

        public void CopyChildNoteNames() => CopyNoteNames(CurrentIdeaNote.Ideas, false);

        #endregion Copying Names

        #region Tree Clipboard Operations

        public void CutNote(bool branch, bool explicitSingle = false)
        {
            if (CurrentIdeaNote == null) return;

            IdeaNote copy = (branch ? CurrentIdeaNote.GetCopy() : CurrentIdeaNote.GetChildlessCopy());
            Clipboard.SetData("IdeaNote", copy.GetData());

            DeleteNote(branch, explicitSingle);
        }

        public void CopyNote(bool branch)
        {
            if (CurrentIdeaNote == null) return;

            IdeaNote copy = (branch ? CurrentIdeaNote.GetCopy() : CurrentIdeaNote.GetChildlessCopy());
            Clipboard.SetData("IdeaNote", copy.GetData());
        }

        public void PasteInTree(IdeaNote parent, int index)
        {
            // Prefer pasting a note oveer anything else
            if (Clipboard.ContainsData("IdeaNote"))
            {
                if (ImportNote((byte[])Clipboard.GetData("IdeaNote"), parent, index)) return;
            }

            // If the clipboard has file drops, add them as notes
            if (Clipboard.ContainsFileDropList())
            {
                StringCollection fileDrops = Clipboard.GetFileDropList();
                if (fileDrops?.Count > 0)
                {
                    LoadFiles(fileDrops.Cast<string>().ToList(), false, parent, index);
                    return;
                }
            }

            // If the clipboard has text, add it as a new text note
            if (Clipboard.ContainsText(TextDataFormat.Rtf))
            {
                TextNote newNote = new TextNote();
                AddNote(newNote, parent, index);
                newNote.Rtf = Clipboard.GetText(TextDataFormat.Rtf);
                return;
            }
            if (Clipboard.ContainsText())
            {
                TextNote newNote = new TextNote();
                AddNote(newNote, parent, index);
                newNote.SetRtfFromPlainText(Clipboard.GetText());
                return;
            }
        }

        #endregion Tree Clipboard Operations

        public void ConvertToTextNote()
        {
            if (CurrentIdeaNote == null) return;

            UndoService.Current[SaveFile].BeginChangeSetBatch("Convert to TextNote", false);

            TextNote note = CurrentIdeaNote.ConvertToTextNote();

            Xceed.Wpf.Toolkit.RichTextBox rtb = new Xceed.Wpf.Toolkit.RichTextBox();
            rtb.Text = note.Rtf;
            rtb.SelectAll();
            if (Settings.Default.UseDefaultFont)
                rtb.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, (FontFamily)Settings.Default.DefaultFont);
            else rtb.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, (FontFamily)Settings.Default.MostRecentFont);
            note.Rtf = rtb.Text;

            ReplaceNote(CurrentIdeaNote, note);
            CurrentIdeaNote = note;

            UndoService.Current[SaveFile].EndChangeSetBatch();
        }

        #region Searching

        /// <summary>
        /// Finds the searchText in the tree, and selects the containing Note, if found.
        /// </summary>
        /// <param name="searchType">0 for Find, 1 for FindNext, -1 for FindPrev</param>
        /// <returns>True if found; false otherwise</returns>
        public bool SearchInTree(IdeaNote startingNote, int searchType, bool firstTry)
        {
            if (startingNote == null)
            {
                if (SaveFile?.Ideas.Count > 0) startingNote = SaveFile.Ideas[0];
                else return false;
            }

            // FindNext skips the starting Note, assuming that the user wants the NEXT matching note, not the current one.
            if (searchType == 0 && startingNote.ContainsText(searchText)) return true;

            IdeaNote nextNote = (searchType == -1 ? startingNote.GetPrevNoteInTree() : startingNote.GetNextNoteInTree());
            while (nextNote != null)
            {
                if (nextNote.ContainsText(searchText))
                {
                    CurrentIdeaNote = nextNote;
                    return true;
                }
                nextNote = (searchType == -1 ? nextNote.GetPrevNoteInTree() : nextNote.GetNextNoteInTree());
            }

            // reaching this point means there are no more notes in the tree
            if (searchType == 0 && firstTry && startingNote != SaveFile.Ideas[0])
                return SearchInTree(SaveFile.Ideas[0], searchType, false);
            else return false;
        }

        public bool FindText(string text, int searchType)
        {
            searchText = text;
            return SearchInTree(CurrentIdeaNote, searchType, true);
        }

        #endregion Searching

        #region Character Notes

        public void AddFamily(bool replace, bool relativesOnly)
        {
            if (CurrentIdeaNote?.IdeaNoteType != nameof(CharacterNote)) return;

            UndoService.Current[SaveFile].BeginChangeSetBatch("Change Family", false);

            if (replace) ClearFamily();
            CurrentCharacterNote.AddFamily();
            if (!relativesOnly) AddNonFamilyAssociates();
            else CurrentCharacterNote.IsExpanded = true;

            UndoService.Current[SaveFile].EndChangeSetBatch();
        }

        public void AddFamilyMember(CharacterRelationshipOption relationship)
        {
            if (CurrentIdeaNote?.IdeaNoteType != nameof(CharacterNote) || relationship == null) return;
            
            CharacterNote newNote = CurrentCharacterNote.AddNewFamilyMember(relationship, true);
            CurrentCharacterNote.IsExpanded = true;
            newNote.IsSelected = true;
        }

        public void GenerateChildren()
        {
            if (CurrentIdeaNote?.IdeaNoteType != nameof(CharacterNote)) return;

            UndoService.Current[SaveFile].BeginChangeSetBatch("Add Children", false);

            CurrentCharacterNote.GenerateChildren(true);
            CurrentCharacterNote.IsExpanded = true;

            UndoService.Current[SaveFile].EndChangeSetBatch();
        }

        public void GenerateSiblings()
        {
            if (CurrentIdeaNote?.IdeaNoteType != nameof(CharacterNote)) return;

            UndoService.Current[SaveFile].BeginChangeSetBatch("Add Siblings", false);

            CurrentCharacterNote.GenerateSiblings(true);
            CurrentCharacterNote.IsExpanded = true;

            UndoService.Current[SaveFile].EndChangeSetBatch();
        }

        public void AddNonFamilyAssociates()
        {
            if (CurrentIdeaNote?.IdeaNoteType != nameof(CharacterNote)) return;

            UndoService.Current[SaveFile].BeginChangeSetBatch("Add Associates", false);

            CurrentCharacterNote.AddNonFamilyAssociates();
            CurrentCharacterNote.IsExpanded = true;

            UndoService.Current[SaveFile].EndChangeSetBatch();
        }

        public void ClearFamily()
        {
            if (CurrentIdeaNote == null) return;

            UndoService.Current[SaveFile].BeginChangeSetBatch("Clear Family", false);

            // Remove only related Character notes.
            List<IdeaNote> notesToRemove = CurrentIdeaNote.Ideas.Where(c =>
                c.IdeaNoteType == nameof(CharacterNote) && ((CharacterNote)c).Relationship != null).ToList();
            foreach (var note in notesToRemove) CurrentIdeaNote.Ideas.Remove(note);

            UndoService.Current[SaveFile].EndChangeSetBatch();
        }

        private void AddOrChangeRelationship(CharacterNote note)
        {
            RelationshipDialog relationshipDialog = new RelationshipDialog(typeof(CharacterRelationshipOption));
            relationshipDialog.ItemsSource = Template.CharacterTemplate.Relationships.ChildOptions;
            relationshipDialog.IsReadOnly = true;
            relationshipDialog.CollectionControl.PropertyGrid.AutoGenerateProperties = false;
            relationshipDialog.CollectionControl.PropertyGrid.PropertyDefinitions = new Xceed.Wpf.Toolkit.PropertyGrid.PropertyDefinitionCollection()
            {
                new Xceed.Wpf.Toolkit.PropertyGrid.PropertyDefinition() { TargetProperties = new List<string>() { nameof(NoteOption.Name) } },
                new Xceed.Wpf.Toolkit.PropertyGrid.PropertyDefinition() { TargetProperties = new List<string>() { nameof(NoteOption.IsChoice) } }
            };
            if (relationshipDialog.ShowDialog() == true)
            {
                if (relationshipDialog.UseCustom && !string.IsNullOrWhiteSpace(relationshipDialog.CustomRelationshipName))
                    note.Relationship = new CharacterRelationshipOption(relationshipDialog.CustomRelationshipName);
                else note.Relationship = relationshipDialog.CollectionControl.SelectedItem as CharacterRelationshipOption;
            }
        }

        public void AddOrChangeRelationship()
        {
            if (CurrentIdeaNote?.IdeaNoteType != nameof(CharacterNote) ||
                CurrentCharacterNote.Parent.IdeaNoteType != nameof(CharacterNote)) return;

            AddOrChangeRelationship(CurrentCharacterNote);
        }

        public void RemoveRelationship()
        {
            if (CurrentIdeaNote?.IdeaNoteType != nameof(CharacterNote)) return;

            CurrentCharacterNote.Relationship = null;
        }

        public void EditRelationships()
        {
            Xceed.Wpf.Toolkit.CollectionControlDialog relationshipDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(CharacterRelationshipOption));
            relationshipDialog.ItemsSource = Template.CharacterTemplate.Relationships.ChildOptions;
            relationshipDialog.NewItemTypes = new List<Type>() { typeof(CharacterRelationshipOption) };
            relationshipDialog.ShowDialog();
            Template.Save();
        }

        #endregion Character Notes

        #endregion Notes

        public void ClearGenreFilter()
        {
            foreach (var note in SaveFile.Ideas) note.MakeVisible();
        }

        public void ApplyGenreFilter()
        {
            foreach (var note in SaveFile.Ideas) note.ShowOnlyMatchingGenres(Genres, PossibleGenres, ExcludedGenres, ShowNoGenres);
        }

        private void AddCustomGenres(ObservableCollection<IdeaNote> ideaCollection)
        {
            foreach (StoryNote idea in ideaCollection.Where(i => i.IdeaNoteType == nameof(StoryNote)))
            {
                foreach (NoteOption genre in idea.Genres.ChildOptions.Where(c => !Genres.Any(g => g.Name == c.Name)))
                {
                    Genres.Add(genre.Clone() as NoteOption);
                    PossibleGenres.Add(genre.Clone() as NoteOption);
                    ExcludedGenres.Add(genre.Clone() as NoteOption);
                }
                AddCustomGenres(idea.Ideas);
            }
        }

        public void InitializeGenres()
        {
            Genres = new ObservableCollection<NoteOption>(Template.StoryTemplate.Genres.ChildOptions.Select(c => c.Clone() as NoteOption));
            PossibleGenres = new ObservableCollection<NoteOption>(Template.StoryTemplate.Genres.ChildOptions.Select(c => c.Clone() as NoteOption));
            ExcludedGenres = new ObservableCollection<NoteOption>(Template.StoryTemplate.Genres.ChildOptions.Select(c => c.Clone() as NoteOption));
            if (SaveFile != null) AddCustomGenres(SaveFile.Ideas);
            foreach (var genre in Genres) genre.IsChecked = false;
            foreach (var genre in PossibleGenres) genre.IsChecked = false;
            foreach (var genre in ExcludedGenres) genre.IsChecked = false;
            ApplyGenreFilter();
            CopyShowingGenres = true;
        }
    }
}
