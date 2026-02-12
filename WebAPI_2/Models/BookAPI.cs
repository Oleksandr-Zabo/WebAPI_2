namespace WebAPI_2.Models
{
    public class GutendexResponse
    {
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public List<GutendexBook> Results { get; set; }
    }

    public class GutendexBook
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<GutendexAuthor> Authors { get; set; }
        public List<string> Summaries { get; set; }
        public Dictionary<string, string> Formats { get; set; }
        public int Download_Count { get; set; }

        public int PublishYear => Authors?.FirstOrDefault()?.Birth_Year + 30 ?? 1900;
    }

    public class GutendexAuthor
    {
        public string Name { get; set; }
        public int? Birth_Year { get; set; }
        public int? Death_Year { get; set; }
    }

    public class GutendexSearchParams
    {
        public string Search { get; set; }
        public int Page { get; set; } = 1;
    }
}
