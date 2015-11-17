using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;

namespace IdeaTree2
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class StoryTemplate
    {
        public NoteOption PlotArchetypes { get; set; } = new NoteOption() { Name = "PlotArchetypes", IsChoice = true };

        public List<string> Protagonists { get; set; } = new List<string>();

        public List<string> SupportingCharacters { get; set; } = new List<string>();

        public List<string> Conflicts { get; set; } = new List<string>();
        
        public NoteOption Backgrounds { get; set; } = new NoteOption() { Name = "Backgrounds", IsChoice = true, IsManualMultiSelect = true };
        
        public NoteOption Resolutions { get; set; } = new NoteOption() { Name = "Resolutions", IsChoice = true, IsManualMultiSelect = true };
        
        public NoteOption Traits { get; set; } = new NoteOption() { Name = "Traits" };
        
        public NoteOption Settings { get; set; } = new NoteOption() { Name = "Settings", IsChoice = true, IsManualMultiSelect = true };
        
        public NoteOption Themes { get; set; } = new NoteOption() { Name = "Themes", IsChoice = true, IsMultiSelect = true };
        
        public NoteOption Genres { get; set; } = new NoteOption() { Name = "Genres", IsChoice = true, IsMultiSelect = true };

        public StoryTemplate() { }

        public NoteOption FindOption(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string root = NoteOption.GetFirstPathNode(path);
            if (root == PlotArchetypes.Name) return PlotArchetypes.FindOption(path);
            else if (root == Backgrounds.Name) return Backgrounds.FindOption(path);
            else if (root == Resolutions.Name) return Resolutions.FindOption(path);
            else if (root == Traits.Name) return Traits.FindOption(path);
            else if (root == Settings.Name) return Settings.FindOption(path);
            else if (root == Themes.Name) return Themes.FindOption(path);
            else if (root == Genres.Name) return Genres.FindOption(path);
            else return null;
        }

        private static string GetConflictString(string protagonist, string supportingCharacter, string conflict)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("The story starts when the protagonist {0}.{1}{1}Another character is {2} who {3}.");
            if (!string.IsNullOrWhiteSpace(protagonist))
                sb.AppendLine($"The story starts when the protagonist {protagonist}.");
            if (!string.IsNullOrWhiteSpace(supportingCharacter))
                sb.Append($"Another character is {supportingCharacter}");
            if (!string.IsNullOrWhiteSpace(conflict))
                sb.AppendLine($" who {conflict}. ");
            else sb.AppendLine(". ");
            return sb.ToString();
        }
    }
    
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CharacterTemplate
    {
        public List<string> Titles { get; set; } = new List<string>();

        public List<string> Suffixes { get; set; } = new List<string>();

        /// <summary>
        /// The suffix used by default for a child with the exact same name as its parent.
        /// </summary>
        public string JuniorSuffix = "Jr.";

        /// <summary>
        /// The suffix used by default for a parent with the exact same name as its child.
        /// </summary>
        public string SeniorSuffix = "Sr.";
        
        public CharacterGenderOption Genders { get; set; } = new CharacterGenderOption() { Name = "Genders", IsChoice = true };
        
        public CharacterNoteOption Races { get; set; } = new CharacterNoteOption() { Name = "Races", IsChoice = true, IsManualMultiSelect = true };
        
        public CharacterNoteOption Ages { get; set; } = new CharacterNoteOption() { Name = "Ages", IsChoice = true };
        
        public CharacterNoteOption Traits { get; set; } = new CharacterNoteOption() { Name = "Traits" };
        
        public CharacterNoteOption Relationships { get; set; } = new CharacterNoteOption() { Name = "Relationships" };

        public CharacterTemplate() { }

        public NoteOption FindOption(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            string root = NoteOption.GetFirstPathNode(path);
            if (root == Genders.Name) return Genders.FindOption(path);
            else if (root == Races.Name) return Races.FindOption(path);
            else if (root == Ages.Name) return Ages.FindOption(path);
            else if (root == Traits.Name) return Traits.FindOption(path);
            else if (root == Relationships.Name) return Relationships.FindOption(path);
            else return null;
        }
    }
    
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class IdeaTreeTemplate
    {
        [ProtoIgnore]
        public static string IdeaTreeTemplateExt = ".itt";
        
        [ProtoIgnore]
        public string CurrentPath { get; set; }

        public StoryTemplate StoryTemplate { get; set; } = new StoryTemplate();

        public CharacterTemplate CharacterTemplate { get; set; } = new CharacterTemplate();

        private static IdeaTreeTemplate defaultTemplate;
        [ProtoIgnore]
        public static IdeaTreeTemplate DefaultTemplate
        {
            get
            {
                if (defaultTemplate == null)
                {
                    defaultTemplate = new IdeaTreeTemplate() { };

                    // Story
                    AddDefaultPlotArchetypes(defaultTemplate);

                    AddDefaultProtagonists(defaultTemplate);
                    AddDefaultSupportingCharacters(defaultTemplate);
                    AddDefaultConflicts(defaultTemplate);

                    AddDefaultBackgrounds(defaultTemplate);
                    AddDefaultResolutions(defaultTemplate);
                    AddDefaultStoryTraits(defaultTemplate);

                    AddDefaultSettings(defaultTemplate);
                    AddDefaultThemes(defaultTemplate);
                    AddDefaultGenres(defaultTemplate);

                    // Character
                    AddDefaultTitles(defaultTemplate);
                    AddDefaultSuffixes(defaultTemplate);
                    AddDefaultGenders(defaultTemplate);
                    AddDefaultRaces(defaultTemplate);
                    AddDefaultAges(defaultTemplate);
                    AddDefaultCharacterTraits(defaultTemplate);
                    AddDefaultRelationships(defaultTemplate);
                }
                return defaultTemplate;
            }
        }

        public IdeaTreeTemplate() { }

        public static IdeaTreeTemplate FromFile(string path)
        {
            IdeaTreeTemplate templateFile = null;
            
            using (Stream file = File.OpenRead(path))
            {
                using (var gzip = new GZipStream(file, CompressionMode.Decompress, true))
                {
                    templateFile = Serializer.Deserialize<IdeaTreeTemplate>(gzip);
                }

                templateFile.CurrentPath = path;
            }

            return templateFile;
        }

        public void Save(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            using (Stream file = File.Create(path))
            {
                using (var gzip = new GZipStream(file, CompressionMode.Compress, true))
                {
                    Serializer.Serialize(gzip, this);
                }
            }

            CurrentPath = path;
        }

        public void Save() => Save(CurrentPath);

        #region Create Default Template

        #region Story

        private static void AddDefaultPlotArchetypes(IdeaTreeTemplate defaultTemplate)
        {
            PlotArchetypeNoteOption archetype = new PlotArchetypeNoteOption("Supplication") { IsChoice = true };
            archetype.AddChild(new NoteOption("Fugitives Imploring the Powerful for Help Against Their Enemies"));
            archetype.AddChild(new NoteOption("Assistance Implored for the Performance of a Pious Duty Which Has Been Forbidden"));
            archetype.AddChild(new NoteOption("Appeals for a Refuge in Which to Die"));
            archetype.AddChild(new NoteOption("Hospitality Besought by the Shipwrecked"));
            archetype.AddChild(new NoteOption("Charity Entreated by Those Cast Off by Their Own People, Whom They Have Disgraced"));
            archetype.AddChild(new NoteOption("Expiation: The Seeking of Pardon, Healing or Deliverance"));
            archetype.AddChild(new NoteOption("The Surrender of a Corpse, or of a Relic, Solicited"));
            archetype.AddChild(new NoteOption("Supplication of the Powerful for Those Dear to the Suppliant"));
            archetype.AddChild(new NoteOption("Supplication to a Relative in Behalf of Another Relative"));
            archetype.AddChild(new NoteOption("Supplication to a Mother's Lover, in Her Behalf"));
            archetype.AddElement(new PlotElementNoteOption("Persecutor"));
            archetype.AddElement(new PlotElementNoteOption("Suppliant"));
            archetype.AddElement(new PlotElementNoteOption("Authority, whose decision is doubtful"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Deliverance") { IsChoice = true };
            archetype.AddChild(new NoteOption("Appearance of a Rescuer to the Condemned"));
            archetype.AddChild(new NoteOption("A Parent Replaced Upon a Throne by His Children"));
            archetype.AddChild(new NoteOption("Rescue by Friends, or by Strangers Grateful for Benefits Or Hospitality"));
            archetype.AddElement(new PlotElementNoteOption("Unfortunate"));
            archetype.AddElement(new PlotElementNoteOption("Threatener"));
            archetype.AddElement(new PlotElementNoteOption("Rescuer"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Crime Pursued by Vengeance") { IsChoice = true };
            archetype.AddChild(new NoteOption("The Avenging of a Slain Parent or Ancestor"));
            archetype.AddChild(new NoteOption("The Avenging of a Slain Child or Descendant"));
            archetype.AddChild(new NoteOption("Vengeance for a Child Dishonored"));
            archetype.AddChild(new NoteOption("The Avenging of a Slain Wife or Husband"));
            archetype.AddChild(new NoteOption("Vengeance for the Dishonor, or Attempted Dishonoring, of a Wife"));
            archetype.AddChild(new NoteOption("Vengeance for a Lover Slain"));
            archetype.AddChild(new NoteOption("Vengeance for a Slain or Injured Friend"));
            archetype.AddChild(new NoteOption("Vengeance for a Sister or Brother Seduced"));
            archetype.AddChild(new NoteOption("Vengeance for Intentional Injury or Spoliation"));
            archetype.AddChild(new NoteOption("Vengeance for Having Been Despoiled During Absence"));
            archetype.AddChild(new NoteOption("Revenge for an Attempted Slaying"));
            archetype.AddChild(new NoteOption("Revenge for a False Accusation"));
            archetype.AddChild(new NoteOption("Vengeance for Violation"));
            archetype.AddChild(new NoteOption("Vengeance for Having Been Robbed of One's Own"));
            archetype.AddChild(new NoteOption("Revenge Upon a Whole Sex for a Deception by One"));
            archetype.AddChild(new NoteOption("Professional Pursuit of Criminals"));
            archetype.AddElement(new PlotElementNoteOption("Avenger"));
            archetype.AddElement(new PlotElementNoteOption("Criminal"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Vengeance Taken for Kindred upon Kindred") { IsChoice = true };
            archetype.AddChild(new NoteOption("A Mother's Death Avenged Upon a Father"));
            archetype.AddChild(new NoteOption("A Mother's Death Avenged Upon a Brother"));
            archetype.AddChild(new NoteOption("A Mother's Death Avenged Upon a Sister"));
            archetype.AddChild(new NoteOption("A Mother's Death Avenged Upon Multiple Siblings"));
            archetype.AddChild(new NoteOption("A Mother's Death Avenged Upon a Son"));
            archetype.AddChild(new NoteOption("A Mother's Death Avenged Upon a Daughter"));
            archetype.AddChild(new NoteOption("A Mother's Death Avenged Upon Multiple Children"));
            archetype.AddChild(new NoteOption("A Mother's Death Avenged Upon a Spouse"));
            archetype.AddChild(new NoteOption("A Father's Death Avenged Upon a Mother"));
            archetype.AddChild(new NoteOption("A Father's Death Avenged Upon a Brother"));
            archetype.AddChild(new NoteOption("A Father's Death Avenged Upon a Sister"));
            archetype.AddChild(new NoteOption("A Father's Death Avenged Upon Multiple Siblings"));
            archetype.AddChild(new NoteOption("A Father's Death Avenged Upon a Son"));
            archetype.AddChild(new NoteOption("A Father's Death Avenged Upon a Daughter"));
            archetype.AddChild(new NoteOption("A Father's Death Avenged Upon Multiple Children"));
            archetype.AddChild(new NoteOption("A Father's Death Avenged Upon a Spouse"));
            archetype.AddChild(new NoteOption("Both Parents' Death Avenged Upon a Brother"));
            archetype.AddChild(new NoteOption("Both Parents' Death Avenged Upon a Sister"));
            archetype.AddChild(new NoteOption("Both Parents' Death Avenged Upon Multiple Siblings"));
            archetype.AddChild(new NoteOption("Both Parents' Death Avenged Upon a Son"));
            archetype.AddChild(new NoteOption("Both Parents' Death Avenged Upon a Daughter"));
            archetype.AddChild(new NoteOption("Both Parents' Death Avenged Upon Multiple Children"));
            archetype.AddChild(new NoteOption("Both Parents' Death Avenged Upon a Spouse"));
            archetype.AddChild(new NoteOption("A Brother's Death Avenged Upon a Mother"));
            archetype.AddChild(new NoteOption("A Brother's Death Avenged Upon a Father"));
            archetype.AddChild(new NoteOption("A Brother's Death Avenged Upon a Sister"));
            archetype.AddChild(new NoteOption("A Brother's Death Avenged Upon Multiple Siblings"));
            archetype.AddChild(new NoteOption("A Brother's Death Avenged Upon a Son"));
            archetype.AddChild(new NoteOption("A Brother's Death Avenged Upon a Daughter"));
            archetype.AddChild(new NoteOption("A Brother's Death Avenged Upon Multiple Children"));
            archetype.AddChild(new NoteOption("A Brother's Death Avenged Upon a Spouse"));
            archetype.AddChild(new NoteOption("A Sister's Death Avenged Upon a Mother"));
            archetype.AddChild(new NoteOption("A Sister's Death Avenged Upon a Father"));
            archetype.AddChild(new NoteOption("A Sister's Death Avenged Upon a Brother"));
            archetype.AddChild(new NoteOption("A Sister's Death Avenged Upon Multiple Siblings"));
            archetype.AddChild(new NoteOption("A Sister's Death Avenged Upon a Son"));
            archetype.AddChild(new NoteOption("A Sister's Death Avenged Upon a Daughter"));
            archetype.AddChild(new NoteOption("A Sister's Death Avenged Upon Multiple Children"));
            archetype.AddChild(new NoteOption("A Sister's Death Avenged Upon a Spouse"));
            archetype.AddChild(new NoteOption("A Son's Death Avenged Upon a Mother"));
            archetype.AddChild(new NoteOption("A Son's Death Avenged Upon a Father"));
            archetype.AddChild(new NoteOption("A Son's Death Avenged Upon a Brother"));
            archetype.AddChild(new NoteOption("A Son's Death Avenged Upon a Sister"));
            archetype.AddChild(new NoteOption("A Son's Death Avenged Upon Multiple Siblings"));
            archetype.AddChild(new NoteOption("A Son's Death Avenged Upon a Daughter"));
            archetype.AddChild(new NoteOption("A Son's Death Avenged Upon Multiple Children"));
            archetype.AddChild(new NoteOption("A Son's Death Avenged Upon a Spouse"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon a Mother"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon a Father"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon a Brother"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon a Sister"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon Multiple Siblings"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon a Son"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon Multiple Children"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon a Husband"));
            archetype.AddChild(new NoteOption("A Daughter's Death Avenged Upon a Wife"));
            archetype.AddChild(new NoteOption("A Spouse's Death Avenged Upon a Mother"));
            archetype.AddChild(new NoteOption("A Spouse's Death Avenged Upon a Father"));
            archetype.AddChild(new NoteOption("A Spouse's Death Avenged Upon a Brother"));
            archetype.AddChild(new NoteOption("A Spouse's Death Avenged Upon a Sister"));
            archetype.AddChild(new NoteOption("A Spouse's Death Avenged Upon Multiple Siblings"));
            archetype.AddChild(new NoteOption("A Spouse's Death Avenged Upon a Son"));
            archetype.AddChild(new NoteOption("A Spouse's Death Avenged Upon a Daughter"));
            archetype.AddChild(new NoteOption("A Spouse's Death Avenged Upon Multiple Children"));
            archetype.AddElement(new PlotElementNoteOption("Avenging Kinsman"));
            archetype.AddElement(new PlotElementNoteOption("Guilty Kinsman"));
            archetype.AddElement(new PlotElementNoteOption("Victim, a Relative of Both"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Pursuit") { IsChoice = true };
            archetype.AddChild(new NoteOption("Fugitives from Justice Pursued for Brigandage, Political Offenses, etc."));
            archetype.AddChild(new NoteOption("Pursued for a Fault of Love"));
            archetype.AddChild(new NoteOption("A Hero Struggling Against a Power"));
            archetype.AddChild(new NoteOption("A Pseudo-Madman Struggling Against an Iago-Like Alienist"));
            archetype.AddElement(new PlotElementNoteOption("Punishment"));
            archetype.AddElement(new PlotElementNoteOption("Fugitive"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Disaster") { IsChoice = true };
            archetype.AddChild(new NoteOption("Defeat Suffered"));
            archetype.AddChild(new NoteOption("A Fatherland Destroyed"));
            archetype.AddChild(new NoteOption("The Fall of Humanity"));
            archetype.AddChild(new NoteOption("A Natural Catastrophe"));
            archetype.AddChild(new NoteOption("A Monarch Overthrown"));
            archetype.AddChild(new NoteOption("Ingratitude Suffered"));
            archetype.AddChild(new NoteOption("The Suffering of Unjust Punishment or Enmity"));
            archetype.AddChild(new NoteOption("An Outrage Suffered"));
            archetype.AddChild(new NoteOption("Abandonment by a Lover or a Spouse"));
            archetype.AddChild(new NoteOption("Children Lost by Their Parents"));
            archetype.AddElement(new PlotElementNoteOption("Vanquished Power"));
            archetype.AddElement(new PlotElementNoteOption("Victorious Enemy"));
            archetype.AddElement(new PlotElementNoteOption("Messenger"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Falling Prey to Cruelty or Misfortune") { IsChoice = true };
            archetype.AddChild(new NoteOption("The Innocent Made the Victim of Ambitious Intrigue"));
            archetype.AddChild(new NoteOption("The Innocent Despoiled by Those Who Should Protect"));
            archetype.AddChild(new NoteOption("The Powerful Dispossessed and Wretched"));
            archetype.AddChild(new NoteOption("A Favorite or an Intimate Finds Himself Forgotten"));
            archetype.AddChild(new NoteOption("The Unfortunate Robbed of Their Only Hope"));
            archetype.AddElement(new PlotElementNoteOption("Unfortunate"));
            archetype.AddElement(new PlotElementNoteOption("Master or Misfortune"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Revolt") { IsChoice = true };
            archetype.AddChild(new NoteOption("A Conspiracy Chiefly of One Individual"));
            archetype.AddChild(new NoteOption("A Conspiracy of Several"));
            archetype.AddChild(new NoteOption("Revolt of One Individual, Who Influences and Involves Others"));
            archetype.AddChild(new NoteOption("A Revolt of Many"));
            archetype.AddElement(new PlotElementNoteOption("Tyrant"));
            archetype.AddElement(new PlotElementNoteOption("Conspirator"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Daring Enterprise") { IsChoice = true };
            archetype.AddChild(new NoteOption("Preparations For War"));
            archetype.AddChild(new NoteOption("War"));
            archetype.AddChild(new NoteOption("A Combat"));
            archetype.AddChild(new NoteOption("Carrying Off a Desired Person or Object"));
            archetype.AddChild(new NoteOption("Recapture of a Desired Object"));
            archetype.AddChild(new NoteOption("Adventurous Expeditions"));
            archetype.AddChild(new NoteOption("Adventure Undertaken for the Purpose of Obtaining a Love-interest"));
            archetype.AddElement(new PlotElementNoteOption("Bold Leader"));
            archetype.AddElement(new PlotElementNoteOption("Object"));
            archetype.AddElement(new PlotElementNoteOption("Adversary"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Abduction") { IsChoice = true };
            archetype.AddChild(new NoteOption("Abduction of an Unwilling Lover"));
            archetype.AddChild(new NoteOption("Abduction of a Consenting Lover"));
            archetype.AddChild(new NoteOption("Recapture of an Abducted Lover without the Slaying of the Abductor"));
            archetype.AddChild(new NoteOption("Recapture of an Abducted Lover with the Slaying of the Ravisher"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Friend"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Mother without the Slaying of the Abductor"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Mother with the Slaying of the Ravisher"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Father"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Brother"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Sister without the Slaying of the Abductor"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Sister with the Slaying of the Ravisher"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Son"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Daughter without the Slaying of the Abductor"));
            archetype.AddChild(new NoteOption("Rescue of a Captive Daughter with the Slaying of the Ravisher"));
            archetype.AddChild(new NoteOption("Rescue of a Soul in Captivity to Error"));
            archetype.AddElement(new PlotElementNoteOption("Abductor"));
            archetype.AddElement(new PlotElementNoteOption("Abducted"));
            archetype.AddElement(new PlotElementNoteOption("Guardian"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("The Enigma") { IsChoice = true };
            archetype.AddChild(new NoteOption("Search for a Person Who Must Be Found on Pain of Death"));
            archetype.AddChild(new NoteOption("A Riddle To Be Solved on Pain of Death"));
            archetype.AddChild(new NoteOption("A Riddle To Be Solved on Pain of Death, Proposed by the Coveted Woman"));
            archetype.AddChild(new NoteOption("Temptations Offered With the Object of Discovering His Name"));
            archetype.AddChild(new NoteOption("Temptations Offered With the Object of Ascertaining the Sex"));
            archetype.AddChild(new NoteOption("Tests for the Purpose of Ascertaining the Mental Condition"));
            archetype.AddElement(new PlotElementNoteOption("Interrogator"));
            archetype.AddElement(new PlotElementNoteOption("Seeker"));
            archetype.AddElement(new PlotElementNoteOption("Problem"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Obtaining") { IsChoice = true };
            archetype.AddChild(new NoteOption("Efforts to Obtain an Object by Ruse or Force"));
            archetype.AddChild(new NoteOption("Endeavor by Means of Persuasive Eloquence Alone"));
            archetype.AddChild(new NoteOption("Eloquence With an Arbitrator"));
            archetype.AddElement(new PlotElementNoteOption("Solicitor"));
            archetype.AddElement(new PlotElementNoteOption("Adversary Who is Refusing"));
            archetype.AddElement(new PlotElementNoteOption("Arbitrator"));
            archetype.AddElement(new PlotElementNoteOption("One of Various Opposing Parties"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Enmity of Kinsmen") { IsChoice = true };
            archetype.AddChild(new NoteOption("Hatred of Siblings: One Sibling Hated by Several"));
            archetype.AddChild(new NoteOption("Reciprocal Hatred of Siblings"));
            archetype.AddChild(new NoteOption("Hatred Between Relatives for Reasons of Self-Interest"));
            archetype.AddChild(new NoteOption("Hatred of Daughter for Mother"));
            archetype.AddChild(new NoteOption("Hatred of Daughter for Father"));
            archetype.AddChild(new NoteOption("Hatred of Son for Mother"));
            archetype.AddChild(new NoteOption("Hatred of Son for Father"));
            archetype.AddChild(new NoteOption("Hatred of Mother for Daughter"));
            archetype.AddChild(new NoteOption("Hatred of Mother for Son"));
            archetype.AddChild(new NoteOption("Hatred of Father for Daughter"));
            archetype.AddChild(new NoteOption("Hatred of Father for Son"));
            archetype.AddChild(new NoteOption("Mutual Hatred of Mother and Daughter"));
            archetype.AddChild(new NoteOption("Mutual Hatred of Mother and Son"));
            archetype.AddChild(new NoteOption("Mutual Hatred of Father and Daughter"));
            archetype.AddChild(new NoteOption("Mutual Hatred of Father and Son"));
            archetype.AddChild(new NoteOption("Hatred of Grandfather for Grandson"));
            archetype.AddChild(new NoteOption("Hatred of Grandfather for Granddaughter"));
            archetype.AddChild(new NoteOption("Hatred of Grandmother for Grandson"));
            archetype.AddChild(new NoteOption("Hatred of Grandmother for Granddaughter"));
            archetype.AddChild(new NoteOption("Hatred of Father-in-law for Son-in-law"));
            archetype.AddChild(new NoteOption("Hatred of Father-in-law for Daughter-in-law"));
            archetype.AddChild(new NoteOption("Hatred of Mother-in-law for Son-in-law"));
            archetype.AddChild(new NoteOption("Hatred of Mother-in-law for Daughter-in-law"));
            archetype.AddChild(new NoteOption("Infanticide"));
            archetype.AddElement(new PlotElementNoteOption("Malevolent Kinsman"));
            archetype.AddElement(new PlotElementNoteOption("Hated Kinsman"));
            archetype.AddElement(new PlotElementNoteOption("Arbitrator"));
            archetype.AddElement(new PlotElementNoteOption("One of Various Reciprocally Hating Kinsmen"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Rivalry of Kinsmen") { IsChoice = true };
            archetype.AddChild(new NoteOption("Malicious Rivalry of a Brother"));
            archetype.AddChild(new NoteOption("Malicious Rivalry of Two Brothers"));
            archetype.AddChild(new NoteOption("Rivalry of Two Brothers, With Adultery on the Part of One"));
            archetype.AddChild(new NoteOption("Malicious Rivalry of a Sister"));
            archetype.AddChild(new NoteOption("Malicious Rivalry of Two Sisters"));
            archetype.AddChild(new NoteOption("Rivalry of Two Sisters, With Adultery on the Part of One"));
            archetype.AddChild(new NoteOption("Rivalry of Father and Son, for an Unmarried Woman"));
            archetype.AddChild(new NoteOption("Rivalry of Father and Son, for a Married Woman"));
            archetype.AddChild(new NoteOption("Rivalry of Father and Son, for a Woman Who is Already the Wife of the Father"));
            archetype.AddChild(new NoteOption("Rivalry of Father and Son, for a Woman Who is Already the Wife of the Son"));
            archetype.AddChild(new NoteOption("Rivalry of Mother and Daughter, for an Unmarried Man"));
            archetype.AddChild(new NoteOption("Rivalry of Mother and Daughter, for a Married Man"));
            archetype.AddChild(new NoteOption("Rivalry of Mother and Daughter, for a Man Who is Already the Husband of the Father"));
            archetype.AddChild(new NoteOption("Rivalry of Mother and Daughter, for a Man Who is Already the Husband of the Son"));
            archetype.AddChild(new NoteOption("Rivalry of Cousins"));
            archetype.AddChild(new NoteOption("Rivalry of Friends"));
            archetype.AddElement(new PlotElementNoteOption("Preferred Kinsman"));
            archetype.AddElement(new PlotElementNoteOption("Rejected Kinsman"));
            archetype.AddElement(new PlotElementNoteOption("Object"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Murderous Adultery") { IsChoice = true };
            archetype.AddChild(new NoteOption("The Slaying of a Husband by a Paramour"));
            archetype.AddChild(new NoteOption("The Slaying of a Husband for a Paramour"));
            archetype.AddChild(new NoteOption("The Slaying of a Trusting Lover"));
            archetype.AddChild(new NoteOption("Slaying of a Wife for a Paramour, and in Self-Interest"));
            archetype.AddElement(new PlotElementNoteOption("One of Two Adulterers"));
            archetype.AddElement(new PlotElementNoteOption("Betrayed Spouse"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Madness") { IsChoice = true };
            archetype.AddChild(new NoteOption("Kinsmen Slain in Madness"));
            archetype.AddChild(new NoteOption("Lover Slain in Madness"));
            archetype.AddChild(new NoteOption("Slaying or Injuring of a Person not Hated"));
            archetype.AddChild(new NoteOption("Disgrace Brought Upon Oneself Through Madness"));
            archetype.AddChild(new NoteOption("Loss of Loved Ones Brought About by Madness"));
            archetype.AddChild(new NoteOption("Madness Brought on by Fear of Hereditary Insanity"));
            archetype.AddElement(new PlotElementNoteOption("Madman"));
            archetype.AddElement(new PlotElementNoteOption("Victim"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Fatal Imprudence") { IsChoice = true };
            archetype.AddChild(new NoteOption("Imprudence the Cause of One's Own Misfortune"));
            archetype.AddChild(new NoteOption("Imprudence the Cause of One's Own Dishonor"));
            archetype.AddChild(new NoteOption("Curiosity the Cause of One's Own Misfortune"));
            archetype.AddChild(new NoteOption("Loss of the Possession of a Loved One, Through Curiosity"));
            archetype.AddChild(new NoteOption("Curiosity the Cause of Death or Misfortune to Others"));
            archetype.AddChild(new NoteOption("Imprudence the Cause of a Relative's Death"));
            archetype.AddChild(new NoteOption("Imprudence the Cause of a Lover's Death"));
            archetype.AddChild(new NoteOption("Credulity the Cause of Kinsmen's Deaths"));
            archetype.AddElement(new PlotElementNoteOption("Imprudent"));
            archetype.AddElement(new PlotElementNoteOption("Victim or Object Lost"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Involuntary Crimes of Love") { IsChoice = true };
            archetype.AddChild(new NoteOption("Discovery that One Has Had One's Mother as Mistress"));
            archetype.AddChild(new NoteOption("Discovery that One Has Had One's Mother as Mistress, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddChild(new NoteOption("Discovery that One Has Married One's Mother"));
            archetype.AddChild(new NoteOption("Discovery that One Has Married One's Mother, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddChild(new NoteOption("Discovery that One Has Had a Sister as Mistress"));
            archetype.AddChild(new NoteOption("Discovery that One Has Had a Sister as Mistress, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddChild(new NoteOption("Discovery that One Has Married One's Sister"));
            archetype.AddChild(new NoteOption("Discovery that One Has Married One's Sister, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Taking a Mother, Unknowingly, as Mistress"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Taking a Mother, Unknowingly, as Mistress, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Taking a Sister, Unknowingly, as Mistress"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Taking a Sister, Unknowingly, as Mistress, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Violating, Unknowingly, a Daughter"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Violating, Unknowingly, a Daughter, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Committing an Adultery Unknowingly"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Committing an Adultery Unknowingly, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddChild(new NoteOption("Adultery Committed Unknowingly"));
            archetype.AddChild(new NoteOption("Adultery Committed Unknowingly, in Which the Crime Has Been Villainously Planned by a Third Person"));
            archetype.AddElement(new PlotElementNoteOption("Lover"));
            archetype.AddElement(new PlotElementNoteOption("Beloved"));
            archetype.AddElement(new PlotElementNoteOption("Revealer"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Slaying of a Kinsman Unrecognized") { IsChoice = true };
            archetype.AddChild(new NoteOption("Being Upon the Point of Slaying a Daughter Unknowingly, by Command of a Divinity or an Oracle"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Slaying a Daughter Unknowingly, Through Hatred of the Lover of the Unrecognized Daughter"));
            archetype.AddChild(new NoteOption("Through Political Necessity"));
            archetype.AddChild(new NoteOption("Through a Rivalry in Love"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Killing a Son Unknowingly"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Killing a Son Unknowingly, Strengthened by Machiavellian Instigations"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Slaying a Brother Unknowingly"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Slaying a Brother Unknowingly, Strengthened by Machiavellian Instigations"));
            archetype.AddChild(new NoteOption("Slaying of a Mother Unrecognized"));
            archetype.AddChild(new NoteOption("A Father Slain Unknowingly, Through Machiavellian Advice"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Killing a Father Unknowingly, Through Machiavellian Advice"));
            archetype.AddChild(new NoteOption("A Father Slain Unknowingly, in Vengeance and Through Instigation"));
            archetype.AddChild(new NoteOption("A Grandfather Slain Unknowingly, in Vengeance and Through Instigation"));
            archetype.AddChild(new NoteOption("Involuntary Killing of a Lover"));
            archetype.AddChild(new NoteOption("Being Upon the Point of Killing a Lover Unrecognized"));
            archetype.AddChild(new NoteOption("Failure to Rescue an Unrecognized Son"));
            archetype.AddChild(new NoteOption("Failure to Rescue an Unrecognized Daughter"));
            archetype.AddChild(new NoteOption("Failure to Rescue an Unrecognized Mother"));
            archetype.AddChild(new NoteOption("Failure to Rescue an Unrecognized Father"));
            archetype.AddElement(new PlotElementNoteOption("Slayer"));
            archetype.AddElement(new PlotElementNoteOption("Unrecognized Victim"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Self-Sacrificing for an Ideal") { IsChoice = true };
            archetype.AddChild(new NoteOption("Sacrifice of Life for the Sake of One's Word"));
            archetype.AddChild(new NoteOption("Life Sacrifice for the Success of One's People"));
            archetype.AddChild(new NoteOption("Life Sacrificed in Filial Piety"));
            archetype.AddChild(new NoteOption("Life Sacrificed for the Sake of One's Faith"));
            archetype.AddChild(new NoteOption("Both Love and Life Sacrificed for One's Faith, or a Cause"));
            archetype.AddChild(new NoteOption("Love Sacrificed to the Interests of State"));
            archetype.AddChild(new NoteOption("Sacrifice of Well-Being to Duty"));
            archetype.AddChild(new NoteOption("The Ideal of 'Honor' Sacrificed to the Ideal of 'Faith'"));
            archetype.AddElement(new PlotElementNoteOption("Hero"));
            archetype.AddElement(new PlotElementNoteOption("Ideal"));
            archetype.AddElement(new PlotElementNoteOption("'Creditor' or, Person or Thing Sacrificed"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Self-Sacrifice for Kindred") { IsChoice = true };
            archetype.AddChild(new NoteOption("Life Sacrificed for that of a Relative or a Loved One"));
            archetype.AddChild(new NoteOption("Life Sacrificed for the Happiness of a Relative or a Loved One"));
            archetype.AddChild(new NoteOption("Ambition Sacrificed for the Happiness of a Parent"));
            archetype.AddChild(new NoteOption("Ambition Sacrificed for the Life of a Parent"));
            archetype.AddChild(new NoteOption("Love Sacrificed for the Sake of a Parent's Life"));
            archetype.AddChild(new NoteOption("Love Sacrificed for the Happiness of One's Child"));
            archetype.AddChild(new NoteOption("Love Sacrificed for the Happiness of One's Child, Caused by Unjust Laws"));
            archetype.AddChild(new NoteOption("Life and Honor Sacrificed for the Life of a Parent or Loved One"));
            archetype.AddChild(new NoteOption("Modesty Sacrificed for the Life of a Relative or a Loved One"));
            archetype.AddElement(new PlotElementNoteOption("Hero"));
            archetype.AddElement(new PlotElementNoteOption("Kinsman"));
            archetype.AddElement(new PlotElementNoteOption("'Creditor' or, Person or Thing Sacrificed"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("All Sacrificed for a Passion") { IsChoice = true };
            archetype.AddChild(new NoteOption("Religious Vows of Chastity Broken for a Passion"));
            archetype.AddChild(new NoteOption("Respect for a Priest Destroyed"));
            archetype.AddChild(new NoteOption("A Future Ruined by Passion"));
            archetype.AddChild(new NoteOption("Power Ruined by Passion"));
            archetype.AddChild(new NoteOption("Ruin of Mind, Health, and Life"));
            archetype.AddChild(new NoteOption("Ruin of Fortunes, Lives, and Honors"));
            archetype.AddChild(new NoteOption("Temptations Destroying the Sense of Duty, of Piety, etc."));
            archetype.AddChild(new NoteOption("Destruction of Honor, Fortune, and Life by Vice"));
            archetype.AddChild(new NoteOption("Destruction of Honor, Fortune, and Life by Non-Erotic Vice"));
            archetype.AddElement(new PlotElementNoteOption("Lover"));
            archetype.AddElement(new PlotElementNoteOption("Object of the Fatal Passion"));
            archetype.AddElement(new PlotElementNoteOption("Person or Thing Sacrificed"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Necessity of Sacrificing Love Ones") { IsChoice = true };
            archetype.AddChild(new NoteOption("Necessity for Sacrificing a Daughter in the Public Interest"));
            archetype.AddChild(new NoteOption("Duty of Sacrificing a Daughter in Fulfillment of a Vow to God"));
            archetype.AddChild(new NoteOption("Duty of Sacrificing Benefactors or Loved Ones to One's Faith"));
            archetype.AddChild(new NoteOption("Duty of Sacrificing One's Child, Unknown to Others, Under the Pressure of Necessity"));
            archetype.AddChild(new NoteOption("Duty of Sacrificing One's Parent, Unknown to Others, Under the Pressure of Necessity"));
            archetype.AddChild(new NoteOption("Duty of Sacrificing One's Spouse, Unknown to Others, Under the Pressure of Necessity"));
            archetype.AddChild(new NoteOption("Duty of Sacrificing a Child-in-law for the Public Good"));
            archetype.AddChild(new NoteOption("Duty of Contending with a Sibling-in-Law for the Public Good"));
            archetype.AddChild(new NoteOption("Duty of Contending with a Friend"));
            archetype.AddElement(new PlotElementNoteOption("Hero"));
            archetype.AddElement(new PlotElementNoteOption("Beloved Victim"));
            archetype.AddElement(new PlotElementNoteOption("Necessity for the Sacrifice"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Rivalry of Superior and Inferior") { IsChoice = true };
            archetype.AddChild(new NoteOption("Of a Man and an Immortal"));
            archetype.AddChild(new NoteOption("Of a Sorcerer and an Ordinary Man"));
            archetype.AddChild(new NoteOption("Of a Sorceress and an Ordinary Woman"));
            archetype.AddChild(new NoteOption("Of Conqueror and Conquered"));
            archetype.AddChild(new NoteOption("Of a Monarch and a Noble"));
            archetype.AddChild(new NoteOption("Of Monarch and Subject"));
            archetype.AddChild(new NoteOption("Of a Powerful Person and an Upstart"));
            archetype.AddChild(new NoteOption("Of Rich and Poor"));
            archetype.AddChild(new NoteOption("Of an Honored Man and a Suspected One"));
            archetype.AddChild(new NoteOption("Rivalry of Two Who are Almost Equal"));
            archetype.AddChild(new NoteOption("Of the Two Successive Husbands of a Divorcee"));
            archetype.AddChild(new NoteOption("Of Lady and Servant"));
            archetype.AddChild(new NoteOption("Rivalry Between Memory or an Ideal and a Living Person"));
            archetype.AddChild(new NoteOption("Double Rivalry (A loves B, who loves C, who loves D, or A)"));
            archetype.AddElement(new PlotElementNoteOption("Superior Rival"));
            archetype.AddElement(new PlotElementNoteOption("Inferior Rival"));
            archetype.AddElement(new PlotElementNoteOption("Object"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Adultery") { IsChoice = true };
            archetype.AddChild(new NoteOption("A Lover Betrayed, For a Younger Lover"));
            archetype.AddChild(new NoteOption("A Lover Betrayed, For a Young Spouse"));
            archetype.AddChild(new NoteOption("A Spouse Betrayed, For a Slave Who Does Not Love in Return"));
            archetype.AddChild(new NoteOption("A Spouse Betrayed, For Debauchery"));
            archetype.AddChild(new NoteOption("A Spouse Betrayed, For a Married Person"));
            archetype.AddChild(new NoteOption("A Wife Betrayed, With the Intention of Bigamy"));
            archetype.AddChild(new NoteOption("A Spouse Betrayed, For a Young Lover, who Does Not Love in Return"));
            archetype.AddChild(new NoteOption("A Wife Envied by a Young Girl Who is in Love With Her Husband"));
            archetype.AddChild(new NoteOption("A Wife Envied by a Courtesan Who is in Love With Her Husband"));
            archetype.AddChild(new NoteOption("An Antagonistic Spouse Sacrificed for a Congenial Lover"));
            archetype.AddChild(new NoteOption("A Spouse, Believed to be Lost, Forgotten for a Rival"));
            archetype.AddChild(new NoteOption("A Commonplace Spouse Sacrificed for a Sympathetic Lover"));
            archetype.AddChild(new NoteOption("A Good Spouse Betrayed for an Inferior Rival"));
            archetype.AddChild(new NoteOption("A Good Spouse Betrayed for a Grotesque Rival"));
            archetype.AddChild(new NoteOption("A Good Spouse Betrayed for a Commonplace Rival, By a Perverse Spouse"));
            archetype.AddChild(new NoteOption("A Good Spouse Betrayed for a Rival Less Attractive, But Useful"));
            archetype.AddChild(new NoteOption("Vengeance of a Deceived Spouse"));
            archetype.AddChild(new NoteOption("Jealousy Sacrificed for the Sake of a Cause"));
            archetype.AddChild(new NoteOption("Spouse Persecuted by a Rejected Rival"));
            archetype.AddElement(new PlotElementNoteOption("Deceived Spouse"));
            archetype.AddElement(new PlotElementNoteOption("Two Adulterers"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Crimes of Love") { IsChoice = true };
            archetype.AddChild(new NoteOption("A Mother in Love with Her Son"));
            archetype.AddChild(new NoteOption("Violation of a Mother by a Son"));
            archetype.AddChild(new NoteOption("A Daughter in Love with her Father"));
            archetype.AddChild(new NoteOption("Violation of a Daughter by a Father"));
            archetype.AddChild(new NoteOption("A Woman Enamored of Her Stepson"));
            archetype.AddChild(new NoteOption("A Woman and Her Stepson Enamored of Each Other"));
            archetype.AddChild(new NoteOption("A Woman Being the Mistress, at the Same Time, of a Father and Son, Both of Whom Accept the Situation"));
            archetype.AddChild(new NoteOption("A Man Being the Lover, at the Same Time, of a Mother and Daughter, Both of Whom Accept the Situation"));
            archetype.AddChild(new NoteOption("A Man Becomes the Lover of a Daughter-in-Law"));
            archetype.AddChild(new NoteOption("A Woman Becomes the Lover of a Son-in-Law"));
            archetype.AddChild(new NoteOption("A Brother and Sister in Love with Each Other"));
            archetype.AddChild(new NoteOption("A Woman Enamored of Another Woman, Who Resists"));
            archetype.AddChild(new NoteOption("A Woman Enamored of Another Woman, Who Yields"));
            archetype.AddChild(new NoteOption("A Woman Enamored of a Beast"));
            archetype.AddElement(new PlotElementNoteOption("Lover"));
            archetype.AddElement(new PlotElementNoteOption("Beloved"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Discovery of the Dishonor of a Loved One") { IsChoice = true };
            archetype.AddChild(new NoteOption("Discovery of a Mother's Shame"));
            archetype.AddChild(new NoteOption("Discovery of a Father's Shame"));
            archetype.AddChild(new NoteOption("Discovery of a Daughter's Dishonor"));
            archetype.AddChild(new NoteOption("Discovery of Dishonor in the Family of One's Fiancee"));
            archetype.AddChild(new NoteOption("Discovery than One's Wife Has Been Violated Before Marriage"));
            archetype.AddChild(new NoteOption("Discovery than One's Wife Has Been Violated Since the Marriage"));
            archetype.AddChild(new NoteOption("Discovery than One's Wife Has Previously Committed a Fault"));
            archetype.AddChild(new NoteOption("Discovery that One's Wife Has Formerly Been a Prostitute"));
            archetype.AddChild(new NoteOption("Discovery that One's Mistress, Formerly a Prostitute, Has Returned to Her Old Life"));
            archetype.AddChild(new NoteOption("Discovery that One's Lover is a Scoundrel"));
            archetype.AddChild(new NoteOption("Discovery that One's Spouse is a Scoundrel"));
            archetype.AddChild(new NoteOption("Duty of Punishing a Son Who is a Traitor to Country"));
            archetype.AddChild(new NoteOption("Duty of Punishing a Son Condemned Under a Law Which the Father Has Made"));
            archetype.AddChild(new NoteOption("Duty of Punishing One's Mother to Avenge One's Father"));
            archetype.AddElement(new PlotElementNoteOption("Discoverer"));
            archetype.AddElement(new PlotElementNoteOption("Guilty One"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Obstacles to Love") { IsChoice = true };
            archetype.AddChild(new NoteOption("Marriage Prevented by Inequality of Rank"));
            archetype.AddChild(new NoteOption("Inequality of Fortune an Impediment to Marriage"));
            archetype.AddChild(new NoteOption("Marriage Prevented by Enemies and Contingent Obstacles"));
            archetype.AddChild(new NoteOption("Marriage Forbidden on Account of One's Previous Betrothal to Another"));
            archetype.AddChild(new NoteOption("A Free Union Impeded by the Opposition of Relatives"));
            archetype.AddChild(new NoteOption("A Free Union Impeded by the Incompatibility of Temper of the Lovers"));
            archetype.AddElement(new PlotElementNoteOption("One of Two Lovers"));
            archetype.AddElement(new PlotElementNoteOption("Obstacle"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("An Enemy Loved") { IsChoice = true };
            archetype.AddChild(new NoteOption("The Loved One Hated by Kinsmen of the Lover"));
            archetype.AddChild(new NoteOption("The Lover Pursued by the Brothers of His Beloved"));
            archetype.AddChild(new NoteOption("The Lover Hated by the Family of His Beloved"));
            archetype.AddChild(new NoteOption("The Beloved is an Enemy of the Party of the Lover"));
            archetype.AddChild(new NoteOption("The Beloved is the Slayer of a Kinsman of the Lover"));
            archetype.AddElement(new PlotElementNoteOption("Beloved Enemy"));
            archetype.AddElement(new PlotElementNoteOption("Lover"));
            archetype.AddElement(new PlotElementNoteOption("Hater"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Ambition") { IsChoice = true };
            archetype.AddChild(new NoteOption("Ambition Watched and Guarded Against by a Kinsman, or By a Person Under Obligation"));
            archetype.AddChild(new NoteOption("Rebellious Ambition"));
            archetype.AddChild(new NoteOption("Ambition and Covetousness Heaping Crime Upon Crime"));
            archetype.AddElement(new PlotElementNoteOption("Ambitious Person"));
            archetype.AddElement(new PlotElementNoteOption("Thing Coveted"));
            archetype.AddElement(new PlotElementNoteOption("Adversary"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Conflict with a God") { IsChoice = true };
            archetype.AddChild(new NoteOption("Struggle Against a Deity"));
            archetype.AddChild(new NoteOption("Strife with the Believers in a God"));
            archetype.AddChild(new NoteOption("Controversy with a Deity"));
            archetype.AddChild(new NoteOption("Punishment for Contempt of a God"));
            archetype.AddChild(new NoteOption("Punishment for Pride Before a God"));
            archetype.AddElement(new PlotElementNoteOption("Mortal"));
            archetype.AddElement(new PlotElementNoteOption("Immortal"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Mistaken Jealousy") { IsChoice = true };
            archetype.AddChild(new NoteOption("The Mistake Originates in the Suspicious Mind of the Jealous One"));
            archetype.AddChild(new NoteOption("Mistaken Jealousy Aroused by Fatal Chance"));
            archetype.AddChild(new NoteOption("Mistaken Jealousy of a Love Which is Purely Platonic"));
            archetype.AddChild(new NoteOption("Baseless Jealousy Aroused by Malicious Rumors"));
            archetype.AddChild(new NoteOption("Jealousy Suggested by a Traitor Who is Moved by Hatred, or Self-Interest"));
            archetype.AddChild(new NoteOption("Reciprocal Jealousy Suggested to Husband and Wife by a Rival"));
            archetype.AddElement(new PlotElementNoteOption("Jealous One"));
            archetype.AddElement(new PlotElementNoteOption("Object of Whose Possession He is Jealous"));
            archetype.AddElement(new PlotElementNoteOption("Supposed Accomplice"));
            archetype.AddElement(new PlotElementNoteOption("Cause or Author of the Mistake"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Erroneous Judgment") { IsChoice = true };
            archetype.AddChild(new NoteOption("False Suspicion Where Faith is Necessary"));
            archetype.AddChild(new NoteOption("False Suspicion of a Lover"));
            archetype.AddChild(new NoteOption("False Suspicion Aroused by a Misunderstood Attitude of a Loved One"));
            archetype.AddChild(new NoteOption("False Suspicions Drawn Upon Oneself to Save a Friend"));
            archetype.AddChild(new NoteOption("False Suspicions Fall Upon the Innocent"));
            archetype.AddChild(new NoteOption("False Suspicions Fall Upon the Innocent, in Which the Innocent had a Guilty Intention, or Believes Himself Guilty"));
            archetype.AddChild(new NoteOption("A Witness to the Crime, in the Interest of a Loved One, Lets Accusation Fall Upon the Innocent"));
            archetype.AddChild(new NoteOption("The Accusation is Allowed to Fall Upon an Enemy"));
            archetype.AddChild(new NoteOption("The Error is Provoked by an Enemy"));
            archetype.AddChild(new NoteOption("False Suspicion Thrown by the Real Culprit Upon One of His Enemies"));
            archetype.AddChild(new NoteOption("Thrown by the Real Culprit Upon the Second Victim Against Whom He Has Plotted From the Beginning"));
            archetype.AddElement(new PlotElementNoteOption("Mistaken One"));
            archetype.AddElement(new PlotElementNoteOption("Victim of the Mistake"));
            archetype.AddElement(new PlotElementNoteOption("Cause or Author of the Mistake"));
            archetype.AddElement(new PlotElementNoteOption("Guilty Person"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Remorse") { IsChoice = true };
            archetype.AddChild(new NoteOption("Remorse for an Unknown Crime"));
            archetype.AddChild(new NoteOption("Remorse for Killing a Kinsman"));
            archetype.AddChild(new NoteOption("Remorse for an Assassination"));
            archetype.AddChild(new NoteOption("Remorse for a Fault of Love"));
            archetype.AddChild(new NoteOption("Remorse for an Adultery"));
            archetype.AddElement(new PlotElementNoteOption("Culprit"));
            archetype.AddElement(new PlotElementNoteOption("Victim or Sin"));
            archetype.AddElement(new PlotElementNoteOption("Interrogator"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Recovery of a Lost One") { IsChoice = true };
            archetype.AddChild(new NoteOption("A Child Stolen"));
            archetype.AddChild(new NoteOption("Unjust Imprisonment"));
            archetype.AddChild(new NoteOption("A Child Searches to Discover a Parent"));
            archetype.AddElement(new PlotElementNoteOption("Seeker"));
            archetype.AddElement(new PlotElementNoteOption("One Found"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);

            archetype = new PlotArchetypeNoteOption("Loss of Loved Ones") { IsChoice = true };
            archetype.AddChild(new NoteOption("Witnessing the Slaying of Kinsmen While Powerless to Prevent It"));
            archetype.AddChild(new NoteOption("Helping to Bring Misfortune Upon One's People Through Professional Secrecy"));
            archetype.AddChild(new NoteOption("Divining the Death of a Loved One"));
            archetype.AddChild(new NoteOption("Learning of the Death of a Kinsman or Ally, and Lapsing into Despair"));
            archetype.AddElement(new PlotElementNoteOption("Kinsman Slain"));
            archetype.AddElement(new PlotElementNoteOption("Kinsman Spectator"));
            archetype.AddElement(new PlotElementNoteOption("Executioner"));
            defaultTemplate.StoryTemplate.PlotArchetypes.AddChild(archetype);
        }

        private static void AddDefaultProtagonists(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.StoryTemplate.Protagonists = new List<string>()
            {
                "volunteers for a job",
                "decides s/he wants revenge",
                "decides s/he must get justice",
                "takes up a new hobby",
                "agrees to a dare",
                "takes a bet",
                "realizes s/he must prove her/himself",
                "refuses to get on an airplane",
                "misses her bus",
                "can't get a cab",
                "misses her train",
                "is mugged",
                "breaks up with a romantic partner",
                "realizes s/he has to save the marriage",
                "answers a newspaper ad",
                "opens an email",
                "meets a hero",
                "tries to overcome a personal fear",
                "gets too curious",
                "opens someone else's mail",
                "opens a package intended for someone else",
                "says the wrong thing while going through airport security",
                "is told s/he must get married immediately",
                "tries to return a lost object",
                "tries to help a lost child",
                "swears to remain single",
                "tries to pick up a reward",
                "agrees to do a favor",
                "is forced to accept a job",
                "joins a new club",
                "meets someone from the internet",
                "tries to cover something up",
                "renovates an old house",
                "chooses a particular book at the library",
                "overhears a discussion",
                "tries to stop a robbery",
                "is abducted",
                "realizes s/he has a supernatural power",
                "is reunited with an old friend",
                "receives a strange object in the mail",
                "is involved in a car accident",
                "stops to help someone in a car accident",
                "inherits an object",
                "breaks a mirror",
                "signs on for a reality show",
                "goes on a cruise",
                "goes on a blind date",
                "recognizes someone who doesn't want to be recognized",
                "reads his/her own obituary",
                "asks about the wrong person",
                "starts at a new school",
                "finds a gun in a partner's nightstand drawer",
                "finds a sword in a partner's closet",
                "walks in on the wrong meeting at work",
                "goes to the hospital for help",
                "hires a lawyer",
                "is charged with jaywalking",
                "shoplifts",
                "kisses a stranger",
                "goes to a party",
                "drives down the wrong street",
                "buys a new car",
                "moves into a new home",
                "tears down a wall",
                "gets lost",
                "finds the car won't start",
                "hitchhikes",
                "hides in a closet",
                "tells the truth",
                "comes out of the closet",
                "admits to a fantasy",
                "cheats on a partner",
                "receives a strange e-mail",
                "finds a strange website",
                "joins an online dating site",
                "tries speed dating",
                "gets drunk",
                "breaks a promise",
                "tells a secret",
                "tells on someone else",
                "uses someone else to get ahead",
                "accepts a new job",
                "pretends to be sick",
                "tries to pay off debt",
                "returns to a childhood home",
                "goes back to school",
                "spies on someone",
                "bluffs through something",
                "cheats on a partner",
                "finds an old book on a friend's shelf",
                "buys a strange figurine at a shop",
                "goes to a palm reader",
                "wakes up in the wrong bed",
                "opens the closet to find clothes that aren't his/hers",
                "begins having nightmares",
                "is forced into a car at gunpoint",
                "is told by a parent that everything has been a lie",
                "walks out on a partner",
                "hires a lawyer",
                "complains to a neighbor about the loud TV"
            };
        }

        private static void AddDefaultSupportingCharacters(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.StoryTemplate.SupportingCharacters = new List<string>()
            {
                "someone your protagonist dated",
                "a thief",
                "a psychic",
                "a computer hacker",
                "an inventor",
                "a con artist",
                "an incredibly attractive person",
                "a killer",
                "someone from another galaxy",
                "a clone",
                "a magic user",
                "someone from the past",
                "someone from the future",
                "a vampire",
                "a hacker",
                "a marksman",
                "a politician",
                "a prostitute",
                "a geneticist",
                "an alchemist",
                "a researcher",
                "an alien entity",
                "a competitor",
                "royalty",
                "a necromancer",
                "a square",
                "an unscrupulous sort",
                "a loner",
                "a student",
                "an arsonist",
                "a bartender",
                "a bus driver",
                "a doctor",
                "an anesthesist",
                "an actor",
                "a bishop",
                "a priest",
                "a pilot",
                "a courier",
                "a journalist",
                "an office manager",
                "a deputy",
                "a producer",
                "an artist",
                "a disc jockey",
                "a detective",
                "a model",
                "a hit man",
                "an executive",
                "a fortune-teller",
                "a geologist",
                "a janitor",
                "a judge",
                "a zoologist",
                "a college professor",
                "a school teacher",
                "a mortician",
                "a painter",
                "a pilot",
                "a soldier",
                "a golf pro",
                "a mechanic",
                "a bounty hunter",
                "a mind reader",
                "a meterologist",
                "a messenger",
                "a prison guard",
                "a pharmacist",
                "a police officer",
                "a psychologist",
                "a spy",
                "a ski instructor",
                "a safecracker",
                "a surgeon",
                "a tailor",
                "a sports editor",
                "an athlete",
                "a letter carrier",
                "a taxi driver",
                "a doorman",
                "a foreign diplomat",
                "a weather announcer",
                "a test pilot",
                "a fighter pilot",
                "an assassin",
                "a waiter",
                "a homeless person",
                "a runaway",
                "a martial artist",
                "a gypsy",
                "an attorney",
                "an activist"
            };
        }

        private static void AddDefaultConflicts(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.StoryTemplate.Conflicts = new List<string>()
            {
                "has been following your protagonist for years",
                "wants something your protagonist owns",
                "plans to use your protagonist as a scapegoat",
                "is trying to hide the truth",
                "is trying to reveal the truth",
                "has no problems with blackmail",
                "has been spying on your protagonist",
                "is trying to escape a bigger enemy",
                "needs someone to replace him/her",
                "is hiding a body",
                "needs money",
                "is determined to settle an old score",
                "is power-hungry",
                "has discovered your protagonist's secret",
                "once hitched a ride with your protagonist",
                "is your protagonist's favorite author",
                "is your protagonist's favorite musician",
                "is a movie star",
                "was forced to commit a crime",
                "has a secret power",
                "is psychic",
                "can move things by thinking about it",
                "wants your protagonist's job",
                "thinks s/he is in competition with your protagonist",
                "is playing a joke that's gone wrong",
                "is trying to cover up a previous crime",
                "believes your protagonist is a long-lost family member",
                "is trying to escape the past",
                "is deeply involved in the occult",
                "thinks s/he is just doing his/her duty",
                "wants some kind of revenge",
                "wants your protagonist dead",
                "wants your protagonist to hide him/her",
                "believes s/he is the reincarnation of a famous person",
                "claims that s/he has been alive since the 1500s",
                "invents strange machines",
                "is incredibly charming",
                "is the most attractive person your protagonist has ever met",
                "says s/he wants to save your protagonist",
                "believes s/he is from another galaxy",
                "believes your protagonist has wronged him/her",
                "has stolen a photograph that belongs to your protagonist",
                "killed someone your protagonist knows",
                "stole money from your protagonist",
                "claimed your protagonist was responsible for some crime s/he committed",
                "has told your protagonist's secret to the press",
                "believes your protagonist owes him/her money",
                "believes your protagonist has something that belongs to him/her",
                "has been blogging about your protagonist online",
                "has been tapping your protagonist's phone",
                "stole your protagonist's cell phone",
                "has a collection of photos of your protagonist",
                "put poison in your protagonist's food",
                "is developing a new weapon",
                "is developing a deadly new poison",
                "was bioengineered",
                "owns a restaurant your protagonist often visits",
                "wants your protagonist fired",
                "has been stealing your protagonist's mail",
                "has been reading your protagonist's e-mail",
                "put a bomb in your protagonist's car",
                "has been forging your protagonist's name",
                "just got out of prison",
                "was just thrown out of school",
                "is carrying a deadly disease",
                "has engineered a deadly disease",
                "wants something your protagonist knows",
                "intends to use your protagonist's talents for evil",
                "uses dark magic",
                "has cast a spell on your protagonist's home",
                "put a curse on your protagonist",
                "is from the past",
                "is from the future",
                "is determined to move in with your protagonist",
                "has just gotten a place next door",
                "has amnesia",
                "has abducted your protagonist's friend",
                "is interfering with your protagonist's job",
                "is stalking your protagonist",
                "creates monsters",
                "can see the future",
                "is researching something terrible",
                "has slandered your protagonist",
                "is determined to make your protagonist look bad",
                "wants your protagonist to go on a road trip",
                "is your protagonist's biggest competition",
                "wants everything done by the rules",
                "is determined to break all the rules",
                "doesn't know how to tell your protagonist something important",
                "wants to break the rules",
                "always shuns other people",
                "desperately needs to be around other people",
                "always ignores the facts",
                "wants revenge",
                "is sensitive to others' auras",
                "has a scathing wit",
                "can travel through time",
                "has supernatural healing powers",
                "run away from the past",
                "bites his/her fingernails",
                "always loses arguments",
                "always loses his/her temper",
                "doesn't like animals",
                "has a gift for poetry",
                "is fluent in six languages",
                "has perfect night vision",
                "has a great body",
                "is skilled in archery"
            };
        }

        private static void AddDefaultBackgrounds(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.StoryTemplate.Backgrounds.AddChild(new NoteOption("No significant background; all significant plot events occur during the main story"));
            defaultTemplate.StoryTemplate.Backgrounds.AddChild(new NoteOption("The protagonist's actions are set against an ongoing conflict, but do not directly affect it"));
            defaultTemplate.StoryTemplate.Backgrounds.AddChild(new NoteOption("The protagonist is drawn into an ongoing conflict"));
            defaultTemplate.StoryTemplate.Backgrounds.AddChild(new NoteOption("The protagonist incites a new conflict among established rivals"));
        }

        private static void AddDefaultResolutions(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.StoryTemplate.Resolutions.AddChild(new NoteOption("The protagonist achieves personal goals, but the driving conflict of the setting remains unresolved"));
            defaultTemplate.StoryTemplate.Resolutions.AddChild(new NoteOption("The principal conflict of the story is resolved by a dramatic event, outside the control of the protagonist"));
            defaultTemplate.StoryTemplate.Resolutions.AddChild(new NoteOption("The protagonist makes peace betweeen the principal rivals in the story's major conflict"));
            defaultTemplate.StoryTemplate.Resolutions.AddChild(new NoteOption("The protagonist resolves the story's major conflicts by overcoming all principal rivals"));
        }

        private static void AddDefaultStoryTraits(IdeaTreeTemplate defaultTemplate)
        {
            NoteOption trait = new NoteOption("Tone") { IsChoice = true };
            trait.AddChild(new NoteOption("Comedic"));
            trait.AddChild(new NoteOption("Light-hearted"));
            trait.AddChild(new NoteOption("Dramatic"));
            trait.AddChild(new NoteOption("Dark"));
            defaultTemplate.StoryTemplate.Traits.AddChild(trait);
            
            trait = new NoteOption("Cast Size") { IsChoice = true };
            trait.AddChild(new NoteOption("Solitary Protagonist"));
            NoteOption subTrait = new NoteOption("Duo") { IsChoice = true };
            subTrait.AddChild(new NoteOption("Partners"));
            subTrait.AddChild(new NoteOption("Rivals"));
            trait.AddChild(subTrait);
            trait.AddChild(new NoteOption("Small Cast"));
            trait.AddChild(new NoteOption("Medium Cast"));
            subTrait = new NoteOption("Large Cast") { IsChoice = true };
            subTrait.AddChild(new NoteOption("Protagonist with supporting cast"));
            subTrait.AddChild(new NoteOption("Ensemble"));
            trait.AddChild(subTrait);
            defaultTemplate.StoryTemplate.Traits.AddChild(trait);
        }

        private static void AddDefaultSettings(IdeaTreeTemplate defaultTemplate)
        {
            NoteOption setting = new NoteOption("Present") { IsChoice = true, IsMultiSelect = true, Weight = 20 };

            NoteOption location = new NoteOption("Home") { IsChoice = true, IsMultiSelect = true, Weight = 75 };
            location.AddChild(new NoteOption("City") { Weight = 50 });
            location.AddChild(new NoteOption("Remote") { Weight = 5 });
            location.AddChild(new NoteOption("Rural") { Weight = 5 });
            location.AddChild(new NoteOption("Suburban") { Weight = 50 });
            setting.AddChild(location);

            location = new NoteOption("Lodging") { IsChoice = true, IsMultiSelect = true, Weight = 25 };
            location.AddChild(new NoteOption("Bed and Breakfast") { Weight = 5 });
            location.AddChild(new NoteOption("Cabin") { Weight = 5 });
            location.AddChild(new NoteOption("Country Club") { Weight = 5 });
            location.AddChild(new NoteOption("Cruise") { Weight = 5 });
            location.AddChild(new NoteOption("Hotel") { Weight = 10 });
            location.AddChild(new NoteOption("Houseguest") { Weight = 5 });
            location.AddChild(new NoteOption("Resort") { Weight = 5 });
            location.AddChild(new NoteOption("Ski Lodge") { Weight = 5 });
            setting.AddChild(location);

            location = new NoteOption("Medical") { IsChoice = true, IsMultiSelect = true, Weight = 10 };
            location.AddChild(new NoteOption("Doctor's Office") { Weight = 3 });
            location.AddChild(new NoteOption("Hospital") { Weight = 3 });
            location.AddChild(new NoteOption("Research Facility"));
            setting.AddChild(location);

            location = new NoteOption("Nature") { IsChoice = true, IsMultiSelect = true, Weight = 25 };
            location.AddChild(new NoteOption("Beach") { Weight = 15 });
            location.AddChild(new NoteOption("Forest") { Weight = 10 });
            location.AddChild(new NoteOption("Desert"));
            location.AddChild(new NoteOption("Lake") { Weight = 10 });
            location.AddChild(new NoteOption("Mountain") { Weight = 10 });
            location.AddChild(new NoteOption("Park") { Weight = 15 });
            location.AddChild(new NoteOption("River") { Weight = 10 });
            location.AddChild(new NoteOption("Sea") { Weight = 10 });
            setting.AddChild(location);

            location = new NoteOption("Recreational") { IsChoice = true, IsMultiSelect = true, Weight = 25 };
            location.AddChild(new NoteOption("Bar") { Weight = 18 });
            location.AddChild(new NoteOption("Movie Theater") { Weight = 18 });
            location.AddChild(new NoteOption("Nightclub") { Weight = 18 });
            location.AddChild(new NoteOption("Opera") { Weight = 4 });
            location.AddChild(new NoteOption("Restaurant") { Weight = 18 });
            location.AddChild(new NoteOption("Spa") { Weight = 6 });
            location.AddChild(new NoteOption("Symphony") { Weight = 4 });
            location.AddChild(new NoteOption("Theater") { Weight = 4 });
            setting.AddChild(location);

            location = new NoteOption("School") { IsChoice = true, IsMultiSelect = true, Weight = 25 };
            location.AddChild(new NoteOption("College") { Weight = 3 });
            location.AddChild(new NoteOption("High School") { Weight = 3 });
            location.AddChild(new NoteOption("Technical/Specialized School"));
            setting.AddChild(location);
            
            setting.AddChild(new NoteOption("Work") { Weight = 50 });

            // Only major countries (with >5 million citizens) are included in the default template,
            // for simplicity's sake.
            location = new NoteOption("Abroad") { IsChoice = true, IsManualMultiSelect = true, Weight = 5 };
            NoteOption subLocation = new NoteOption("Africa") { IsChoice = true, IsManualMultiSelect = true };
            NoteOption subSubLocation = new NoteOption("Central Africa") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Angola"));
            subSubLocation.AddChild(new NoteOption("Cameroon"));
            subSubLocation.AddChild(new NoteOption("Chad"));
            subSubLocation.AddChild(new NoteOption("Democratic Republic of the Congo"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("East Africa") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Burundi"));
            subSubLocation.AddChild(new NoteOption("Eritrea"));
            subSubLocation.AddChild(new NoteOption("Ethiopia"));
            subSubLocation.AddChild(new NoteOption("Kenya"));
            subSubLocation.AddChild(new NoteOption("Madagascar"));
            subSubLocation.AddChild(new NoteOption("Malawi"));
            subSubLocation.AddChild(new NoteOption("Mozambique"));
            subSubLocation.AddChild(new NoteOption("Rwanda"));
            subSubLocation.AddChild(new NoteOption("Somalia"));
            subSubLocation.AddChild(new NoteOption("South Sudan"));
            subSubLocation.AddChild(new NoteOption("Uganda"));
            subSubLocation.AddChild(new NoteOption("Tanzania"));
            subSubLocation.AddChild(new NoteOption("Zambia"));
            subSubLocation.AddChild(new NoteOption("Zimbabwe"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("North Africa") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Algeria"));
            subSubLocation.AddChild(new NoteOption("Egypt"));
            subSubLocation.AddChild(new NoteOption("Libya"));
            subSubLocation.AddChild(new NoteOption("Morocco"));
            subSubLocation.AddChild(new NoteOption("Sudan"));
            subSubLocation.AddChild(new NoteOption("Tunisia"));
            subLocation.AddChild(subSubLocation);
            subLocation.AddChild(new NoteOption("South Africa"));
            subSubLocation = new NoteOption("West Africa") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Benin"));
            subSubLocation.AddChild(new NoteOption("Burkina Faso"));
            subSubLocation.AddChild(new NoteOption("Côte d'Ivoire"));
            subSubLocation.AddChild(new NoteOption("Ghana"));
            subSubLocation.AddChild(new NoteOption("Guinea"));
            subSubLocation.AddChild(new NoteOption("Mali"));
            subSubLocation.AddChild(new NoteOption("Niger"));
            subSubLocation.AddChild(new NoteOption("Nigeria"));
            subSubLocation.AddChild(new NoteOption("Senegal"));
            subSubLocation.AddChild(new NoteOption("Sierra Leone"));
            subSubLocation.AddChild(new NoteOption("Togo"));
            subLocation.AddChild(subSubLocation);
            location.AddChild(subLocation);
            subLocation = new NoteOption("America") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation = new NoteOption("Caribbean") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true };
            subSubLocation.AddChild(new NoteOption("Cuba"));
            subSubLocation.AddChild(new NoteOption("Dominican Republic"));
            subSubLocation.AddChild(new NoteOption("Haiti"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("Central America") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("El Salvador"));
            subSubLocation.AddChild(new NoteOption("Guatemala"));
            subSubLocation.AddChild(new NoteOption("Honduras"));
            subSubLocation.AddChild(new NoteOption("Nicaragua"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("North America") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Canada"));
            subSubLocation.AddChild(new NoteOption("Mexico"));
            // The default template assumes a "home" location in the U.S., and therefore does not consider it "abroad."
            // When localizing the template for other locations, this weight should be re-set to 1, and the weight of
            // the local "home" region set to 0 instead.
            subSubLocation.AddChild(new NoteOption("United States") { Weight = 0 });
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("South America") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Argentina"));
            subSubLocation.AddChild(new NoteOption("Bolivia"));
            subSubLocation.AddChild(new NoteOption("Brazil"));
            subSubLocation.AddChild(new NoteOption("Chile"));
            subSubLocation.AddChild(new NoteOption("Colombia"));
            subSubLocation.AddChild(new NoteOption("Ecuador"));
            subSubLocation.AddChild(new NoteOption("Paraguay"));
            subSubLocation.AddChild(new NoteOption("Peru"));
            subSubLocation.AddChild(new NoteOption("Venezuela"));
            subLocation.AddChild(subSubLocation);
            location.AddChild(subLocation);
            subLocation = new NoteOption("Asia") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation = new NoteOption("Central Asia") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Afghanistan"));
            subSubLocation.AddChild(new NoteOption("Kazakhstan"));
            subSubLocation.AddChild(new NoteOption("Kyrgyzstan"));
            subSubLocation.AddChild(new NoteOption("Tajikistan"));
            subSubLocation.AddChild(new NoteOption("Turkmenistan"));
            subSubLocation.AddChild(new NoteOption("Uzbekistan"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("East Asia") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("China"));
            subSubLocation.AddChild(new NoteOption("Japan"));
            subSubLocation.AddChild(new NoteOption("Korea"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("Southeast Asia") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Cambodia"));
            subSubLocation.AddChild(new NoteOption("Indonesia"));
            subSubLocation.AddChild(new NoteOption("Malaysia"));
            subSubLocation.AddChild(new NoteOption("The Philipines"));
            subSubLocation.AddChild(new NoteOption("Thailand"));
            subSubLocation.AddChild(new NoteOption("Vietnam"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("South Asia") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Bangladesh"));
            subSubLocation.AddChild(new NoteOption("India"));
            subSubLocation.AddChild(new NoteOption("Nepal"));
            subSubLocation.AddChild(new NoteOption("Pakistan"));
            subLocation.AddChild(subSubLocation);
            location.AddChild(subLocation);
            subLocation = new NoteOption("Europe") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation = new NoteOption("Eastern Europe") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Belarus"));
            subSubLocation.AddChild(new NoteOption("Bulgaria"));
            subSubLocation.AddChild(new NoteOption("Czech Republic"));
            subSubLocation.AddChild(new NoteOption("Hungary"));
            subSubLocation.AddChild(new NoteOption("Poland"));
            subSubLocation.AddChild(new NoteOption("Romania"));
            subSubLocation.AddChild(new NoteOption("Russia"));
            subSubLocation.AddChild(new NoteOption("Slovakia"));
            subSubLocation.AddChild(new NoteOption("Ukraine"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("Northern Europe") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Denmark"));
            subSubLocation.AddChild(new NoteOption("England"));
            subSubLocation.AddChild(new NoteOption("Finland"));
            subSubLocation.AddChild(new NoteOption("Ireland"));
            subSubLocation.AddChild(new NoteOption("Norway"));
            subSubLocation.AddChild(new NoteOption("Scotland"));
            subSubLocation.AddChild(new NoteOption("Sweden"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("Southern Europe") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Greece"));
            subSubLocation.AddChild(new NoteOption("Italy"));
            subSubLocation.AddChild(new NoteOption("Portugal"));
            subSubLocation.AddChild(new NoteOption("Serbia"));
            subSubLocation.AddChild(new NoteOption("Spain"));
            subLocation.AddChild(subSubLocation);
            subSubLocation = new NoteOption("Western Europe") { IsChoice = true, IsManualMultiSelect = true };
            subSubLocation.AddChild(new NoteOption("Austria"));
            subSubLocation.AddChild(new NoteOption("Belgium"));
            subSubLocation.AddChild(new NoteOption("France"));
            subSubLocation.AddChild(new NoteOption("Germany"));
            subSubLocation.AddChild(new NoteOption("Netherlands"));
            subSubLocation.AddChild(new NoteOption("Switzerland"));
            subLocation.AddChild(subSubLocation);
            location.AddChild(subLocation);
            subLocation = new NoteOption("Middle East") { IsChoice = true, IsManualMultiSelect = true };
            subLocation.AddChild(new NoteOption("Azerbaijan"));
            subLocation.AddChild(new NoteOption("Iran"));
            subLocation.AddChild(new NoteOption("Iraq"));
            subLocation.AddChild(new NoteOption("Israel"));
            subLocation.AddChild(new NoteOption("Jordan"));
            subLocation.AddChild(new NoteOption("Lebanon"));
            subLocation.AddChild(new NoteOption("Palestine"));
            subLocation.AddChild(new NoteOption("Saudi Arabia"));
            subLocation.AddChild(new NoteOption("Syria"));
            subLocation.AddChild(new NoteOption("Turkey"));
            subLocation.AddChild(new NoteOption("United Arab Emirates"));
            subLocation.AddChild(new NoteOption("Yemen"));
            location.AddChild(subLocation);
            subLocation = new NoteOption("Oceania") { IsChoice = true, IsManualMultiSelect = true };
            subLocation.AddChild(new NoteOption("Australia"));
            subSubLocation = new NoteOption("Melanesia") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true };
            subSubLocation.AddChild(new NoteOption("Papua New Guinea"));
            subLocation.AddChild(subSubLocation);
            subLocation.AddChild(new NoteOption("Micronesia"));
            subLocation.AddChild(new NoteOption("New Zealand"));
            location.AddChild(subLocation);
            setting.AddChild(location);

            defaultTemplate.StoryTemplate.Settings.AddChild(setting);

            setting = new NoteOption("Ancient") { IsChoice = true, IsMultiSelect = true };
            setting.AddChild(new NoteOption("Hyborian"));
            setting.AddChild(new NoteOption("Celtic"));
            setting.AddChild(new NoteOption("Egyptian"));
            setting.AddChild(new NoteOption("Greco-Roman"));
            setting.AddChild(new NoteOption("Scandinavian"));
            setting.AddChild(new NoteOption("Stone Age"));
            defaultTemplate.StoryTemplate.Settings.AddChild(setting);

            setting = new NoteOption("Medieval") { IsChoice = true, IsMultiSelect = true };
            setting.AddChild(new NoteOption("Arabia"));
            setting.AddChild(new NoteOption("Asia"));
            setting.AddChild(new NoteOption("Europe"));
            defaultTemplate.StoryTemplate.Settings.AddChild(setting);

            setting = new NoteOption("Renaissance") { IsChoice = true, IsMultiSelect = true };
            setting.AddChild(new NoteOption("High Seas"));
            setting.AddChild(new NoteOption("New World"));
            setting.AddChild(new NoteOption("Noble Court"));
            defaultTemplate.StoryTemplate.Settings.AddChild(setting);

            setting = new NoteOption("Victorian") { IsChoice = true, IsMultiSelect = true };
            setting.AddChild(new NoteOption("City"));
            setting.AddChild(new NoteOption("Colony"));
            setting.AddChild(new NoteOption("Country Manor"));
            setting.AddChild(new NoteOption("Courtly Society"));
            setting.AddChild(new NoteOption("Wild West"));
            defaultTemplate.StoryTemplate.Settings.AddChild(setting);

            setting = new NoteOption("Future") { IsChoice = true, IsMultiSelect = true };
            setting.AddChild(new NoteOption("Alien Planet"));
            setting.AddChild(new NoteOption("Dystopian Society"));
            setting.AddChild(new NoteOption("Spaceship"));
            setting.AddChild(new NoteOption("Space Station"));
            setting.AddChild(new NoteOption("Utopian Society"));
            defaultTemplate.StoryTemplate.Settings.AddChild(setting);
        }

        private static void AddDefaultThemes(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Alienation") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Beauty") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Betrayal") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Birth") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Capitalism") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Change vs. tradition") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Chaos vs. order") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Class") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Coming of age") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Communication") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Companionship") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Conformity") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Convention vs. rebellion") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Darkness vs. light") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Death") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Discovery") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Disillusionment vs. dreams") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Displacement") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Duty") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Escape") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Faith vs. doubt") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Family") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Fate vs. free will") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Fear") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Femininity") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Fortune") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Fulfillment") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Generation gap") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Good vs. evil") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Greed") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Heroism") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Honor") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Hope vs. despair") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Home") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Humanity vs. nature") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Identity") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Illusion") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Immortality") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Individual vs. society") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Injustice") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Innocence") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Isolation") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Journey") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Judgement") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Knowledge vs. ignorance") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Loneliness") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Loss") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Love") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Manipulation") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Masculinity") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Materialism") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Moral character") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Names") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Nationalism") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Nature") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Oppression") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Parenthood") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Patriotism") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Permanence") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Politics") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Power") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Pride") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Progress") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Race and racism") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Rebirth") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Religion") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Reunion") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Sacrifice") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Self-awareness") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Self-preservation") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Self-reliance") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Simplicity") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Strength") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Survival") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Technology") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Temptation") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Totalitarianism") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Vanity") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Vulnerability") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("War") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Wealth") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Wisdom") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Words") { Weight = 10 });
            defaultTemplate.StoryTemplate.Themes.AddChild(new NoteOption("Youth") { Weight = 10 });
        }

        private static void AddDefaultGenres(IdeaTreeTemplate defaultTemplate)
        {
            NoteOption genre = new NoteOption("Adventure") { Weight = 20, IsChoice = true, IsManualMultiSelect = true };
            genre.AddChild(new NoteOption("Epic"));
            genre.AddChild(new NoteOption("Lost World"));
            genre.AddChild(new NoteOption("Nautical"));
            genre.AddChild(new NoteOption("Picaresque"));
            genre.AddChild(new NoteOption("Robinsonade"));
            genre.AddChild(new NoteOption("Western"));
            defaultTemplate.StoryTemplate.Genres.AddChild(genre);

            genre = new NoteOption("Crime") { Weight = 20, IsChoice = true, IsManualMultiSelect = true };
            genre.AddChild(new NoteOption("Caper"));
            genre.AddChild(new NoteOption("Detective Story"));
            genre.AddChild(new NoteOption("Espionage Thriller"));
            genre.AddChild(new NoteOption("Legal Thriller"));
            genre.AddChild(new NoteOption("Murder Mystery"));
            defaultTemplate.StoryTemplate.Genres.AddChild(genre);

            genre = new NoteOption("Fantasy") { Weight = 20, IsChoice = true, IsManualMultiSelect = true };
            genre.AddChild(new NoteOption("Alternate Reality"));
            genre.AddChild(new NoteOption("Contemporary Fantasy"));
            genre.AddChild(new NoteOption("Dark Fantasy"));
            genre.AddChild(new NoteOption("Dying Earth"));
            genre.AddChild(new NoteOption("Epic / High Fantasy"));
            genre.AddChild(new NoteOption("Fairy Tale"));
            genre.AddChild(new NoteOption("Fantasy Romance"));
            genre.AddChild(new NoteOption("Historical Fantasy"));
            genre.AddChild(new NoteOption("Low Fantasy"));
            genre.AddChild(new NoteOption("Mythic"));
            NoteOption subGenre = new NoteOption("Science Fantasy") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true };
            NoteOption subSubGenre = new NoteOption("Planetary Romance") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true };
            subSubGenre.AddChild(new NoteOption("Sword and Planet"));
            subGenre.AddChild(subSubGenre);
            genre.AddChild(subGenre);
            genre.AddChild(new NoteOption("Superhero"));
            genre.AddChild(new NoteOption("Sword and Sorcery"));
            genre.AddChild(new NoteOption("Urban Fantasy"));
            defaultTemplate.StoryTemplate.Genres.AddChild(genre);

            genre = new NoteOption("Horror") { Weight = 20, IsChoice = true, IsManualMultiSelect = true };
            genre.AddChild(new NoteOption("Body Horror"));
            genre.AddChild(new NoteOption("Gothic"));
            genre.AddChild(new NoteOption("Occult Detective"));
            genre.AddChild(new NoteOption("Psychological Thriller"));
            subGenre = new NoteOption("Supernatural / Paranormal") { IsChoice = true, IsManualMultiSelect = true };
            subGenre.AddChild(new NoteOption("Ghost Story"));
            subGenre.AddChild(new NoteOption("Lovecraftian"));
            subGenre.AddChild(new NoteOption("Paranormal Romance"));
            subSubGenre = new NoteOption("Monster Story") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true };
            subSubGenre.AddChild(new NoteOption("Mummy Story"));
            subSubGenre.AddChild(new NoteOption("Vampire Story"));
            subSubGenre.AddChild(new NoteOption("Werewolf Story"));
            subGenre.AddChild(subSubGenre);
            genre.AddChild(subGenre);
            defaultTemplate.StoryTemplate.Genres.AddChild(genre);

            genre = new NoteOption("Realistic") { Weight = 20, IsChoice = true, IsManualMultiSelect = true };
            genre.AddChild(new NoteOption("Bildungsroman"));
            genre.AddChild(new NoteOption("Biography"));
            genre.AddChild(new NoteOption("Contemporary Romance"));
            subGenre = new NoteOption("Historical Fiction") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true };
            subGenre.AddChild(new NoteOption("Alternative History"));
            subGenre.AddChild(new NoteOption("Historical Romance"));
            subGenre = new NoteOption("Political Fiction") { IsChoice = true, IsManualMultiSelect = true };
            subGenre.AddChild(new NoteOption("Political Thriller"));
            subGenre.AddChild(new NoteOption("Political Satire"));
            genre.AddChild(subGenre);
            genre.AddChild(new NoteOption("Urban Fiction"));
            defaultTemplate.StoryTemplate.Genres.AddChild(genre);

            genre = new NoteOption("Science Fiction") { Weight = 20, IsChoice = true, IsManualMultiSelect = true };
            genre.AddChild(new NoteOption("Alien Invasion"));
            genre.AddChild(new NoteOption("Post-Apocalyptic"));
            genre.AddChild(new NoteOption("Cyberpunk"));
            genre.AddChild(new NoteOption("Dystopian"));
            genre.AddChild(new NoteOption("Space Opera"));
            genre.AddChild(new NoteOption("Steampunk"));
            defaultTemplate.StoryTemplate.Genres.AddChild(genre);
        }

        #endregion Story

        #region Character

        private static void AddDefaultTitles(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.CharacterTemplate.Titles = new List<string>()
            {
                "Abbess",
                "Abbot",
                "Adm.",
                "Admiral",
                "Archbishop",
                "Archduke",
                "Baron",
                "Baroness",
                "Baronet",
                "Bishop",
                "Br.",
                "Brother",
                "Caliph",
                "Capt.",
                "Captain",
                "Cardinal",
                "Cdr.",
                "Chairman",
                "Chairperson",
                "Chairwoman",
                "Coach",
                "Col.",
                "Colonel",
                "Congressman",
                "Congresswoman",
                "Commander",
                "Commodore",
                "Corporal",
                "Councillor",
                "Councilman",
                "Councilor",
                "Councilperson",
                "Councilwoman",
                "Count",
                "Countess",
                "Cpl.",
                "Cpt.",
                "Czar",
                "Czarina",
                "Deacon",
                "Doctor",
                "Dr.",
                "Duchess",
                "Duke",
                "Earl",
                "Emir",
                "Emperor",
                "Empress",
                "Ens.",
                "Ensign",
                "Father",
                "Fr.",
                "Friar",
                "Gen.",
                "General",
                "Governor",
                "Grand Duchess",
                "Grand Duke",
                "Judge",
                "Khan",
                "King",
                "Lady",
                "Lcdr.",
                "Lieutenant",
                "Lieutenant Colonel",
                "Lieutenant Commander",
                "Ltc.",
                "Lt.",
                "Lord",
                "Madam",
                "Maharajah",
                "Maj.",
                "Major",
                "Marchioness",
                "Margrave",
                "Marquess",
                "Marquis",
                "Marquise",
                "Master",
                "Mayor",
                "Minister",
                "Miss",
                "Mister",
                "Mistress",
                "Monsignor",
                "Mother",
                "Mother Superior",
                "Mr.",
                "Mrs.",
                "Ms.",
                "Patriarch",
                "Pope",
                "Prelate",
                "President",
                "Priestess",
                "Prime Minister",
                "Prince",
                "Princess",
                "Principal",
                "Private",
                "Professor",
                "Queen",
                "Rabbi",
                "Rajah",
                "Rev.",
                "Reverend",
                "Senator",
                "Sergeant",
                "Sgt.",
                "Sir",
                "Sister",
                "Sr.",
                "Sultan",
                "Tsar",
                "Tsarina",
                "Viscount",
                "Viscountess"
            };
        }

        private static void AddDefaultSuffixes(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.CharacterTemplate.Suffixes = new List<string>()
            {
                ", Esq.",
                "III",
                "IV",
                ", Jr.",
                "V",
                "VI",
                "VII",
                "VIII",
                "IX",
                "X",
                "XI",
                "XII",
                "XIII",
                "XIV",
                "XV",
                "XVI",
                "XVII",
                "XVIII",
                "XIX",
                "XX",
                "XXI",
                "XXII",
                "XXIII",
                "XXIV",
                "XXV",
                "XXVI",
                "XXVII",
                "XXVIII",
                "XXIX",
                "XXX",
                ", Sr."
            };
        }

        public static void AddDefaultGenders(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.CharacterTemplate.Genders.AddChild(new CharacterGenderOption(CharacterGenderOption.manArchetype)
            { Archetype = CharacterGenderOption.manArchetype, Opposite = CharacterGenderOption.womanArchetype });

            defaultTemplate.CharacterTemplate.Genders.AddChild(new CharacterGenderOption(CharacterGenderOption.womanArchetype)
            { Archetype = CharacterGenderOption.womanArchetype, Opposite = CharacterGenderOption.manArchetype });
        }

        public static bool CreateNameFiles(string path)
        {
            if (!Directory.Exists(path))
            {
                try { Directory.CreateDirectory(path); }
                catch (Exception)
                {
                    return false;
                }
            }
            string uriPath = path.Replace(Path.DirectorySeparatorChar, '/');
            if (!File.Exists($"{path}{Path.DirectorySeparatorChar}female_names.txt"))
            {
                try
                {
                    using (var stream = Application.GetResourceStream(new Uri($"pack://application:,,,/{uriPath}/female_names.txt")).Stream)
                    {
                        using (var writer = new FileStream($"{path}{Path.DirectorySeparatorChar}female_names.txt", FileMode.OpenOrCreate))
                        {
                            stream.CopyTo(writer);
                        }
                    }
                }
                catch (Exception)
                {
                    // As a fallback, try making a dummy file that will allow the program to keep operating without errors, and indicate to the user that the correct file is missing
                    try
                    {
                        File.WriteAllText($"{path}{Path.DirectorySeparatorChar}female_names.txt", "NAME FILE MISSING");
                    }
                    catch (Exception) { } // Ignore continued failure; errors will be shown when generating names from missing files
                }
            }
            if (!File.Exists($"{path}{Path.DirectorySeparatorChar}male_names.txt"))
            {
                try
                {
                    using (var stream = Application.GetResourceStream(new Uri($"pack://application:,,,/{uriPath}/male_names.txt")).Stream)
                    {
                        using (var writer = new FileStream($"{path}{Path.DirectorySeparatorChar}male_names.txt", FileMode.OpenOrCreate))
                        {
                            stream.CopyTo(writer);
                        }
                    }
                }
                catch (Exception)
                {
                    // As a fallback, try making a dummy file that will allow the program to keep operating without errors, and indicate to the user that the correct file is missing
                    try
                    {
                        File.WriteAllText($"{path}{Path.DirectorySeparatorChar}male_names.txt", "NAME FILE MISSING");
                    }
                    catch (Exception) { } // Ignore continued failure; errors will be shown when generating names from missing files
                }
            }
            if (!File.Exists($"{path}{Path.DirectorySeparatorChar}surnames.txt"))
            {
                try
                {
                    using (var stream = Application.GetResourceStream(new Uri($"pack://application:,,,/{uriPath}/surnames.txt")).Stream)
                    {
                        using (var writer = new FileStream($"{path}{Path.DirectorySeparatorChar}surnames.txt", FileMode.OpenOrCreate))
                        {
                            stream.CopyTo(writer);
                        }
                    }
                }
                catch (Exception)
                {
                    // As a fallback, try making a dummy file that will allow the program to keep operating without errors, and indicate to the user that the correct file is missing
                    try
                    {
                        File.WriteAllText($"{path}{Path.DirectorySeparatorChar}surnames.txt", "NAME FILE MISSING");
                    }
                    catch (Exception) { } // Ignore continued failure; errors will be shown when generating names from missing files
                }
            }
            return true;
        }

        public static void AddDefaultRaces(IdeaTreeTemplate defaultTemplate)
        {
            // Racial groups are organized primarily by predominant shared physical characteristics,
            // to simplify the use of modifiers targeted by race which adjust the probability of certain
            // features, such as hair or eye color, etc.

            // Within these physically-organized groups, specific cultural or national backgrounds are
            // provided, with the primary intention of allowing culturally-relevant name generation.

            // Only major countries (with >5 million citizens) are included in the default template,
            // for simplicity's sake.

            // Some major countries are omitted due to a lack of readily-accessible, reliable lists
            // of common local names. Rather than have name selection fall back on the (probably inappropriate)
            // default culture, the country is omitted. It is left to users to add any missing cultures, and
            // appropriate lists of names, on an as-needed basis.

            // There are also a handful of major countries which are omitted because their cultural naming conventions
            // are too different from most other cultures to be generated according to the same 'first name and surname'
            // pattern. It is again left to users to add any missing cultures, and generate names in a manner deemed
            // appropriate, on an as-needed basis.

            #region Asian

            CharacterRaceOption race = new CharacterRaceOption("Asian") { IsChoice = true, IsManualMultiSelect = true };
            CharacterRaceOption subRace = new CharacterRaceOption("Central Asian") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Afghani")
            {
                MaleNameFile = @"NameFiles\Middle Eastern\male_names.txt",
                FemaleNameFile = @"NameFiles\Middle Eastern\female_names.txt",
                SurnameFile = @"NameFiles\Middle Eastern\surnames.txt"
            });
            bool canCreateNames = canCreateNames = CreateNameFiles(@"NameFiles\Middle Eastern");
            subRace.AddChild(new CharacterRaceOption("Kazakh")
            {
                MaleNameFile = @"NameFiles\Asian\Central Asian\Kazakh\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Central Asian\Kazakh\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Central Asian\Kazakh\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Central Asian\Kazakh");
            race.AddChild(subRace);
            subRace = new CharacterRaceOption("Eastern Asian") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Chinese")
            {
                MaleNameFile = @"NameFiles\Asian\Eastern Asian\Chinese\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Eastern Asian\Chinese\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Eastern Asian\Chinese\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Eastern Asian\Chinese");
            subRace.AddChild(new CharacterRaceOption("Japanese")
            {
                MaleNameFile = @"NameFiles\Asian\Eastern Asian\Japanese\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Eastern Asian\Japanese\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Eastern Asian\Japanese\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Eastern Asian\Japanese");
            subRace.AddChild(new CharacterRaceOption("Korean")
            {
                MaleNameFile = @"NameFiles\Asian\Eastern Asian\Korean\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Eastern Asian\Korean\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Eastern Asian\Korean\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Eastern Asian\Korean");
            race.AddChild(subRace);
            subRace = new CharacterRaceOption("Southeast Asian") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Cambodian")
            {
                MaleNameFile = @"NameFiles\Asian\Southeast Asian\Cambodian\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southeast Asian\Cambodian\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southeast Asian\Cambodian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southeast Asian\Cambodian");
            subRace.AddChild(new CharacterRaceOption("Filipino")
            {
                MaleNameFile = @"NameFiles\Asian\Southeast Asian\Filipino\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southeast Asian\Filipino\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southeast Asian\Filipino\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southeast Asian\Filipino");
            subRace.AddChild(new CharacterRaceOption("Indonesian")
            {
                MaleNameFile = @"NameFiles\Asian\Southeast Asian\Indonesian\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southeast Asian\Indonesian\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southeast Asian\Indonesian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southeast Asian\Indonesian");
            subRace.AddChild(new CharacterRaceOption("Malaysian")
            {
                MaleNameFile = @"NameFiles\Asian\Southeast Asian\Malaysian\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southeast Asian\Malaysian\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southeast Asian\Malaysian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southeast Asian\Malaysian");
            subRace.AddChild(new CharacterRaceOption("Thai")
            {
                MaleNameFile = @"NameFiles\Asian\Southeast Asian\Thai\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southeast Asian\Thai\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southeast Asian\Thai\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southeast Asian\Thai");
            subRace.AddChild(new CharacterRaceOption("Vietnamese")
            {
                MaleNameFile = @"NameFiles\Asian\Southeast Asian\Vietnamese\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southeast Asian\Vietnamese\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southeast Asian\Vietnamese\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southeast Asian\Vietnamese");
            race.AddChild(subRace);
            subRace = new CharacterRaceOption("Southern Asian") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Bangladeshi")
            {
                MaleNameFile = @"NameFiles\Asian\Southern Asian\Bangladeshi\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southern Asian\Bangladeshi\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southern Asian\Bangladeshi\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southern Asian\Bangladeshi");
            subRace.AddChild(new CharacterRaceOption("Indian")
            {
                MaleNameFile = @"NameFiles\Asian\Southern Asian\Indian\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southern Asian\Indian\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southern Asian\Indian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southern Asian\Indian");
            subRace.AddChild(new CharacterRaceOption("Nepalese")
            {
                MaleNameFile = @"NameFiles\Asian\Southern Asian\Nepalese\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southern Asian\Nepalese\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southern Asian\Nepalese\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Asian\Southern Asian\Nepalese");
            subRace.AddChild(new CharacterRaceOption("Pakistani")
            {
                MaleNameFile = @"NameFiles\Middle Eastern\male_names.txt",
                FemaleNameFile = @"NameFiles\Middle Eastern\female_names.txt",
                SurnameFile = @"NameFiles\Middle Eastern\surnames.txt"
            });
            race.AddChild(subRace);
            defaultTemplate.CharacterTemplate.Races.AddChild(race);

            #endregion Asian

            #region Black

            race = new CharacterRaceOption("Black") { IsChoice = true, IsManualMultiSelect = true };
            subRace = new CharacterRaceOption("African") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Eastern African")
            {
                MaleNameFile = @"NameFiles\Black\African\Eastern African\male_names.txt",
                FemaleNameFile = @"NameFiles\Black\African\Eastern African\female_names.txt",
                SurnameFile = @"NameFiles\Black\African\Eastern African\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Black\African\Eastern African");
            subRace.AddChild(new CharacterRaceOption("Southern African")
            {
                MaleNameFile = @"NameFiles\Black\African\Southern African\male_names.txt",
                FemaleNameFile = @"NameFiles\Black\African\Southern African\female_names.txt",
                SurnameFile = @"NameFiles\Black\African\Southern African\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Black\African\Southern African");
            subRace.AddChild(new CharacterRaceOption("Western African")
            {
                MaleNameFile = @"NameFiles\Black\African\Western African\male_names.txt",
                FemaleNameFile = @"NameFiles\Black\African\Western African\female_names.txt",
                SurnameFile = @"NameFiles\Black\African\Western African\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Black\African\Western African");
            race.AddChild(subRace);
            subRace = new CharacterRaceOption("American") { IsChoice = true, IsManualMultiSelect = true };
            CharacterRaceOption subSubRace = new CharacterRaceOption("Caribbean Islander") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Cuban")
            {
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            });
            subSubRace.AddChild(new CharacterRaceOption("Dominican")
            {
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            });
            subSubRace.AddChild(new CharacterRaceOption("Haitian")
            {
                MaleNameFile = @"NameFiles\Black\American\Caribbean Islander\Haitian\male_names.txt",
                FemaleNameFile = @"NameFiles\Black\American\Caribbean Islander\Haitian\female_names.txt",
                SurnameFile = @"NameFiles\Black\American\Caribbean Islander\Haitian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Black\American\Caribbean Islander\Haitian");
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("Central American")
            {
                IsChoice = true, IsManualMultiSelect = true,
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            };
            subSubRace.AddChild(new CharacterRaceOption("Salvadorian"));
            subSubRace.AddChild(new CharacterRaceOption("Guatemalan"));
            subSubRace.AddChild(new CharacterRaceOption("Honduran"));
            subSubRace.AddChild(new CharacterRaceOption("Nicaraguan"));
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("North American") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Canadian")
            {
                MaleNameFile = @"NameFiles\Caucasian\American\North American\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\American\North American\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\American\North American\surnames.txt"
            });
            subSubRace.AddChild(new CharacterRaceOption("Mexican")
            {
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            });
            subSubRace.AddChild(new CharacterRaceOption("United States African American")
            {
                MaleNameFile = @"NameFiles\Black\American\North American\United States American\male_names.txt",
                FemaleNameFile = @"NameFiles\Black\American\North American\United States American\female_names.txt",
                SurnameFile = @"NameFiles\Black\American\North American\United States American\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Black\American\North American\United States American");
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("South American")
            {
                IsChoice = true,
                IsManualMultiSelect = true,
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            };
            subSubRace.AddChild(new CharacterRaceOption("Argentinian"));
            subSubRace.AddChild(new CharacterRaceOption("Bolivian"));
            subSubRace.AddChild(new CharacterRaceOption("Brazilian")
            {
                MaleNameFile = @"NameFiles\Hispanic\European\Portuguese\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\European\Portuguese\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\European\Portuguese\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Hispanic\European\Portuguese");
            subSubRace.AddChild(new CharacterRaceOption("Chilean"));
            subSubRace.AddChild(new CharacterRaceOption("Colombian"));
            subSubRace.AddChild(new CharacterRaceOption("Ecuadorian"));
            subSubRace.AddChild(new CharacterRaceOption("Peruvian"));
            subSubRace.AddChild(new CharacterRaceOption("Venezuelan"));
            subRace.AddChild(subSubRace);
            race.AddChild(subRace);
            subRace = new CharacterRaceOption("European") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Black English")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Northern European\English\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Northern European\English\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Northern European\English\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Northern European\English");
            subRace.AddChild(new CharacterRaceOption("French")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Western European\French\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Western European\French\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Western European\French\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Western European\French");
            subRace.AddChild(new CharacterRaceOption("Spanish")
            {
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            });
            race.AddChild(subRace);
            defaultTemplate.CharacterTemplate.Races.AddChild(race);

            #endregion Black

            #region Caucasian

            race = new CharacterRaceOption("Caucasian") { IsDefault = true, IsChoice = true, IsManualMultiSelect = true };
            subRace = new CharacterRaceOption("American") { IsDefault = true, IsChoice = true, IsManualMultiSelect = true };
            subSubRace = new CharacterRaceOption("North American")
            {
                IsDefault = true, IsChoice = true, IsManualMultiSelect = true,
                MaleNameFile = @"NameFiles\Caucasian\American\North American\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\American\North American\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\American\North American\surnames.txt"
            };
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\American\North American");
            subSubRace.AddChild(new CharacterRaceOption("Canadian"));
            subSubRace.AddChild(new CharacterRaceOption("United States American") { IsDefault = true });
            subRace.AddChild(subSubRace);
            race.AddChild(subRace);
            race.AddChild(new CharacterRaceOption("Australian")
            {
                MaleNameFile = @"NameFiles\Caucasian\Australian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\Australian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\Australian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\Australian");
            subRace = new CharacterRaceOption("European") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace = new CharacterRaceOption("Eastern European") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Czech")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Czech\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Czech\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Eastern European\Czech\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Eastern European\Czech");
            subSubRace.AddChild(new CharacterRaceOption("Polish")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Polish\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Polish\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Eastern European\Polish\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Eastern European\Polish");
            subSubRace.AddChild(new CharacterRaceOption("Romanian")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Romanian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Romanian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Eastern European\Romanian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Eastern European\Romanian");
            subSubRace.AddChild(new CharacterRaceOption("Russian")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Russian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Russian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Eastern European\Russian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Eastern European\Russian");
            subSubRace.AddChild(new CharacterRaceOption("Ukrainian")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Ukrainian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Eastern European\Ukrainian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Eastern European\Ukrainian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Eastern European\Ukrainian");
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("Northern European") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Danish")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Northern European\Danish\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Northern European\Danish\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Northern European\Danish\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Northern European\Danish");
            subSubRace.AddChild(new CharacterRaceOption("English")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Northern European\English\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Northern European\English\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Northern European\English\surnames.txt"
            });
            subSubRace.AddChild(new CharacterRaceOption("Finnish")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Northern European\Finnish\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Northern European\Finnish\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Northern European\Finnish\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Northern European\Finnish");
            subSubRace.AddChild(new CharacterRaceOption("Irish")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Northern European\Irish\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Northern European\Irish\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Northern European\Irish\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Northern European\Irish");
            subSubRace.AddChild(new CharacterRaceOption("Norwegian")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Northern European\Norwegian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Northern European\Norwegian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Northern European\Norwegian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Northern European\Norwegian");
            subSubRace.AddChild(new CharacterRaceOption("Scottish")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Northern European\Scottish\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Northern European\Scottish\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Northern European\Scottish\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Northern European\Scottish");
            subSubRace.AddChild(new CharacterRaceOption("Swedish")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Northern European\Swedish\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Northern European\Swedish\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Northern European\Swedish\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Northern European\Swedish");
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("Southern European") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Greek")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Southern European\Greek\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Southern European\Greek\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Southern European\Greek\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Southern European\Greek");
            subSubRace.AddChild(new CharacterRaceOption("Italian")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Southern European\Italian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Southern European\Italian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Southern European\Italian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Southern European\Italian");
            subSubRace.AddChild(new CharacterRaceOption("Serbian")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Southern European\Serbian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Southern European\Serbian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Southern European\Serbian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Southern European\Serbian");
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("Western European") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Austrian")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Western European\Austrian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Western European\Austrian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Western European\Austrian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Western European\Austrian");
            subSubRace.AddChild(new CharacterRaceOption("Belgian")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Western European\Belgian\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Western European\Belgian\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Western European\Belgian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Western European\Belgian");
            subSubRace.AddChild(new CharacterRaceOption("Dutch")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Western European\Dutch\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Western European\Dutch\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Western European\Dutch\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Western European\Dutch");
            subSubRace.AddChild(new CharacterRaceOption("French")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Western European\French\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Western European\French\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Western European\French\surnames.txt"
            });
            subSubRace.AddChild(new CharacterRaceOption("German")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Western European\German\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Western European\German\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Western European\German\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Western European\German");
            subSubRace.AddChild(new CharacterRaceOption("Swiss")
            {
                MaleNameFile = @"NameFiles\Caucasian\European\Western European\Swiss\male_names.txt",
                FemaleNameFile = @"NameFiles\Caucasian\European\Western European\Swiss\female_names.txt",
                SurnameFile = @"NameFiles\Caucasian\European\Western European\Swiss\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Caucasian\European\Western European\Swiss");
            subRace.AddChild(subSubRace);
            race.AddChild(subRace);
            defaultTemplate.CharacterTemplate.Races.AddChild(race);

            #endregion Caucasian

            #region Hispanic

            race = new CharacterRaceOption("Hispanic")
            {
                IsChoice = true, IsManualMultiSelect = true,
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            };
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Hispanic");
            subRace = new CharacterRaceOption("American") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace = new CharacterRaceOption("Caribbean Islander") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Cuban"));
            subSubRace.AddChild(new CharacterRaceOption("Dominican"));
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("Central American") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Salvadorian"));
            subSubRace.AddChild(new CharacterRaceOption("Guatemalan"));
            subSubRace.AddChild(new CharacterRaceOption("Honduran"));
            subSubRace.AddChild(new CharacterRaceOption("Nicaraguan"));
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("North American") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Canadian"));
            subSubRace.AddChild(new CharacterRaceOption("Mexican"));
            subSubRace.AddChild(new CharacterRaceOption("United States Hispanic American"));
            subRace.AddChild(subSubRace);
            subSubRace = new CharacterRaceOption("South American") { IsChoice = true, IsManualMultiSelect = true };
            subSubRace.AddChild(new CharacterRaceOption("Argentinian"));
            subSubRace.AddChild(new CharacterRaceOption("Bolivian"));
            subSubRace.AddChild(new CharacterRaceOption("Brazilian")
            {
                MaleNameFile = @"NameFiles\Hispanic\European\Portuguese\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\European\Portuguese\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\European\Portuguese\surnames.txt"
            });
            subSubRace.AddChild(new CharacterRaceOption("Chilean"));
            subSubRace.AddChild(new CharacterRaceOption("Colombian"));
            subSubRace.AddChild(new CharacterRaceOption("Ecuadorian"));
            subSubRace.AddChild(new CharacterRaceOption("Peruvian"));
            subSubRace.AddChild(new CharacterRaceOption("Venezuelan"));
            subRace.AddChild(subSubRace);
            race.AddChild(subRace);
            subRace = new CharacterRaceOption("European") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Portuguese")
            {
                MaleNameFile = @"NameFiles\Hispanic\European\Portuguese\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\European\Portuguese\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\European\Portuguese\surnames.txt"
            });
            subRace.AddChild(new CharacterRaceOption("Spanish"));
            race.AddChild(subRace);
            race.AddChild(new CharacterRaceOption("Filipino")
            {
                MaleNameFile = @"NameFiles\Asian\Southeast Asian\Filipino\male_names.txt",
                FemaleNameFile = @"NameFiles\Asian\Southeast Asian\Filipino\female_names.txt",
                SurnameFile = @"NameFiles\Asian\Southeast Asian\Filipino\surnames.txt"
            });
            defaultTemplate.CharacterTemplate.Races.AddChild(race);

            #endregion Hispanic

            #region Indigenous Peoples

            race = new CharacterRaceOption("Indigenous Peoples") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true };
            subRace = new CharacterRaceOption("Indigenous American") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Indigenous Central American")
            {
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            });
            subRace.AddChild(new CharacterRaceOption("Indigenous North American")
            {
                MaleNameFile = @"NameFiles\Indigenous Peoples\Indigenous American\Indigenous North American\male_names.txt",
                FemaleNameFile = @"NameFiles\Indigenous Peoples\Indigenous American\Indigenous North American\female_names.txt",
                SurnameFile = @"NameFiles\Indigenous Peoples\Indigenous American\Indigenous North American\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Indigenous Peoples\Indigenous American\Indigenous North American");
            subRace.AddChild(new CharacterRaceOption("Indigenous South American")
            {
                MaleNameFile = @"NameFiles\Hispanic\male_names.txt",
                FemaleNameFile = @"NameFiles\Hispanic\female_names.txt",
                SurnameFile = @"NameFiles\Hispanic\surnames.txt"
            });
            race.AddChild(subRace);
            race.AddChild(new CharacterRaceOption("Indigenous Australian")
            {
                MaleNameFile = @"NameFiles\Indigenous Peoples\Indigenous Australian\male_names.txt",
                FemaleNameFile = @"NameFiles\Indigenous Peoples\Indigenous Australian\female_names.txt",
                SurnameFile = @"NameFiles\Indigenous Peoples\Indigenous Australian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Indigenous Peoples\Indigenous Australian");
            race.AddChild(new CharacterRaceOption("Oceania Native")
            {
                MaleNameFile = @"NameFiles\Indigenous Peoples\Oceania Native\male_names.txt",
                FemaleNameFile = @"NameFiles\Indigenous Peoples\Oceania Native\female_names.txt",
                SurnameFile = @"NameFiles\Indigenous Peoples\Oceania Native\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Indigenous Peoples\Oceania Native");
            defaultTemplate.CharacterTemplate.Races.AddChild(race);

            #endregion Indigenous Peoples

            #region Middle Eastern

            race = new CharacterRaceOption("Middle Eastern")
            {
                IsChoice = true, IsManualMultiSelect = true,
                MaleNameFile = @"NameFiles\Middle Eastern\male_names.txt",
                FemaleNameFile = @"NameFiles\Middle Eastern\female_names.txt",
                SurnameFile = @"NameFiles\Middle Eastern\surnames.txt"
            };
            subRace = new CharacterRaceOption("Northern African") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Algerian"));
            subRace.AddChild(new CharacterRaceOption("Egyptian"));
            subRace.AddChild(new CharacterRaceOption("Moroccan"));
            subRace.AddChild(new CharacterRaceOption("Sudanese"));
            subRace.AddChild(new CharacterRaceOption("Tunisian"));
            race.AddChild(subRace);
            subRace = new CharacterRaceOption("Western Asian") { IsChoice = true, IsManualMultiSelect = true };
            subRace.AddChild(new CharacterRaceOption("Azerbaijani")
            {
                MaleNameFile = @"NameFiles\Middle Eastern\Western Asian\Azerbaijani\male_names.txt",
                FemaleNameFile = @"NameFiles\Middle Eastern\Western Asian\Azerbaijani\female_names.txt",
                SurnameFile = @"NameFiles\Middle Eastern\Western Asian\Azerbaijani\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Middle Eastern\Western Asian\Azerbaijani");
            subRace.AddChild(new CharacterRaceOption("Emirati"));
            subRace.AddChild(new CharacterRaceOption("Iranian")
            {
                MaleNameFile = @"NameFiles\Middle Eastern\Western Asian\Iranian\male_names.txt",
                FemaleNameFile = @"NameFiles\Middle Eastern\Western Asian\Iranian\female_names.txt",
                SurnameFile = @"NameFiles\Middle Eastern\Western Asian\Iranian\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Middle Eastern\Western Asian\Iranian");
            subRace.AddChild(new CharacterRaceOption("Iraqi"));
            subRace.AddChild(new CharacterRaceOption("Israeli")
            {
                MaleNameFile = @"NameFiles\Middle Eastern\Western Asian\Israeli\male_names.txt",
                FemaleNameFile = @"NameFiles\Middle Eastern\Western Asian\Israeli\female_names.txt",
                SurnameFile = @"NameFiles\Middle Eastern\Western Asian\Israeli\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Middle Eastern\Western Asian\Israeli");
            subRace.AddChild(new CharacterRaceOption("Jordanian"));
            subRace.AddChild(new CharacterRaceOption("Palestinian"));
            subRace.AddChild(new CharacterRaceOption("Saudi"));
            subRace.AddChild(new CharacterRaceOption("Syrian"));
            subRace.AddChild(new CharacterRaceOption("Turkish")
            {
                MaleNameFile = @"NameFiles\Middle Eastern\Western Asian\Turkish\male_names.txt",
                FemaleNameFile = @"NameFiles\Middle Eastern\Western Asian\Turkish\female_names.txt",
                SurnameFile = @"NameFiles\Middle Eastern\Western Asian\Turkish\surnames.txt"
            });
            if (canCreateNames) canCreateNames = CreateNameFiles(@"NameFiles\Middle Eastern\Western Asian\Israeli");
            subRace.AddChild(new CharacterRaceOption("Yemeni"));
            race.AddChild(subRace);
            defaultTemplate.CharacterTemplate.Races.AddChild(race);

            #endregion Middle Eastern
        }

        public static void AddDefaultAges(IdeaTreeTemplate defaultTemplate)
        {
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MaxAge = 1, Weight = 2000 });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 1, MaxAge = 2, Weight = 2000 });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 3, MaxAge = 12, Weight = 4000 });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 13, MaxAge = 19, Weight = 9000 });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 20, MaxAge = 65, Weight = 30000 });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 66, MaxAge = 75, Weight = 4000 });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 76, MaxAge = 85, Weight = 3000, OmitFromFamilyTree = true });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 86, MaxAge = 90, Weight = 1000, OmitFromFamilyTree = true });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 91, MaxAge = 95, Weight = 500, OmitFromFamilyTree = true });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 96, MaxAge = 100, Weight = 100, OmitFromFamilyTree = true });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 101, MaxAge = 110, Weight = 10, OmitFromFamilyTree = true });
            defaultTemplate.CharacterTemplate.Ages.AddChild(new CharacterAgeOption() { MinAge = 111, MaxAge = 120, OmitFromFamilyTree = true });
        }

        public static void AddDefaultCharacterTraits(IdeaTreeTemplate defaultTemplate)
        {
            CharacterNoteOption trait = new CharacterNoteOption("Height") { IsChoice = true, AllowsNone = true, NoneWeight = 3 };
            CharacterNoteOption subTrait = new CharacterNoteOption("Short");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 2, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Tall");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 2, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            trait.AddChild(subTrait);
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            trait = new CharacterNoteOption("Build") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true, NoneWeight = 90 };
            subTrait = new CharacterNoteOption("Athletic") { Weight = 30 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 15 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 15, MinAge = 40, MaxAge = 59 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, MinAge = 60, MaxAge = 69 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MinAge = 70 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Heavy") { Weight = 30 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 12 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 5, MinAge = 13, MaxAge = 19 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, MinAge = 20, MaxAge = 39 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 18, MinAge = 70 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Muscular") { Weight = 18 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Priority = 20, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Priority = 10, MaxAge = 19 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 5, MinAge = 50, MaxAge = 59 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Priority = 10, MinAge = 60 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Slim") { Weight = 30 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 12 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 45, MinAge = 13, MaxAge = 15 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 45, MinAge = 70 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 90, MinAge = 80 });
            trait.AddChild(subTrait);
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            #region Hair

            trait = new CharacterNoteOption("Hair");
            subTrait = new CharacterNoteOption("Hair Length") { IsChoice = true, AllowsNone = true, NoneWeight = 6 };
            CharacterNoteOption subSubTrait = new CharacterNoteOption("Bald");
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 30, MaxAge = 1, Priority = 0 });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 6, Priority = 10, Races = new ObservableCollection<string>() { "Black" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MinAge = 2, MaxAge = 18, Priority = 20 });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Priority = 30, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 6, MinAge = 50, Priority = 40 });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Long") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 10, Priority = 0 });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MinAge = 70, Priority = 10, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            NoteOptionCharacterModifier modifier = new NoteOptionCharacterModifier() { Weight = 6, Priority = 20, Races = new ObservableCollection<string>() { "Black" } };
            modifier.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(modifier);
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 9, Priority = 30, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Short") { Weight = 6 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 10, Priority = 0 });
            modifier = new NoteOptionCharacterModifier() { Weight = 2, Priority = 20, Races = new ObservableCollection<string>() { "Black" } };
            modifier.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 9, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(modifier);
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Priority = 30, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Hair Texture") { IsChoice = true, AllowsNone = true, NoneWeight = 12 };
            subSubTrait = new CharacterNoteOption("Curly") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Priority = 10, Races = new ObservableCollection<string>() { "Asian", "Indigenous Peoples" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 8, Priority = 20, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Frizzy");
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Priority = 10, Races = new ObservableCollection<string>() { "Asian" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 2, Priority = 20, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Force = true, Priority = 30, Races = new ObservableCollection<string>() { "Black" } });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Silky") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Priority = 10, Races = new ObservableCollection<string>() { "Black" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, Priority = 10, Races = new ObservableCollection<string>() { "Asian", "Indigenous Peoples" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 8, Priority = 20, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Wavy") { Weight = 3 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Priority = 10, Races = new ObservableCollection<string>() { "Black" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Priority = 10, Races = new ObservableCollection<string>() { "Asian" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, Priority = 20, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Hair Color") { IsChoice = true, IsManualMultiSelect = true };
            subSubTrait = new CharacterNoteOption("Black") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 30, Races = new ObservableCollection<string>() { "Asian", "Black" } });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Blond") { IsChoice = true, Weight = 5, AllowsNone = true };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Races = new ObservableCollection<string>() { "Asian", "Black", "Hispanic", "Indigenous Peoples", "Middle Eastern" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 35, Races = new ObservableCollection<string>() { "Caucasian\\European\\Northern European" } });
            subSubTrait.AddChild(new CharacterNoteOption("Ash"));
            subSubTrait.AddChild(new CharacterNoteOption("Dirty"));
            subSubTrait.AddChild(new CharacterNoteOption("Flaxen"));
            subSubTrait.AddChild(new CharacterNoteOption("Golden"));
            subSubTrait.AddChild(new CharacterNoteOption("Platinum"));
            subSubTrait.AddChild(new CharacterNoteOption("Sandy"));
            subSubTrait.AddChild(new CharacterNoteOption("Strawberry"));
            subSubTrait.AddChild(new CharacterNoteOption("Yellow"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Brown") { IsChoice = true, Weight = 10, AllowsNone = true };
            subSubTrait.AddChild(new CharacterNoteOption("Light"));
            subSubTrait.AddChild(new CharacterNoteOption("Dark"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Gray") { IsChoice = true, Weight = 0, AllowsNone = true };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, MinAge = 40, MaxAge = 50, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 20, MinAge = 50, MaxAge = 60, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 30, MinAge = 60, MaxAge = 70, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 60, MinAge = 70, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 5, MinAge = 50, MaxAge = 60 });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, MinAge = 60, MaxAge = 70 });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 30, MinAge = 70, MaxAge = 80 });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 60, MinAge = 80 });
            subSubTrait.AddChild(new CharacterNoteOption("Salt-and-Pepper"));
            subSubTrait.AddChild(new CharacterNoteOption("Silver"));
            subSubTrait.AddChild(new CharacterNoteOption("White"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Red") { IsChoice = true, AllowsNone = true };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Races = new ObservableCollection<string>() { "Asian", "Black", "Indigenous Peoples" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 5, Races = new ObservableCollection<string>() { "Caucasian\\European\\Northern European" } });
            subSubTrait.AddChild(new CharacterNoteOption("Auburn"));
            subSubTrait.AddChild(new CharacterNoteOption("Copper"));
            subSubTrait.AddChild(new CharacterNoteOption("Ginger"));
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Hair Style") { IsChoice = true, AllowsNone = true, NoneWeight = 16 };
            subSubTrait = new CharacterNoteOption("Afro") { Weight = 0 };
            modifier = new NoteOptionCharacterModifier() { Weight = 5, TargetPath = "Hair\\Hair Texture\\Frizzy" };
            modifier.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Length\\Bald" });
            modifier.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Length\\Short" });
            subSubTrait.Modifiers.Add(modifier);
            subSubTrait.AddChild(new CharacterNoteOption("French Braided"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Braided") { Weight = 0, IsChoice = true, AllowsNone = true, NoneWeight = 3 };
            modifier = new NoteOptionCharacterModifier() { Weight = 5, TargetPath = "Hair\\Hair Length\\Long" };
            modifier.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(modifier);
            subSubTrait.AddChild(new CharacterNoteOption("French Braided"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Parted") { Weight = 8, IsChoice = true };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Length\\Bald" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Texture\\Frizzy" });
            subSubTrait.AddChild(new CharacterNoteOption("Parted at the Side"));
            CharacterNoteOption subSubSubTrait = new CharacterNoteOption("Parted in the Middle");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Length\\Short" });
            subSubTrait.AddChild(subSubSubTrait);
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Pinned") { Weight = 3, IsChoice = true };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Length\\Bald" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Length\\Short" });
            subSubTrait.AddChild(new CharacterNoteOption("In a Bun"));
            subSubTrait.AddChild(new CharacterNoteOption("Pinned Back"));
            subSubTrait.AddChild(new CharacterNoteOption("Pinned Up"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Ponytail") { Weight = 0, IsChoice = true, AllowsNone = true, NoneWeight = 3 };
            modifier = new NoteOptionCharacterModifier() { Weight = 8, TargetPath = "Hair\\Hair Length\\Long" };
            modifier.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 4, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(modifier);
            subSubTrait.AddChild(new CharacterNoteOption("Half Ponytail"));
            subSubTrait.AddChild(new CharacterNoteOption("High Ponytail"));
            subSubSubTrait = new CharacterNoteOption("Pigtails");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.AddChild(subSubSubTrait);
            subSubSubTrait = new CharacterNoteOption("Side Ponytail");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.AddChild(subSubSubTrait);
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Spiked") { Weight = 3, IsChoice = true, AllowsNone = true, NoneWeight = 120 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Length\\Bald" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Hair\\Hair Texture\\Frizzy" });
            subSubSubTrait = new CharacterNoteOption("Mohawk");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subSubTrait.AddChild(subSubSubTrait);
            subSubSubTrait = new CharacterNoteOption("Mullet") { Weight = 5 };
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subSubTrait.AddChild(subSubSubTrait);
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            #endregion Hair

            #region Face

            trait = new CharacterNoteOption("Face") { };
            subTrait = new CharacterNoteOption("Eye Color") { IsChoice = true };
            subTrait.AddChild(new NoteOption("Amber") { Weight = 5 });
            subSubTrait = new CharacterNoteOption("Blue") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Races = new ObservableCollection<string>() { "Asian", "Black", "Indigenous Peoples" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Races = new ObservableCollection<string>() { "Hispanic", "Middle Eastern" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 35, Races = new ObservableCollection<string>() { "Caucasian\\European\\Northern European" } });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Brown") { IsChoice = true, Weight = 20, AllowsNone = true };
            subSubSubTrait = new CharacterNoteOption("Dark");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 3, Races = new ObservableCollection<string>() { "Asian", "Indigenous Peoples" } });
            subSubTrait.AddChild(subSubSubTrait);
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Gray") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Races = new ObservableCollection<string>() { "Asian", "Black", "Indigenous Peoples" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Races = new ObservableCollection<string>() { "Hispanic", "Middle Eastern" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 35, Races = new ObservableCollection<string>() { "Caucasian\\European\\Northern European" } });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Green") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Races = new ObservableCollection<string>() { "Asian", "Black", "Indigenous Peoples" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Races = new ObservableCollection<string>() { "Hispanic", "Middle Eastern" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 20, Races = new ObservableCollection<string>() { "Caucasian\\European\\Northern European" } });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Hazel") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Races = new ObservableCollection<string>() { "Asian", "Black", "Indigenous Peoples" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, Races = new ObservableCollection<string>() { "Hispanic", "Middle Eastern" } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, Races = new ObservableCollection<string>() { "Caucasian\\European\\Northern European" } });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Facial Hair") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Beard") { Weight = 5, IsChoice = true, IsManualMultiSelect = true };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subSubTrait.AddChild(new CharacterNoteOption("Chin Strap"));
            subSubTrait.AddChild(new CharacterNoteOption("Full Beard"));
            subSubTrait.AddChild(new CharacterNoteOption("Goatee"));
            subSubTrait.AddChild(new CharacterNoteOption("Muttonchops"));
            subSubTrait.AddChild(new CharacterNoteOption("Soul Patch"));
            subSubTrait.AddChild(new CharacterNoteOption("Van Dyke"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Moustache") { Weight = 5, IsChoice = true, IsManualMultiSelect = true };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subSubTrait.AddChild(new CharacterNoteOption("Brush"));
            subSubTrait.AddChild(new CharacterNoteOption("Fu Manchu"));
            subSubTrait.AddChild(new CharacterNoteOption("Handlebar"));
            subSubTrait.AddChild(new CharacterNoteOption("Horseshoe"));
            subSubTrait.AddChild(new CharacterNoteOption("Pencil-thin"));
            subSubTrait.AddChild(new CharacterNoteOption("Walrus"));
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Features") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subTrait.AddChild(new CharacterNoteOption("Heavy Eyebrows"));
            subTrait.AddChild(new CharacterNoteOption("High Cheekbones"));
            subSubTrait = new CharacterNoteOption("Jaw") { IsChoice = true, IsManualMultiSelect = true };
            subSubSubTrait = new CharacterNoteOption("Strong");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            subSubTrait.AddChild(subSubSubTrait);
            subSubTrait.AddChild(new CharacterNoteOption("Weak"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Lips") { IsChoice = true, IsManualMultiSelect = true };
            subSubSubTrait = new CharacterNoteOption("Bee-Stung");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.AddChild(subSubSubTrait);
            subSubTrait.AddChild(new CharacterNoteOption("Thin"));
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Nose") { IsChoice = true, IsManualMultiSelect = true };
            subSubTrait.AddChild(new CharacterNoteOption("Hooked"));
            subSubTrait.AddChild(new CharacterNoteOption("Large"));
            subSubTrait.AddChild(new CharacterNoteOption("Upturned"));
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            #endregion Face

            trait = new CharacterNoteOption("Complexion") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            trait.AddChild(new NoteOption("Birthmark") { Weight = 2 });
            trait.AddChild(new NoteOption("Freckles") { Weight = 2 });
            subTrait = new CharacterNoteOption("Pale") { Weight = 10 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 20, Races = new ObservableCollection<string>() { "Asian" } });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Races = new ObservableCollection<string>() { "Black" } });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 5, Races = new ObservableCollection<string>() { "Indigenous Peoples", "Hispanic", "Middle Eastern" } });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 20, Races = new ObservableCollection<string>() { "Caucasian\\European\\Northern European" } });
            trait.AddChild(subTrait);
            trait.AddChild(new NoteOption("Scar") { Weight = 2 });
            subTrait = new CharacterNoteOption("Tan") { Weight = 10 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Complexion\\Pale" });
            trait.AddChild(subTrait);
            trait.AddChild(new NoteOption("Tattoo") { Weight = 2 });
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            #region Features

            trait = new CharacterNoteOption("Features");
            subTrait = new CharacterNoteOption("Shoulders") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subTrait.AddChild(new CharacterNoteOption("Broad") { Weight = 10 });
            subSubTrait = new CharacterNoteOption("Narrow") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Features\\Shoulders\\Broad" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Hands") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subTrait.AddChild(new CharacterNoteOption("Big") { Weight = 10 });
            subSubTrait = new CharacterNoteOption("Delicate") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Features\\Hands\\Large" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Belly") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subTrait.AddChild(new CharacterNoteOption("Firm") { Weight = 10 });
            subSubTrait = new CharacterNoteOption("Flabby") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Features\\Belly\\Firm" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Hips") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subTrait.AddChild(new CharacterNoteOption("Narrow") { Weight = 10 });
            subSubTrait = new CharacterNoteOption("Wide") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Features\\Hips\\Narrow" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Legs") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Long") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Height\\Short" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 20, TargetPath = "Height\\Tall" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Short") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Priority = 50, TargetPath = "Features\\Legs\\Long" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Height\\Tall" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 20, TargetPath = "Height\\Short" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            #endregion Features

            #region Personality

            trait = new CharacterNoteOption("Personality") { };

            #region Social

            subTrait = new CharacterNoteOption("Social") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Commanding") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Submissive" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Diffident") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Commanding" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Intense" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Judgmental" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Forceful") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Diffident" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Submissive" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Gregarious") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Antagonistic" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Depressed" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Stoic" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Introverted") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Gregarious" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Outgoing") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Introverted" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Depressed" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Outspoken") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Introverted" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Quiet") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outgoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outspoken" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Reserved") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outgoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outspoken" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Retiring") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Gregarious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outgoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outspoken" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Shy") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Commanding" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Forceful" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Gregarious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outgoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outspoken" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Dominant" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Talkative") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Quiet" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Reserved" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Retiring" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Shy" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Timid") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Commanding" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Forceful" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outspoken" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Dominant" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Intense" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);

            #endregion Social

            #region Interpersonal

            subTrait = new CharacterNoteOption("Interpersonal") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Accommodating") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Judgmental" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Antagonistic") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Gregarious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outgoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Accommodating" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Cold") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Adventurous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Emotional" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Volatile" });
            subTrait.AddChild(subSubTrait);
            subTrait.AddChild(new CharacterNoteOption("Deceitful") { Weight = 2 });
            subSubTrait = new CharacterNoteOption("Dominant") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Shy" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Timid" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Friendly") { Weight = 15 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Antagonistic" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Honest") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Deceitful" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Kind") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Antagonistic" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subTrait.AddChild(subSubTrait);
            subTrait.AddChild(new CharacterNoteOption("Manipulative") { Weight = 2 });
            subTrait.AddChild(new CharacterNoteOption("Passive-Aggressive") { Weight = 2 });
            subSubTrait = new CharacterNoteOption("Polite") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Antagonistic" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Submissive") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Commanding" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Forceful" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Dominant" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Judgmental" });
            subTrait.AddChild(subSubTrait);
            subTrait.AddChild(new CharacterNoteOption("Suspicious") { Weight = 2 });
            subSubTrait = new CharacterNoteOption("Trusting") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Manipulative" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Passive-Aggressive" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Suspicious" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);

            #endregion Interpersonal

            #region Disposition

            subTrait = new CharacterNoteOption("Disposition") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Adventurous") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Calm" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Analytical") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Adventurous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Volatile" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Compulsive") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Adventurous" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Disciplined") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Adventurous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Volatile" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Easygoing") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Analytical" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Compulsive" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Disciplined" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Stoic" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Frivolous") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Analytical" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Compulsive" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Disciplined" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Stoic" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Intense") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Diffident" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Timid" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Judgmental") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Diffident" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Accommodating" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Submissive" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Obsessive") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Resolute") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Frivolous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Volatile" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Stubborn") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);

            #endregion Disposition

            #region Emotion

            subTrait = new CharacterNoteOption("Emotion") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Calm") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Adventurous" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Depressed") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Gregarious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Outgoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Adventurous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Emotional") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Calm" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Stoic") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Gregarious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Frivolous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Emotional" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Volatile") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Analytical" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Disciplined" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Resolute" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Calm" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Stoic" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);

            #endregion Emotion

            #region Self-Image

            subTrait = new CharacterNoteOption("Self-Image") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Conceited") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Shy" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Timid" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Confident") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Timid" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Humble") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Commanding" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Dominant" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Judgmental" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Conceited" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Narcissistic") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Shy" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Timid" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Humble" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Proud") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Humble" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);

            #endregion Self-Image

            #region Generosity

            subTrait = new CharacterNoteOption("Generosity") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Avaricious") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Humble" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Envious") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Humble" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Confident" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Narcissistic" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Generous") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Antagonistic" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Avaricious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Envious" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Greedy") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Humble" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Generous" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Magnanimous") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Antagonistic" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Avaricious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Envious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Greedy" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Philanthropic") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Antagonistic" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Cold" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Avaricious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Envious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Greedy" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Stingy") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Self-Image\\Humble" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Generous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Magnanimous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Philanthropic" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Thrifty") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Generous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Magnanimous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Generosity\\Philanthropic" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);

            #endregion Generosity

            #region Intellect

            subTrait = new CharacterNoteOption("Intellect") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subSubTrait = new CharacterNoteOption("Absent-Minded") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Disciplined" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Airheaded") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Social\\Commanding" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Analytical" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Disciplined" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Intense" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Obsessive" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Manipulative" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Emotion\\Stoic" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Cunning") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Trusting" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Frivolous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Airheaded" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Gullible") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Suspicious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Cunning" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Insightful") { Weight = 5 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Airheaded" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Gullible" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Intelligent") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Airheaded" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Mentally Impaired") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Cunning" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Intelligent" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Naive") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Suspicious" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Cunning" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Insightful" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Sly") { Weight = 2 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Interpersonal\\Trusting" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Frivolous" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Airheaded" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Mentally Impaired" });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Wise") { Weight = 10 };
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Airheaded" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Gullible" });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Intellect\\Naive" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);

            #endregion Intellect

            #region Orientation

            subTrait = new CharacterNoteOption("Orientation") { IsChoice = true };
            subTrait.AddChild(new CharacterOrientationOption("Heterosexual") { Weight = 93, IncludesOpposite = true });
            subTrait.AddChild(new CharacterOrientationOption("Bisexual") { Weight = 3, IncludesAny = true });
            subTrait.AddChild(new CharacterOrientationOption("Homosexual") { Weight = 3, IncludesSame = true });
            subTrait.AddChild(new CharacterOrientationOption("Asexual"));
            trait.AddChild(subTrait);

            #endregion Orientation

            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            #endregion Personality

            #region Appearance

            trait = new CharacterNoteOption("Appearance") { IsChoice = true, IsMultiSelect = true, AllowsNone = true };
            subTrait = new CharacterNoteOption("Fastidious") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Messy") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Disciplined" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Fastidious" });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Neat") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Messy" });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Polished") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Easygoing" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Messy" });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Slovenly") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Disciplined" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Fastidious" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Neat" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Polished" });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Unkempt") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Disposition\\Disciplined" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Fastidious" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Neat" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Polished" });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Well-Groomed") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Messy" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Slovenly" });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Personality\\Appearance\\Unkempt" });
            trait.AddChild(subTrait);
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            #endregion Appearance

            #region Profession

            trait = new CharacterNoteOption("Profession") { IsChoice = true, IsManualMultiSelect = true, AllowsNone = true, NoneWeight = 16 };
            subTrait = new CharacterNoteOption("Academic Administration") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Accountant") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Actor");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 15 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Administrative Assistant") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Advertising") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Aeronautical Engineer");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Airplane Mechanic");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Architect") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Athlete");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Attorney") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Auto Mechanic") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Automotive Engineer");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Bailiff");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Banker") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Bank Teller") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Bartender") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Bounty Hunter");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Carpenter") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Car Salesperson") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Cashier") { Weight = 5 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 15 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Caterer") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("CEO");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("CFO");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("CTO");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Chef") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Chemical Engineer");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Cleric");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Clothier") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Coach") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Craftmaking");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Confectioner");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Construction") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Consultant") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Curator");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Dance Instructor");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Dancer") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Dental Hygienist") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Dentist") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 25 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Doctor") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 27 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Driver") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Editor") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Electrical Engineer") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Enlisted In The Military") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Farmer");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Fisherman");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Government Worker") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Hotel Manager") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Insurance Salesperson") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("IT Technician") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Janitor") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Jeweler");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Journalist") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Judge");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 29 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Legal Aid") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Librarian");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Maid") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Miner");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Musician") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Nurse") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Office Worker") { Weight = 5 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Officer In The Military") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Pilot");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Police Detective");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Police Officer");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Politician");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Private Investigator");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Prostitute");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Postal Worker");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Real Estate Agent") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Retail Salesperson") { Weight = 5 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 15 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Retail Manager") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Retired") { Weight = 0 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 4, MinAge = 55, MaxAge = 65 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Force = true, MinAge = 66 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Salesperson") { Weight = 5 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Shoe Salesperson");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Small Business Owner") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Social Worker") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Software Developer") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Stock Broker") { Weight = 2 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Student") { Weight = 0 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Force = true, MinAge = 5, MaxAge = 17 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, MinAge = 18, MaxAge = 21 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 5, MinAge = 22, MaxAge = 23 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 3, MinAge = 24, MaxAge = 25 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Teacher") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MinAge = 66 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Telemarketer") { Weight = 3 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Travel Agent");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 17 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Veterinarian");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 23 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Wait Staff") { Weight = 4 };
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 15 });
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Writer");
            subTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 21 });
            trait.AddChild(subTrait);
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);

            #endregion

            trait = new CharacterNoteOption("Miscellaneous");
            subTrait = new CharacterNoteOption("Fertility") { IsChoice = true, IsMultiSelect = true, AllowsNone = true, NoneWeight = 90 };
            subSubTrait = new CharacterNoteOption("Infertile");
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 13 });
            subTrait.AddChild(subSubTrait);
            subSubTrait = new CharacterNoteOption("Pregnant");
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Priority = 0, Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, Priority = 10, MaxAge = 13 });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            subTrait = new CharacterNoteOption("Disability") { IsChoice = true, IsMultiSelect = true, AllowsNone = true, NoneWeight = 90 };
            subTrait.AddChild(new CharacterNoteOption("Blind"));
            subTrait.AddChild(new CharacterNoteOption("Deaf"));
            subSubTrait = new CharacterNoteOption("Prosthesis") { IsChoice = true, IsMultiSelect = true };
            subSubTrait.AddChild(new CharacterNoteOption("One Arm"));
            subSubTrait.AddChild(new CharacterNoteOption("One Leg"));
            subSubSubTrait = new CharacterNoteOption("Both Arms");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Miscellaneous\\Disability\\Prosthesis\\One Arm" });
            subSubTrait.AddChild(subSubSubTrait);
            subSubSubTrait = new CharacterNoteOption("Both Legs");
            subSubSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Miscellaneous\\Disability\\Prosthesis\\One Leg" });
            subSubTrait.AddChild(subSubSubTrait);
            subTrait.AddChild(subSubTrait);
            subTrait.AddChild(new CharacterNoteOption("Paraplegic"));
            subSubTrait = new CharacterNoteOption("Quadraplegic");
            subSubTrait.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, TargetPath = "Miscellaneous\\Disability\\Paraplegic" });
            subTrait.AddChild(subSubTrait);
            trait.AddChild(subTrait);
            defaultTemplate.CharacterTemplate.Traits.AddChild(trait);
        }

        public static void AddDefaultRelationships(IdeaTreeTemplate defaultTemplate)
        {
            CharacterRelationshipOption relationship = new CharacterRelationshipOption("Significant Other") { MinAgeOffset = -20, MaxAgeOffset = 20, RequiresOrientationMatch = true };
            CharacterRelationshipOption subRelationship = new CharacterRelationshipOption("Spouse")
            {
                IsChoice = true, Weight = 75, Max = 1, MinAge = 18, SharesMasculineSurname = true,
                ReciprocalRelationship = "Significant Other\\Spouse",
                IncompatibleRelationships = new ObservableCollection<string>() { "Significant Other\\Betrothed", "Significant Other\\Sweetheart" }
            };
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, MaxAge = 24 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 7, MinAge = 25, MaxAge = 29 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 50, MinAge = 36, MaxAge = 39 });
            subRelationship.AddChild(new CharacterRelationshipOption("Husband")
            { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subRelationship.AddChild(new CharacterRelationshipOption("Wife")
            { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            relationship.AddChild(subRelationship);
            subRelationship = new CharacterRelationshipOption("Betrothed")
            {
                IsChoice = true, Weight = 7, Max = 1, MinAge = 18,
                ReciprocalRelationship = "Significant Other\\Betrothed",
                IncompatibleRelationships = new ObservableCollection<string>() { "Significant Other\\Spouse", "Significant Other\\Sweetheart" }
            };
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, MaxAge = 24 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 2, MinAge = 25, MaxAge = 29 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 5, MinAge = 36, MaxAge = 39 });
            subRelationship.AddChild(new CharacterRelationshipOption("Fiancé")
            { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subRelationship.AddChild(new CharacterRelationshipOption("Fiancée")
            { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            relationship.AddChild(subRelationship);
            subRelationship = new CharacterRelationshipOption("Sweetheart")
            {
                IsChoice = true, Weight = 5, Max = 1, MinAge = 13,
                ReciprocalRelationship = "Significant Other\\Sweetheart",
                IncompatibleRelationships = new ObservableCollection<string>() { "Significant Other\\Spouse", "Significant Other\\Betrothed" }
            };
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 13 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 20, MinAge = 16, MaxAge = 29 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, MinAge = 30, MaxAge = 49 });
            subRelationship.AddChild(new CharacterRelationshipOption("Boyfriend")
            { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subRelationship.AddChild(new CharacterRelationshipOption("Girlfriend")
            { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            relationship.AddChild(subRelationship);
            subRelationship = new CharacterRelationshipOption("Lover")
            {
                IsChoice = true, Weight = 2,
                ReciprocalRelationship = "Significant Other\\Lover",
                RequiredRelationships = new ObservableCollection<string>() { "Significant Other\\Spouse" }
            };
            subRelationship.AddChild(new CharacterRelationshipOption("Paramour")
            { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subRelationship.AddChild(new CharacterRelationshipOption("Mistress")
            { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            relationship.AddChild(subRelationship);
            defaultTemplate.CharacterTemplate.Relationships.AddChild(relationship);

            relationship = new CharacterRelationshipOption("Ex") { MinAgeOffset = -15, MaxAgeOffset = 15 };
            subRelationship = new CharacterRelationshipOption("Ex-Spouse") { IsChoice = true, Weight = 50, SecondWeight = 5, Max = 2, MinAge = 18, ReciprocalRelationship = "Ex\\Ex-Spouse"};
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 1, MaxAge = 24 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 3, MinAge = 25, MaxAge = 29 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 10, MinAge = 30, MaxAge = 35 });
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 20, MinAge = 35, MaxAge = 44 });
            subRelationship.AddChild(new CharacterRelationshipOption("Ex-Husband") { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subRelationship.AddChild(new CharacterRelationshipOption("Ex-Wife") { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            relationship.AddChild(subRelationship);
            subRelationship = new CharacterRelationshipOption("Ex-Sweetheart") { IsChoice = true, Weight = 5, ReciprocalRelationship = "Ex\\Ex-Sweetheart"};
            subRelationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 13 });
            subRelationship.AddChild(new CharacterRelationshipOption("Ex-Boyfriend") { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            subRelationship.AddChild(new CharacterRelationshipOption("Ex-Girlfriend") { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            relationship.AddChild(subRelationship);
            defaultTemplate.CharacterTemplate.Relationships.AddChild(relationship);

            relationship = new CharacterRelationshipOption("Child")
            { IsChoice = true, Weight = 2, MinAgeOffset = -45, MaxAgeOffset = -18, IsBloodRelative = true, AlwaysSharesSurname = true, ReciprocalRelationship = "Parent" };
            relationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 0, MaxAge = 29 });
            relationship.Modifiers.Add(new NoteOptionCharacterModifier() { Weight = 85, MinAge = 61 });
            relationship.AddChild(new CharacterRelationshipOption("Son") { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            relationship.AddChild(new CharacterRelationshipOption("Daughter") { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            defaultTemplate.CharacterTemplate.Relationships.AddChild(relationship);

            relationship = new CharacterRelationshipOption("Parent")
            {
                IsChoice = true, MinAgeOffset = 18, MaxAgeOffset = 45, Max = 2, IsBloodRelative = true, SharesFamilySurname = true,
                ReciprocalRelationship = "Child"
            };
            relationship.AddChild(new CharacterRelationshipOption("Father") { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            relationship.AddChild(new CharacterRelationshipOption("Mother") { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            defaultTemplate.CharacterTemplate.Relationships.AddChild(relationship);

            relationship = new CharacterRelationshipOption("Sibling") { IsChoice = true, MaxAgeOffset = 27, MinAgeOffset = -27, IsBloodRelative = true, SharesFamilySurname = true };
            relationship.AddChild(new CharacterRelationshipOption("Brother") { Genders = new ObservableCollection<string>() { CharacterGenderOption.manArchetype } });
            relationship.AddChild(new CharacterRelationshipOption("Sister") { Genders = new ObservableCollection<string>() { CharacterGenderOption.womanArchetype } });
            defaultTemplate.CharacterTemplate.Relationships.AddChild(relationship);

            relationship = new CharacterRelationshipOption("Colleague") { IsFamily = false, Weight = 60, MinAge = 18, MaxAge = 65 };
            relationship.AddChild(new CharacterRelationshipOption("Boss")
            {
                IsFamily = false, Max = 2, Weight = 40, SecondWeight = 20, MinAge = 25, MaxAge = 65, ReciprocalRelationship = "Colleague\\Employee"
            });
            relationship.AddChild(new CharacterRelationshipOption("Employee")
            {
                IsFamily = false, Weight = 10, SecondWeight = 20, ThirdWeight = 50, MinAge = 18, MaxAge = 65, ReciprocalRelationship = "Colleague\\Boss"
            });
            defaultTemplate.CharacterTemplate.Relationships.AddChild(relationship);
            
            defaultTemplate.CharacterTemplate.Relationships.AddChild(new CharacterRelationshipOption("Friend") { IsFamily = false, Weight = 80, MinAgeOffset = -5, MaxAgeOffset = 5 });
            defaultTemplate.CharacterTemplate.Relationships.AddChild(new CharacterRelationshipOption("Neighbor") { IsFamily = false, Max = 5, Weight = 60, MinAge = 18 });
            defaultTemplate.CharacterTemplate.Relationships.AddChild(new CharacterRelationshipOption("Roommate")
            {
                IsFamily = false, Max = 3, Weight = 20, MinAge = 18, MinAgeOffset = -5, MaxAgeOffset = 5
            });
            defaultTemplate.CharacterTemplate.Relationships.AddChild(new CharacterRelationshipOption("Student")
            {
                IsFamily = false, Max = 24, Weight = 5, SecondWeight = 75, ThirdWeight = 100, MinAge = 5, MaxAge = 25, MaxAgeOffset = -6,
                ReciprocalRelationship = "Teacher",
                IncompatibleRelationships = new ObservableCollection<string>() { "Teacher" }
            });
            defaultTemplate.CharacterTemplate.Relationships.AddChild(new CharacterRelationshipOption("Teacher")
            {
                IsFamily = false, Max = 6, Weight = 5, SecondWeight = 80, MinAge = 26, MaxAge = 65, MinAgeOffset = 6,
                ReciprocalRelationship = "Student",
                IncompatibleRelationships = new ObservableCollection<string>() { "Student" }
            });
        }

        #endregion Character

        #endregion Create Default Template
    }
}
