namespace WebAPI_2.DAL.Entities
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Navigation property for many-to-many Book-Genre
        public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
    }
}
