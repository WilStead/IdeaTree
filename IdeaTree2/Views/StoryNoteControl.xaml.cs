using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for StoryNoteControl.xaml
    /// </summary>
    public partial class StoryNoteControl : UserControl
    {
        public static readonly RoutedCommand RandomizeNoteOption = new RoutedCommand();
        public static readonly RoutedCommand AddCustomChild = new RoutedCommand();
        public static readonly RoutedCommand SaveCustomToTemplate = new RoutedCommand();

        public StoryNoteControl() { InitializeComponent(); }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            tabControl_Main.SelectedIndex = 0;
            ((StoryNote)DataContext).Backgrounds.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Backgrounds, false);
            ((StoryNote)DataContext).Resolutions.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Resolutions, false);
            ((StoryNote)DataContext).Settings.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Settings, false);
            ((StoryNote)DataContext).Settings.CollapseAllChildren();
            foreach (var child in ((StoryNote)DataContext).Settings.ChildOptions)
            {
                if (child.IsChecked) child.IsExpanded = true;
            }
            ((StoryNote)DataContext).Traits.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Traits, false);
            ((StoryNote)DataContext).Themes.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Themes, false);
            ((StoryNote)DataContext).Genres.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Genres, false);
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

        #region Plot

        private void button_NewPlotElement_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).ChoosePlotElement();

        private void button_NewPlotSubtype_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).ChoosePlotSubtype();

        private void comboBox_PlotSubtype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                NoteOption newSubtype = e.AddedItems[0] as NoteOption;
                if (newSubtype != null) newSubtype.IsChecked = true;
            }
        }

        private void comboBox_PlotElement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                PlotElementNoteOption newElement = e.AddedItems[0] as PlotElementNoteOption;
                if (newElement != null) newElement.IsChecked = true;
            }
        }

        private void button_NewPlot_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).ChoosePlot();

        private void comboBox_Archetype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                PlotArchetypeNoteOption newArchetype = e.AddedItems[0] as PlotArchetypeNoteOption;
                if (newArchetype != null) newArchetype.IsChecked = true;
            }
        }

        private void button_EditPlot_Click(object sender, RoutedEventArgs e)
        {
            if (((StoryNote)DataContext).RootSaveFile?.Template?.StoryTemplate?.PlotArchetypes == null)
            {
                MessageBox.Show("Error: Current Story Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(PlotArchetypeNoteOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(PlotArchetypeNoteOption) };
            editDialog.ItemsSource = ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.PlotArchetypes.ChildOptions;
            editDialog.ShowDialog();
            ((StoryNote)DataContext).RootSaveFile.Template.Save();
            ((StoryNote)DataContext).RefreshTemplate();
        }

        #endregion Plot

        #region Character / Conflict

        private void button_NewCharacterConflict_Click(object sender, RoutedEventArgs e)
        {
            ((StoryNote)DataContext).ChooseCharacterConflict(toggleButton_LockProtagonist.IsChecked ?? false,
                toggleButton_LockSupporting.IsChecked ?? false, toggleButton_LockConflict.IsChecked ?? false);
        }

        private void button_EditCharacterConflictChoices_Click(object sender, RoutedEventArgs e)
        {
            if (((StoryNote)DataContext).RootSaveFile?.Template?.StoryTemplate == null)
            {
                MessageBox.Show("Error: Current Story Template not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            CharacterConflictTemplateEditor editDialog = new CharacterConflictTemplateEditor();
            editDialog.DataContext = ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate;
            editDialog.ShowDialog();
            ((StoryNote)DataContext).RootSaveFile.Template.Save();
            ((StoryNote)DataContext).RefreshTemplate();
        }

        private void button_ClearCharacterConflict_Click(object sender, RoutedEventArgs e)
        {
            toggleButton_LockProtagonist.IsChecked = false;
            toggleButton_LockSupporting.IsChecked = false;
            toggleButton_LockConflict.IsChecked = false;

            textBox_CharacterConflict.Clear();
        }

        #endregion Character / Conflict

        #region Background

        private void button_NewBackground_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Backgrounds.Choose();

        private void button_EditBackgrounds_Click(object sender, RoutedEventArgs e)
        {
            if (((StoryNote)DataContext).RootSaveFile?.Template?.StoryTemplate?.Backgrounds == null)
            {
                MessageBox.Show("Error: Current Story Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Backgrounds.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(NoteOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(NoteOption) };
            editDialog.ItemsSource = ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Backgrounds.ChildOptions;
            editDialog.ShowDialog();
            ((StoryNote)DataContext).RootSaveFile.Template.Save();
            ((StoryNote)DataContext).Backgrounds.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Backgrounds, true);
            ((StoryNote)DataContext).RefreshTemplate();
        }

        private void button_AddCustomBackground_Click(object sender, RoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
            if (!string.IsNullOrWhiteSpace(response))
            {
                NoteOption custom = new NoteOption(response);
                ((StoryNote)DataContext).Backgrounds.AddChild(custom);
                custom.IsChecked = true;
            }
        }

        #endregion Background

        #region Resolution

        private void button_NewResolution_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Resolutions.Choose();

        private void button_EditResolutions_Click(object sender, RoutedEventArgs e)
        {
            if (((StoryNote)DataContext).RootSaveFile?.Template?.StoryTemplate?.Resolutions == null)
            {
                MessageBox.Show("Error: Current Story Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Resolutions.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(NoteOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(NoteOption) };
            editDialog.ItemsSource = ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Resolutions.ChildOptions;
            editDialog.ShowDialog();
            ((StoryNote)DataContext).RootSaveFile.Template.Save();
            ((StoryNote)DataContext).Resolutions.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Resolutions, true);
            ((StoryNote)DataContext).RefreshTemplate();
        }

        private void button_AddCustomResolution_Click(object sender, RoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
            if (!string.IsNullOrWhiteSpace(response))
            {
                NoteOption custom = new NoteOption(response) { IsChecked = true };
                ((StoryNote)DataContext).Resolutions.AddChild(custom);
            }
        }

        #endregion Resolution

        #region Traits

        private void button_NewTraits_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Traits.Choose();

        private void button_EditTraits_Click(object sender, RoutedEventArgs e)
        {
            if (((StoryNote)DataContext).RootSaveFile?.Template?.StoryTemplate?.Traits == null)
            {
                MessageBox.Show("Error: Current Story Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Traits.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(NoteOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(NoteOption) };
            editDialog.ItemsSource = ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Traits.ChildOptions;
            editDialog.ShowDialog();
            ((StoryNote)DataContext).RootSaveFile.Template.Save();
            ((StoryNote)DataContext).Traits.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Traits, true);
            ((StoryNote)DataContext).RefreshTemplate();
        }

        private void button_AddCustomTrait_Click(object sender, RoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
            if (!string.IsNullOrWhiteSpace(response))
            {
                NoteOption custom = new NoteOption(response) { IsChecked = true };
                ((StoryNote)DataContext).Traits.AddChild(custom);
            }
        }

        private void button_ClearTraits_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Traits.DeselectAllChildren();

        #endregion Traits

        #region Setting

        private void button_NewSetting_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Settings.Choose();

        private void button_AppendSetting_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Settings.Choose(true);

        private void button_EditSettings_Click(object sender, RoutedEventArgs e)
        {
            if (((StoryNote)DataContext).RootSaveFile?.Template?.StoryTemplate?.Settings == null)
            {
                MessageBox.Show("Error: Current Story Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Settings.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(NoteOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(NoteOption) };
            editDialog.ItemsSource = ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Settings.ChildOptions;
            editDialog.ShowDialog();
            ((StoryNote)DataContext).RootSaveFile.Template.Save();
            ((StoryNote)DataContext).Settings.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Settings, true);
            ((StoryNote)DataContext).RefreshTemplate();
        }

        private void button_AddCustomSetting_Click(object sender, RoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
            if (!string.IsNullOrWhiteSpace(response))
            {
                NoteOption custom = new NoteOption(response) { IsChecked = true };
                ((StoryNote)DataContext).Settings.AddChild(custom);
            }
        }

        private void button_ClearSetting_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Settings.DeselectAllChildren();

        #endregion Setting

        #region Themes

        private void button_NewThemes_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Themes.Choose();

        private void button_AppendThemes_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Themes.Choose(true);

        private void button_EditThemes_Click(object sender, RoutedEventArgs e)
        {
            if (((StoryNote)DataContext).RootSaveFile?.Template?.StoryTemplate?.Themes == null)
            {
                MessageBox.Show("Error: Current Story Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Themes.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(NoteOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(NoteOption) };
            editDialog.ItemsSource = ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Themes.ChildOptions;
            editDialog.ShowDialog();
            ((StoryNote)DataContext).RootSaveFile.Template.Save();
            ((StoryNote)DataContext).Themes.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Themes, true);
            ((StoryNote)DataContext).RefreshTemplate();
        }

        private void button_AddCustomTheme_Click(object sender, RoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
            if (!string.IsNullOrWhiteSpace(response))
            {
                NoteOption custom = new NoteOption(response) { IsChecked = true };
                ((StoryNote)DataContext).Themes.AddChild(custom);
            }
        }

        private void button_ClearThemes_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Themes.DeselectAllChildren();

        #endregion Themes

        #region Genres

        private void UpdateGenreTabs()
        {
            tabControl_Genres.SelectedItem = ((StoryNote)DataContext).Genres.ChildOptions.FirstOrDefault(c => c.IsChecked);
        }

        private void button_NewGenres_Click(object sender, RoutedEventArgs e)
        {
            ((StoryNote)DataContext).Genres.Choose();
            UpdateGenreTabs();
        }

        private void button_AppendGenres_Click(object sender, RoutedEventArgs e)
        {
            ((StoryNote)DataContext).Genres.Choose(true);
            UpdateGenreTabs();
        }

        private void button_EditGenres_Click(object sender, RoutedEventArgs e)
        {
            if (((StoryNote)DataContext).RootSaveFile?.Template?.StoryTemplate?.Genres == null)
            {
                MessageBox.Show("Error: Current Story Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Genres.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(NoteOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(NoteOption) };
            editDialog.ItemsSource = ((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Genres.ChildOptions;
            editDialog.ShowDialog();
            ((StoryNote)DataContext).RootSaveFile.Template.Save();
            ((StoryNote)DataContext).Genres.MergeWithOption(((StoryNote)DataContext).RootSaveFile.Template.StoryTemplate.Genres, true);
            ((StoryNote)DataContext).RefreshTemplate();
        }

        private void button_ClearGenres_Click(object sender, RoutedEventArgs e) => ((StoryNote)DataContext).Genres.DeselectAllChildren();

        #endregion Genres

        private void button_NewStory_Click(object sender, RoutedEventArgs e)
        {
            toggleButton_LockProtagonist.IsChecked = false;
            toggleButton_LockSupporting.IsChecked = false;
            toggleButton_LockConflict.IsChecked = false;

            ((StoryNote)DataContext).ChooseAll();
            UpdateGenreTabs();
        }

        private void RandomizeNoteOptionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TreeView treeView = (TreeView)e.Parameter;
            NoteOption noteOption = (NoteOption)treeView.SelectedItem;
            if (noteOption != null) noteOption.Choose();
        }

        private void AddCustomChildCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TreeView treeView = (TreeView)e.Parameter;
            NoteOption noteOption = (NoteOption)treeView.SelectedItem;
            if (noteOption != null)
            {
                string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    NoteOption custom = new NoteOption(response);
                    noteOption.AddChild(custom);
                    custom.IsChecked = true;
                }
            }
        }

        private void CanCommandNoteOption(object sender, CanExecuteRoutedEventArgs e)
        {
            TreeView treeView = (TreeView)e.Parameter;
            NoteOption noteOption = (NoteOption)treeView.SelectedItem;
            if (noteOption == null) e.CanExecute = false;
            else e.CanExecute = true;
        }

        private void SaveCustomToTemplateCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TreeView treeView = (TreeView)e.Parameter;
            NoteOption noteOption = (NoteOption)treeView.SelectedItem;
            NoteOption templateOption = noteOption.RootIdea?.RootSaveFile?.Template?.StoryTemplate?.FindOption(noteOption.RootOption?.Path);
            if (treeView == null || noteOption == null || templateOption == null) return;

            templateOption.MergeWithOption(noteOption.RootOption, false);
            templateOption.DeselectAllChildren();
            noteOption.RootIdea.RootSaveFile.Template.Save();
            noteOption.RefreshTemplate();
        }

        private void CanSaveCustomToTemplate(object sender, CanExecuteRoutedEventArgs e)
        {
            TreeView treeView = (TreeView)e.Parameter;
            NoteOption noteOption = (NoteOption)treeView.SelectedItem;
            NoteOption templateOption = noteOption.RootIdea?.RootSaveFile?.Template?.StoryTemplate?.FindOption(noteOption.RootOption?.Path);
            if (treeView == null || noteOption == null || templateOption == null) e.CanExecute = false;
            else if (templateOption.FindOption(noteOption.Path) == null) e.CanExecute = true;
            else e.CanExecute = false;
        }

        private void GenreTab_Check(object sender, RoutedEventArgs e)
        {
            DependencyObject item = sender as DependencyObject;
            while (item != null && item.GetType() != typeof(TabItem))
                item = VisualTreeHelper.GetParent(item);
            if (item != null) ((TabItem)item).IsSelected = true;
        }
    }
}
