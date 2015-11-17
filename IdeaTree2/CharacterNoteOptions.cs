using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace IdeaTree2
{
    [ProtoContract(AsReferenceDefault = true)]
    [ProtoInclude(100, typeof(CharacterGenderOption))]
    [ProtoInclude(200, typeof(CharacterRaceOption))]
    [ProtoInclude(300, typeof(CharacterAgeOption))]
    [ProtoInclude(400, typeof(CharacterOrientationOption))]
    [ProtoInclude(500, typeof(CharacterRelationshipOption))]
    public class CharacterNoteOption : NoteOption
    {
        public CharacterNoteOption() : base() { }

        public CharacterNoteOption(string name) : base(name) { }

        protected override bool DoesModifierApply(NoteOptionModifier modifier)
        {
            bool baseApplies = base.DoesModifierApply(modifier);
            bool match = false;
            if (RootIdea?.GetType() == typeof(CharacterNote) &&
                modifier.GetType() == typeof(NoteOptionCharacterModifier))
            {
                CharacterNote host = (CharacterNote)RootIdea;
                NoteOptionCharacterModifier cModifier = (NoteOptionCharacterModifier)modifier;

                // Does not match if a gender is specified, but the character's gender (or its archetype) doesn't match
                if (cModifier.Genders.Count > 0 && (host.EffectiveGender == null ||
                    (!cModifier.Genders.Contains(host.EffectiveGender.Name) &&
                    !cModifier.Genders.Contains(host.EffectiveGender.Archetype))))
                    return false;
                else match = true;

                // Does not match if a race is specified, but the character's race doesn't match
                if (cModifier.Races.Count > 0)
                {
                    bool found = false;
                    foreach (var race in cModifier.Races)
                    {
                        if (host.Races.FindOption(race)?.IsChecked == true) found = true;
                    }
                    if (!found) return false;
                    match = true;
                }

                // Does not match if an age range is specified, but the character's age is outside the range
                if (cModifier.MinAge.HasValue && host.AgeYears < cModifier.MinAge) return false;
                if (cModifier.MaxAge.HasValue && host.AgeYears > cModifier.MaxAge) return false;
                if (cModifier.MinAge.HasValue || cModifier.MaxAge.HasValue) match = true;
            }

            // If the modifier had character-specific conditions which were met, and omits the usual target, it is considered a match
            if (string.IsNullOrEmpty(modifier.TargetPath) && match) return true;

            // Otherwise, use the usual matching criteria only.
            return baseApplies;
        }
    }
    
    [ProtoContract(AsReferenceDefault = true)]
    public class CharacterGenderOption : CharacterNoteOption
    {
        /// <summary>
        /// The Name of the gender Archetype considered to be the "opposite sex" with respect to this one.
        /// May be null.
        /// </summary>
        /// <remarks>
        /// Used to determine the appropriate gender to fit a given character's orientation.
        /// </remarks>
        [ProtoMember(1)]
        public string Opposite { get; set; }

        /// <summary>
        /// The Name of a <see cref="CharacterGenderOption"/> considered to be the "base" gender associated with this one.
        /// Typically either "Woman" or "Man". May be null for genders which don't adhere more closely to either common gender norm.
        /// </summary>
        /// <remarks>
        /// Used to generate a gender-appropriate name, and to determine correct orientation matches for randomized couples.
        /// </remarks>
        /// <example>
        /// For instance, a gender of "Male-to-Female Transsexual" might have "Woman" as its Archetype, indicating that "Woman" is the
        /// "base" gender which most closely reflects its expressed traits.
        /// </example>
        [ProtoMember(2)]
        public string Archetype { get; set; }

        public static string womanArchetype = "Woman";
        public static string manArchetype = "Man";

        [Browsable(false)]
        public bool IsFeminine => Archetype == womanArchetype;
        [Browsable(false)]
        public bool IsMasculine => Archetype == manArchetype;

        public CharacterGenderOption() : base() { }

        public CharacterGenderOption(string name) : base(name) { }

        public CharacterGenderOption GetArchetypeClone(string archetype)
        {
            CharacterGenderOption clone = (CharacterGenderOption)MemberwiseClone();
            clone.ChildOptions = new ObservableCollection<NoteOption>();
            foreach (CharacterGenderOption child in ChildOptions.Where(c => c is CharacterGenderOption &&
                (((CharacterGenderOption)c).Archetype == archetype || string.IsNullOrEmpty(((CharacterGenderOption)c).Archetype))))
                clone.AddChild(child.GetArchetypeClone(archetype));
            return clone;
        }
    }
    
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class CharacterRaceOption : CharacterNoteOption
    {
        private string maleNameFile;
        /// <summary>
        /// The path of a file containing names appropriate for masculine characters of this heritage.
        /// </summary>
        [DisplayName("Male Name File")]
        public string MaleNameFile
        {
            get
            {
                if (!string.IsNullOrEmpty(maleNameFile)) return maleNameFile;
                else if (Parent != null && Parent is CharacterRaceOption)
                    return ((CharacterRaceOption)Parent).MaleNameFile;
                else return null;
            }
            set { maleNameFile = value; }
        }

        [ProtoIgnore]
        public string RootedMaleNameFile => System.IO.Path.IsPathRooted(MaleNameFile) ? MaleNameFile : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MaleNameFile);

        private string femaleNameFile;
        /// <summary>
        /// The path of a file containing names appropriate for feminine characters of this heritage.
        /// </summary>
        /// <remarks>
        /// Also used as the default when a gender is not marked as either feminine or masculine.
        /// </remarks>
        [DisplayName("Female Name File")]
        public string FemaleNameFile
        {
            get
            {
                if (!string.IsNullOrEmpty(femaleNameFile)) return femaleNameFile;
                else if (Parent != null && Parent is CharacterRaceOption)
                    return ((CharacterRaceOption)Parent).FemaleNameFile;
                else return null;
            }
            set { femaleNameFile = value; }
        }

        [ProtoIgnore]
        public string RootedFemaleNameFile => System.IO.Path.IsPathRooted(FemaleNameFile) ? FemaleNameFile : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FemaleNameFile);

        private string surnameFile;
        /// <summary>
        /// The path of a file containing surnames appropriate for characters of this heritage.
        /// </summary>
        [DisplayName("Surname File")]
        public string SurnameFile
        {
            get
            {
                if (!string.IsNullOrEmpty(surnameFile)) return surnameFile;
                else if (Parent != null && Parent is CharacterRaceOption)
                    return ((CharacterRaceOption)Parent).SurnameFile;
                else return null;
            }
            set { surnameFile = value; }
        }

        [ProtoIgnore]
        public string RootedSurnameFile => System.IO.Path.IsPathRooted(SurnameFile) ? SurnameFile : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SurnameFile);

        /// <summary>
        /// Set this to true if this race should be used to generate random names when the chance of generating a
        /// culturally-based name is less than 100, and the random selection indicates that a cultural name will
        /// not be used.
        /// The default race will also be used as a fallback when random selection fails for some reason.
        /// </summary>
        /// <remarks>
        /// Marking more than one race as default is allowed, but only the first race marked as the default in a template will
        /// actually be used.
        /// </remarks>
        [DisplayName("Is Default")]
        public bool IsDefault { get; set; }

        public CharacterRaceOption() : base() { }

        public CharacterRaceOption(string name) : base(name) { }
    }
    
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class CharacterAgeOption : CharacterNoteOption
    {
        /// <summary>
        /// The minimum limit for this age range.
        /// </summary>
        [DisplayName("Min Age")]
        public int? MinAge { get; set; }

        /// <summary>
        /// The maximum limit for this age range.
        /// </summary>
        [DisplayName("Max Age")]
        public int? MaxAge { get; set; }

        /// <summary>
        /// If this property is set to true, the random family generation procedures
        /// will omit characters of this age range from a randomly generated family tree.
        ///  This does not prevent the age range from being selected when randomly
        /// determining a character's age.
        /// </summary>
        /// <example>
        /// This is used, for example, by the default template to omit elderly
        /// characters from family trees, to avoid the appearance that every character
        /// has extremely long-lived ancestors.
        /// </example>
        [DisplayName("Omit from Family Tree")]
        public bool OmitFromFamilyTree { get; set; }

        public CharacterAgeOption() : base() { }
    }
    
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic)]
    public class CharacterOrientationOption : CharacterNoteOption
    {
        /// <summary>
        /// Indicates that characters with this orientation will have relationships with opposite-gendered characters,
        /// as indicated by the <see cref="CharacterGenderOption.Opposite"/> property of their gender.
        /// </summary>
        /// <example>
        /// For example: a character whose gender is "Man" and whose orientation has this property will have
        /// relationships with characters of the gender "Woman."
        /// </example>
        [DisplayName("Includes Opposite")]
        public bool IncludesOpposite { get; set; }

        /// <summary>
        /// Indicates that characters with this orientation will have relationships with characters whose
        /// genders are similar to their opposite gender.
        /// "Similar" genders are any which share the same <see cref="CharacterGenderOption.Archetype"/>.
        /// </summary>
        /// <remarks>
        /// Overrides <see cref="IncludesOpposite"/> if true.
        /// </remarks>
        /// <example>
        /// For example: a character whose gender is "Man" and whose orientation has this property will have
        /// relationships with characters of any gender which has the "Woman" <see cref="CharacterGenderOption.Archetype"/>.
        /// </example>
        [DisplayName("Includes Similar to Opposite")]
        public bool IncludesSimilarToOpposite { get; set; }

        /// <summary>
        /// Indicates that characters with this orientation will have relationships with characters of the same gender.
        /// "Same" is considered to be the <see cref="CharacterGenderOption.Archetype"/> of a gender, not its exact match.
        /// </summary>
        /// <example>
        /// For example: a character whose gender is "Man" and whose orientation has this property will have
        /// relationships with characters of the gender "Man."
        /// </example>
        [DisplayName("Includes Same")]
        public bool IncludesSame { get; set; }

        /// <summary>
        /// Indicates that characters with this orientation will have relationships with characters whose
        /// genders are similar to their gender.
        /// "Similar" genders are any which share the same <see cref="CharacterGenderOption.Archetype"/>.
        /// </summary>
        /// <remarks>
        /// Overrides <see cref="IncludesSame"/> if true.
        /// </remarks>
        /// <example>
        /// For example: a character whose gender is "Man" and whose orientation has this property will have
        /// relationships with characters of any gender which has the "Man" <see cref="CharacterGenderOption.Archetype"/>.
        /// </example>
        [DisplayName("Includes Similar")]
        public bool IncludesSimilar { get; set; }

        /// <summary>
        /// Indicates that characters with this orientation will have relationships with characters of any gender.
        /// </summary>
        /// <remarks>
        /// Overrides all the other, more-specific properties.
        /// </remarks>
        [DisplayName("Includes Any")]
        public bool IncludesAny { get; set; }

        /// <summary>
        /// A shortcut property, which indicates whether or not this orientation includes multiple options
        /// (or the "Any" option, which includes multiple options by its nature).
        /// </summary>
        [ProtoIgnore]
        [Browsable(false)]
        public bool IncludesMultiple
        {
            get
            {
                if (IncludesAny) return true;
                int count = 0;
                if (IncludesOpposite) count++;
                if (IncludesSimilarToOpposite) count++;
                if (IncludesSame) count++;
                if (IncludesSimilar) count++;
                if (count > 1) return true;
                else return false;
            }
        }

        public CharacterOrientationOption() : base() { }

        public CharacterOrientationOption(string name) : base(name) { }
    }
    
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 4)]
    public class CharacterRelationshipOption : CharacterNoteOption
    {
        private int? minAge;
        /// <summary>
        /// Indicates the minimum age necessary to be in this type of relationship.
        /// </summary>
        [DisplayName("Min Age")]
        public int? MinAge
        {
            get
            {
                if (minAge.HasValue) return minAge;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).MinAge;
                else return null;
            }
            set { minAge = value; }
        }

        private int? maxAge;
        /// <summary>
        /// Indicates the maximum age allowed to be in this type of relationship.
        /// </summary>
        [DisplayName("Max Age")]
        public int? MaxAge
        {
            get
            {
                if (maxAge.HasValue) return maxAge;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).MaxAge;
                else return null;
            }
            set { maxAge = value; }
        }

        private int? minAgeOffset;
        /// <summary>
        /// Indicates the most a character can be younger than the other character to be in this type of relationship.
        /// </summary>
        [DisplayName("Min Age Offset")]
        public int? MinAgeOffset
        {
            get
            {
                if (minAgeOffset.HasValue) return minAgeOffset;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).MinAgeOffset;
                else return null;
            }
            set { minAgeOffset = value; }
        }

        private int? maxAgeOffset;
        /// <summary>
        /// Indicates the most a character can be older than the other character to be in this type of relationship.
        /// </summary>
        [DisplayName("Max Age Offset")]
        public int? MaxAgeOffset
        {
            get
            {
                if (maxAgeOffset.HasValue) return maxAgeOffset;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).MaxAgeOffset;
                else return null;
            }
            set { maxAgeOffset = value; }
        }
        
        private bool isFamily = true;
        /// <summary>
        /// Indicates that this is a familial relationship (including those related by blood or marriage).
        /// </summary>
        [DisplayName("Is Family")]
        public bool IsFamily
        {
            get
            {
                if (isFamily) return isFamily;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).IsFamily;
                else return false;
            }
            set { isFamily = value; }
        }

        private bool isBloodRelative;
        /// <summary>
        /// Indicates that this is a direct familial relationship (including only those related by blood, not by marriage).
        /// </summary>
        [DisplayName("Is Blood Relative")]
        public bool IsBloodRelative
        {
            get
            {
                if (isBloodRelative) return isBloodRelative;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).IsBloodRelative;
                else return false;
            }
            set { isBloodRelative = value; }
        }

        private string reciprocalRelationship;
        /// <summary>
        /// The Path of the Relationship that mirrors this one.
        /// Presumed to be the same Relationship, if left null.
        /// </summary>
        [DisplayName("Reciprocal Relationship")]
        public string ReciprocalRelationship
        {
            get
            {
                if (!string.IsNullOrEmpty(reciprocalRelationship)) return reciprocalRelationship;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                {
                    string parentReciprocal = ((CharacterRelationshipOption)Parent).ReciprocalRelationship;
                    if (!string.IsNullOrEmpty(parentReciprocal)) return parentReciprocal;
                }
                return Name;
            }
            set { reciprocalRelationship = value; }
        }

        private bool alwaysSharesSurname;
        /// <summary>
        /// Indicates that a character with this relationship will have the same Surname as its Parent.
        /// </summary>
        [DisplayName("Always Shares Surname")]
        public bool AlwaysSharesSurname
        {
            get
            {
                if (alwaysSharesSurname) return alwaysSharesSurname;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).AlwaysSharesSurname;
                else return false;
            }
            set { alwaysSharesSurname = value; }
        }

        private bool sharesFeminineSurname;
        /// <summary>
        /// Indicates that a character with this relationship will only have the same Surname as its Parent
        /// if the Parent's gender is classified as feminine.
        /// </summary>
        [DisplayName("Shares Feminine Surname")]
        public bool SharesFeminineSurname
        {
            get
            {
                if (sharesFeminineSurname) return sharesFeminineSurname;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).SharesFeminineSurname;
                else return false;
            }
            set { sharesFeminineSurname = value; }
        }

        private bool sharesMasculineSurname;
        /// <summary>
        /// Indicates that a character with this relationship will only have the same Surname as its Parent
        /// if the Parent's gender is classified as masculine.
        /// </summary>
        [DisplayName("Shares Masculine Surname")]
        public bool SharesMasculineSurname
        {
            get
            {
                if (sharesMasculineSurname) return sharesMasculineSurname;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).SharesMasculineSurname;
                else return false;
            }
            set { sharesMasculineSurname = value; }
        }

        private bool sharesFamilySurname;
        /// <summary>
        /// Indicates that a character with this relationship will only have the same Surname as its Parent
        /// if the Parent's surname is their original surname name (i.e., not a married name).
        /// </summary>
        /// <remarks>
        /// An original surname is assumed to be one which is not derived from a Relationship, or from a blood Relationship.
        /// </remarks>
        [DisplayName("Shares Family Surname")]
        public bool SharesFamilySurname
        {
            get
            {
                if (sharesFamilySurname) return sharesFamilySurname;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).SharesFamilySurname;
                else return false;
            }
            set { sharesFamilySurname = value; }
        }

        private bool requiresOrientationMatch;
        /// <summary>
        /// Indicates that this relationship requires a compatible orientation with the other party.
        /// </summary>
        [DisplayName("Requires Orientation Match")]
        public bool RequiresOrientationMatch
        {
            get
            {
                if (requiresOrientationMatch) return requiresOrientationMatch;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).RequiresOrientationMatch;
                else return false;
            }
            set { requiresOrientationMatch = value; }
        }

        private int? max;
        /// <summary>
        /// The maximum number of this relationship one character should have simultaneously.
        /// </summary>
        /// <remarks>
        /// Note that this does not prevent manually adding more, it only limits the random
        /// generation of characters with this relationship.
        /// Also, this only applies to child Notes of a character. The Relationship a character
        /// has with its own parent Note isn't counted towards this total even if it's the reciprocal
        /// Relationship of this one.
        /// </remarks>
        public int? Max
        {
            get
            {
                if (max.HasValue) return max;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).Max;
                else return null;
            }
            set { max = value; }
        }

        private int? secondWeight;
        /// <summary>
        /// The chance of additional relatives with the same relationship drops off after the first, by default.
        /// Setting this property re-sets the weight to a specific value for the second relative.
        /// </summary>
        /// <example>
        /// This can be used to grant a low initial chance of having a relative of a certain kind, but a high chance
        /// of having more than one when it occurs (for instance, few people have students, but when one does have them,
        /// it is usually more than one, not just a single pupil).
        /// 
        /// It can also be used in the opposite way, to make multiples possible but even more unlikely than the default
        /// drop-off. For instance, it is significantly less likely to have multiple ex-spouses than it is to have just one.
        /// </example>
        [DisplayName("Second Weight")]
        public int? SecondWeight
        {
            get
            {
                if (secondWeight.HasValue) return secondWeight;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).SecondWeight;
                else return null;
            }
            set { secondWeight = value; }
        }

        private int? thirdWeight;
        /// <summary>
        /// The chance of additional relatives with the same relationship drops off after the first, by default.
        /// Setting this property re-sets the weight to a specific value for the third relative.
        /// </summary>
        /// <remarks>
        /// Just like <see cref="SecondWeight"/>, but for the third relationship of this type, rather than the second.
        /// It is possible to set both values, to exercise very fine-grained control over the weights for the first
        /// three relationships.
        /// </remarks>
        [DisplayName("Third Weight")]
        public int? ThirdWeight
        {
            get
            {
                if (thirdWeight.HasValue) return thirdWeight;
                else if (Parent != null && Parent is CharacterRelationshipOption)
                    return ((CharacterRelationshipOption)Parent).ThirdWeight;
                else return null;
            }
            set { thirdWeight = value; }
        }

        [ProtoIgnore]
        public ObservableCollection<string> Genders { get; set; } = new ObservableCollection<string>();
        [ProtoMember(1, OverwriteList = true)]
        private IList<string> GenderList
        {
            get { return Genders; }
            set { Genders = new ObservableCollection<string>(value); }
        }

        [ProtoIgnore]
        public ObservableCollection<string> IncompatibleRelationships { get; set; } = new ObservableCollection<string>();
        [ProtoMember(2, OverwriteList = true)]
        private IList<string> IncompatibleRelationshipList
        {
            get { return IncompatibleRelationships; }
            set { IncompatibleRelationships = new ObservableCollection<string>(value); }
        }

        [ProtoIgnore]
        public ObservableCollection<string> RequiredRelationships { get; set; } = new ObservableCollection<string>();
        [ProtoMember(3, OverwriteList = true)]
        private IList<string> RequiredRelationshipList
        {
            get { return RequiredRelationships; }
            set { RequiredRelationships = new ObservableCollection<string>(value); }
        }

        public CharacterRelationshipOption() : base() { }

        public CharacterRelationshipOption(string name) : base(name) { }

        public CharacterRelationshipOption GetReciprocalRelationship()
        {
            if (string.IsNullOrEmpty(ReciprocalRelationship)) return this;
            return FindOption(ReciprocalRelationship) as CharacterRelationshipOption;
        }
    }
    
    [ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 3)]
    public class NoteOptionCharacterModifier : NoteOptionModifier
    {
        /// <summary>
        /// Indicates the minimum age at which this modifier applies.
        /// </summary>
        [DisplayName("Min Age")]
        public int? MinAge { get; set; }

        /// <summary>
        /// Indicates the maximum age at which this modifier applies.
        /// </summary>
        [DisplayName("Max Age")]
        public int? MaxAge { get; set; }
        
        [ProtoIgnore]
        public ObservableCollection<string> Genders { get; set; } = new ObservableCollection<string>();
        [ProtoMember(1, OverwriteList = true)]
        private IList<string> GenderList
        {
            get { return Genders; }
            set { Genders = new ObservableCollection<string>(value); }
        }

        [ProtoIgnore]
        public ObservableCollection<string> Races { get; set; } = new ObservableCollection<string>();
        [ProtoMember(2, OverwriteList = true)]
        private IList<string> RaceList
        {
            get { return Races; }
            set { Races = new ObservableCollection<string>(value); }
        }

        public NoteOptionCharacterModifier() : base() { }
    }
}
