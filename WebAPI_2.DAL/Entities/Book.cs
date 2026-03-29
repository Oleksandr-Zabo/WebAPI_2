namespace WebAPI_2.DAL.Entities
{
    public class Book
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string ISBN { get; set; }

        public int PublishYear { get; set; }

        public decimal? Price { get; set; }

        public Guid AuthorId { get; set; }
        public Author Author { get; set; }

        // Navigation property for many-to-many Book-Genre
        public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

    }
}
