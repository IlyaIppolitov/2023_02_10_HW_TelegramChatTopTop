using AngleSharp;
using AngleSharp.Dom;
using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace libToGetVacancies
{

    // Класс для идентификации контакта
    public class VacancyData
    {
        public string Vacancy { get; set; }
        public Uri Link { get; set; }
        public string Date { get; set; }

        public VacancyData(string vacancy, string link, string date)
        {
            Vacancy = vacancy;
            Link = new System.Uri(link);
            Date = date;
        }
    }

    public class FuncToGetVacancies
    {
        public static async Task<int> getNumberOfPages(string mainPage,
                                                string selector = "div > div > main > div.feed-pagination.flex.align-center",
                                                string attributeName = "data-total")
        {
            int numberOfPages;
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(mainPage);

            // Проверка успешного запроса
            if (document.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Плохой код ответа при попытке чтения количества страниц: {document.StatusCode}");
            }

            // Парсинг с проверкой результата
            if (!int.TryParse(document.QuerySelector(selector).GetAttribute(attributeName), out numberOfPages))
            {
                throw new ArgumentException("Не получилось распарсить число страниц!");
            }
            return (numberOfPages);
        }

        public static async Task<ConcurrentBag<VacancyData>> FillVacanciesFromPage(ConcurrentBag<VacancyData> bag, string pageAddress)
        {
            IConfiguration config = Configuration.Default.WithDefaultLoader();

            IBrowsingContext context = BrowsingContext.New(config);
            IDocument document = await context.OpenAsync(pageAddress);

            // Проверка успешного запроса
            if (document.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Ответ от сервера не соответствует ожидаемому! Получен ответ {document.StatusCode}"); 
            }

            string selector = "div.feed__items > div > article";
            IHtmlCollection<IElement> articles = document.QuerySelectorAll(selector);

            foreach (var article in articles)
            {
                bag.Add(new VacancyData(
                    article.QuerySelector("div > div > a > div.flex.align-between > h2").TextContent,
                    document.Origin + article.QuerySelector("div > div.preview-card__content > a").GetAttribute("href"),
                    article.QuerySelector("div > div.preview-card__publish > div.publish-info").GetAttribute("title"))
                );
            }
            return bag;
        }

        public static async Task<ConcurrentBag<VacancyData>> GetAllVacancies()
        {

            var uriMainPage = "https://proglib.io/vacancies/all";
            var uri = "https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=";
            // Определение количества страниц
            var numberOfPages = await FuncToGetVacancies.getNumberOfPages(uriMainPage);

            // Инициализация массива с необходимым количеством элементов
            var arrOfPages = Enumerable.Range(0, numberOfPages).ToArray();

            // Определение макимального количество параллельных потоков
            ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 5 };

            ConcurrentBag<VacancyData> bag = new ConcurrentBag<VacancyData>();

            //Считывание необходимых данных с сайта параллельно
            await Parallel.ForEachAsync(arrOfPages, parallelOptions, async (i, _) =>
            {
                await FuncToGetVacancies.FillVacanciesFromPage(bag, uri + (i + 1));
            });

            return bag;
        }


    }

}