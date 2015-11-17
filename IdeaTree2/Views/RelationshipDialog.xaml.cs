using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace IdeaTree2
{
    /// <summary>
    /// Interaction logic for RelationshipDialog.xaml
    /// </summary>
    public partial class RelationshipDialog : Window, INotifyPropertyChanged
    {
        #region Properties

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IList), typeof(RelationshipDialog), new UIPropertyMetadata(null));
        public IList ItemsSource
        {
            get
            {
                return (IList)GetValue(ItemsSourceProperty);
            }
            set
            {
                SetValue(ItemsSourceProperty, value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(nameof(ItemsSource)));
            }
        }

        public static readonly DependencyProperty ItemsSourceTypeProperty = DependencyProperty.Register("ItemsSourceType", typeof(Type), typeof(RelationshipDialog), new UIPropertyMetadata(null));
        public Type ItemsSourceType
        {
            get
            {
                return (Type)GetValue(ItemsSourceTypeProperty);
            }
            set
            {
                SetValue(ItemsSourceTypeProperty, value);
            }
        }

        public static readonly DependencyProperty NewItemTypesProperty = DependencyProperty.Register("NewItemTypes", typeof(IList), typeof(RelationshipDialog), new UIPropertyMetadata(null));
        public IList<Type> NewItemTypes
        {
            get
            {
                return (IList<Type>)GetValue(NewItemTypesProperty);
            }
            set
            {
                SetValue(NewItemTypesProperty, value);
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RelationshipDialog), new UIPropertyMetadata(false));

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsReadOnly
        {
            get
            {
                return (bool)GetValue(IsReadOnlyProperty);
            }
            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        public CollectionControl CollectionControl => _collectionControl;

        public bool UseCustom => checkBox_UseCustom.IsChecked ?? false;

        public string CustomRelationshipName => textBox_CustomName.Text;

        #endregion //Properties

        public RelationshipDialog() { InitializeComponent(); }

        public RelationshipDialog(Type itemsourceType) : this() { ItemsSourceType = itemsourceType; }

        public RelationshipDialog(Type itemsourceType, IList<Type> newItemTypes) : this(itemsourceType) { NewItemTypes = newItemTypes; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _collectionControl.PersistChanges();
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void SubtypeButton_Click(object sender, RoutedEventArgs e)
        {
            CharacterRelationshipOption selectedRelationship = CollectionControl.SelectedItem as CharacterRelationshipOption;
            if (selectedRelationship?.ChildOptions?.Count > 0)
            {
                CollectionControl.Items.Clear();
                ItemsSource = selectedRelationship.ChildOptions;
            }
        }
    }
}
