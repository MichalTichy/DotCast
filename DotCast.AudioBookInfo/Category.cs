using System.Reflection;

namespace DotCast.AudioBookInfo
{
    using System.Collections.Generic;
    using System.Reflection;

    public class Category
    {
        public string Name { get; init; }

        private Category()
        {
        }

        public static Category Architecture { get; } = new() {Name = "Architecture"};
        public static Category BiographiesAndMemoirs { get; } = new() {Name = "Biographies and Memoirs"};
        public static Category TravelAndPlaceDescriptions { get; } = new() {Name = "Travel and Place Descriptions"};
        public static Category QuotesAndProverbs { get; } = new() {Name = "Quotes and Proverbs"};
        public static Category DetectiveStoriesCrime { get; } = new() {Name = "Detective Stories, Crime"};
        public static Category Plays { get; } = new() {Name = "Plays"};
        public static Category GirlsNovels { get; } = new() {Name = "Girls' Novels"};
        public static Category Adventure { get; } = new() {Name = "Adventure"};
        public static Category Transportation { get; } = new() {Name = "Transportation"};
        public static Category SpiritualLiterature { get; } = new() {Name = "Spiritual Literature"};
        public static Category HouseAndHome { get; } = new() {Name = "House and Home"};
        public static Category EcologyLivingEnvironment { get; } = new() {Name = "Ecology, Living Environment"};
        public static Category EconomicsAndBusiness { get; } = new() {Name = "Economics and Business"};
        public static Category Encyclopedias { get; } = new() {Name = "Encyclopedias"};
        public static Category Erotica { get; } = new() {Name = "Erotica"};
        public static Category EsotericismAstrologyOccultism { get; } = new() {Name = "Esotericism, Astrology, Occultism"};
        public static Category Fantasy { get; } = new() {Name = "Fantasy"};
        public static Category ColumnsEssays { get; } = new() {Name = "Columns, Essays"};
        public static Category Philosophy { get; } = new() {Name = "Philosophy"};
        public static Category Gamebooks { get; } = new() {Name = "Gamebooks"};
        public static Category HistoricalNovels { get; } = new() {Name = "Historical Novels"};
        public static Category History { get; } = new() {Name = "History"};
        public static Category Hobby { get; } = new() {Name = "Hobby"};
        public static Category Horror { get; } = new() {Name = "Horror"};
        public static Category Music { get; } = new() {Name = "Music"};
        public static Category Humor { get; } = new() {Name = "Humor"};
        public static Category LanguagesLinguistics { get; } = new() {Name = "Languages, Linguistics"};
        public static Category Comics { get; } = new() {Name = "Comics"};
        public static Category Cookbooks { get; } = new() {Name = "Cookbooks"};
        public static Category CzechLiterature { get; } = new() {Name = "Czech Literature"};
        public static Category FactLiterature { get; } = new() {Name = "Fact Literature"};
        public static Category EducationalLiterature { get; } = new() {Name = "Educational Literature"};
        public static Category SlovakLiterature { get; } = new() {Name = "Slovak Literature"};
        public static Category WorldLiterature { get; } = new() {Name = "World Literature"};
        public static Category MapsAndAtlases { get; } = new() {Name = "Maps and Atlases"};
        public static Category MathematicsAndLogic { get; } = new() {Name = "Mathematics and Logic"};
        public static Category Mythology { get; } = new() {Name = "Mythology"};
        public static Category Religion { get; } = new() {Name = "Religion"};
        public static Category NewAge { get; } = new() {Name = "New Age"};
        public static Category Novellas { get; } = new() {Name = "Novellas"};
        public static Category AboutLiterature { get; } = new() {Name = "About Literature"};
        public static Category PictorialPublications { get; } = new() {Name = "Pictorial Publications"};
        public static Category ColoringBooks { get; } = new() {Name = "Coloring Books"};
        public static Category PersonalDevelopmentAndStyle { get; } = new() {Name = "Personal Development and Style"};
        public static Category PCLiterature { get; } = new() {Name = "PC Literature"};
        public static Category Poetry { get; } = new() {Name = "Poetry"};
        public static Category FairyTales { get; } = new() {Name = "Fairy Tales"};
        public static Category PoliticalScienceInternationalRelations { get; } = new() {Name = "Political Science, International Relations"};
        public static Category Legends { get; } = new() {Name = "Legends"};
        public static Category ShortStories { get; } = new() {Name = "Short Stories"};
        public static Category PragueStudies { get; } = new() {Name = "Prague Studies"};
        public static Category Law { get; } = new() {Name = "Law"};
        public static Category ForChildrenAndYouth { get; } = new() {Name = "For Children and Youth"};
        public static Category ForTheLittleOnes { get; } = new() {Name = "For the Little Ones"};
        public static Category ForWomen { get; } = new() {Name = "For Women"};
        public static Category Industry { get; } = new() {Name = "Industry"};
        public static Category Stories { get; } = new() {Name = "Stories"};
        public static Category NatureAnimals { get; } = new() {Name = "Nature, Animals"};
        public static Category NaturalSciences { get; } = new() {Name = "Natural Sciences"};
        public static Category PsychologyAndPedagogy { get; } = new() {Name = "Psychology and Pedagogy"};
        public static Category Family { get; } = new() {Name = "Family"};
        public static Category Novels { get; } = new() {Name = "Novels"};
        public static Category ScienceFiction { get; } = new() {Name = "Science Fiction"};
        public static Category SociologySociety { get; } = new() {Name = "Sociology, Society"};
        public static Category Sport { get; } = new() {Name = "Sport"};
        public static Category CleverKidsCraftsGamesStyle { get; } = new() {Name = "Clever Kids - Crafts, Games, Style"};
        public static Category TechnologyAndElectronics { get; } = new() {Name = "Technology and Electronics"};
        public static Category Thrillers { get; } = new() {Name = "Thrillers"};
        public static Category TouristGuide { get; } = new() {Name = "Tourist Guide"};
        public static Category TextbooksAndDictionaries { get; } = new() {Name = "Textbooks and Dictionaries"};
        public static Category Art { get; } = new() {Name = "Art"};
        public static Category War { get; } = new() {Name = "War"};
        public static Category Science { get; } = new() {Name = "Science"};
        public static Category Universe { get; } = new() {Name = "Universe"};
        public static Category Military { get; } = new() {Name = "Military"};
        public static Category Education { get; } = new() {Name = "Education"};
        public static Category Mysteries { get; } = new() {Name = "Mysteries"};
        public static Category Garden { get; } = new() {Name = "Garden"};
        public static Category Health { get; } = new() {Name = "Health"};
        public static Category Healthcare { get; } = new() {Name = "Healthcare"};
        public static Category Agriculture { get; } = new() {Name = "Agriculture"};
        public static Category JournalismPublicism { get; } = new() {Name = "Journalism, Publicism"};

        public static IEnumerable<Category> GetAll()
        {
            return typeof(Category)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(Category))
                .Select(f => (Category) f.GetValue(null));
        }
    }
}