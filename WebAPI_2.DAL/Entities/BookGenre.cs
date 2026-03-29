namespace WebAPI_2.DAL.Entities
{
    public class BookGenre //Junction Table
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; } = null!;
        
        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;
    }
}
