using DotCast.AudioBookInfo;

namespace DotCast.Infrastructure.BookInfoProvider.DatabazeKnih
{
    internal class CategoryMapper
    {
        private readonly Dictionary<string, Category> czechNameToCategoryMap;

        internal CategoryMapper()
        {
            czechNameToCategoryMap = new Dictionary<string, Category>
            {
                {"Architektura", Category.Architecture},
                {"Biografie a memoáry", Category.BiographiesAndMemoirs},
                {"Cestopisy a místopisy", Category.TravelAndPlaceDescriptions},
                {"Citáty a pranostiky", Category.QuotesAndProverbs},
                {"Detektivky, krimi", Category.DetectiveStoriesCrime},
                {"Divadelní hry", Category.Plays},
                {"Dívčí romány", Category.GirlsNovels},
                {"Dobrodružné", Category.Adventure},
                {"Doprava", Category.Transportation},
                {"Duchovní literatura", Category.SpiritualLiterature},
                {"Dům a byt", Category.HouseAndHome},
                {"Ekologie, životní prostředí", Category.EcologyLivingEnvironment},
                {"Ekonomie a byznys", Category.EconomicsAndBusiness},
                {"Encyklopedie", Category.Encyclopedias},
                {"Erotika", Category.Erotica},
                {"Esoterika, astrologie, okultismus", Category.EsotericismAstrologyOccultism},
                {"Fantasy", Category.Fantasy},
                {"Fejetony, eseje", Category.ColumnsEssays},
                {"Filozofie", Category.Philosophy},
                {"Gamebooky", Category.Gamebooks},
                {"Historické romány", Category.HistoricalNovels},
                {"Historie", Category.History},
                {"Hobby", Category.Hobby},
                {"Horory", Category.Horror},
                {"Hudba", Category.Music},
                {"Humor", Category.Humor},
                {"Jazyky, lingvistika", Category.LanguagesLinguistics},
                {"Komiksy", Category.Comics},
                {"Kuchařky", Category.Cookbooks},
                {"Česká literatura", Category.CzechLiterature},
                {"Literatura česká", Category.CzechLiterature},
                {"Literatura faktu", Category.FactLiterature},
                {"Naučná literatura", Category.EducationalLiterature},
                {"Slovenská literatura", Category.SlovakLiterature},
                {"Světová literatura", Category.WorldLiterature},
                {"Literatura světová", Category.WorldLiterature},
                {"Mapy a atlasy", Category.MapsAndAtlases},
                {"Matematika a logika", Category.MathematicsAndLogic},
                {"Mytologie", Category.Mythology},
                {"Náboženství", Category.Religion},
                {"New Age", Category.NewAge},
                {"Novely", Category.Novellas},
                {"O literatuře", Category.AboutLiterature},
                {"Obrázkové publikace", Category.PictorialPublications},
                {"Omalovánky", Category.ColoringBooks},
                {"Osobní rozvoj a styl", Category.PersonalDevelopmentAndStyle},
                {"PC literatura", Category.PCLiterature},
                {"Poezie", Category.Poetry},
                {"Pohádky", Category.FairyTales},
                {"Politologie, mezinárodní vztahy", Category.PoliticalScienceInternationalRelations},
                {"Pověsti", Category.Legends},
                {"Povídky", Category.ShortStories},
                {"Pražské studie", Category.PragueStudies},
                {"Právo", Category.Law},
                {"Pro děti a mládež", Category.ForChildrenAndYouth},
                {"Pro nejmenší", Category.ForTheLittleOnes},
                {"Pro ženy", Category.ForWomen},
                {"Průmysl", Category.Industry},
                {"Příběhy", Category.Stories},
                {"Příroda, zvířata", Category.NatureAnimals},
                {"Přírodní vědy", Category.NaturalSciences},
                {"Psychologie a pedagogika", Category.PsychologyAndPedagogy},
                {"Rodina", Category.Family},
                {"Romány", Category.Novels},
                {"Sci-fi", Category.ScienceFiction},
                {"Sociologie, společnost", Category.SociologySociety},
                {"Sport", Category.Sport},
                {"Šikovné děti - tvoření, hry, styl", Category.CleverKidsCraftsGamesStyle},
                {"Technika a elektronika", Category.TechnologyAndElectronics},
                {"Thrillery", Category.Thrillers},
                {"Turistické průvodce", Category.TouristGuide},
                {"Učebnice a slovníky", Category.TextbooksAndDictionaries},
                {"Umění", Category.Art},
                {"Válka", Category.War},
                {"Válečné", Category.War},
                {"Věda", Category.Science},
                {"Vesmír", Category.Universe},
                {"Vojenství", Category.Military},
                {"Vzdělání", Category.Education},
                {"Záhady", Category.Mysteries},
                {"Zahrada", Category.Garden},
                {"Zdraví", Category.Health},
                {"Zdravotnictví", Category.Healthcare},
                {"Zemědělství", Category.Agriculture},
                {"Žurnalistika, publicistika", Category.JournalismPublicism}
            };
        }

        public Category? GetCategoryByCzechName(string czechName)
        {
            if (czechNameToCategoryMap.TryGetValue(czechName, out var category))
            {
                return category;
            }

            return null;
        }
    }
}