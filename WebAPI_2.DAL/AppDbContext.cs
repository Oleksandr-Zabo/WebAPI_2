using WebAPI_2.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebAPI_2.DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        
        // add DbSets
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Book entity
            modelBuilder.Entity<Book>(entity =>
            {
                // Configure decimal precision for Price
                entity.Property(b => b.Price)
                    .HasPrecision(18, 2); // 18 digits total, 2 after decimal point

                // Configure ISBN as unique
                entity.HasIndex(b => b.ISBN)
                    .IsUnique();

                // Configure relationship with Author
                entity.HasOne(b => b.Author)
                    .WithMany()
                    .HasForeignKey(b => b.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure many-to-many relationship between Book and Genre
            modelBuilder.Entity<BookGenre>()
                .HasKey(bg => new { bg.BookId, bg.GenreId });
            
            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId);
            
            modelBuilder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId);

            // Configure many-to-many relationship between User and Book (SavedBooks)
            modelBuilder.Entity<User>()
                .HasMany(u => u.SavedBooks)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "UserSavedBooks",
                    j => j.HasOne<Book>().WithMany().HasForeignKey("BookId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "BookId");
                        j.ToTable("UserSavedBooks");
                    });
            
            // Seed initial genres
            modelBuilder.Entity<Genre>().HasData(
                new Genre { Id = 1, Name = "Unknown", Description = "Genre not specified or undefined" },
                new Genre { Id = 2, Name = "Fiction", Description = "Literary works of imaginative narration" },
                new Genre { Id = 3, Name = "Non-Fiction", Description = "Factual and informative writing" },
                new Genre { Id = 4, Name = "Science Fiction", Description = "Speculative fiction with scientific themes" },
                new Genre { Id = 5, Name = "Fantasy", Description = "Fiction with magical or supernatural elements" },
                new Genre { Id = 6, Name = "Mystery", Description = "Fiction dealing with puzzling crimes" },
                new Genre { Id = 7, Name = "Romance", Description = "Fiction focused on romantic relationships" },
                new Genre { Id = 8, Name = "Thriller", Description = "Suspenseful and exciting fiction" },
                new Genre { Id = 9, Name = "History", Description = "Books about historical events and periods" },
                new Genre { Id = 10, Name = "Biography", Description = "Life stories of real people" },
                new Genre { Id = 11, Name = "Self-Help", Description = "Books for personal development" }
            );
        }
    }
}
