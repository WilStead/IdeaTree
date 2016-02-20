using IdeaTree2.Properties;
using MonitoredUndo;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace IdeaTree2
{
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class CharacterNote : IdeaNote
    {
        [ProtoIgnore]
        public override string IdeaNoteType => nameof(CharacterNote);

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(Title), title, value);
                SetProperty(ref title, value);
                OnPropertyChanged(nameof(Name));
            }
        }

        private string firstName;
        public string FirstName
        {
            get { return firstName; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(FirstName), firstName, value);
                SetProperty(ref firstName, value);
                OnPropertyChanged(nameof(Name));
            }
        }

        private string surname;
        public string Surname
        {
            get { return surname; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(Surname), surname, value);
                SetProperty(ref surname, value);
                OnPropertyChanged(nameof(Name));
            }
        }

        private string birthName;
        public string BirthName
        {
            get { return birthName; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(BirthName), birthName, value);
                SetProperty(ref birthName, value);
                OnPropertyChanged(nameof(Name));
            }
        }

        private string suffix;
        public string Suffix
        {
            get { return suffix; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(Suffix), suffix, value);
                SetProperty(ref suffix, value);
                OnPropertyChanged(nameof(Name));
            }
        }

        private int ageYears;
        public int AgeYears
        {
            get { return ageYears; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(AgeYears), ageYears, value);
                SetProperty(ref ageYears, value);
                OnPropertyChanged(nameof(AgeString));
            }
        }

        private byte ageMonths;
        public byte AgeMonths
        {
            get { return ageMonths; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(AgeMonths), ageMonths, value);
                SetProperty(ref ageMonths, value);
                OnPropertyChanged(nameof(AgeString));
            }
        }
        
        [ProtoIgnore]
        public string AgeString
        {
            get
            {
                if (AgeYears == 0 && AgeMonths == 0) return string.Empty;
                if (AgeMonths > 0)
                {
                    if (AgeYears > 0)
                    {
                        if (AgeYears < 3)
                            return $" ({(AgeYears * 12) + AgeMonths} mo.)";
                        else return $" ({AgeYears} yr. & {AgeMonths} mo.)";
                    }
                    else return $" ({AgeMonths} mo.)";
                }
                return $" ({AgeYears})";
            }
        }

        #region Randomization Fields

        private static int maxAge = 150;
        private static int ageOfMajority = 18;

        [ProtoIgnore]
        public static int culturalFirstNameChance = 50;
        [ProtoIgnore]
        public static int culturalSurnameChance = 100;

        [ProtoIgnore]
        public static int inheritedMaleFirstNameChance = 3;

        /// <summary>
        /// The odds of marrying a person of the same race are statistically higher than marrying someone of a differing race.
        /// Therefore, before making the usual random selection of race for a newly generated spouse, there is a chance that the
        /// race will be the same as the existing spouse.
        /// </summary>
        private static int spouseSharesRaceChance = 85;

        /// <summary>
        /// The chance to have a relationship of a given type decreases with each
        /// such relationship the character is already in.
        /// </summary>
        private static double multipleRelationshipMultiplier = 0.75d;

        private static int chanceToHaveSingleDad = 5;
        private static int chanceToHaveSingleMom = 5;

        private static int chanceToHaveKidsWhenMarried = 85;
        private static int chanceToHaveKids_SingleDivorcedMom = 50;
        private static int chanceToHaveKids_SingleUndivorcedMom_30to60 = 10;
        private static int chanceToHaveKids_SingleDivorcedDad_30to60 = 10;

        /// <summary>
        /// The children of a woman with an ex-husband have a 75% chance of having his surname rather than hers
        /// (unless children taking their father's name is not enabled in the current Template).
        /// </summary>
        private static int chanceForChildrenToHaveExesSurname = 75;

        /// <summary>
        /// There is a chance that any character, regardless of actual orientation, will be in
        /// a hetero marriage, due to the pressure of social conventions.
        /// </summary>
        private static int chanceOfForcedHeteroMarriage = 10;

        /// <summary>
        /// An orientation which includes any gender is somewhat more likely than usual to have a lover
        /// (of a different gender than their spouse).
        /// </summary>
        private static double bisexualLoverChanceMultiplier = 5;

        /// <summary>
        /// There is an even higher chance that a character who is married to someone
        /// of a gender that does not match their preference will take a lover (of a preferred gender).
        /// </summary>
        private static double homosexualLoverChanceMultiplier = 10;

        /// <summary>
        /// When selecting a lover, a character whose orientation includes multiple genders
        /// has a high chance that their extra-marital relationship will be with someone of
        /// a gender not represented in their marriage.
        /// </summary>
        private static int chanceOfAlternateGenderLoverForBisexual = 85;

        #endregion Randomization Fields

        public CharacterGenderOption Genders { get; set; } = new CharacterGenderOption() { Name = "Genders", IsChoice = true };
        
        [ProtoIgnore]
        public CharacterGenderOption EffectiveGender => Genders.LowestCheckedChild as CharacterGenderOption;

        public CharacterNoteOption Races { get; set; } = new CharacterNoteOption() { Name = "Races", IsChoice = true, IsManualMultiSelect = true };

        private CharacterRelationshipOption relationship;
        public CharacterRelationshipOption Relationship
        {
            get { return relationship; }
            set
            {
                DefaultChangeFactory.Current.OnChanging(this, nameof(Relationship), relationship, value);
                SetProperty(ref relationship, value);
            }
        }
        
        public CharacterNoteOption Traits { get; set; } = new CharacterNoteOption() { Name = "Traits" };

        private static Random random = new Random();

        public CharacterNote() : base()
        {
            Genders.RootIdea = this;
            Races.RootIdea = this;
            Traits.RootIdea = this;
        }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            Genders.RootIdea = this;
            Races.RootIdea = this;
            Traits.RootIdea = this;
        }

        [ProtoBeforeSerialization]
        private void BeforeSerialization()
        {
            Races = (CharacterNoteOption)Races.GetCheckedChildrenClone();
            Traits = (CharacterNoteOption)Traits.GetCheckedChildrenClone();
        }

        public string GetFullName()
        {
            StringBuilder fullName = new StringBuilder();
            if (!string.IsNullOrEmpty(Title)) fullName.Append(Title);
            if (!string.IsNullOrEmpty(FirstName))
            {
                if (fullName.Length > 0) fullName.Append(" ");
                fullName.Append(FirstName);
            }
            if (!string.IsNullOrEmpty(Surname))
            {
                if (fullName.Length > 0) fullName.Append(" ");
                fullName.Append(Surname);
            }
            if (!string.IsNullOrEmpty(Suffix))
            {
                if (fullName.Length > 0) fullName.Append(" ");
                fullName.Append(Suffix);
            }
            return fullName.ToString();
        }

        public override string GetDefaultName()
        {
            string fullName = GetFullName();
            if (string.IsNullOrEmpty(fullName)) return "[Character Note]";
            else return fullName;
        }

        public override TextNote ConvertToTextNote()
        {
            TextNote newNote = new TextNote()
            {
                // default character names will normally be left alone, as they are highly descriptive;
                // they should probably be retained as explicit names after conversion
                ExplicitName = string.IsNullOrEmpty(this.ExplicitName) ? GetDefaultName() : this.ExplicitName,
                IsExpanded = this.IsExpanded,
                IsSelected = this.IsSelected
            };
            newNote.Ideas = Ideas;

            if (Relationship != null)
            {
                StringBuilder sb = new StringBuilder();
                if (Relationship != null)
                    sb.Append($"{Relationship.Name}: ");
                sb.Append(GetDefaultName());
                sb.Append($", {Genders.Name}");
                sb.AppendLine(AgeString);
                newNote.AddParagraph(sb.ToString());
            }
            newNote.AddParagraph(string.Empty);
            foreach (var line in Races.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);
            newNote.AddParagraph(string.Empty);
            foreach (var line in Traits.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);
            newNote.AddParagraph(string.Empty);
            foreach (var line in Traits.GetSummary(0).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                newNote.AddParagraph(line);

            return newNote;
        }

        public override bool ContainsText(string text)
        {
            string matchText = text.ToLowerInvariant();
            return (Name.ToLowerInvariant().Contains(matchText) || Title?.ToLowerInvariant().Contains(matchText) == true ||
                FirstName?.ToLowerInvariant().Contains(matchText) == true || Surname?.ToLowerInvariant().Contains(matchText) == true ||
                AgeYears.ToString().Contains(matchText) || AgeMonths.ToString().Contains(matchText) ||
                Genders.LowestCheckedChild?.Name.ToLowerInvariant().Contains(matchText) == true ||
                Relationship?.Name.ToLowerInvariant().Contains(matchText) == true || Races.ContainsText(matchText) ||
                Traits.ContainsText(matchText));
        }

        #region Randomization

        #region Names

        public void AssignFamilyFirstName()
        {
            // There is a slightly greater likelihood of an inherited name for a male child.
            CharacterGenderOption gender = EffectiveGender;
            if (gender.IsMasculine && random.Next(1, 101) <= inheritedMaleFirstNameChance)
            {
                CharacterNote father = GetFather();
                if (father != null) FirstName = father.FirstName;

                // Apply in reverse when adding a father to a son.
                else if (Relationship.IsInPath("Father") &&
                    ((CharacterNote)Parent).EffectiveGender?.IsMasculine == true)
                    FirstName = ((CharacterNote)Parent).FirstName;

                else NewFirstName(culturalFirstNameChance);

            }
            else NewFirstName(culturalFirstNameChance);

            if (Relationship == null) return;

            // Siblings aren't generally given identical first names.
            while (GetSiblings().Any(s => s.FirstName == FirstName))
                NewFirstName(culturalFirstNameChance);

            if (gender?.IsMasculine == true)
            {
                // If this is an inherited name, append the required suffix to all relatives in the chain.
                int generation = GetNameGeneration(gender, 1, true);
                int totalGenerations = GetNameGeneration(gender, generation, false);
                if (totalGenerations > 1)
                {
                    AssignGenerationalSuffix(gender, generation, true);
                    AssignGenerationalSuffix(gender, generation, false);
                }
            }
        }

        private void AssignFamilySurname()
        {
            if (Relationship == null)
            {
                NewSurname(culturalSurnameChance);
                return;
            }

            CharacterNote parent = Parent as CharacterNote;
            CharacterGenderOption parentGender = parent.EffectiveGender;

            // Share the parent character's surname if the relationship indicates that.
            if (!string.IsNullOrWhiteSpace(parent.Surname))
            {
                if (Relationship.AlwaysSharesSurname)
                {
                    Surname = parent.Surname;
                    if (!Relationship.IsBloodRelative) NewBirthName(culturalSurnameChance);
                    return;
                }
                else if (parentGender?.IsMasculine == true && Relationship.SharesMasculineSurname)
                {
                    Surname = parent.Surname;
                    if (!Relationship.IsBloodRelative) NewBirthName(culturalSurnameChance);
                    return;
                }
                else if (parentGender?.IsFeminine == true && Relationship.SharesFeminineSurname)
                {
                    Surname = parent.Surname;
                    if (!Relationship.IsBloodRelative) NewBirthName(culturalSurnameChance);
                    return;
                }
                else if (Relationship.SharesFamilySurname)
                {
                    string familyName = parent.GetFamilyName();
                    if (!string.IsNullOrEmpty(familyName))
                    {
                        Surname = familyName;
                        if (!Relationship.IsBloodRelative) NewBirthName(culturalSurnameChance);
                        return;
                    }
                    // If a family name should be shared, but the relative doesn't have one (their surname is
                    // adopted from a relative, and their birth name hasn't been determined yet), then a
                    // family name is decided now, and set as the relative's birth name for future reference.
                    NewSurname(culturalSurnameChance);
                    parent.BirthName = Surname;
                    return;
                }
            }

            // The characters' surnames may also be shared in the reverse direction (from this character to its parent).
            CharacterRelationshipOption reciprocal = Relationship.GetReciprocalRelationship();
            CharacterGenderOption gender = EffectiveGender;
            bool shareSurname = (reciprocal.AlwaysSharesSurname || (reciprocal.SharesMasculineSurname && gender?.IsMasculine == true)
                || (reciprocal.SharesFeminineSurname && gender?.IsFeminine == true));
            if (shareSurname)
            {
                // If the parent character has no other relationships, it is assumed that their surname
                // was acquired from this newly-included relationship all along.
                // This may have unexpected results when, e.g. adding a husband after other relatives are
                // already included (the husband will seem to have taken his wife's family name), but will have
                // good results when a family is randomly generated from scratch (since spouses are added first).
                // This will result in a woman's husband being assigned her same surname, then additional
                // relatives of hers will correctly identify it as her married name, and select a different
                // family name for themselves.
                if (!string.IsNullOrWhiteSpace(parent.Surname) && parent.Relationship == null &&
                    parent.Ideas.Count(i => i != this && i.IdeaNoteType == nameof(CharacterNote) &&
                    ((CharacterNote)i).Relationship != null) == 0)
                {
                    Surname = parent.Surname;
                    if (!Relationship.IsBloodRelative) parent.NewBirthName(culturalSurnameChance);
                    return;
                }
                // Otherwise, as long as the parent hasn't already inherited a surname from someone else, they will acquire
                // a new surname from this new relationship.
                else if (!parent.HasAdoptedSurname())
                {
                    NewSurname(culturalSurnameChance);
                    parent.BirthName = parent.Surname;
                    parent.Surname = Surname;
                    return;
                }
            }

            // If none of the above cases were true, this character's surname is not shared with their parent at all,
            // so a random surname is generated.
            NewSurname(culturalSurnameChance);
        }

        private void AssignGenerationalSuffix(CharacterGenderOption gender, int generation, bool up)
        {
            if (generation == 1)
                Suffix = RootSaveFile?.Template?.CharacterTemplate?.SeniorSuffix;
            else if (generation == 2)
                Suffix = RootSaveFile?.Template?.CharacterTemplate?.JuniorSuffix;
            else
            {
                StringBuilder sb = new StringBuilder();
                if (generation >= 5)
                {
                    sb.Append('V');
                    // Will generate bad results for >=9, but a family tree with 9 living generations is unlikely.
                    for (int i = 5; i < generation; i++)
                        sb.Append('I');
                }
                else
                {
                    for (int i = 0; i < generation && i < 3; i++)
                        sb.Append('I');
                    if (generation == 4)
                        sb.Append('V');
                }
                Suffix = sb.ToString();
            }

            IEnumerable<CharacterNote> relatives = up ? GetParents() : GetChildren();
            relatives = relatives.Where(p => p.EffectiveGender?.Archetype == gender.Archetype);
            if (relatives.Any(p => p.FirstName == FirstName && p.Surname == Surname))
            {
                CharacterNote relative = relatives.OrderByDescending(p => p.GetNameGeneration(gender, 0, up)).FirstOrDefault();
                relative?.AssignGenerationalSuffix(gender, up ? generation - 1 : generation + 1, up);
            }
        }

        /// <summary>
        /// A binary search that finds the final name in the collection that has a total weight greater than or
        /// equal to the target.
        /// </summary>
        private static string FindName(List<KeyValuePair<int, string>> nameValues, int value)
        {
            int middle = nameValues.Count / 2;

            // If the chosen pivot either has the exact target weight value, or is greater than the target while
            // its next-lowest neighbor is not, then the target has been found and is returned.
            if (value == nameValues[middle].Key ||
                (value < nameValues[middle].Key && (middle == 0 || value > nameValues[middle - 1].Key)))
                return nameValues[middle].Value;

            // If the target weight is lower than the pivot's range, recursively search the lower part of the list.
            else if (value < nameValues[middle].Key)
                return FindName(nameValues.GetRange(0, middle), value);

            // Otherwise, recursively search the higher part of the list.
            else return FindName(nameValues.GetRange(middle + 1, nameValues.Count - middle - 1), value);
        }

        private CharacterRaceOption GetDefaultRace()
        {
            CharacterRaceOption race = null;
            // Fall back on the current Template's default race.
            race = RootSaveFile?.Template?.CharacterTemplate?.Races?.ChildOptions?.Cast<CharacterRaceOption>().Where(o => o.IsDefault).FirstOrDefault();
            // If the chosen race has any default children, choose from among them, recursively.
            while (race.ChildOptions.Where(c => c is CharacterRaceOption).Cast<CharacterRaceOption>().Any(r => r.IsDefault))
                race = race.ChildOptions.Cast<CharacterRaceOption>().Where(o => o.IsDefault).FirstOrDefault();
            // If none found, fall back on any race.
            if (race == null)
                race = RootSaveFile?.Template?.CharacterTemplate?.Races?.ChildOptions?.Cast<CharacterRaceOption>().FirstOrDefault();
            // If still none found, no race can be chosen.
            if (race == null) return null;
            return race;
        }

        private string GetFamilyName(bool descendingOnly = false)
        {
            // If this character has an explicit birth name, use that.
            if (!string.IsNullOrEmpty(BirthName)) return BirthName;

            // If this character hasn't taken their surname from a non-blood relative,
            // their surname is the family name.
            if (!string.IsNullOrEmpty(Surname) && !HasAdoptedSurname())
                return Surname;

            // In most cases, a birth name should have been set as a side effect of checking
            // for an adopted surname, if they had one. In that case, it can now be returned.
            if (!string.IsNullOrEmpty(BirthName)) return BirthName;

            // Otherwise, no family name can reasonably be determined.
            return null;
        }

        /// <summary>
        /// Interprets text files with lines in the format "Name,1" where the integer following the comma indicates
        /// the relative weight of that name among the choices. A random selection is then made from among the
        /// names in the file, taking into account the given weights.
        /// </summary>
        /// <remarks>
        /// Names with no comma or number following them are assigned a default value of 1. Therefore, a file of
        /// names only will have all equal chances of being chosen.
        /// 
        /// No validation or exception-handling are performed on the file operations. Any errors are expected to be
        /// dealt with by program users in the next execution by modifying and/or selecting valid source files.
        /// </remarks>
        private static string GetName(string file)
        {
            if (!File.Exists(file)) return null;

            int total = 0;

            // Names are stored not with their own weight, but with the total weight of all names read so far, which
            // allows for easy binary searching of the collection once a target value is selected.
            List<KeyValuePair<int, string>> nameValues = new List<KeyValuePair<int, string>>();

            using (StreamReader names = File.OpenText(file))
            {
                while (!names.EndOfStream)
                {
                    string line = names.ReadLine();
                    string[] parts = line.Split(',');

                    if (parts.Length == 0) continue;
                    else
                    {
                        if (parts.Length > 1) total += int.Parse(parts[1]);
                        else total++;

                        nameValues.Add(new KeyValuePair<int, string>(total, parts[0]));
                    }
                }
            }

            return FindName(nameValues, random.Next(total + 1));
        }

        private int GetNameGeneration(CharacterGenderOption gender, int generations, bool up)
        {
            IEnumerable<CharacterNote> relatives = up ? GetParents() : GetChildren();
            relatives = relatives.Where(p => p.EffectiveGender?.Archetype == gender.Archetype);
            if (relatives.Any(p => p.FirstName == FirstName && p.Surname == Surname))
                generations += relatives.Max(p => p.GetNameGeneration(gender, generations, up));

            return generations;
        }

        private CharacterRaceOption GetNominalCulture(int chanceOfCulturalName)
        {
            List<CharacterRaceOption> races = Races.ChildOptions.Where(c => c.IsChecked).Cast<CharacterRaceOption>().ToList();
            CharacterRaceOption race = null;

            // If chance dictates that a cultural name won't be generated, restrict the selection to the race(s) marked as default.
            bool useDefault = false;
            if (random.Next(1, 101) > chanceOfCulturalName) useDefault = true;

            // If the character has no assigned race...
            if (races.Count == 0 || useDefault) race = GetDefaultRace();
            else
            {
                // Each race has an equal chance of being the culture from which the name is drawn.
                race = races[random.Next(races.Count)];
                // If the chosen race has any selected children, choose from among them, recursively.
                while (race.ChildOptions.Any(c => c.IsChecked))
                {
                    races = race.ChildOptions.Cast<CharacterRaceOption>().Where(c => c.IsChecked && (!useDefault || c.IsDefault)).ToList();
                    race = races[random.Next(races.Count)];
                }
            }

            // Ensure that the chosen race has a name file associated with it.
            // If not, search up its parent hierarchy until one is found.
            // (If no name files exist in a race's hierarchy, a null value will be returned.)
            if (EffectiveGender?.IsMasculine == true)
            {
                while (!File.Exists(race?.RootedMaleNameFile) && race?.Parent != null) race = race.Parent as CharacterRaceOption;
            }
            else
            {
                while (!File.Exists(race?.RootedFemaleNameFile) && race?.Parent != null) race = race.Parent as CharacterRaceOption;
            }
            if (race == null) return GetDefaultRace();
            else return race;
        }

        private bool HasAdoptedSurname()
        {
            if (string.IsNullOrEmpty(Surname)) return false;

            if (!string.IsNullOrEmpty(BirthName) && BirthName != Surname) return true;

            // The spouse relationship is treated as a special case, to account for spouses found elsewhere in the family tree,
            // rather than as direct parent or child notes (e.g. a mother and father note of a common parent are assumed to be
            // spouses, despite not having any direct relationship between them).
            // If this is found to be the case, a birth name is assigned, to simplify any future calculations.
            CharacterRelationshipOption spouseRelationship =
                RootSaveFile?.Template?.CharacterTemplate?.Relationships?.FindOption("Significant Other\\Spouse") as CharacterRelationshipOption;
            if (spouseRelationship != null && !spouseRelationship.IsBloodRelative)
            {
                if (spouseRelationship.AlwaysSharesSurname)
                {
                    NewBirthName(culturalSurnameChance);
                    return true;
                }
                else
                {
                    List<CharacterNote> spouses = GetSpouses();
                    if (spouseRelationship.SharesMasculineSurname && spouses.Any(s => s.EffectiveGender?.IsMasculine == true && s.Surname == Surname))
                    {
                        NewBirthName(culturalSurnameChance);
                        return true;
                    }
                    if (spouseRelationship.SharesFeminineSurname && spouses.Any(s => s.EffectiveGender?.IsFeminine == true && s.Surname == Surname))
                    {
                        NewBirthName(culturalSurnameChance);
                        return true;
                    }
                }
            }

            return false;
        }

        public void NewBirthName(int chanceOfCulturalName)
        {
            CharacterRaceOption race = GetNominalCulture(chanceOfCulturalName);
            BirthName = GetName(race?.RootedSurnameFile);
        }

        public void NewFirstName(int chanceOfCulturalName)
        {
            CharacterRaceOption race = GetNominalCulture(chanceOfCulturalName);
            FirstName = GetName(EffectiveGender?.IsMasculine == true ? race?.RootedMaleNameFile : race?.RootedFemaleNameFile);
        }

        public void NewSurname(int chanceOfCulturalName)
        {
            CharacterRaceOption race = GetNominalCulture(chanceOfCulturalName);
            Surname = GetName(race?.RootedSurnameFile);
        }

        #endregion Names

        #region Family

        #region Existing Relationship Info

        private List<CharacterNote> GetParents()
        {
            List<CharacterNote> parents = new List<CharacterNote>();

            if (Relationship?.IsInPath("Child") == true) parents.Add((CharacterNote)Parent);

            foreach (CharacterNote relative in Ideas.Where(i =>
                i.IdeaNoteType == nameof(CharacterNote) && ((CharacterNote)i).Relationship != null))
            {
                if (relative.Relationship.IsInPath("Parent")) parents.Add(relative);
                else if (relative.Relationship.IsInPath("Sibling")) parents.AddRange(relative.GetParents());
            }

            return parents;
        }

        private CharacterNote GetParent(string genderArchetype)
        {
            List<CharacterNote> parents = new List<CharacterNote>();

            if (Relationship?.IsInPath("Child") == true &&
                ((CharacterNote)Parent).EffectiveGender.Archetype == genderArchetype)
                return (CharacterNote)Parent;

            foreach (CharacterNote relative in Ideas.Where(i =>
                i.IdeaNoteType == nameof(CharacterNote) && ((CharacterNote)i).Relationship != null))
            {
                if (relative.Relationship.IsInPath("Parent") &&
                    relative.EffectiveGender.Archetype == genderArchetype) return relative;
            }

            foreach (CharacterNote relative in Ideas.Where(i =>
                i.IdeaNoteType == nameof(CharacterNote) && ((CharacterNote)i).Relationship != null))
            {
                if (relative.Relationship.IsInPath("Sibling"))
                {
                    CharacterNote parent = relative.GetParent(genderArchetype);
                    if (parent != null) return parent;
                }
            }

            return null;
        }

        private CharacterNote GetMother() => GetParent(CharacterGenderOption.manArchetype);

        private CharacterNote GetFather() => GetParent(CharacterGenderOption.manArchetype);

        private List<CharacterNote> GetChildren()
        {
            List<CharacterNote> children = new List<CharacterNote>();

            if (Relationship?.IsInPath("Parent") == true) children.Add((CharacterNote)Parent);

            foreach (CharacterNote relative in Ideas.Where(i =>
                i.IdeaNoteType == nameof(CharacterNote) && ((CharacterNote)i).Relationship != null))
            {
                if (relative.Relationship.IsInPath("Child")) children.Add(relative);
                else if (relative.Relationship.IsInPath("Spouse")) children.AddRange(relative.GetChildren());
            }

            return children;
        }

        private List<CharacterNote> GetSiblings()
        {
            List<CharacterNote> siblings = new List<CharacterNote>();

            if (Relationship?.IsInPath("Sibling") == true) siblings.Add((CharacterNote)Parent);

            foreach (CharacterNote relative in Ideas.Where(i =>
                i.IdeaNoteType == nameof(CharacterNote) && ((CharacterNote)i).Relationship != null))
            {
                if (relative.Relationship.IsInPath("Sibling")) siblings.Add(relative);
                else if (relative.Relationship.IsInPath("Parent")) siblings.AddRange(relative.GetChildren());
            }

            return siblings;
        }

        private bool IsMarried()
        {
            if (Relationship?.IsInPath("Spouse") == true) return true;

            if (Ideas.Any(i =>
                i.IdeaNoteType == nameof(CharacterNote) &&
                ((CharacterNote)i).Relationship?.IsInPath("Spouse") == true))
                return true;

            return GetChildren().SelectMany(c => c.GetParents()).Any(p => p != this);
        }

        private List<CharacterNote> GetSpouses()
        {
            List<CharacterNote> spouses = new List<CharacterNote>();

            if (Relationship?.IsInPath("Spouse") == true) spouses.Add((CharacterNote)Parent);

            spouses.AddRange(Ideas.Where(i =>
                i.IdeaNoteType == nameof(CharacterNote) &&
                ((CharacterNote)i).Relationship?.IsInPath("Spouse") == true).Cast<CharacterNote>());

            spouses.AddRange(GetChildren().SelectMany(c => c.GetParents()));

            spouses = spouses.Distinct().Where(s => s != this).ToList();

            return spouses;
        }

        private List<CharacterNote> GetExSpouses()
        {
            List<CharacterNote> exSpouses = new List<CharacterNote>();

            if (Relationship?.IsInPath("Ex-Spouse") == true) exSpouses.Add((CharacterNote)Parent);

            exSpouses.AddRange(Ideas.Where(i =>
                i.IdeaNoteType == nameof(CharacterNote) &&
                ((CharacterNote)i).Relationship?.IsInPath("Ex-Spouse") == true).Cast<CharacterNote>());

            return exSpouses;
        }

        private int CountRelationship(CharacterRelationshipOption relationship)
        {
            int count = 0;

            if (Relationship?.Path.StartsWith(relationship.ReciprocalRelationship) == true) count++;

            count += Ideas.Count(i => i.IdeaNoteType == nameof(CharacterNote) && ((CharacterNote)i).Relationship.Path.StartsWith(relationship.Path));

            return count;
        }

        private bool HasAnyRelationships(IEnumerable<string> relationships)
        {
            if (Relationship != null &&
                relationships.Any(r => Relationship.ReciprocalRelationship.StartsWith(r) == true)) return true;

            if (Ideas.Any(i => i.IdeaNoteType == nameof(CharacterNote) &&
                relationships.Any(r =>((CharacterNote)i).Relationship.Path.StartsWith(r)))) return true;

            // Special cases for spouses, siblings, and children: these might be found elsewhere in the
            // family tree (rather than as direct parent or child notes).
            if (relationships.Any(r => r.StartsWith("Significant Other\\Spouse")) && IsMarried())
                return true;
            if (relationships.Any(r => r.StartsWith("Sibling")) && GetSiblings().Count > 0)
                return true;
            if (relationships.Any(r => r.StartsWith("Child")) && GetChildren().Count > 0)
                return true;

            return false;
        }

        private bool HasAllRelationships(IEnumerable<string> relationships)
        {
            Dictionary<string, bool> matrix = new Dictionary<string, bool>();
            foreach (string relationship in relationships)
            {
                matrix.Add(relationship, false);

                // Special cases for spouses, siblings, and children: these might be found elsewhere in the
                // family tree (rather than as direct parent or child notes).
                if (relationship.StartsWith("Significant Other\\Spouse") && IsMarried())
                    matrix[relationship] = true;
                if (relationship.StartsWith("Sibling") && GetSiblings().Count > 0)
                    matrix[relationship] = true;
                if (relationship.StartsWith("Child") && GetChildren().Count > 0)
                    matrix[relationship] = true;
            }

            foreach (string relationship in relationships)
            {
                if (Relationship?.ReciprocalRelationship?.StartsWith(relationship) == true)
                    matrix[relationship] = true;

                if (Ideas.Any(i => i.IdeaNoteType == nameof(CharacterNote) &&
                    ((CharacterNote)i).Relationship.Path.StartsWith(relationship)))
                    matrix[relationship] = true;
            }

            return !matrix.Any(r => !r.Value);
        }

        #endregion Existing Relationship Info

        #region Significant Others

        private bool CheckForRelationship(CharacterRelationshipOption relationship, int numAlready)
        {
            if (numAlready >= relationship.Max) return false;

            if (AgeYears < relationship?.MinAge) return false;
            if (AgeYears > relationship?.MaxAge) return false;

            if (HasAnyRelationships(relationship.IncompatibleRelationships) ||
                !HasAllRelationships(relationship.RequiredRelationships))
                return false;

            bool force = false;
            int weight;
            if (numAlready > 1 && relationship.SecondWeight.HasValue)
                weight = relationship.SecondWeight.Value;
            else if (numAlready > 2 && relationship.ThirdWeight.HasValue)
                weight = relationship.ThirdWeight.Value;
            else weight = relationship.GetEffectiveWeight(out force);
            bool chosen = random.Next(1, 101) <= (weight * Math.Pow(multipleRelationshipMultiplier, numAlready));
            return force || chosen;
        }

        private bool CheckForSpouse(CharacterRelationshipOption relationship, int numAlready)
        {
            // Parents' spouses (or lack thereof) are determined by their relative, and not generated at this level.
            if (Relationship?.IsInPath("Parent") == true) return false;
            
            if (relationship != null) return CheckForRelationship(relationship, numAlready);
            else return false;
        }

        private bool CheckForExSpouse(CharacterRelationshipOption relationship, int numAlready)
        {
            // The character must be at least the minimum age for a spouse, plus 3 years for every ex
            // (including the proposed new one) and also including an additional 3 years if the
            // character is currently married. This allows for a reasonable amount of time for each
            // marriage, divorce, and remarriage.
            if (AgeYears < (18 + (3 * (numAlready + (IsMarried() ? 2 : 1)))))
                return false;

            if (relationship != null) return CheckForRelationship(relationship, numAlready);
            else return false;
        }

        private bool CheckForLover(CharacterRelationshipOption relationship, int numAlready)
        {
            if (numAlready >= relationship.Max) return false;
            
            if (AgeYears < relationship?.MinAge) return false;
            if (AgeYears > relationship?.MaxAge) return false;

            if (HasAnyRelationships(relationship.IncompatibleRelationships) ||
                !HasAllRelationships(relationship.RequiredRelationships))
                return false;
            
            bool force = false;
            int weight;
            if (numAlready > 1 && relationship.SecondWeight.HasValue)
                weight = relationship.SecondWeight.Value;
            else if (numAlready > 2 && relationship.ThirdWeight.HasValue)
                weight = relationship.ThirdWeight.Value;
            else weight = relationship.GetEffectiveWeight(out force);

            NoteOption orientationOption = Traits.FindOption("Personality\\Orientation");
            CharacterOrientationOption orientation = orientationOption.LowestCheckedChild as CharacterOrientationOption;
            if (orientation != null)
            {
                // An orientation which includes any gender is somewhat more likely than usual to have a lover
                // (of a different gender than their spouse).
                if (orientation?.IncludesAny == true) weight = (int)Math.Round(weight * bisexualLoverChanceMultiplier);
                else
                {
                    List<CharacterNote> spouses = GetSpouses();
                    bool orientationSatisfied = false;
                    CharacterGenderOption gender = EffectiveGender;
                    foreach (CharacterNote spouse in spouses)
                    {
                        CharacterGenderOption partnerGender = spouse.EffectiveGender;
                        if (gender == null || partnerGender == null) continue;
                        
                        if (orientation.IncludesSame && gender.Archetype == partnerGender.Name)
                        {
                            orientationSatisfied = true;
                            break;
                        }
                        if (orientation.IncludesSimilar && gender.Archetype == partnerGender.Archetype)
                        {
                            orientationSatisfied = true;
                            break;
                        }
                        if (orientation.IncludesOpposite && gender.Opposite != null && gender.Opposite == partnerGender.Name)
                        {
                            orientationSatisfied = true;
                            break;
                        }
                        if (orientation.IncludesSimilarToOpposite && gender.Opposite != null && gender.Opposite == partnerGender.Archetype)
                        {
                            orientationSatisfied = true;
                            break;
                        }
                    }
                    // There is an even higher chance that a character who is married to someone
                    // of a gender that does not match their preference will take a lover (of a preferred gender).
                    if (!orientationSatisfied) weight = (int)Math.Round(weight * homosexualLoverChanceMultiplier);
                }
            }

            bool chosen = random.Next(1, 101) <= weight * Math.Pow(multipleRelationshipMultiplier, numAlready);
            return force || chosen;
        }

        private bool SetOrientationMatchedGender(CharacterNote partner, CharacterOrientationOption partnerOrientation)
        {
            CharacterGenderOption partnerGender = partner.EffectiveGender;
            if (partnerGender == null) return false;

            List<CharacterGenderOption> candidateGenders = new List<CharacterGenderOption>();
            if (partnerOrientation.IncludesSame)
                candidateGenders.AddRange(Genders.FindOptionsByName(new List<NoteOption>(), partnerGender.Archetype).Cast<CharacterGenderOption>());
            if (partnerOrientation.IncludesSimilar)
                candidateGenders.AddRange(Genders.ChildOptions.Cast<CharacterGenderOption>().Where(g => g.Archetype == partnerGender.Archetype));
            if (partnerOrientation.IncludesOpposite && partnerGender.Opposite != null)
                candidateGenders.AddRange(Genders.FindOptionsByName(new List<NoteOption>(), partnerGender.Opposite).Cast<CharacterGenderOption>());
            if (partnerOrientation.IncludesSimilarToOpposite && partnerGender.Opposite != null)
                candidateGenders.AddRange(Genders.ChildOptions.Cast<CharacterGenderOption>().Where(g => g.Archetype == partnerGender.Opposite));
            
            if (candidateGenders.Count == 0) return false;

            // Use a dummy NoteOption so that the weighted choice algorithms may be leveraged.
            NoteOption genderOptions = new NoteOption() { RootIdea = this, ChildOptions = new ObservableCollection<NoteOption>(candidateGenders), IsChoice = true };
            genderOptions.Choose();

            return true;
        }

        private bool SetSignificantOtherGender(CharacterNote partner, CharacterRelationshipOption relationship)
        {
            // Ensure gender options are up to date.
            if (RootSaveFile?.Template?.CharacterTemplate?.Genders != null)
                Genders.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Genders, false);

            // There is a chance that any character, regardless of actual orientation, will be in
            // a hetero marriage (or ex-marriage), due to the pressure of social conventions.
            if (relationship.Path.Contains("Spouse") && random.Next(101) <= chanceOfForcedHeteroMarriage)
            {
                CharacterGenderOption gender = Genders.FindOptionsByName(new List<NoteOption>(), partner.EffectiveGender?.Opposite).FirstOrDefault() as CharacterGenderOption;
                if (gender != null)
                {
                    gender.IsChecked = true;
                    return true;
                }
            }

            // Otherwise, select a gender that corresponds to the character's orientation.
            NoteOption orientationOption = partner.Traits.FindOption("Personality\\Orientation");
            CharacterOrientationOption partnerOrientation = orientationOption.LowestCheckedChild as CharacterOrientationOption;
            if (partnerOrientation != null)
            {
                // When selecting a lover, a character whose orientation includes multiple genders
                // has a high chance that their extra-marital relationship will be with someone of
                // a gender not represented in their marriage.
                if (relationship.IsInPath("Lover") && partnerOrientation.IncludesMultiple &&
                    random.Next(101) <= chanceOfAlternateGenderLoverForBisexual)
                {
                    IEnumerable<CharacterGenderOption> spouseGenders = partner.GetSpouses().Select(s => s.EffectiveGender);
                    List<CharacterGenderOption> candidateGenders =
                        Genders.ChildOptions.Cast<CharacterGenderOption>().Where(g => !spouseGenders.Any(s => s.Archetype == g.Archetype)).ToList();
                    if (candidateGenders.Count > 0)
                    {
                        // Use a dummy NoteOption so that the weighted choice algorithms may be leveraged.
                        NoteOption genderOptions = new NoteOption() { RootIdea = this, ChildOptions = new ObservableCollection<NoteOption>(candidateGenders), IsChoice = true };
                        genderOptions.Choose();
                        return true;
                    }
                }
                return SetOrientationMatchedGender(partner, partnerOrientation);
            }
            return false;
        }

        private void GetSignificantOtherAgeRange(CharacterGenderOption partnerGender, out int? minimum, out int? maximum)
        {
            minimum = null;
            maximum = null;

            // The conventional "half + 7" rule is applied to find a preliminary minimum.
            minimum = (int)Math.Floor((AgeYears / 2.0) + 7);
            // A preliminary maximum is calculated by reversing the formula.
            maximum = (int)Math.Ceiling((AgeYears - 7) * 2.0);

            // Age differences among significant others follow a predictable distribution, based partially on gender.
            CharacterGenderOption gender = EffectiveGender;
            if ((gender?.IsMasculine == true && partnerGender?.IsFeminine == true) ||
                (gender?.IsFeminine == true && partnerGender?.IsMasculine == true))
            {
                int val = random.Next(1, 101);
                int maleMinOffset = 0;
                int? maleMaxOffset = null;
                if (val <= 1) maleMinOffset = 20;
                else if (val <= 3)
                {
                    maleMinOffset = 15;
                    maleMaxOffset = 19;
                }
                else if (val <= 7)
                {
                    maleMinOffset = 10;
                    maleMaxOffset = 14;
                }
                else if (val <= 19)
                {
                    maleMinOffset = 6;
                    maleMaxOffset = 9;
                }
                else if (val <= 32)
                {
                    maleMinOffset = 4;
                    maleMaxOffset = 5;
                }
                else if (val <= 53)
                {
                    maleMinOffset = 2;
                    maleMaxOffset = 3;
                }
                else if (val <= 86)
                {
                    maleMinOffset = -1;
                    maleMaxOffset = 1;
                }
                else if (val <= 93)
                {
                    maleMinOffset = -3;
                    maleMaxOffset = -2;
                }
                else if (val <= 96)
                {
                    maleMinOffset = -5;
                    maleMaxOffset = -4;
                }
                else if (val <= 99)
                {
                    maleMinOffset = -9;
                    maleMaxOffset = -6;
                }
                else
                {
                    maleMinOffset = -15;
                    maleMaxOffset = -10;
                }
                if (gender.IsMasculine)
                {
                    if (maleMaxOffset.HasValue)
                        minimum = Math.Max(minimum ?? 0, AgeYears - maleMaxOffset.Value);
                    maximum = Math.Min(maximum ?? maxAge, AgeYears - maleMinOffset);
                }
                else
                {
                    minimum = Math.Max(minimum ?? 0, AgeYears + maleMinOffset);
                    if (maleMaxOffset.HasValue)
                        maximum = Math.Min(maximum ?? maxAge, AgeYears + maleMaxOffset.Value);
                }
            }

            // As a special consideration for significant others, relationships
            // on opposite sides of the age of consent are not generated.
            if (AgeYears >= ageOfMajority) minimum = Math.Max(minimum ?? 0, ageOfMajority);
            else maximum = Math.Min(maximum ?? maxAge, ageOfMajority - 1);
        }

        private bool GenerateSignificantOther(CharacterRelationshipOption relationship, int numAlready)
        {
            if (relationship.IsInPath("Spouse"))
            {
                if (!CheckForSpouse(relationship, numAlready)) return false;
            }
            else if (relationship.IsInPath("Ex-Spouse"))
            {
                if (!CheckForExSpouse(relationship, numAlready)) return false;
            }
            else if (relationship.IsInPath("Lover"))
            {
                if (!CheckForLover(relationship, numAlready)) return false;
            }
            else if (!CheckForRelationship(relationship, numAlready)) return false;

            CharacterNote newNote = new CharacterNote();
            AddChild(newNote);
            newNote.Relationship = relationship;
            
            if (!relationship.RequiresOrientationMatch || !newNote.SetSignificantOtherGender(this, relationship))
                newNote.ChooseGender();

            int? minimum, maximum;
            GetSignificantOtherAgeRange(newNote.EffectiveGender, out minimum, out maximum);
            if (!newNote.ChooseAge(false, minimum, maximum)) // False return means no valid age range exists for the relationship (min > max).
            {
                Ideas.Remove(newNote);
                return false;
            }

            newNote.SetInheritedRaces();
            newNote.AssignFamilySurname();
            newNote.AssignFamilyFirstName();

            if (RootSaveFile?.Template?.CharacterTemplate?.Traits != null)
                newNote.Traits.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Traits, true);
            newNote.Traits.Choose();

            // There is a chance that a character of any orientation will be in a
            // heterosexual marriage (or ex-marriage), due to societal pressure.
            // Otherwise, the new character's orientation will be adjusted, if necessary, to
            // fit the relationship.
            if (relationship.RequiresOrientationMatch &&
                (!relationship.Path.Contains("Spouse") ||
                random.Next(101) > chanceOfForcedHeteroMarriage ||
                EffectiveGender?.Archetype != newNote.EffectiveGender?.Opposite))
                newNote.ReconcileOrientation(this, newNote.Relationship);

            return true;
        }

        private void GenerateSignificantOthers(CharacterRelationshipOption relationship)
        {
            int numAlready = CountRelationship(relationship);
            while (GenerateSignificantOther(relationship, numAlready)) numAlready++;
        }

        #endregion Significant Others

        public void AddFamily(int depth = 0, bool immediateOnly = false)
        {
            // Do not generate families for non-blood relatives beyond those of the original character.
            if (depth > 1 && Relationship?.IsBloodRelative == false) return;

            // Only immediate family are generated for non-blood relatives beyond those of the original character.
            immediateOnly = immediateOnly || (depth == 1 && Relationship?.IsBloodRelative == false);
            GenerateFamily(immediateOnly);

            foreach (CharacterNote relative in
                Ideas.Where(i => i.IdeaNoteType == nameof(CharacterNote) && ((CharacterNote)i).Relationship != null))
                relative.AddFamily(depth + 1, immediateOnly);
        }

        public CharacterNote AddNewFamilyMember(CharacterRelationshipOption relationship, bool manual)
        {
            if (relationship == null) return null;

            CharacterNote newNote = new CharacterNote();
            AddChild(newNote);

            // First see if the relationship specified is a gender-neutral option with gender-specific child options.
            // If so, choose a gender first, then select the appropriate child relationship.
            bool genderChosen = false;
            if (relationship.Genders?.Count == 0 &&
                relationship.ChildOptions.Cast<CharacterRelationshipOption>().Count(c => c.Genders?.Count > 0) > 1)
            {
                newNote.SetRelationshipMatchingGender(this, relationship);
                genderChosen = true;
            }
            // If no gender-specific options were selected, the choice will
            // fall back on the gender-neutral original below.

            if (newNote.Relationship == null) newNote.Relationship = relationship;

            // False return means no valid age range exists for the relationship (min > max).
            // Ignored when adding manually.
            if (!newNote.ChooseAge() && !manual)
            {
                Ideas.Remove(newNote);
                return null;
            }

            if (!genderChosen) newNote.ChooseGender();
            newNote.SetInheritedRaces();
            newNote.AssignFamilySurname();
            newNote.AssignFamilyFirstName();

            if (RootSaveFile?.Template?.CharacterTemplate?.Traits != null)
                newNote.Traits.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Traits, true);
            newNote.Traits.Choose();
            if (newNote.Relationship.RequiresOrientationMatch)
            {
                newNote.ReconcileOrientation(this, newNote.Relationship);
                CharacterRelationshipOption reciprocalRelationship =
                    RootSaveFile.Template.CharacterTemplate.Relationships.FindOption(newNote.Relationship.ReciprocalRelationship) as CharacterRelationshipOption;
                ReconcileOrientation(newNote, reciprocalRelationship);
            }

            return newNote;
        }

        public void AddNonFamilyAssociates()
        {
            if (RootSaveFile?.Template?.CharacterTemplate?.Relationships == null) return;

            int emptyCycleCount = 0; // Avoids making excessive attempts as probabilities diminish.
            int associateCount = 0;
            while (emptyCycleCount < 3 && associateCount < Settings.Default.MaxAssociates)
            {
                emptyCycleCount++;

                foreach (var nonFamilyRelationship in
                    RootSaveFile.Template.CharacterTemplate.Relationships.ChildOptions.Cast<CharacterRelationshipOption>().Where(r => !r.IsFamily))
                {
                    // Odds go down by 3/4 for each of the same relationship already possessed.
                    if (random.Next(1, 101) <= nonFamilyRelationship.Weight * Math.Pow(0.75d, CountRelationship(nonFamilyRelationship)) &&
                        !HasAnyRelationships(nonFamilyRelationship.IncompatibleRelationships) &&
                        HasAllRelationships(nonFamilyRelationship.RequiredRelationships))
                    {
                        CharacterNote associate = AddNewFamilyMember(nonFamilyRelationship, false);
                        if (associate != null)
                        {
                            associateCount++;
                            emptyCycleCount = 0;
                            associate.AddFamily(1, true);
                        }
                    }
                    if (associateCount >= Settings.Default.MaxAssociates) break;
                }
            }
        }

        private void CalculateChildAgeRange(CharacterRelationshipOption childRelationship, out int minChildAge, out int maxChildAge)
        {
            minChildAge = int.MinValue;
            maxChildAge = int.MaxValue;

            if (childRelationship == null) return;

            if (childRelationship.MinAgeOffset.HasValue) minChildAge = AgeYears + childRelationship.MinAgeOffset.Value;
            if (childRelationship.MaxAgeOffset.HasValue) maxChildAge = AgeYears + childRelationship.MaxAgeOffset.Value;

            if (!EffectiveGender.IsFeminine) // Non-women determine their children's possible ages from their female spouse, if possible.
            {
                IEnumerable<CharacterNote> wives = GetSpouses().Where(s => s.EffectiveGender.IsFeminine).Cast<CharacterNote>();
                if (wives.Count() > 0)
                {
                    if (childRelationship.MinAgeOffset.HasValue) minChildAge = wives.Min(w => w.AgeYears) + childRelationship.MinAgeOffset.Value;
                    if (childRelationship.MaxAgeOffset.HasValue) maxChildAge = wives.Max(w => w.AgeYears) + childRelationship.MaxAgeOffset.Value;
                }
                else // If no female spouses are found, adjust the range already set from this character by the likely offsets for the missing mother.
                {
                    CharacterRelationshipOption wifeRelationship =
                        RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Wife") as CharacterRelationshipOption;
                    if (wifeRelationship != null)
                    {
                        minChildAge += wifeRelationship.MinAgeOffset ?? 0;
                        maxChildAge += wifeRelationship.MaxAgeOffset ?? 0;
                    }
                }
            }
        }

        private void CalculateSiblingAgeRange(out int minSiblingAge, out int maxSiblingAge)
        {
            minSiblingAge = int.MinValue;
            maxSiblingAge = int.MaxValue;

            CharacterRelationshipOption childRelationship =
                RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Child") as CharacterRelationshipOption;
            if (childRelationship == null) return;

            CharacterNote mother = GetMother();
            CharacterNote father = GetFather();
            if (mother != null)
            {
                if (childRelationship.MinAgeOffset.HasValue) minSiblingAge = mother.AgeYears + childRelationship.MinAgeOffset.Value;
                if (childRelationship.MaxAgeOffset.HasValue) maxSiblingAge = mother.AgeYears + childRelationship.MaxAgeOffset.Value;
            }
            else if (father != null) // Extrapolate minimum and maximum possible ex-wife/widow's ages from father's age.
            {
                if (childRelationship.MinAgeOffset.HasValue) minSiblingAge = father.AgeYears + childRelationship.MinAgeOffset.Value;
                if (childRelationship.MaxAgeOffset.HasValue) maxSiblingAge = father.AgeYears + childRelationship.MaxAgeOffset.Value;
                CharacterRelationshipOption wifeRelationship =
                    RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Wife") as CharacterRelationshipOption;
                if (wifeRelationship != null)
                {
                    minSiblingAge += wifeRelationship.MinAgeOffset ?? 0;
                    maxSiblingAge += wifeRelationship.MaxAgeOffset ?? 0;
                }
            }
            // Extrapolate minimum and maximum possible mother's info from age.
            else
            {
                if (childRelationship.MinAgeOffset.HasValue) minSiblingAge = AgeYears - childRelationship.MaxAgeOffset.Value + childRelationship.MinAgeOffset.Value;
                if (childRelationship.MaxAgeOffset.HasValue) maxSiblingAge = AgeYears - childRelationship.MinAgeOffset.Value + childRelationship.MaxAgeOffset.Value;
            }
        }

        private List<CharacterNote> GenerateChildren(bool woman, int minChildAge, int maxChildAge,
            bool married, bool divorced, bool siblings, int numAlready, bool manual)
        {
            List<CharacterNote> children = new List<CharacterNote>();

            CharacterRelationshipOption relationship =
                RootSaveFile.Template.CharacterTemplate.Relationships.FindOption(siblings ? "Sibling" : "Child") as CharacterRelationshipOption;
            if (relationship == null) return children;

            // Constrain the age bounds. If there are no valid ages, stop now.
            minChildAge = Math.Max(0, minChildAge);
            maxChildAge = Math.Min(maxAge, maxChildAge);
            if (minChildAge > maxChildAge) return children;

            // If there are already some children, and this isn't manual selection of more, decide whether to generate more before proceeding.
            if (!manual)
            {
                if (numAlready >= relationship.Max) return children;
                else if (numAlready == 1) { if (random.Next(4) == 0) return children; }
                else if (numAlready > 1 && random.Next(1, (2 * (numAlready - 1)) + 1) != 1) return children;
            }

            // Decide whether to stop now based on the character's situation.
            // If the user is manually adding children, don't make these determinations.
            // If determining siblings there's at least 1 child already (the character), so these checks can be skipped.
            if (!manual && !siblings && !ShouldHaveChildren(relationship, woman, married, divorced)) return children;
            
            int childAge;
            bool first = true;
            int numTwins = 0;
            int numChildren = numAlready;
            while (maxChildAge >= 0 && maxChildAge >= minChildAge)
            {
                childAge = random.Next(minChildAge, maxChildAge + 1);
                if (!first)
                {
                    if (childAge == maxChildAge)
                    {
                        // First time, 1/4 chance of allowing twins; second, 1/8; third, 1/16; &c.
                        // Triplets, quadruplets, &c have the same increasingly small chance of being allowed.
                        if (random.Next(4 * (numTwins + 1)) != 0)
                        {
                            maxChildAge--;
                            continue;
                        }
                        else numTwins++;
                    }
                    // There's a 50% chance of adjusting a 1-year separation to 2, to simulate the relative
                    // rarity of such closely-spaced children.
                    else if (childAge == maxChildAge - 1 && random.Next(2) == 0)
                    {
                        childAge--;
                        if (childAge < 0) return children;
                    }
                }

                CharacterNote newNote = new CharacterNote();
                AddChild(newNote);

                // Pick a child relationship (gender-specific variant)
                relationship.DeselectAllChildren();
                relationship.Choose();
                newNote.Relationship = relationship.LowestCheckedChild as CharacterRelationshipOption;

                newNote.AgeYears = childAge;
                if (newNote.AgeYears == 0) newNote.AgeMonths = (byte)random.Next(13);

                newNote.ChooseGender();
                newNote.SetInheritedRaces();
                newNote.AssignFamilySurname();
                newNote.AssignFamilyFirstName();

                if (RootSaveFile?.Template?.CharacterTemplate?.Traits != null)
                    newNote.Traits.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Traits, true);
                newNote.Traits.Choose();

                children.Add(newNote);

                first = false;
                maxChildAge = childAge;
                numChildren++;
                if (numChildren >= relationship.Max) return children;
                // The chances of having multiple kids follows a different progression than the standard for other relationships:
                // Once a couple has a single child, there is a 3 in 4 chance that they will have a second.
                else if (numChildren == 1) { if (random.Next(1, 101) <= 25) return children; }
                // After two, the chances of having additional children drop off rapidly,
                // starting at 1 in 2 for a third, then 1 in 4 for a fourth, 1 in 6 for a fifth, etc.
                else if (random.Next(1, (2 * (numChildren - 1)) + 1) != 1) return children;
            }
            return children;
        }

        public void GenerateChildren(bool manual)
        {
            // If the character is a spouse or parent, children (or lack thereof) are defined by their relative, and not generated at this level.
            // This is ignored when the user is manually adding children directly to a character.
            if (!manual && (Relationship?.IsInPath("Spouse") == true || Relationship?.IsInPath("Parent") == true))
                return;

            CharacterRelationshipOption childRelationship =
                RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Child") as CharacterRelationshipOption;

            int minChildAge, maxChildAge;
            CalculateChildAgeRange(childRelationship, out minChildAge, out maxChildAge);

            List<CharacterNote> exes = GetExSpouses();
            CharacterNote exHusband = null;
            if (exes.Count > 0) exHusband = exes.FirstOrDefault(e => e.EffectiveGender.IsMasculine);
            // The children of a woman with an ex-husband have a 75% chance of having his surname rather than hers
            // (unless children taking their father's name is not enabled in the current Template).
            bool useExSurname = exHusband != null && EffectiveGender.IsFeminine &&
                (childRelationship?.AlwaysSharesSurname == true || childRelationship?.SharesMasculineSurname == true) &&
                random.Next(1, 101) <= chanceForChildrenToHaveExesSurname;
            if (useExSurname)
            {
                foreach (CharacterNote child in
                    GenerateChildren(EffectiveGender.IsFeminine, minChildAge, maxChildAge, IsMarried(), exes.Count > 0, false, GetChildren().Count, manual))
                {
                    child.Surname = exHusband.Surname;
                    // Once the chance fails, use stops from then on, since younger children would not resume
                    // using the surname after the first child born after the end of the marriage.
                    if (random.Next(1, 101) > chanceForChildrenToHaveExesSurname) break;
                }
            }
            else GenerateChildren(EffectiveGender.IsFeminine, minChildAge, maxChildAge, IsMarried(), exes.Count > 0, false, GetChildren().Count, manual);
        }

        private void GenerateFamily(bool immediateOnly)
        {
            if (RootSaveFile?.Template?.CharacterTemplate?.Relationships == null) return;

            foreach (CharacterRelationshipOption relationship in
                RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Significant Other").ChildOptions)
                GenerateSignificantOthers(relationship);

            GenerateChildren(false);

            if (!immediateOnly || AgeYears < ageOfMajority)
            {
                GenerateParents();
                GenerateSiblings(false);
            }
        }

        /// <summary>
        /// Allows generating a parent for this character as if generating a spouse for one of their existing parents.
        /// This allows generating correct gender and orientation matching for between the 'spouses.'
        /// </summary>
        /// <param name="spouse">The character to be used as the hypothetical spouse.</param>
        private void GenerateParentAsSpouse(CharacterNote spouse)
        {
            if (RootSaveFile?.Template?.CharacterTemplate?.Relationships == null) return;
            CharacterRelationshipOption relationship =
                RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Parent") as CharacterRelationshipOption;
            CharacterRelationshipOption spouseRelationship =
                RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Significant Other\\Spouse") as CharacterRelationshipOption;
            if (relationship == null || spouseRelationship == null) return;

            CharacterNote newNote = new CharacterNote();
            AddChild(newNote);
            newNote.Relationship = relationship;

            int? spouseMinAge = spouseRelationship.MinAge;
            if (spouseRelationship.MinAgeOffset.HasValue)
                spouseMinAge = Math.Max(spouseMinAge ?? int.MinValue, spouse.AgeYears + spouseRelationship.MinAgeOffset.Value);
            int? spouseMaxAge = spouseRelationship.MaxAge;
            if (spouseRelationship.MaxAgeOffset.HasValue)
                spouseMaxAge = Math.Min(spouseMaxAge ?? int.MaxValue, spouse.AgeYears + spouseRelationship.MaxAgeOffset.Value);
            if (!newNote.ChooseAge(false, spouseMinAge, spouseMaxAge)) // False return means no valid age range exists for the relationship (min > max).
            {
                Ideas.Remove(newNote);
                return;
            }
            
            if (!spouseRelationship.RequiresOrientationMatch || !newNote.SetSignificantOtherGender(spouse, spouseRelationship))
                newNote.ChooseGender();

            newNote.SetInheritedRaces();
            newNote.AssignFamilySurname();
            newNote.AssignFamilyFirstName();

            if (RootSaveFile?.Template?.CharacterTemplate?.Traits != null)
                newNote.Traits.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Traits, true);
            newNote.Traits.Choose();

            // There is a chance that a character of any orientation will be in a
            // heterosexual marriage (or ex-marriage), due to societal pressure.
            // Otherwise, the new character's orientation will be adjusted, if necessary, to
            // fit the relationship.
            if (spouseRelationship.RequiresOrientationMatch &&
                (random.Next(101) > chanceOfForcedHeteroMarriage ||
                spouse.EffectiveGender?.Archetype != newNote.EffectiveGender?.Opposite))
                newNote.ReconcileOrientation(spouse, spouseRelationship);
        }

        private void GenerateParents()
        {
            // If the character is a child or sibling, parents (or lack thereof) are defined by their relative, and not generated at this level.
            if (Relationship?.IsInPath("Child") == true || Relationship?.IsInPath("Sibling") == true)
                return;

            bool generateMother = random.Next(1, 101) > chanceToHaveSingleDad;
            bool generateFather = random.Next(1, 101) > chanceToHaveSingleMom;

            // Account for any existing parents.
            CharacterNote mother = GetMother();
            if (mother != null) generateMother = false;
            CharacterNote father = GetFather();
            if (father != null) generateFather = false;

            if (!generateMother && !generateFather && mother == null && father == null)
            {
                // Kids under 18 get at least 1 parent, and those over 18 have a
                // 50% chance of having a single parent rather than no parent at all.
                if (AgeYears < 18 || random.Next(1, 101) <= 50)
                {
                    if (random.Next(chanceToHaveSingleDad + chanceToHaveSingleMom) < chanceToHaveSingleDad)
                        generateFather = true;
                    else generateMother = true;
                }
            }

            if (generateMother)
            {
                // N.B. generating the new parent as a spouse of the existing parent may result in a different gender
                // than the one which would be indicated by the names of the variables, if the current parent's orientation
                // indicates this outcome. This is considered correct operation.
                if (father != null) GenerateParentAsSpouse(father);
                else AddNewFamilyMember(RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Parent\\Mother") as CharacterRelationshipOption, false);
            }
            if (generateFather)
            {
                if (mother != null) GenerateParentAsSpouse(mother);
                else AddNewFamilyMember(RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Parent\\Father") as CharacterRelationshipOption, false);
            }
        }

        public void GenerateSiblings(bool manual)
        {
            // If the character is a child or sibling, siblings (or lack thereof) are defined by their relative, and not generated at this level.
            // This is ignored when the user is manually adding siblings directly to a character.
            if (!manual && (Relationship?.IsInPath("Child") == true || Relationship?.IsInPath("Sibling") == true))
                return;

            int minSiblingAge, maxSiblingAge;
            CalculateSiblingAgeRange(out minSiblingAge, out maxSiblingAge);

            GenerateChildren(true, minSiblingAge, maxSiblingAge, true, false, true, GetSiblings().Count, manual);
        }

        private void ReconcileOrientation(CharacterNote partner, CharacterRelationshipOption relationship)
        {
            NoteOption orientationOptions = Traits.FindOption("Personality\\Orientation");
            if (orientationOptions == null) return; // No adjustment necessary/possible without any orientation traits.
            
            CharacterOrientationOption orientation = orientationOptions.LowestCheckedChild as CharacterOrientationOption;
            if (orientation == null || orientation.IncludesAny == true) return; // No adjustment necessary; any gender is accepted.

            CharacterGenderOption gender = EffectiveGender;
            CharacterGenderOption partnerGender = partner.EffectiveGender;
            if (gender == null || partnerGender == null) return; // No adjustment possible without gender information.
            
            List<NoteOption> candidateOrientations = new List<NoteOption>();
            if (gender.Opposite == partnerGender.Archetype && !orientation.IncludesOpposite && !orientation.IncludesSimilarToOpposite)
                candidateOrientations.AddRange(orientationOptions.ChildOptions.Cast<CharacterOrientationOption>().Where(o =>
                    o.IncludesOpposite || o.IncludesSimilarToOpposite || o.IncludesAny));
            else if (gender.Archetype == partnerGender.Archetype && !orientation.IncludesSame && !orientation.IncludesSimilar)
                candidateOrientations.AddRange(orientationOptions.ChildOptions.Cast<CharacterOrientationOption>().Where(o =>
                    o.IncludesSame || o.IncludesSimilar || o.IncludesAny));
            else if (gender.Opposite == partnerGender.Name && !orientation.IncludesOpposite)
                candidateOrientations.AddRange(orientationOptions.ChildOptions.Cast<CharacterOrientationOption>().Where(o => o.IncludesOpposite || o.IncludesAny));
            else if (gender.Archetype == partnerGender.Name && !orientation.IncludesSame)
                candidateOrientations.AddRange(orientationOptions.ChildOptions.Cast<CharacterOrientationOption>().Where(o => o.IncludesSame || o.IncludesAny));
            if (candidateOrientations.Count > 0)
            {
                // Use a dummy NoteOption so that the weighted choice algorithms may be leveraged.
                NoteOption options = new NoteOption() { RootIdea = this, ChildOptions = new ObservableCollection<NoteOption>(candidateOrientations), IsChoice = true };
                orientationOptions.DeselectAllChildren();
                options.Choose();
            }
        }

        public void SetInheritedRaces()
        {
            CharacterNoteOption inheritedRaces = new CharacterNoteOption() { Name = "Races", RootIdea = this, IsChoice = true, IsManualMultiSelect = true };
            List<CharacterNote> parents = GetParents();
            if (parents.Count != 0)
            {
                foreach (var races in parents.Select(p => p.Races))
                    inheritedRaces.MergeWithOption(races, true, true);
            }
            else
            {
                // Without a parent to go by, siblings are a good alternate to determine the family heritage.
                List<CharacterNote> siblings = GetSiblings();
                if (siblings.Count != 0)
                {
                    foreach (var races in siblings.Select(p => p.Races))
                        inheritedRaces.MergeWithOption(races, true, true);
                }
                else
                {
                    // Without parents or siblings, children are the only possible fallback, although this will
                    // obviously be flawed, not accounting for situations of mixed heritage.
                    List<CharacterNote> children = GetChildren();
                    if (children.Count != 0)
                    {
                        foreach (var races in children.Select(p => p.Races))
                            inheritedRaces.MergeWithOption(races, true, true);
                    }
                    // The odds of marrying a person of the same race are statistically higher than marrying someone of a differing race.
                    // Therefore, before making the usual random selection of race for a newly generated spouse, there is a chance that the
                    // race will be the same as the existing spouse.
                    else
                    {
                        List<CharacterNote> spouses = GetSpouses();
                        if (random.Next(1, 101) < spouseSharesRaceChance && spouses.Count > 0)
                        {
                            // If there is more than one, choose one at random.
                            CharacterNote chosenSpouse = spouses[random.Next(spouses.Count)];
                            inheritedRaces.MergeWithOption(chosenSpouse.Races, true, true);
                        }
                        else
                        {
                            // Otherwise, a random assignment must be made.
                            inheritedRaces.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Races, true);
                            inheritedRaces.Choose();
                        }
                    }
                }
            }
            Races = inheritedRaces;
            OnPropertyChanged(nameof(Races));
        }

        private void SetRelationshipMatchingGender(CharacterNote partner, CharacterRelationshipOption relationship)
        {
            // If the relationship is orientation-specific, pick an appropriate gender.
            bool choseGender = false;
            if (relationship.RequiresOrientationMatch)
            {
                NoteOption orientationOptions = partner.Traits.FindOption("Personality\\Orientation");
                CharacterOrientationOption partnerOrientation = orientationOptions.LowestCheckedChild as CharacterOrientationOption;
                choseGender = SetOrientationMatchedGender(partner, partnerOrientation);
                // A choice must only be forced if the partner has an orientation, and it is gender-specific.
                if (partnerOrientation != null && !partnerOrientation.IncludesAny)
                {
                    // Ensure gender options are up to date.
                    if (RootSaveFile?.Template?.CharacterTemplate?.Genders != null)
                        Genders.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Genders, false);

                    List<CharacterGenderOption> genders = new List<CharacterGenderOption>();
                    if (partnerOrientation.IncludesSimilarToOpposite)
                        genders.AddRange(Genders.GetArchetypeClone(partner.EffectiveGender.Opposite).ChildOptions.Cast<CharacterGenderOption>());
                    else if (partnerOrientation.IncludesOpposite)
                        genders.AddRange(Genders.FindOptionsByName(new List<NoteOption>(), partner.EffectiveGender.Opposite).Cast<CharacterGenderOption>());
                    if (partnerOrientation.IncludesSimilar)
                        genders.AddRange(Genders.GetArchetypeClone(partner.EffectiveGender.Archetype).ChildOptions.Cast<CharacterGenderOption>());
                    else if (partnerOrientation.IncludesSame)
                        genders.AddRange(Genders.FindOptionsByName(new List<NoteOption>(), partner.EffectiveGender.Archetype).Cast<CharacterGenderOption>());

                    if (genders.Count > 0)
                    {
                        // Use a dummy NoteOption so that the weighted choice algorithms may be leveraged.
                        NoteOption genderOptions = new NoteOption() { RootIdea = this, ChildOptions = new ObservableCollection<NoteOption>(genders), IsChoice = true };
                        genderOptions.Choose();
                        Genders.FindOption(genderOptions.LowestCheckedChild.Path).IsChecked = true;
                        choseGender = true;
                    }
                }
            }
            // Otherwise, choose one at random.
            if (!choseGender) ChooseGender();
            foreach (CharacterRelationshipOption childRelationship in relationship.ChildOptions)
            {
                if (childRelationship.Genders.Any(g => g == EffectiveGender.Archetype))
                {
                    Relationship = childRelationship;
                    return;
                }
            }
            return;
        }

        private bool ShouldHaveChildren(CharacterRelationshipOption relationship, bool woman, bool married, bool divorced)
        {
            bool force;
            int weight = relationship.GetEffectiveWeight(out force);
            if (force) return true;

            // Certain situations will override the usual weight.
            if (married)
            {
                if (random.Next(1, 101) > chanceToHaveKidsWhenMarried) return false;
            }

            else if (divorced && woman) // 50% chance of being a single divorced mom.
            {
                if (random.Next(1, 101) > chanceToHaveKids_SingleDivorcedMom) return false;
            }

            else if (AgeYears >= 30 && AgeYears <= 60)
            {
                if (woman) // 10% chance of being a single mom (not divorced) between ages 30 and 60.
                {
                    if (random.Next(1, 101) > chanceToHaveKids_SingleUndivorcedMom_30to60)
                        return false;
                }
                else if (divorced) // 10% chance of being a single divorced dad between ages 30 and 60.
                {
                    if (random.Next(1, 101) > chanceToHaveKids_SingleDivorcedDad_30to60)
                        return false;
                }
            }

            // In other situations, the usual weight is used.
            else if (random.Next(101) > weight) return false;

            return true;
        }

        #endregion Family

        private bool ChooseAge(NoteOption ageOptions, bool manual = false, int? minimum = 0, int? maximum = null)
        {
            ageOptions.Choose();

            CharacterAgeOption age = (CharacterAgeOption)ageOptions.ChildOptions.FirstOrDefault(a => a.IsChecked);
            if (age == null) return false;

            if (!manual && age.OmitFromFamilyTree) return false;

            int min = minimum ?? int.MinValue;
            int max = maximum ?? int.MaxValue;
            min = Math.Max(min, age.MinAge ?? min);
            max = Math.Min(max, age.MaxAge ?? max);

            if (min > max) return false;

            int years = random.Next(min, max + 1);
            if (years < 0 || years > maxAge) return false;
            AgeYears = years;

            if (AgeYears == 0) AgeMonths = (byte)random.Next(1, 12);
            else AgeMonths = 0;

            return true;
        }

        public bool ChooseAge(bool manual = false, int? secondaryMin = null, int? secondaryMax = null, bool relationshipRestrictions = false)
        {
            if (RootSaveFile?.Template?.CharacterTemplate?.Ages == null) return false;

            int min = int.MinValue;
            int max = int.MaxValue;
            if (relationshipRestrictions)
            {
                if (Relationship?.MinAge.HasValue == true) min = Relationship.MinAge.Value;
                if (Relationship?.MinAgeOffset.HasValue == true)
                    min = Math.Max(min, ((CharacterNote)Parent).AgeYears + Relationship.MinAgeOffset.Value);

                if (Relationship?.MaxAge.HasValue == true) max = Relationship.MaxAge.Value;
                if (Relationship?.MaxAgeOffset.HasValue == true)
                    max = Math.Min(max, ((CharacterNote)Parent).AgeYears + Relationship.MaxAgeOffset.Value);

                if (Relationship?.IsInPath("Child") == true)
                {
                    CharacterRelationshipOption childRelationship =
                        RootSaveFile.Template.CharacterTemplate.Relationships.FindOption("Child") as CharacterRelationshipOption;
                    ((CharacterNote)Parent).CalculateChildAgeRange(childRelationship, out min, out max);
                }
                else if (Relationship?.IsInPath("Sibling") == true) CalculateSiblingAgeRange(out min, out max);
                else if (Relationship?.IsInPath("Significant Other") == true)
                {
                    int? minimum, maximum;
                    ((CharacterNote)Parent).GetSignificantOtherAgeRange(EffectiveGender, out minimum, out maximum);
                    if (minimum.HasValue) min = minimum.Value;
                    if (maximum.HasValue) max = maximum.Value;
                }
            }

            if (secondaryMin.HasValue) min = Math.Max(min, secondaryMin.Value);
            if (secondaryMax.HasValue) max = Math.Min(max, secondaryMax.Value);

            List<CharacterAgeOption> ages = RootSaveFile.Template.CharacterTemplate.Ages.ChildOptions.Cast<CharacterAgeOption>().Where(a => a.MinAge <= max && a.MaxAge >= min).ToList();
            if (ages.Count == 0) return false;

            // Use a dummy NoteOption so that the weighted choice algorithms may be leveraged.
            NoteOption ageOptions = new NoteOption() { RootIdea = this, ChildOptions = new ObservableCollection<NoteOption>(ages), IsChoice = true };
            return ChooseAge(ageOptions, manual, min, max);
        }

        public void ChooseGender()
        {
            // Ensure gender options are up to date.
            if (RootSaveFile?.Template?.CharacterTemplate?.Genders != null)
                Genders.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Genders, false);

            if (Relationship == null)
            {
                Genders.Choose();
                return;
            }

            List<CharacterGenderOption> genders = new List<CharacterGenderOption>();
            foreach (var gender in Relationship.Genders)
            {
                List<CharacterGenderOption> matches = Genders.GetArchetypeClone(gender).ChildOptions.Cast<CharacterGenderOption>().ToList();
                if (matches.Count > 0) genders = genders.Concat(matches).ToList();
            }
            // If the relationship doesn't specify gender, select from among all of them.
            if (genders.Count == 0)
                genders = new List<CharacterGenderOption>(Genders.ChildOptions.Where(c =>
                    c is CharacterGenderOption).Cast<CharacterGenderOption>());

            // Use a dummy NoteOption so that the weighted choice algorithms may be leveraged.
            NoteOption genderOptions = new NoteOption() { RootIdea = this, ChildOptions = new ObservableCollection<NoteOption>(genders), IsChoice = true };
            genderOptions.Choose();
            Genders.FindOption(genderOptions.LowestCheckedChild.Path).IsChecked = true;
        }

        public void ChooseTraits(bool relationshipRestrictions, bool append = false)
        {
            Traits.Choose(append);
            if (relationshipRestrictions && Relationship != null && Relationship.RequiresOrientationMatch)
                ReconcileOrientation(Parent as CharacterNote, Relationship);
        }

        public void RandomizeAll(bool relationshipRestrictions, int chanceOfCulturalFirstName, int chanceOfCulturalSurname)
        {
            if (relationshipRestrictions)
            {
                ChooseAge();
                ChooseGender();
                SetInheritedRaces();
            }
            else
            {
                if (RootSaveFile?.Template?.CharacterTemplate?.Genders != null)
                    Genders.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Genders, false);
                Genders.Choose();

                if (RootSaveFile?.Template?.CharacterTemplate?.Races != null)
                    Races.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Races, true);
                Races.Choose();

                ChooseAge(RootSaveFile.Template.CharacterTemplate.Ages, true);
            }

            NewFirstName(chanceOfCulturalFirstName);
            NewSurname(chanceOfCulturalSurname);

            Traits.Choose();
            if (relationshipRestrictions && Relationship != null && Relationship.RequiresOrientationMatch)
                ReconcileOrientation(Parent as CharacterNote, Relationship);
        }

        public void SetDefaultRace()
        {
            if (RootSaveFile?.Template?.CharacterTemplate?.Races != null)
                Races.MergeWithOption(RootSaveFile.Template.CharacterTemplate.Races, true);
            CharacterRaceOption defaultRace = Races.ChildOptions.Where(r => r is CharacterRaceOption).Cast<CharacterRaceOption>().Where(r => r.IsDefault).FirstOrDefault();
            if (defaultRace != null)
            {
                CharacterRaceOption defaultChild = null;
                do
                {
                    defaultChild = defaultRace.ChildOptions.Where(r => r is CharacterRaceOption).Cast<CharacterRaceOption>().Where(r => r.IsDefault).FirstOrDefault();
                    if (defaultChild != null) defaultRace = defaultChild;
                } while (defaultChild != null);
                defaultRace.Choose(); // Selects the lowest race marked default; selects one of its children at random, if none of them are marked default.
            }
            else Races.ChildOptions.FirstOrDefault()?.Choose(); // If there is no default, select the first race (and choose randomly from among its children).
        }

        #endregion Randomization
    }

    /// <summary>
    /// Represents a <see cref="CharacterRelationshipOption"/> as its Name string.
    /// </summary>
    public sealed class RelationshipNameConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CharacterRelationshipOption relationship = value as CharacterRelationshipOption;
            if (relationship == null) return string.Empty;
            return $"{relationship.Name}: ";
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a <see cref="CharacterGenderOption"/> as a color based on its Archetype.
    /// </summary>
    public sealed class GenderColorConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CharacterGenderOption gender = value as CharacterGenderOption;
            if (gender?.Name.Equals(CharacterGenderOption.womanArchetype, StringComparison.CurrentCultureIgnoreCase) ?? false)
                return Brushes.DarkMagenta;
            else if (gender?.Name.Equals(CharacterGenderOption.manArchetype, StringComparison.CurrentCultureIgnoreCase) ?? false)
                return Brushes.DarkBlue;
            else if (gender?.IsFeminine ?? false) return Brushes.Purple;
            else if (gender?.IsMasculine ?? false) return Brushes.DarkViolet;
            else return Brushes.Black;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public sealed class BirthNameBoolConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string birthName = value as string;
            if (string.IsNullOrEmpty(birthName)) return false;
            else return true;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class RelationshipBoolConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CharacterNote character = value as CharacterNote;
            return character?.Relationship != null;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
