using Microsoft.Practices.Prism.Mvvm;
using MonitoredUndo;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace IdeaTree2
{
    [ProtoContract(AsReferenceDefault = true)]
    [ProtoInclude(100, typeof(CharacterNoteOption))]
    [ProtoInclude(200, typeof(PlotElementNoteOption))]
    [ProtoInclude(300, typeof(PlotArchetypeNoteOption))]
    public class NoteOption : BindableBase, ISupportsUndo
    {
        private static Random random = new Random();

        private string name;
        [ProtoMember(1)]
        public string Name
        {
            get { return name; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(Name), name, value);
                SetProperty(ref name, value);
            }
        }
        
        [Browsable(false)]
        public NoteOption RootOption
        {
            get
            {
                if (Parent != null) return Parent.RootOption;
                else return this;
            }
        }

        private IdeaNote rootIdea;
        [Browsable(false)]
        public IdeaNote RootIdea
        {
            get
            {
                if (rootIdea != null) return rootIdea;
                else if (Parent != null) return Parent.RootIdea;
                else return null;
            }
            set { rootIdea = value; }
        }
        
        [Browsable(false)]
        public NoteOption Parent { get; set; }
        
        public static char pathSeparatorChar = '\\';
        
        /// <summary>
        /// A string which describes the sequence of options from the root to this option (not including the root itself).
        /// </summary>
        /// <remarks>The <see cref="pathSeparatorChar"/> character is used as a separator.</remarks>
        [Browsable(false)]
        public string Path
        {
            get
            {
                if (Parent != null) return $"{Parent.Path}{pathSeparatorChar}{Name}";
                else return Name;
            }
        }

        /// <summary>
        /// The relative chance of that this option will be selected, among all its sibling options.
        /// For MultiSelect choices, this represents an absolute percentage chance.
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public int Weight { get; set; } = 1;

        protected int effectiveWeight;

        /// <summary>
        /// Indicates that this option will choose one (or more) of its child options at random when it is selected.
        /// Options which are not choices are considered categories instead, and ALL of its children become active
        /// when it is selected (possibly making random choices of their own).
        /// </summary>
        [DisplayName("Is Choice")]
        [ProtoMember(3)]
        public bool IsChoice { get; set; }

        /// <summary>
        /// Indicates that each of this option's children has a chance to become active when making a random selection.
        /// If false, only one child option is selected at random, instead.
        /// </summary>
        /// <remarks>
        /// This property is ignored if <see cref="IsChoice"/> is false.
        /// </remarks>
        [DisplayName("Is Multi-Select")]
        [ProtoMember(4)]
        public bool IsMultiSelect { get; set; }

        /// <summary>
        /// Indicates that only one of this option's children is chosen when making a random selection, but the
        /// user may activate additional children by hand. If false, activating any child causes all others to
        /// become deactivated, instead.
        /// </summary>
        /// <remarks>
        /// This property is ignored if <see cref="IsMultiSelect"/> is true, or if <see cref="IsChoice"/> is false.
        /// </remarks>
        [DisplayName("Is Manual Multi-Select")]
        [ProtoMember(5)]
        public bool IsManualMultiSelect { get; set; }

        /// <summary>
        /// If true, when randomly selecting child options, there is a chance that no child will be selected.
        /// </summary>
        /// <remarks>
        /// This property is ignored if <see cref="IsChoice"/> is false.
        /// </remarks>
        [DisplayName("Allows None")]
        [ProtoMember(6)]
        public bool AllowsNone { get; set; }

        /// <summary>
        /// When <see cref="AllowsNone"/> is true, and <see cref="IsMultiSelect"/> is false, this is the weight given
        /// to the chance that no child will be selected.
        /// </summary>
        /// <remarks>
        /// This property is ignored if <see cref="AllowsNone"/> is false.
        /// </remarks>
        [DisplayName("None Weight")]
        [ProtoMember(7, IsRequired = true)]
        public int NoneWeight { get; set; } = 1;

        [Browsable(false)]
        public bool InternalCheckUpdate { get; set; }

        private bool isChecked;
        [Browsable(false)]
        [ProtoMember(8)]
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked == value) return;

                DefaultChangeFactory.Current.OnChanging(this, nameof(IsChecked), isChecked, value);
                SetProperty(ref isChecked, value);
                UpdateLowestCheckedChild();
                UpdateSummaries();

                if (RootIdea?.RootSaveFile != null)
                {
                    RootIdea.RootSaveFile.ChangedSinceLastSave = true;
                    RootIdea.RootSaveFile.ChangedSinceLastSaveExceptExpansion = true;
                }

                if (!value)
                {
                    // All children are unchecked when a parent is unchecked.
                    foreach (var child in ChildOptions) child.IsChecked = value;
                }
                else SetChecked(value);
            }
        }
        
        private bool isExpanded;
        [Browsable(false)]
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }
        
        [Browsable(false)]
        public string LeafSummary
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var child in LowestCheckedChildren(new List<NoteOption>()))
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(GetChildPathNode(child.Path).Replace("\\", ": "));
                }
                return sb.ToString();
            }
        }
        
        [Browsable(false)]
        public string Summary => GetSummary(0);
        
        [Browsable(false)]
        public NoteOption LowestCheckedChild => LowestCheckedChildren(new List<NoteOption>()).FirstOrDefault();
        
        [NewItemTypes(typeof(NoteOptionModifier), typeof(NoteOptionCharacterModifier))]
        public ObservableCollection<NoteOptionModifier> Modifiers { get; set; } = new ObservableCollection<NoteOptionModifier>();
        [ProtoMember(9, OverwriteList = true)]
        private IList<NoteOptionModifier> ModifierList
        {
            get { return Modifiers; }
            set { Modifiers = new ObservableCollection<NoteOptionModifier>(value); }
        }
        
        [DisplayName("Child Options")]
        [NewItemTypes(typeof(NoteOption), typeof(CharacterNoteOption), typeof(CharacterGenderOption), typeof(CharacterRaceOption), typeof(CharacterAgeOption), typeof(CharacterOrientationOption), typeof(CharacterRelationshipOption))]
        public ObservableCollection<NoteOption> ChildOptions { get; set; } = new ObservableCollection<NoteOption>();
        [ProtoMember(10, OverwriteList = true)]
        private IList<NoteOption> ChildOptionList
        {
            get { return ChildOptions; }
            set
            {
                ChildOptions = new ObservableCollection<NoteOption>(value);
                ChildOptions.CollectionChanged += ChildOptions_CollectionChanged;
                foreach (NoteOption item in ChildOptions)
                    item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public NoteOption() { ChildOptions.CollectionChanged += ChildOptions_CollectionChanged; }

        public NoteOption(string name) : base() { Name = name; }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            foreach (var child in ChildOptions) child.Parent = this;
        }

        public void AddChild(NoteOption child)
        {
            if (child == this) throw new Exception("Cannot add a NoteOption as its own child.");
            child.Parent = this;
            ChildOptions.Add(child);
        }

        private void ChildOptions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (NoteOption item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (NoteOption item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
            }
            DefaultChangeFactory.Current.OnCollectionChanged(this, nameof(ChildOptions), ChildOptions, e);
            OnPropertyChanged(nameof(LeafSummary));
            OnPropertyChanged(nameof(Summary));
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(LeafSummary));
            OnPropertyChanged(nameof(Summary));
        }

        public virtual void Choose() { ChooseFromList(ChildOptions); }

        public void ChooseFromList(ObservableCollection<NoteOption> list)
        {
            // First erase any previous choice(s)
            DeselectAllInList(list);

            // Options which aren't choices are categories; have each of their children make their choices, instead.
            if (!IsChoice)
            {
                // If effective weight is 0, do not select from this category.
                bool force = false;
                int weight = GetEffectiveWeight(out force);
                if (force || weight > 0)
                {
                    // If there are no children, simply select the item.
                    if (list.Count == 0) IsChecked = true;
                    else
                    {
                        foreach (var child in list) child.Choose();
                    }
                    return;
                }
                return;
            }

            int totalWeight = 0;
            bool madeSelection = false;
            foreach (var child in list)
            {
                bool force;
                child.effectiveWeight = child.GetEffectiveWeight(out force);
                if (force)
                {
                    child.IsChecked = true;
                    child.Choose();
                    if (!IsMultiSelect) return;
                }
                totalWeight += child.effectiveWeight;
                if (IsMultiSelect)
                {
                    if (random.Next(1, 101) <= child.effectiveWeight)
                    {
                        child.IsChecked = true;
                        child.Choose();
                        madeSelection = true;
                    }
                }
            }
            // If a multi-select choice did not select any of its choices, select one
            // as if they were a single-selection set of weighted choices, unless none
            // is specifically allowed.
            if (!IsMultiSelect || (!madeSelection && !AllowsNone))
            {
                if (AllowsNone) totalWeight += NoneWeight;
                int selectedValue = random.Next(1, totalWeight + 1);
                for (int i = 0; i < list.Count; i++)
                {
                    if (selectedValue <= list[i].effectiveWeight)
                    {
                        list[i].IsChecked = true;
                        list[i].Choose();
                        break;
                    }
                    else selectedValue -= list[i].effectiveWeight;
                }
            }
        }

        public object Clone()
        {
            NoteOption clone = (NoteOption)MemberwiseClone();
            clone.ChildOptions = new ObservableCollection<NoteOption>();
            foreach (var child in ChildOptions)
                clone.AddChild((NoteOption)child.Clone());
            return clone;
        }

        public void CollapseAllChildren()
        {
            foreach (var child in ChildOptions)
            {
                child.IsExpanded = false;
                child.CollapseAllChildren();
            }
        }

        public bool ContainsText(string text)
        {
            if (Name.ToLowerInvariant().Contains(text)) return true;
            return ChildOptions.Any(c => c.ContainsText(text));
        }

        public void DeselectAllChildren() => DeselectAllInList(ChildOptions);

        public void DeselectAllInList(ObservableCollection<NoteOption> list)
        {
            foreach (var child in list)
            {
                child.IsChecked = false;
                child.IsExpanded = false;
            }
        }

        protected virtual bool DoesModifierApply(NoteOptionModifier modifier)
        {
            NoteOption option = FindOption(modifier.TargetPath);
            return (option?.IsChecked == true);
        }

        public NoteOption FindChildOption(string path)
        {
            string firstNode = GetFirstPathNode(path);
            NoteOption child = ChildOptions.FirstOrDefault(c => c.Name == firstNode);
            if (child == null) return null;

            else if (child.Path.EndsWith(path)) return child;

            else return child.FindChildOption(GetChildPathNode(path));
        }

        public NoteOption FindOption(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            NoteOption root = RootOption;
            if (root?.Path == null) return null;

            if (root.Path == path) return root;
            else if (path.StartsWith(root.Path)) return root.FindChildOption(GetChildPathNode(path));
            else return root.FindChildOption(path);
        }

        public List<NoteOption> FindOptionsByName(List<NoteOption> list, string name)
        {
            if (Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) list.Add(this);
            foreach (var child in ChildOptions) child.FindOptionsByName(list, name);
            return list;
        }

        public NoteOption GetCheckedChildrenClone()
        {
            NoteOption clone = (NoteOption)Clone();

            ObservableCollection<NoteOption> newChildren = new ObservableCollection<NoteOption>();
            foreach (var child in ChildOptions)
            {
                if (child.IsChecked) newChildren.Add(child.GetCheckedChildrenClone());
            }
            clone.ChildOptions = newChildren;

            return clone;
        }

        public NoteOption GetChildlessClone()
        {
            NoteOption clone = (NoteOption)MemberwiseClone();
            clone.ChildOptions = new ObservableCollection<NoteOption>();
            return clone;
        }

        private static string GetChildPathNode(string path)
        {
            int index = path.IndexOf(pathSeparatorChar);
            if (index == -1) return null;
            else return path.Substring(path.IndexOf(pathSeparatorChar) + 1);
        }

        public static string GetFirstPathNode(string path)
        {
            int index = path.IndexOf(pathSeparatorChar);
            if (index == -1) return path;
            else return path.Substring(0, index);
        }

        public int GetEffectiveWeight(out bool force)
        {
            List<NoteOptionModifier> modifiers = new List<NoteOptionModifier>();
            foreach (var modifier in Modifiers.Where(m => DoesModifierApply(m)))
            {
                SetModifierEffectiveValues(modifier);
                modifiers.Add(modifier);
            }
            // Get only the highest-priority items.
            modifiers = modifiers.OrderBy(m => m.Priority).Where(m => m.Priority == modifiers.First().Priority).ToList();
            if (modifiers.Any(m => m.EffectiveForce)) force = true;
            else force = false;
            if (modifiers.Count > 0) return (int)Math.Round(modifiers.Average(m => m.Weight));
            else return Weight;
        }

        public string GetSummary(int indentLevel)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < indentLevel; i++) sb.Append("\t");
            sb.AppendLine(Name);
            foreach (var child in ChildOptions.Where(c => c.IsChecked))
                sb.Append(child.GetSummary(indentLevel + 1));
            return sb.ToString();
        }

        public object GetUndoRoot() => RootIdea?.GetUndoRoot();

        /// <summary>
        /// Returns true if <paramref name="pathNode"/> is in the Path of this <see cref="NoteOption"/>.
        /// </summary>
        /// <param name="pathNode">The Name of a <see cref="NoteOption"/> potentially within this one's Path.</param>
        /// <returns>True if <paramref name="pathNode"/> is in the Path of this <see cref="NoteOption"/>; Otherwise, false.</returns>
        public bool IsInPath(string pathNode)
        {
            string[] pathNodes = Path.Split(new char[] { pathSeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            return (pathNodes.Contains(pathNode));
        }

        public List<NoteOption> LowestCheckedChildren(List<NoteOption> list)
        {
            bool addedChildren = false;
            foreach (var child in ChildOptions.Where(c => c.IsChecked))
            {
                addedChildren = true;
                list = child.LowestCheckedChildren(list);
            }

            if (!addedChildren && IsChecked) list.Add(this);
            return list;
        }

        public void MergeWithOption(NoteOption other, bool overwrite, bool mergeChecked = false)
        {
            if (overwrite)
            {
                // Merge properties.
                Weight = other.Weight;
                IsChoice = other.IsChoice;
                IsMultiSelect = other.IsMultiSelect;
                IsManualMultiSelect = other.IsManualMultiSelect;
                AllowsNone = other.AllowsNone;
                NoneWeight = other.NoneWeight;
            }
            // Do not un-check, only check to match.
            if (mergeChecked && other.IsChecked) IsChecked = true;

            // Combine matching children.
            foreach (var matchingChild in ChildOptions.Where(c => other.ChildOptions.Any(o => c.Name == o.Name)))
            {
                matchingChild.MergeWithOption(other.ChildOptions.First(o => o.Name == matchingChild.Name), overwrite, mergeChecked);
            }

            foreach (var newChild in other.ChildOptions.Where(o => !ChildOptions.Any(c => c.Name == o.Name)))
                AddChild((NoteOption)newChild.Clone());
        }

        public void RefreshTemplate() => OnPropertyChanged(nameof(RootIdea));

        protected virtual void SetChecked(bool value)
        {
            // If no children are checked, make a choice.
            if (!InternalCheckUpdate && ChildOptions.Count > 0 && !ChildOptions.Any(c => c.IsChecked))
                Choose();

            if (Parent != null)
            {
                // Checking causes the parent to expand and check.
                bool icu = Parent.InternalCheckUpdate;
                Parent.InternalCheckUpdate = true;
                Parent.IsExpanded = true;
                Parent.IsChecked = true;
                Parent.InternalCheckUpdate = icu;

                // Single-select choices uncheck all siblings when checked.
                if (Parent.IsChoice && !Parent.IsMultiSelect && !Parent.IsManualMultiSelect)
                {
                    foreach (var sibling in Parent.ChildOptions.Where(s => s != this))
                        sibling.IsChecked = false;
                }
            }
        }

        private void SetModifierEffectiveValues(NoteOptionModifier modifier)
        {
            int effectiveWeight = modifier.Weight;
            bool effectiveForce = modifier.Force;

            List<NoteOptionModifier> subModifiers = new List<NoteOptionModifier>();
            foreach (var subModifier in modifier.Modifiers.Where(s => DoesModifierApply(s)))
            {
                SetModifierEffectiveValues(subModifier);
                subModifiers.Add(subModifier);
            }
            subModifiers = subModifiers.OrderBy(m => m.Priority).Where(m => m.Priority == subModifiers.First().Priority).ToList();
            if (subModifiers.Count > 0)
            {
                if (subModifiers.Any(m => m.Force)) effectiveForce = true;
                else effectiveForce = false;

                effectiveWeight = (int)Math.Round(subModifiers.Average(m => m.Weight));
            }

            modifier.EffectiveWeight = effectiveWeight;
            modifier.EffectiveForce = effectiveForce;
        }

        public void ShowInTree()
        {
            if (Parent == null) return;
            Parent.IsExpanded = true;
            Parent.ShowInTree();
        }

        public override string ToString() => Name;

        public void UpdateSummaries()
        {
            OnPropertyChanged(nameof(LeafSummary));
            OnPropertyChanged(nameof(Summary));
            if (Parent != null) Parent.UpdateSummaries();
        }

        public void UpdateLowestCheckedChild()
        {
            OnPropertyChanged(nameof(LowestCheckedChild));
            if (Parent != null) Parent.UpdateLowestCheckedChild();
        }
    }
    
    [ProtoContract(AsReferenceDefault = true)]
    [ProtoInclude(100, typeof(NoteOptionCharacterModifier))]
    public class NoteOptionModifier
    {
        /// <summary>
        /// The full Path of the option which this modifier references.
        /// </summary>
        /// <example>
        /// A modifier with the TargetPath "Traits\\SomeTrait" will only take effect when
        /// the SomeTrait option, a child of the Traits option, is checked.
        /// </example>
        [DisplayName("Target Path")]
        [ProtoMember(1)]
        public string TargetPath { get; set; }

        /// <summary>
        /// When this modifier is active, this value replaces the normal <see cref="NoteOption.Weight"/> of
        /// its parent <see cref="NoteOption"/>.
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public int Weight { get; set; } = 1;
        
        private int effectiveWeight;
        [Browsable(false)]
        public int EffectiveWeight
        {
            get { return effectiveWeight; }
            set { effectiveWeight = value; }
        }

        /// <summary>
        /// When multiple modifiers are all active, only those with the highest priority actually take effect.
        /// </summary>
        [ProtoMember(3, IsRequired = true)]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// If this property is true, then anytime the modifier is active, its parent <see cref="NoteOption"/> is
        /// automatically checked, regardless of its <see cref="NoteOption.Weight"/>.
        /// </summary>
        [ProtoMember(4)]
        public bool Force { get; set; }

        [Browsable(false)]
        public bool EffectiveForce { get; set; }
        
        [ExpandableObject]
        [NewItemTypes(typeof(NoteOptionModifier), typeof(NoteOptionCharacterModifier))]
        public ObservableCollection<NoteOptionModifier> Modifiers { get; set; } = new ObservableCollection<NoteOptionModifier>();
        [ProtoMember(5, OverwriteList = true)]
        private IList<NoteOptionModifier> ModifierList
        {
            get { return Modifiers; }
            set { Modifiers = new ObservableCollection<NoteOptionModifier>(value); }
        }

        public NoteOptionModifier() { }
    }

    /// <summary>
    /// Represents a <see cref="NoteOption"/> as a color, depending on whether it is part of a template or not.
    /// </summary>
    public sealed class NoteOptionColorConverter : IMultiValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return Brushes.LightPink; // problem

            NoteOption option = values[0] as NoteOption;
            NoteOption templateOption = values[1] as NoteOption;
            if (option == null || templateOption == null) return Brushes.LightPink; // problem

            else if (templateOption.FindOption(option.Path) == null) return Brushes.SteelBlue; // not in template
            else return Brushes.Transparent; // in template
        }

        /// <inheritdoc/>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
