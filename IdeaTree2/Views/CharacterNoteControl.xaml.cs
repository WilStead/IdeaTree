using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for CharacterNoteControl.xaml
    /// </summary>
    public partial class CharacterNoteControl : UserControl
    {
        public static readonly RoutedCommand RandomizeNoteOption = new RoutedCommand();
        public static readonly RoutedCommand AddCustomChild = new RoutedCommand();
        public static readonly RoutedCommand SaveCustomToTemplate = new RoutedCommand();
        public static readonly RoutedCommand CollapseAll = new RoutedCommand();
        public static readonly RoutedCommand ExpandAll = new RoutedCommand();

        public CharacterNoteControl() { InitializeComponent(); }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((CharacterNote)DataContext).Genders.MergeWithOption(((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Genders, false);
            ((CharacterNote)DataContext).Races.MergeWithOption(((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Races, false);
            ((CharacterNote)DataContext).Traits.MergeWithOption(((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Traits, false);
            ((CharacterNote)DataContext).Traits.CollapseAllChildren();
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

        private void button_EditTitlesAndSuffixes_Click(object sender, RoutedEventArgs e)
        {
            if (((CharacterNote)DataContext).RootSaveFile?.Template?.CharacterTemplate == null)
            {
                MessageBox.Show("Error: Current Character Template not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TitlesAndSuffixesTemplateEditor editDialog = new TitlesAndSuffixesTemplateEditor();
            editDialog.DataContext = ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate;
            editDialog.ShowDialog();
            ((CharacterNote)DataContext).RootSaveFile.Template.Save();
            ((CharacterNote)DataContext).RefreshTemplate();
        }

        private void button_NewFirstName_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_RelationshipRestriction.IsChecked ?? false)
                ((CharacterNote)DataContext).AssignFamilyFirstName();
            else ((CharacterNote)DataContext).NewFirstName(integerUpDown_CulturalFirstName.Value ?? (integerUpDown_CulturalFirstName.DefaultValue ?? 50));
        }

        private void button_CopySurname_Click(object sender, RoutedEventArgs e)
        {
            if (((CharacterNote)DataContext).Parent is CharacterNote)
                ((CharacterNote)DataContext).Surname = ((CharacterNote)((CharacterNote)DataContext).Parent).Surname;
        }

        private void button_CopyFamilyName_Click(object sender, RoutedEventArgs e)
        {
            if (((CharacterNote)DataContext).Parent is CharacterNote)
                ((CharacterNote)DataContext).BirthName = ((CharacterNote)((CharacterNote)DataContext).Parent).BirthName;
        }

        private void button_NewSurname_Click(object sender, RoutedEventArgs e) =>
            ((CharacterNote)DataContext).NewSurname(integerUpDown_CulturalSurname.Value ?? (integerUpDown_CulturalSurname.DefaultValue ?? 100));

        private void button_EditGenders_Click(object sender, RoutedEventArgs e)
        {
            if (((CharacterNote)DataContext).RootSaveFile?.Template?.CharacterTemplate?.Genders == null)
            {
                MessageBox.Show("Error: Current Character Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Genders.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(CharacterGenderOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(CharacterGenderOption) };
            editDialog.ItemsSource = ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Genders.ChildOptions;
            editDialog.ShowDialog();
            ((CharacterNote)DataContext).RootSaveFile.Template.Save();
            ((CharacterNote)DataContext).Genders.MergeWithOption(((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Genders, true);
            ((CharacterNote)DataContext).RefreshTemplate();
        }

        private void button_CustomGender_Click(object sender, RoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
            if (!string.IsNullOrWhiteSpace(response))
            {
                CharacterGenderOption custom = new CharacterGenderOption(response) { IsChecked = true };
                ((CharacterNote)DataContext).Genders.DeselectAllChildren();
                ((CharacterNote)DataContext).Genders.AddChild(custom);
            }
        }

        private void button_NewGender_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_RelationshipRestriction.IsChecked ?? false) ((CharacterNote)DataContext).ChooseGender();
            else ((CharacterNote)DataContext).Genders.Choose();
        }

        private void button_EditRaces_Click(object sender, RoutedEventArgs e)
        {
            if (((CharacterNote)DataContext).RootSaveFile?.Template?.CharacterTemplate?.Races == null)
            {
                MessageBox.Show("Error: Current Character Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Races.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(CharacterRaceOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(CharacterRaceOption) };
            editDialog.ItemsSource = ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Races.ChildOptions;
            editDialog.ShowDialog();
            ((CharacterNote)DataContext).RootSaveFile.Template.Save();
            ((CharacterNote)DataContext).Races.MergeWithOption(((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Races, true);
            ((CharacterNote)DataContext).RefreshTemplate();
        }

        private void button_CustomRace_Click(object sender, RoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
            if (!string.IsNullOrWhiteSpace(response))
            {
                CharacterRaceOption custom = new CharacterRaceOption(response) { IsChecked = true };
                ((CharacterNote)DataContext).Races.AddChild(custom);
            }
        }

        private void button_DefaultRace_Click(object sender, RoutedEventArgs e) => ((CharacterNote)DataContext).SetDefaultRace();

        private void button_NewRaces_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_RelationshipRestriction.IsChecked ?? false) ((CharacterNote)DataContext).SetInheritedRaces();
            else ((CharacterNote)DataContext).Races.Choose();
        }

        private void button_EditAges_Click(object sender, RoutedEventArgs e)
        {
            if (((CharacterNote)DataContext).RootSaveFile?.Template?.CharacterTemplate?.Ages == null)
            {
                MessageBox.Show("Error: Current Character Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Ages.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(CharacterAgeOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(CharacterAgeOption) };
            editDialog.ItemsSource = ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Ages.ChildOptions;
            editDialog.ShowDialog();
            ((CharacterNote)DataContext).RootSaveFile.Template.Save();
            ((CharacterNote)DataContext).RefreshTemplate();
        }

        private void button_NewAge_Click(object sender, RoutedEventArgs e) => ((CharacterNote)DataContext).ChooseAge(true, null, null, checkBox_RelationshipRestriction.IsChecked ?? false);

        private void button_NewTraits_Click(object sender, RoutedEventArgs e) => ((CharacterNote)DataContext).ChooseTraits(checkBox_RelationshipRestriction.IsChecked ?? false);

        private void button_EditTraits_Click(object sender, RoutedEventArgs e)
        {
            if (((CharacterNote)DataContext).RootSaveFile?.Template?.CharacterTemplate?.Traits == null)
            {
                MessageBox.Show("Error: Current Character Template not found, or missing data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Traits.DeselectAllChildren();
            Xceed.Wpf.Toolkit.CollectionControlDialog editDialog = new Xceed.Wpf.Toolkit.CollectionControlDialog(typeof(CharacterNoteOption));
            editDialog.NewItemTypes = new List<Type>() { typeof(CharacterNoteOption) };
            editDialog.ItemsSource = ((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Traits.ChildOptions;
            editDialog.ShowDialog();
            ((CharacterNote)DataContext).RootSaveFile.Template.Save();
            ((CharacterNote)DataContext).Traits.MergeWithOption(((CharacterNote)DataContext).RootSaveFile.Template.CharacterTemplate.Traits, true);
            ((CharacterNote)DataContext).RefreshTemplate();
        }

        private void button_AddCustomTrait_Click(object sender, RoutedEventArgs e)
        {
            string response = PromptDialog.Prompt("Enter your custom value:", "Custom Value", false);
            if (!string.IsNullOrWhiteSpace(response))
            {
                CharacterNoteOption custom = new CharacterNoteOption(response) { IsChecked = true };
                ((CharacterNote)DataContext).Traits.AddChild(custom);
            }
        }

        private void button_ClearTraits_Click(object sender, RoutedEventArgs e) => ((CharacterNote)DataContext).Traits.DeselectAllChildren();

        private void button_NewCharacter_Click(object sender, RoutedEventArgs e) =>
            ((CharacterNote)DataContext).RandomizeAll(checkBox_RelationshipRestriction.IsChecked ?? false,
                integerUpDown_CulturalFirstName.Value ?? (integerUpDown_CulturalFirstName.DefaultValue ?? 50),
                integerUpDown_CulturalSurname.Value ?? (integerUpDown_CulturalSurname.DefaultValue ?? 100));

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
            NoteOption templateOption = noteOption.RootIdea?.RootSaveFile?.Template?.CharacterTemplate?.Traits;
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
            NoteOption templateOption = noteOption?.RootIdea?.RootSaveFile?.Template?.CharacterTemplate?.Traits;
            if (treeView == null || noteOption == null || templateOption == null) e.CanExecute = false;
            else if (templateOption.FindOption(noteOption.Path) == null) e.CanExecute = true;
            else e.CanExecute = false;
        }

        private void ExpandAllItems(ItemsControl items, bool expand)
        {
            foreach (var item in items.Items)
            {
                ItemsControl childItems = items.ItemContainerGenerator.ContainerFromItem(item) as ItemsControl;
                if (childItems != null) ExpandAllItems(childItems, expand);
                TreeViewItem child = childItems as TreeViewItem;
                if (child != null) child.IsExpanded = expand;
            }
        }

        private void CollapseAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TreeView treeView = (TreeView)e.Parameter;
            if (treeView != null) ExpandAllItems(treeView, false);
        }

        private void ExpandAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            TreeView treeView = (TreeView)e.Parameter;
            TreeViewItem selectedItem = treeView.ItemContainerGenerator.ContainerFromItem(treeView.SelectedItem) as TreeViewItem;
            if (selectedItem != null) ExpandAllItems(selectedItem, true);
            else if (treeView != null) ExpandAllItems(treeView, true);
        }
    }
}
