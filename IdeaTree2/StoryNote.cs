using MonitoredUndo;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace IdeaTree2
{
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class StoryNote : IdeaNote
    {
        [ProtoIgnore]
        public override string IdeaNoteType => nameof(StoryNote);

        private int counter;
        public int Counter
        {
            get { return counter; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(Counter), counter, value);
                SetProperty(ref counter, value);
            }
        }

        private string plotArchetype;
        public string PlotArchetype
        {
            get { return plotArchetype; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(PlotArchetype), plotArchetype, value);
                SetProperty(ref plotArchetype, value);
            }
        }

        private string plotSubtype;
        public string PlotSubtype
        {
            get { return plotSubtype; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(PlotSubtype), plotSubtype, value);
                SetProperty(ref plotSubtype, value);
            }
        }

        private string plotElement;
        public string PlotElement
        {
            get { return plotElement; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(PlotElement), plotElement, value);
                SetProperty(ref plotElement, value);
            }
        }

        private string characterConflict;
        public string CharacterConflict
        {
            get { return characterConflict; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(CharacterConflict), characterConflict, value);
                SetProperty(ref characterConflict, value);
            }
        }

        public NoteOption Backgrounds { get; set; } = new NoteOption() { Name = "Backgrounds", IsChoice = true, IsManualMultiSelect = true };

        public NoteOption Resolutions { get; set; } = new NoteOption() { Name = "Resolutions", IsChoice = true, IsManualMultiSelect = true };

        public NoteOption Traits { get; set; } = new NoteOption() { Name = "Traits" };

        public NoteOption Settings { get; set; } = new NoteOption() { Name = "Settings", IsChoice = true, IsManualMultiSelect = true };

        public NoteOption Themes { get; set; } = new NoteOption() { Name = "Themes", IsChoice = true, IsMultiSelect = true };

        public NoteOption Genres { get; set; } = new NoteOption() { Name = "Genres", IsChoice = true, IsMultiSelect = true };

        private static Random random = new Random();

        public StoryNote() : base()
        {
            Backgrounds.RootIdea = this;
            Resolutions.RootIdea = this;
            Traits.RootIdea = this;
            Settings.RootIdea = this;
            Themes.RootIdea = this;
            Genres.RootIdea = this;
        }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            Backgrounds.RootIdea = this;
            Resolutions.RootIdea = this;
            Traits.RootIdea = this;
            Settings.RootIdea = this;
            Themes.RootIdea = this;
            Genres.RootIdea = this;
        }

        [ProtoBeforeSerialization]
        private void BeforeSerialization()
        {
            Backgrounds = Backgrounds.GetCheckedChildrenClone();
            Resolutions = Resolutions.GetCheckedChildrenClone();
            Traits = Traits.GetCheckedChildrenClone();
            Settings = Settings.GetCheckedChildrenClone();
            Themes = Themes.GetCheckedChildrenClone();
            Genres = Genres.GetCheckedChildrenClone();
        }

        public override string GetDefaultName() => "[Story Note]";

        private string GetPlotString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(PlotArchetype))
            {
                sb.AppendLine($"Plot type: {PlotArchetype}");
                if (!string.IsNullOrWhiteSpace(PlotSubtype))
                    sb.AppendLine($"; {PlotSubtype}. ");
                PlotArchetypeNoteOption archetype =
                    RootSaveFile?.Template?.StoryTemplate.PlotArchetypes.ChildOptions.FirstOrDefault(a =>
                    a.Name.Equals(PlotArchetype)) as PlotArchetypeNoteOption;
                if (archetype != null)
                {
                    sb.Append("Elements: ");
                    for (int i = 0; i < archetype.ElementOptions.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(archetype.ElementOptions[i].Name);
                    }
                    sb.Append("; ");
                }
                if (!string.IsNullOrWhiteSpace(PlotElement))
                    sb.AppendLine($"Protagonist is {PlotElement}. ");
            }
            return sb.ToString();
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
            
            newNote.AddParagraph(GetPlotString());
            newNote.AddParagraph(string.Empty);
            foreach (var line in CharacterConflict.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);
            newNote.AddParagraph(string.Empty);
            foreach (var line in Backgrounds.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);
            newNote.AddParagraph(string.Empty);
            foreach (var line in Resolutions.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);
            newNote.AddParagraph(string.Empty);
            foreach (var line in Traits.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);
            newNote.AddParagraph(string.Empty);
            foreach (var line in Settings.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);
            newNote.AddParagraph(string.Empty);
            foreach (var line in Themes.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);
            newNote.AddParagraph(string.Empty);
            foreach (var line in Genres.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);

            return newNote;
        }

        private bool MatchesGenres(ObservableCollection<NoteOption> requiredGenres, ObservableCollection<NoteOption> possibleGenres,
            ObservableCollection<NoteOption> excludedGenres, bool showNoGenres, bool topLevel = true)
        {
            if (topLevel && !Genres.ChildOptions.Any(g => g.IsChecked)) return showNoGenres;

            foreach (var genre in requiredGenres.Where(g => g.IsChecked))
            {
                if (Genres.FindOption(genre.Path)?.IsChecked != true) return false;
                if (!MatchesGenres(genre.ChildOptions, possibleGenres, excludedGenres, showNoGenres, false)) return false;
            }

            foreach (var genre in excludedGenres.Where(g => g.IsChecked))
            {
                if (Genres.FindOption(genre.Path)?.IsChecked == true) return false;
                if (!MatchesGenres(requiredGenres, possibleGenres, genre.ChildOptions, showNoGenres, false)) return false;
            }

            if (!possibleGenres.Any(g => g.IsChecked)) return true;
            bool possibleHit = false;
            foreach (var genre in possibleGenres.Where(g => g.IsChecked))
            {
                if (Genres.FindOption(genre.Path)?.IsChecked == true)
                    possibleHit = !genre.ChildOptions.Any(g => g.IsChecked) ||
                        MatchesGenres(requiredGenres, genre.ChildOptions, excludedGenres, showNoGenres, false);
            }
            if (!possibleHit) return false;

            return true;
        }

        public override bool ShowOnlyMatchingGenres(ObservableCollection<NoteOption> requiredGenres, ObservableCollection<NoteOption> possibleGenres,
            ObservableCollection<NoteOption> excludedGenres, bool showNoGenres, bool force = false)
        {
            bool show = force || MatchesGenres(requiredGenres, possibleGenres, excludedGenres, showNoGenres);
            foreach (var child in Ideas)
            {
                show = show || child.ShowOnlyMatchingGenres(requiredGenres, possibleGenres, excludedGenres, showNoGenres, force || show);
            }
            if (show) Visibility = Visibility.Visible;
            else Visibility = Visibility.Collapsed;
            return show;
        }

        public override bool ContainsText(string text)
        {
            string matchText = text.ToLowerInvariant();
            return (Name.ToLowerInvariant().Contains(matchText) || PlotArchetype?.ToLowerInvariant().Contains(matchText) == true ||
                PlotSubtype?.ToLowerInvariant().Contains(matchText) == true || PlotElement?.ToLowerInvariant().Contains(matchText) == true ||
                CharacterConflict?.ToLowerInvariant().Contains(matchText) == true ||
                Backgrounds?.ContainsText(matchText) == true || Resolutions?.ContainsText(matchText) == true ||
                Traits?.ContainsText(matchText) == true || Settings?.ContainsText(matchText) == true ||
                Themes?.ContainsText(matchText) == true || Genres?.ContainsText(matchText) == true);
        }

        #region Randomization

        #region Plot

        public void ChoosePlotElement()
        {
            if (string.IsNullOrEmpty(PlotArchetype) ||
                RootSaveFile?.Template?.StoryTemplate?.PlotArchetypes?.ChildOptions?.Count <= 0 ||
                RootSaveFile?.Template?.StoryTemplate?.PlotArchetypes?.ChildOptions?.Any(o => o.Name.Equals(PlotArchetype)) != true) return;

            PlotArchetypeNoteOption archetype = (PlotArchetypeNoteOption)RootSaveFile.Template.StoryTemplate.PlotArchetypes.ChildOptions.First(o => o.Name.Equals(PlotArchetype));
            archetype.ChooseElement();
            var element = archetype.ElementOptions.FirstOrDefault(o => o.IsChecked)?.Name;
            if (!string.IsNullOrEmpty(element)) PlotElement = element;
        }

        public void ChoosePlotSubtype()
        {
            if (string.IsNullOrEmpty(PlotArchetype) ||
                RootSaveFile?.Template?.StoryTemplate?.PlotArchetypes?.ChildOptions?.Count <= 0 ||
                RootSaveFile?.Template?.StoryTemplate?.PlotArchetypes?.ChildOptions?.Any(o => o.Name.Equals(PlotArchetype)) != true)
                return;

            PlotArchetypeNoteOption archetype = (PlotArchetypeNoteOption)RootSaveFile.Template.StoryTemplate.PlotArchetypes.ChildOptions.First(o => o.Name.Equals(PlotArchetype));
            archetype.ChooseFromList(archetype.ChildOptions);

            var subtype = archetype.ChildOptions.FirstOrDefault(o => o.IsChecked)?.Name;
            if (!string.IsNullOrEmpty(subtype)) PlotSubtype = subtype;
        }

        public void ChoosePlot()
        {
            if (RootSaveFile?.Template?.StoryTemplate?.PlotArchetypes == null) return;
            
            RootSaveFile.Template.StoryTemplate.PlotArchetypes.Choose();

            PlotArchetypeNoteOption archetype = (PlotArchetypeNoteOption)RootSaveFile.Template.StoryTemplate.PlotArchetypes.ChildOptions.FirstOrDefault(o => o.IsChecked);
            if (archetype == null) return;

            PlotArchetype = archetype.Name;

            var subtype = archetype.ChildOptions.FirstOrDefault(o => o.IsChecked)?.Name;
            if (!string.IsNullOrEmpty(subtype)) PlotSubtype = subtype;

            archetype.ChooseElement();
            var element = archetype.ElementOptions.FirstOrDefault(o => o.IsChecked)?.Name;
            if (!string.IsNullOrEmpty(element)) PlotElement = element;
        }

        #endregion Plot

        #region Character / Conflict

        private string GetProtagonistFromCharacterConflict()
        {
            string protagonist = null;

            if (!string.IsNullOrEmpty(CharacterConflict) &&
                CharacterConflict.StartsWith("The story starts when the protagonist "))
            {
                int endIndex = CharacterConflict.IndexOf('.');
                if (endIndex == -1) endIndex = CharacterConflict.IndexOf(Environment.NewLine);

                if (endIndex == -1) protagonist = CharacterConflict.Substring(38);
                else protagonist = CharacterConflict.Substring(38, endIndex - 38);
            }

            return protagonist;
        }

        private string GetSupportingCharacterFromCharacterConflict()
        {
            string supportingCharacter = null;

            if (!string.IsNullOrEmpty(CharacterConflict))
            {
                int startIndex = CharacterConflict.IndexOf("Another character is ");
                if (startIndex != -1)
                {
                    startIndex += 21;
                    int endIndex = CharacterConflict.IndexOf(" who ");

                    if (endIndex == -1) supportingCharacter = CharacterConflict.Substring(startIndex);
                    else supportingCharacter = CharacterConflict.Substring(startIndex, endIndex - startIndex);
                }
            }

            return supportingCharacter;
        }

        private string GetConflictFromCharacterConflict()
        {
            string conflict = null;

            if (!string.IsNullOrEmpty(CharacterConflict))
            {
                int startIndex = CharacterConflict.IndexOf(" who ");
                if (startIndex != -1)
                {
                    startIndex += 5;
                    int endIndex = CharacterConflict.LastIndexOf('.');

                    if (endIndex == -1) conflict = CharacterConflict.Substring(startIndex);
                    else conflict = CharacterConflict.Substring(startIndex, endIndex - startIndex);
                }
            }

            return conflict;
        }

        public void ChooseCharacterConflict(bool lockProtagonist, bool lockSupportingCharacter, bool lockConflict)
        {
            if (RootSaveFile?.Template?.StoryTemplate == null) return;

            string protagonist = GetProtagonistFromCharacterConflict();
            string supportingCharacter = GetSupportingCharacterFromCharacterConflict();
            string conflict = GetConflictFromCharacterConflict();

            if (RootSaveFile.Template.StoryTemplate.Protagonists?.Count > 0 && (!lockProtagonist || string.IsNullOrEmpty(protagonist)))
                protagonist = RootSaveFile.Template.StoryTemplate.Protagonists[random.Next(RootSaveFile.Template.StoryTemplate.Protagonists.Count)];

            if (RootSaveFile.Template.StoryTemplate.SupportingCharacters?.Count > 0 && (!lockSupportingCharacter || string.IsNullOrEmpty(supportingCharacter)))
                supportingCharacter = RootSaveFile.Template.StoryTemplate.SupportingCharacters[random.Next(RootSaveFile.Template.StoryTemplate.SupportingCharacters.Count)];

            if (RootSaveFile.Template.StoryTemplate.Conflicts?.Count > 0 && (!lockConflict || string.IsNullOrEmpty(conflict)))
                conflict = RootSaveFile.Template.StoryTemplate.Conflicts[random.Next(RootSaveFile.Template.StoryTemplate.Conflicts.Count)];

            CharacterConflict = $"The story starts when the protagonist {protagonist}.{Environment.NewLine}{Environment.NewLine}Another character is {supportingCharacter} who {conflict}.";
        }

        #endregion Character / Conflict

        public void ChooseAll()
        {
            ChoosePlot();
            ChooseCharacterConflict(false, false, false);
            Backgrounds.Choose();
            Resolutions.Choose();
            Traits.Choose();
            Settings.Choose();
            Themes.Choose();
            Genres.Choose();
        }

        #endregion Randomization
    }
    
    [ProtoContract(AsReferenceDefault = true)]
    public class PlotElementNoteOption : NoteOption
    {
        public PlotElementNoteOption() : base() { }

        public PlotElementNoteOption(string name) : base(name) { }

        protected override void SetChecked(bool value)
        {
            // If no children are checked, make a choice.
            if (ChildOptions.Count > 0 && !ChildOptions.Any(c => c.IsChecked))
                Choose();

            if (Parent != null)
            {
                // Checking causes the parent to expand and check.
                Parent.IsExpanded = true;
                Parent.IsChecked = true;

                // Single-select choices uncheck all siblings when checked.
                // Elements only uncheck other elements (not standard child options).
                if (Parent is PlotArchetypeNoteOption && Parent.IsChoice && !Parent.IsMultiSelect && !Parent.IsManualMultiSelect)
                {
                    foreach (var sibling in ((PlotArchetypeNoteOption)Parent).ElementOptions.Where(s => s != this))
                        sibling.IsChecked = false;
                }
            }
        }
    }
    
    [ProtoContract(AsReferenceDefault = true)]
    public class PlotArchetypeNoteOption : NoteOption
    {
        public ObservableCollection<NoteOption> ElementOptions { get; set; } = new ObservableCollection<NoteOption>();
        [ProtoMember(1, OverwriteList = true)]
        private IList<NoteOption> ElementOptionList
        {
            get { return ElementOptions; }
            set { ElementOptions = new ObservableCollection<NoteOption>(value); }
        }

        public PlotArchetypeNoteOption() : base() { }

        public PlotArchetypeNoteOption(string name) : base(name) { }

        public void AddElement(NoteOption child)
        {
            child.Parent = this;
            ElementOptions.Add(child);
        }

        public override bool Choose(bool append = false)
        {
            bool madeSelection = ChooseFromList(ChildOptions, append);
            madeSelection = madeSelection || ChooseFromList(ElementOptions, append);
            return madeSelection;
        }

        public void ChooseElement() => ChooseFromList(ElementOptions);
    }

    /// <summary>
    /// Represents a collection of Element <see cref="NoteOption"/>s as a string.
    /// </summary>
    public sealed class PlotElementConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<NoteOption> elementOptions = value as ObservableCollection<NoteOption>;
            if (elementOptions == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (var element in elementOptions)
            {
                if (sb.Length > 0) sb.Append("; ");
                sb.Append(element.Name);
            }
            return sb.ToString();
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
