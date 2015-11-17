using IdeaTree2.Properties;
using MonitoredUndo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private string searchText;
        private StringComparison searchStringComparison;

        #region Commands

        #region File Operations

        public static readonly RoutedCommand Revert = new RoutedCommand();
        public static readonly RoutedCommand NewTemplate = new RoutedCommand();
        public static readonly RoutedCommand LoadTemplate = new RoutedCommand();
        public static readonly RoutedCommand DefaultTemplate = new RoutedCommand();
        public static readonly RoutedCommand SetPassword = new RoutedCommand();
        public static readonly RoutedCommand ExportNote = new RoutedCommand();
        public static readonly RoutedCommand ExportBranch = new RoutedCommand();
        public static readonly RoutedCommand ExportNoteAsRtf = new RoutedCommand();
        public static readonly RoutedCommand ExportNoteAsTxt = new RoutedCommand();
        public static readonly RoutedCommand ImportAsSibling = new RoutedCommand();
        public static readonly RoutedCommand ImportAsChild = new RoutedCommand();

        #endregion File Operations

        #region Notes

        #region Tree Operations

        #region New Notes

        public static readonly RoutedCommand NewChildText = new RoutedCommand();
        public static readonly RoutedCommand NewChildImage = new RoutedCommand();
        public static readonly RoutedCommand NewChildMedia = new RoutedCommand();
        public static readonly RoutedCommand NewChildStory = new RoutedCommand();
        public static readonly RoutedCommand NewChildCharacter = new RoutedCommand();
        public static readonly RoutedCommand NewChildTimeline = new RoutedCommand();
        public static readonly RoutedCommand NewChildFile = new RoutedCommand();

        public static readonly RoutedCommand NewSiblingText = new RoutedCommand();
        public static readonly RoutedCommand NewSiblingImage = new RoutedCommand();
        public static readonly RoutedCommand NewSiblingMedia = new RoutedCommand();
        public static readonly RoutedCommand NewSiblingStory = new RoutedCommand();
        public static readonly RoutedCommand NewSiblingCharacter = new RoutedCommand();
        public static readonly RoutedCommand NewSiblingTimeline = new RoutedCommand();
        public static readonly RoutedCommand NewSiblingFile = new RoutedCommand();

        #endregion New Notes

        #region Moving Notes

        public static readonly RoutedCommand MoveNoteUp = new RoutedCommand();
        public static readonly RoutedCommand MakeNoteFirst = new RoutedCommand();
        public static readonly RoutedCommand MoveNoteDown = new RoutedCommand();
        public static readonly RoutedCommand MakeNoteLast = new RoutedCommand();
        public static readonly RoutedCommand PromoteNote = new RoutedCommand();
        public static readonly RoutedCommand MakeNoteTop = new RoutedCommand();
        public static readonly RoutedCommand DemoteNote = new RoutedCommand();

        #endregion Moving Notes

        public static readonly RoutedCommand DeleteSingleNote = new RoutedCommand();
        public static readonly RoutedCommand DeleteNoteBranch = new RoutedCommand();

        public static readonly RoutedCommand ExpandAll = new RoutedCommand();
        public static readonly RoutedCommand ExpandNote = new RoutedCommand();
        public static readonly RoutedCommand CollapseAll = new RoutedCommand();
        public static readonly RoutedCommand CollapseNote = new RoutedCommand();

        #endregion Tree Operations

        public static readonly RoutedCommand RenameNote = new RoutedCommand();

        #region Copying Names

        public static readonly RoutedCommand CopyTopLevelNoteNames = new RoutedCommand();
        public static readonly RoutedCommand CopyTreeNoteNames = new RoutedCommand();
        public static readonly RoutedCommand CopyNoteName = new RoutedCommand();
        public static readonly RoutedCommand CopyNoteChildrenNames = new RoutedCommand();
        public static readonly RoutedCommand CopyBranchNames = new RoutedCommand();

        #endregion Copying Names

        public static readonly RoutedCommand CutSingleNote = new RoutedCommand();
        public static readonly RoutedCommand CopySingleNote = new RoutedCommand();
        public static readonly RoutedCommand PasteInTreeAsChild = new RoutedCommand();

        public static readonly RoutedCommand ConvertToTextNote = new RoutedCommand();

        public static readonly RoutedCommand FindInTree = new RoutedCommand();
        public static readonly RoutedCommand FindNextInTree = new RoutedCommand();
        public static readonly RoutedCommand FindPrevInTree = new RoutedCommand();

        #region Character Notes

        public static readonly RoutedCommand NewFamily = new RoutedCommand();
        public static readonly RoutedCommand AddFamilyOnly = new RoutedCommand();
        public static readonly RoutedCommand AddFamilyAndAssociates = new RoutedCommand();
        public static readonly RoutedCommand AddFamilyMember = new RoutedCommand();
        public static readonly RoutedCommand GenerateChildren = new RoutedCommand();
        public static readonly RoutedCommand GenerateSiblings = new RoutedCommand();
        public static readonly RoutedCommand AddNonFamilyAssociates = new RoutedCommand();
        public static readonly RoutedCommand ClearFamily = new RoutedCommand();
        public static readonly RoutedCommand AddOrChangeRelationship = new RoutedCommand();
        public static readonly RoutedCommand RemoveRelationship = new RoutedCommand();
        public static readonly RoutedCommand EditRelationships = new RoutedCommand();

        #endregion Character Notes

        public static readonly RoutedCommand GenreCheck = new RoutedCommand();

        #endregion Notes

        #region Rich Text

        public static readonly RoutedCommand FindNext = new RoutedCommand();
        public static readonly RoutedCommand FindPrev = new RoutedCommand();

        public static readonly RoutedCommand ConvertCase = new RoutedCommand();
        public static readonly RoutedCommand InsertEnDash = new RoutedCommand();
        public static readonly RoutedCommand InsertEmDash = new RoutedCommand();
        public static readonly RoutedCommand InsertEllipsis = new RoutedCommand();

        public static readonly RoutedCommand PastePlainText = new RoutedCommand();
        public static readonly RoutedCommand PasteLink = new RoutedCommand();

        #endregion Rich Text

        #endregion Commands

        public MainWindow() { InitializeComponent(); }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            richTextBox_Main.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(RequestNavigateHandler));

            if (Settings.Default.Maximized) WindowState = WindowState.Maximized;
            if (Settings.Default.MostRecentFont != null)
                comboBox_FontFamily.SelectedItem = Settings.Default.MostRecentFont;

            noteTreeView.Focus();
        }

        private void RequestNavigateHandler(object sender, RequestNavigateEventArgs e) => Process.Start(e.Uri.ToString());

        private void CloseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (((MainViewModel)DataContext).CheckClosing()) Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.Save();
            if (!((MainViewModel)DataContext).CheckClosing()) e.Cancel = true;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            Settings.Default.Maximized = (WindowState == WindowState.Maximized);
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
                e.Effects = DragDropEffects.Copy;
            else e.Effects = DragDropEffects.None;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ((MainViewModel)DataContext).LoadFiles(files.Where(f => File.Exists(f)).ToList(), false, ((MainViewModel)DataContext).CurrentIdeaNote, -1);
        }

        private void noteTreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject item = e.OriginalSource as DependencyObject;
            while (item != null && !(item is TreeViewItem))
            {
                DependencyObject temp = null;
                if (item is Visual) temp = VisualTreeHelper.GetParent(item);

                if (temp == null) item = LogicalTreeHelper.GetParent(item);
                else item = temp;
            }

            TreeViewItem treeViewItem = item as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        #region RichText

        private void UpdateCaretPosition()
        {
            TextPointer caretLineStart = richTextBox_Main.CaretPosition.GetLineStartPosition(0);
            TextPointer p = richTextBox_Main.Document.ContentStart.GetLineStartPosition(0);

            int columnNumber = Math.Max(richTextBox_Main.CaretPosition.GetLineStartPosition(0).GetOffsetToPosition(richTextBox_Main.CaretPosition) - 1, 0);

            int currentLineNumber = 1;
            while (true)
            {
                if (caretLineStart.CompareTo(p) < 0) break;

                int result;
                p = p.GetLineStartPosition(1, out result);
                if (result == 0) break;

                currentLineNumber++;
            }
            
            statusBarItem_Position.Content = $"{currentLineNumber}:{columnNumber}";
        }

        private void UpdateFontToggleButtonState(RibbonToggleButton button, DependencyProperty formatProp, object targetValue)
        {
            object currentValue = richTextBox_Main.Selection.GetPropertyValue(formatProp);
            button.IsChecked = (currentValue == DependencyProperty.UnsetValue) ? false : currentValue != null && currentValue.Equals(targetValue);
        }

        private void UpdateFontUIStates()
        {
            UpdateFontToggleButtonState(toggleButton_Bold, TextElement.FontWeightProperty, FontWeights.Bold);
            UpdateFontToggleButtonState(toggleButton_Italic, TextElement.FontStyleProperty, FontStyles.Italic);
            UpdateFontToggleButtonState(toggleButton_Underline, Inline.TextDecorationsProperty, TextDecorations.Underline);
            UpdateFontToggleButtonState(toggleButton_Superscript, Typography.VariantsProperty, FontVariants.Superscript);
            UpdateFontToggleButtonState(toggleButton_Subscript, Typography.VariantsProperty, FontVariants.Subscript);
            UpdateFontToggleButtonState(toggleButton_alignLeft, Paragraph.TextAlignmentProperty, TextAlignment.Left);
            UpdateFontToggleButtonState(toggleButton_alignCenter, Paragraph.TextAlignmentProperty, TextAlignment.Center);
            UpdateFontToggleButtonState(toggleButton_alignRight, Paragraph.TextAlignmentProperty, TextAlignment.Right);
            UpdateFontToggleButtonState(toggleButton_alignJustify, Paragraph.TextAlignmentProperty, TextAlignment.Justify);

            // bullets or numbered lists
            Paragraph startParagraph = richTextBox_Main.Selection.Start.Paragraph;
            Paragraph endParagraph = richTextBox_Main.Selection.End.Paragraph;
            if (startParagraph?.Parent is ListItem && endParagraph?.Parent is ListItem &&
                ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
            {
                TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
                if (markerStyle == TextMarkerStyle.Disc) toggleButton_Bullets.IsChecked = true;
                else if (markerStyle == TextMarkerStyle.Decimal) toggleButton_Numbering.IsChecked = true;
            }
            else
            {
                toggleButton_Bullets.IsChecked = false;
                toggleButton_Numbering.IsChecked = false;
            }

            // font family
            object currentTextFont = richTextBox_Main.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamily fontFamily = (FontFamily)((currentTextFont == DependencyProperty.UnsetValue) ? null : currentTextFont);
            if (fontFamily != null) comboBox_FontFamily.SelectedItem = fontFamily;

            // font size
            object currentTextSize = richTextBox_Main.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (currentTextSize == DependencyProperty.UnsetValue) comboBox_FontSize.SelectedValue = null;
            else comboBox_FontSize.SelectedItem = (int)((double)currentTextSize);

            // color
            object currentColor = richTextBox_Main.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            SolidColorBrush fontColor = (SolidColorBrush)((currentColor == DependencyProperty.UnsetValue) ? null : currentColor);
            if (fontColor != null) colorPicker_textColor.SelectedColor = fontColor.Color;
        }

        private void UpdateText()
        {
            UpdateCaretPosition();
            UpdateFontUIStates();
        }

        private void richTextBox_Main_TextChanged(object sender, TextChangedEventArgs e) => UpdateText();

        private void richTextBox_Main_SelectionChanged(object sender, RoutedEventArgs e) => UpdateText();

        private void comboBox_FontFamily_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FontFamily chosenFont = (FontFamily)e.NewValue;
            if (chosenFont != null)
            {
                richTextBox_Main?.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, chosenFont);
                richTextBox_Main?.Focus();
            }
        }

        private void comboBox_FontSize_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            richTextBox_Main?.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (double)((int)e.NewValue));
            richTextBox_Main?.Focus();
        }

        private void colorPicker_textColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            richTextBox_Main?.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(e.NewValue ?? Colors.Black));
            richTextBox_Main?.Focus();
        }

        private void ToggleBaselineAlignment(BaselineAlignment alignment)
        {
            object currentValue = richTextBox_Main.Selection.GetPropertyValue(Inline.BaselineAlignmentProperty);
            object currentTextSize = richTextBox_Main.Selection.GetPropertyValue(TextElement.FontSizeProperty);

            BaselineAlignment newAlignment = alignment;
            double fontSizeMultiplier = 0.75d;
            if ((BaselineAlignment)currentValue == alignment)
            {
                newAlignment = BaselineAlignment.Baseline;
                fontSizeMultiplier = 1 / 0.75d;
            }

            richTextBox_Main?.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, newAlignment);
            if (currentTextSize != DependencyProperty.UnsetValue)
                richTextBox_Main?.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, (double)currentTextSize * fontSizeMultiplier);
        }

        private void richTextBox_Main_ToggleSuperscript(object sender, ExecutedRoutedEventArgs e)
            => ToggleBaselineAlignment(BaselineAlignment.Superscript);

        private void richTextBox_Main_ToggleSubscript(object sender, ExecutedRoutedEventArgs e)
            => ToggleBaselineAlignment(BaselineAlignment.Subscript);

        #region Searching

        private TextRange FindTextInRange(TextRange targetRange, bool findPrev)
        {
            int offset = (findPrev ? targetRange.Text.LastIndexOf(searchText, searchStringComparison) : targetRange.Text.IndexOf(searchText, searchStringComparison));
            if (offset < 0) return null;

            for (TextPointer start = targetRange.Start.GetPositionAtOffset(offset); start != targetRange.End; start = start.GetPositionAtOffset(1))
            {
                TextRange result = new TextRange(start, start.GetPositionAtOffset(searchText.Length));
                if (result.Text.Equals(searchText, searchStringComparison)) return result;
            }

            return null;
        }

        /// <summary>
        /// Finds the searchText in richTextBox_Main, and selects it if found.
        /// </summary>
        /// <param name="searchType">0 for Find, 1 for FindNext, -1 for FindPrev</param>
        /// <returns>True if found; false otherwise</returns>
        private bool SearchText(int searchType, bool firstTry)
        {
            TextRange targetRange;

            switch (searchType)
            {
                case -1: // find prev
                    targetRange = new TextRange(richTextBox_Main.Document.ContentStart,
                        richTextBox_Main.Selection.Start.GetPositionAtOffset(0));
                    break;
                case 1: // find next
                    targetRange = new TextRange(richTextBox_Main.Selection.End.GetPositionAtOffset(1),
                        richTextBox_Main.Document.ContentEnd);
                    break;
                default: // find (start at caret on 1st pass; start from beginning and end at caret on 2nd)
                    if (firstTry)
                        targetRange = new TextRange(richTextBox_Main.Selection.End.GetPositionAtOffset(1),
                            richTextBox_Main.Document.ContentEnd);
                    else targetRange = new TextRange(richTextBox_Main.Document.ContentStart,
                        richTextBox_Main.Selection.End.GetPositionAtOffset(0));
                    break;
            }

            TextRange foundRange = FindTextInRange(targetRange, (searchType == -1));
            if (foundRange == null)
            {
                if (firstTry && searchType == 0) return SearchText(0, false);
                else return false;
            }

            richTextBox_Main.Selection.Select(foundRange.Start, foundRange.End);
            richTextBox_Main.Focus();
            return true;
        }

        public void FindText(string text, int searchType, bool matchCase)
        {
            searchText = text;
            searchStringComparison = (matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
            if (!SearchText(searchType, true))
            {
                string failMessage = null;
                switch (searchType)
                {
                    case 1: failMessage = "End of the document reached"; break;
                    case -1: failMessage = "Beginning of the document reached"; break;
                    default: failMessage = "Search text not found."; break;
                }
                MessageBox.Show(failMessage, "No Matches", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else Application.Current.MainWindow.Activate();
        }

        public void ReplaceText(string searchText, string replaceText, bool matchCase, bool all)
        {
            this.searchText = searchText;
            searchStringComparison = (matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
            int matches = 0;
            if (all)
            {
                while (SearchText(0, true))
                {
                    matches++;
                    richTextBox_Main.Selection.Text = replaceText;
                }
            }
            else if (SearchText(0, true))
            {
                matches++;
                richTextBox_Main.Selection.Text = replaceText;
            }
            if (matches == 0) MessageBox.Show("Search text not found.\n\nNo replacements made.",
                "No Matches", MessageBoxButton.OK, MessageBoxImage.Information);
            else MessageBox.Show($"{matches} replacement{(matches > 1 ? "s" : string.Empty)} made.",
                $"{matches} Matches", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void richTextBox_Main_FindCommand(object sender, ExecutedRoutedEventArgs e)
        {
            FindWindow findWindow = new FindWindow(false);
            findWindow.Owner = this;
            findWindow.Show();
        }

        private void richTextBox_Main_FindNextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SearchText(1, true);
        }

        private void richTextBox_Main_FindPrevCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SearchText(-1, true);
        }

        #endregion Searching

        private static Func<string, string> GetCaseChange(TextInfo info, string text)
        {
            if (info.ToLower(text) == text) return (txt) => info.ToTitleCase(txt);
            else if (info.ToUpper(text) == text) return (txt) => info.ToLower(txt);
            else return (txt) => info.ToUpper(txt);
        }

        private void richTextBox_Main_ConvertCase(object sender, ExecutedRoutedEventArgs e)
        {
            List<TextRange> ranges = new List<TextRange>();

            var prev = richTextBox_Main?.Selection.Start;
            if (prev == null) return;

            for (var pointer = prev; pointer.CompareTo(richTextBox_Main?.Selection.End) <= 0; pointer = pointer.GetPositionAtOffset(1, LogicalDirection.Forward))
            {
                var contextAfter = pointer.GetPointerContext(LogicalDirection.Forward);
                var contextBefore = pointer.GetPointerContext(LogicalDirection.Backward);
                if (contextBefore != TextPointerContext.Text && contextAfter == TextPointerContext.Text)
                    prev = pointer;
                if (contextBefore == TextPointerContext.Text && contextAfter != TextPointerContext.Text && prev != pointer)
                {
                    ranges.Add(new TextRange(prev, pointer));
                    prev = null;
                }
            }
            ranges.Add(new TextRange(prev ?? richTextBox_Main?.Selection.End, richTextBox_Main?.Selection.End));

            var info = (CultureInfo.CurrentUICulture).TextInfo;
            var text = string.Join(" ", ranges.Select(r => r.Text).Where(r => !string.IsNullOrWhiteSpace(r)));
            Func<string, string> changeCase = GetCaseChange(info, text);

            foreach (var range in ranges)
            {
                if (!range.IsEmpty && !string.IsNullOrWhiteSpace(range.Text))
                    range.Text = changeCase(range.Text);
            }
        }

        private void InsertText(string text)
        {
            if (!string.IsNullOrEmpty(richTextBox_Main.Selection.Text))
                richTextBox_Main.Selection.Text = string.Empty;
            richTextBox_Main.CaretPosition.GetInsertionPosition(richTextBox_Main.CaretPosition.LogicalDirection).InsertTextInRun(text);
            if (richTextBox_Main.CaretPosition.LogicalDirection == LogicalDirection.Backward)
                richTextBox_Main.CaretPosition = richTextBox_Main.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
        }

        private void richTextBox_Main_InsertEnDash(object sender, ExecutedRoutedEventArgs e) => InsertText("–");

        private void richTextBox_Main_InsertEmDash(object sender, ExecutedRoutedEventArgs e) => InsertText("—");

        private void richTextBox_Main_InsertEllipsis(object sender, ExecutedRoutedEventArgs e) => InsertText("…");

        private void PastePlainTextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            RichTextBox rtb = new RichTextBox();
            rtb.Paste();
            rtb.SelectAll();
            rtb.Selection.ClearAllProperties();
            rtb.Copy();
            richTextBox_Main.Paste();
        }

        private void PasteLinkCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Clipboard.ContainsText()) return;
            string text = Clipboard.GetText();
            if (File.Exists(text) || Directory.Exists(text)) // Format file links correctly.
                text = FileNote.GetPathAsLink(text);
            richTextBox_Main.Selection.Text = string.Empty; // Clear any selection.
            Hyperlink link = new Hyperlink(new Run(text),
                richTextBox_Main.CaretPosition.GetInsertionPosition(richTextBox_Main.CaretPosition.LogicalDirection));
            link.NavigateUri = new Uri(text);
            if (richTextBox_Main.CaretPosition.LogicalDirection == LogicalDirection.Backward)
                richTextBox_Main.CaretPosition = richTextBox_Main.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
        }

        #endregion RichText

        #region File Operations

        private void NewCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).NewFile();

        private void RunBusyWork(DoWorkEventHandler work)
        {
            busyIndicator_main.IsBusy = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += work;
            worker.RunWorkerCompleted += (o, ea) =>
            {
                busyIndicator_main.IsBusy = false;
                Application.Current.MainWindow.Activate();
            };
            worker.RunWorkerAsync();
        }

        private void OpenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel vm = ((MainViewModel)DataContext);
            RunBusyWork((o, ea) => { vm.Open(true, null, -1); });
        }

        private void SaveCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel vm = ((MainViewModel)DataContext);
            RunBusyWork((o, ea) => { vm.Save(); });
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((MainViewModel)DataContext).SaveFile?.ChangedSinceLastSave == true;
        }

        private void SaveAsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel vm = ((MainViewModel)DataContext);
            RunBusyWork((o, ea) => { vm.SaveAs(); });
        }

        private void RevertCommand(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel vm = ((MainViewModel)DataContext);
            RunBusyWork((o, ea) => { vm.Revert(); });
        }

        private void NewTemplateCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).NewTemplate();

        private void LoadTemplateCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).LoadTemplate();

        private void DefaultTemplateCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).UseDefaultTemplate();

        private void SetPasswordCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).SetPassword();

        private void ExportNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).ExportNote(false);

        private void ExportBranchCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).ExportNote(true);

        private void ExportNoteAsRtfCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).ExportNoteAsRtf();

        private void ExportNoteAsTxtCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).ExportNoteAsTxt();

        #endregion File Operations

        private void UndoCommand(object sender, ExecutedRoutedEventArgs e) => UndoService.Current[(MainViewModel)DataContext].Undo();

        private void CanUndo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UndoService.Current[(MainViewModel)DataContext].CanUndo;
        }

        private void RedoCommand(object sender, ExecutedRoutedEventArgs e) => UndoService.Current[(MainViewModel)DataContext].Redo();

        private void CanRedo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = UndoService.Current[(MainViewModel)DataContext].CanRedo;
        }

        #region Notes

        private void noteTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((MainViewModel)DataContext).CurrentIdeaNote = e.NewValue as IdeaNote;
        }

        private void CanExecuteNoteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (((MainViewModel)DataContext).CurrentIdeaNote != null);
        }

        private void CanExecuteTextNoteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (((MainViewModel)DataContext).CurrentIdeaNote != null &&
                ((MainViewModel)DataContext).CurrentIdeaNote.IdeaNoteType == nameof(TextNote));
        }

        #region Tree Operations

        #region Adding Notes

        private void ImportAsSiblingCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).Open(false, ((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        private void ImportAsChildCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).Open(false, ((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void NewChildTextCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewTextNote(((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void NewChildImageCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewImageNote(((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void NewChildMediaCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewMediaNote(((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void NewChildStoryCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewStoryNote(((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void NewChildCharacterCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewCharacterNote(((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void NewChildTimelineCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewTimelineNote(((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void NewChildFileCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewFileNote(((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void NewSiblingTextCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewTextNote(((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        private void NewSiblingImageCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewImageNote(((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        private void NewSiblingMediaCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewMediaNote(((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        private void NewSiblingStoryCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewStoryNote(((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        private void NewSiblingCharacterCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewCharacterNote(((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        private void NewSiblingTimelineCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewTimelineNote(((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        private void NewSiblingFileCommand(object sender, ExecutedRoutedEventArgs e)
            => ((MainViewModel)DataContext).NewFileNote(((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        #endregion Adding Notes

        #region Moving Notes

        private void MoveNoteUpCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).MoveNoteUp();

        private void MakeNoteFirstCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).MakeNoteFirst();

        private void MoveNoteDownCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).MoveNoteDown();

        private void MakeNoteLastCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).MakeNoteLast();

        private void PromoteNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).PromoteNote();

        private void MakeNoteTopCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).MakeNoteTop();

        private void DemoteNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).DemoteNote();

        #endregion Moving Notes

        private void DeleteNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).DeleteNote(false);

        private void DeleteSingleNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).DeleteNote(false, true);

        private void DeleteNoteBranchCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).DeleteNote(true);

        private void ExpandAllCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).ExpandAll();

        private void ExpandNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).ExpandNote();

        private void CollapseAllCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CollapseAll();

        private void CollapseNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CollapseNote();

        #endregion Tree Operations

        private void RenameNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).RenameNote();

        #region Copying Names

        private void CopyTopLevelNoteNamesCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CopyTopLevelNoteNames(false);

        private void CopyTreeNoteNamesCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CopyTopLevelNoteNames(true);

        private void CopyNoteNameCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CopyNoteNames(false);

        private void CopyNoteChildrenNamesCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CopyChildNoteNames();

        private void CopyBranchNamesCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CopyNoteNames(true);

        #endregion Copying Names

        #region Tree Clipboard Operations

        private void CutSingleNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CutNote(false, true);

        private void CutBranchCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CutNote(true);

        private void CopySingleNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CopyNote(false);

        private void CopyBranchCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).CopyNote(true);

        private void PasteInTreeAsChildCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).PasteInTree(((MainViewModel)DataContext).CurrentIdeaNote, -1);

        private void PasteInTreeAsSiblingCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).PasteInTree(((MainViewModel)DataContext).CurrentIdeaNote?.Parent,
                ((MainViewModel)DataContext).CurrentIdeaNote?.GetNewSiblingIndex() ?? -1);

        #endregion Tree Clipboard Operations

        private void ConvertToTextNoteCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).ConvertToTextNote();

        #region Searching

        private TreeViewItem GetTreeViewItemFromIdeaNote(ItemsControl items, Stack<IdeaNote> stack)
        {
            if (items == null || stack == null || stack.Count == 0) return null;

            IdeaNote note = stack.Pop();
            
            TreeViewItem item = items.ItemContainerGenerator.ContainerFromItem(note) as TreeViewItem;
            if (item == null) return null;

            if (stack.Count == 0) return item;
            else
            {
                item.IsExpanded = true;
                UpdateLayout();
                return GetTreeViewItemFromIdeaNote(item, stack);
            }
        }

        public void FindTextInTree(string text, int searchType)
        {
            if (!((MainViewModel)DataContext).FindText(text, searchType))
                MessageBox.Show("Search text not found.", "No Matches", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (((MainViewModel)DataContext).CurrentIdeaNote != null)
            {
                Application.Current.MainWindow.Activate();
                Stack<IdeaNote> stack = ((MainViewModel)DataContext).CurrentIdeaNote.GetRootToChildStack();
                TreeViewItem item = GetTreeViewItemFromIdeaNote(noteTreeView, stack);
                if (item != null)
                {
                    item.BringIntoView();
                    if (((MainViewModel)DataContext).CurrentIdeaNote.IdeaNoteType == nameof(TextNote))
                        FindText(text, 0, false);
                }
            }
        }

        private void FindInTreeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            FindWindow findWindow = new FindWindow(true);
            findWindow.Owner = this;
            findWindow.Show();
        }

        private void FindNextInTreeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ((MainViewModel)DataContext).SearchInTree(((MainViewModel)DataContext).CurrentIdeaNote, 1, true);
        }

        private void FindPrevInTreeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ((MainViewModel)DataContext).SearchInTree(((MainViewModel)DataContext).CurrentIdeaNote, -1, true);
        }

        #endregion Searching

        #region Character Notes

        private void NewFamilyCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).AddFamily(true, false);

        private void AddFamilyOnlyCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).AddFamily(false, true);

        private void AddFamilyAndAssociatesCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).AddFamily(false, false);

        private void AddFamilyMemberCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).AddFamilyMember(e.Parameter as CharacterRelationshipOption);

        private void GenerateChildrenCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).GenerateChildren();

        private void GenerateSiblingsCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).GenerateSiblings();

        private void AddNonFamilyAssociatesCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).AddNonFamilyAssociates();

        private void ClearFamilyCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).ClearFamily();

        private void AddOrChangeRelationshipCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).AddOrChangeRelationship();

        private void RemoveRelationshipCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).RemoveRelationship();

        private void EditRelationshipsCommand(object sender, ExecutedRoutedEventArgs e) => ((MainViewModel)DataContext).EditRelationships();

        #endregion Character Notes

        #endregion Notes

        private void MenuItem_GenreExplorer_Checked(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).ShowGenres = !((MainViewModel)DataContext).ShowGenres;
            if (((RibbonToggleButton)sender).IsChecked ?? false) ((MainViewModel)DataContext).InitializeGenres();
            else ((MainViewModel)DataContext).ClearGenreFilter();
        }

        private void GenreCheckCommand(object sender, ExecutedRoutedEventArgs e)
        {
            CheckBox checkBox = e.OriginalSource as CheckBox;
            NoteOption genre = e.Parameter as NoteOption;
            genre.InternalCheckUpdate = true;
            genre.IsChecked = checkBox.IsChecked ?? false;
            genre.InternalCheckUpdate = false;
            ((MainViewModel)DataContext).ApplyGenreFilter();
        }
    }
}
