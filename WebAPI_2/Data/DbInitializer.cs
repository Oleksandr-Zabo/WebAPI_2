using Microsoft.EntityFrameworkCore;
using WebAPI_2.Core;
using WebAPI_2.DAL;
using WebAPI_2.DAL.Entities;
using WebAPI_2.Services;

namespace WebAPI_2.Data
{
    /// <summary>
    /// Database initializer using auto-generated GUIDs (Guid.NewGuid())
    /// Cleaner and less error-prone than hardcoded GUIDs
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Seed admin user if not exists
            if (!context.Users.Any(u => u.Email == "admin@booklibrary.com"))
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    NickName = "admin",
                    Email = "admin@booklibrary.com",
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    Role = Roles.Admin,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }

            // Seed Authors if not exists
            if (!context.Authors.Any())
            {
                // Create authors with variables for easy reference
                var georgeOrwell = new Author { Id = Guid.NewGuid(), FirstName = "George", LastName = "Orwell", BirthDate = new DateTime(1903, 6, 25) };
                var janeAusten = new Author { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Austen", BirthDate = new DateTime(1775, 12, 16) };
                var jrrTolkien = new Author { Id = Guid.NewGuid(), FirstName = "J.R.R.", LastName = "Tolkien", BirthDate = new DateTime(1892, 1, 3) };
                var agathaChristie = new Author { Id = Guid.NewGuid(), FirstName = "Agatha", LastName = "Christie", BirthDate = new DateTime(1890, 9, 15) };
                var stephenKing = new Author { Id = Guid.NewGuid(), FirstName = "Stephen", LastName = "King", BirthDate = new DateTime(1947, 9, 21) };
                var jkRowling = new Author { Id = Guid.NewGuid(), FirstName = "J.K.", LastName = "Rowling", BirthDate = new DateTime(1965, 7, 31) };
                var isaacAsimov = new Author { Id = Guid.NewGuid(), FirstName = "Isaac", LastName = "Asimov", BirthDate = new DateTime(1920, 1, 2) };
                var ernestHemingway = new Author { Id = Guid.NewGuid(), FirstName = "Ernest", LastName = "Hemingway", BirthDate = new DateTime(1899, 7, 21) };
                var leoTolstoy = new Author { Id = Guid.NewGuid(), FirstName = "Leo", LastName = "Tolstoy", BirthDate = new DateTime(1828, 9, 9) };
                var markTwain = new Author { Id = Guid.NewGuid(), FirstName = "Mark", LastName = "Twain", BirthDate = new DateTime(1835, 11, 30) };
                var fScottFitzgerald = new Author { Id = Guid.NewGuid(), FirstName = "F. Scott", LastName = "Fitzgerald", BirthDate = new DateTime(1896, 9, 24) };
                var harperLee = new Author { Id = Guid.NewGuid(), FirstName = "Harper", LastName = "Lee", BirthDate = new DateTime(1926, 4, 28) };
                var gabrielGarciaMarquez = new Author { Id = Guid.NewGuid(), FirstName = "Gabriel Garc?a", LastName = "M?rquez", BirthDate = new DateTime(1927, 3, 6) };
                var danBrown = new Author { Id = Guid.NewGuid(), FirstName = "Dan", LastName = "Brown", BirthDate = new DateTime(1964, 6, 22) };
                var pauloCoelho = new Author { Id = Guid.NewGuid(), FirstName = "Paulo", LastName = "Coelho", BirthDate = new DateTime(1947, 8, 24) };
                var yuvalHarari = new Author { Id = Guid.NewGuid(), FirstName = "Yuval Noah", LastName = "Harari", BirthDate = new DateTime(1976, 2, 24) };
                var malcolmGladwell = new Author { Id = Guid.NewGuid(), FirstName = "Malcolm", LastName = "Gladwell", BirthDate = new DateTime(1963, 9, 3) };
                var michelleObama = new Author { Id = Guid.NewGuid(), FirstName = "Michelle", LastName = "Obama", BirthDate = new DateTime(1964, 1, 17) };
                var viktorFrankl = new Author { Id = Guid.NewGuid(), FirstName = "Viktor", LastName = "Frankl", BirthDate = new DateTime(1905, 3, 26) };
                var daleCarnegie = new Author { Id = Guid.NewGuid(), FirstName = "Dale", LastName = "Carnegie", BirthDate = new DateTime(1888, 11, 24) };
                var neilGaiman = new Author { Id = Guid.NewGuid(), FirstName = "Neil", LastName = "Gaiman", BirthDate = new DateTime(1960, 11, 10) };
                var brandonSanderson = new Author { Id = Guid.NewGuid(), FirstName = "Brandon", LastName = "Sanderson", BirthDate = new DateTime(1975, 12, 19) };
                var andyWeir = new Author { Id = Guid.NewGuid(), FirstName = "Andy", LastName = "Weir", BirthDate = new DateTime(1972, 6, 16) };
                var gillianFlynn = new Author { Id = Guid.NewGuid(), FirstName = "Gillian", LastName = "Flynn", BirthDate = new DateTime(1971, 2, 24) };
                var suzanneCollins = new Author { Id = Guid.NewGuid(), FirstName = "Suzanne", LastName = "Collins", BirthDate = new DateTime(1962, 8, 10) };

                context.Authors.AddRange(new[] {
                    georgeOrwell, janeAusten, jrrTolkien, agathaChristie, stephenKing,
                    jkRowling, isaacAsimov, ernestHemingway, leoTolstoy, markTwain,
                    fScottFitzgerald, harperLee, gabrielGarciaMarquez, danBrown, pauloCoelho,
                    yuvalHarari, malcolmGladwell, michelleObama, viktorFrankl, daleCarnegie,
                    neilGaiman, brandonSanderson, andyWeir, gillianFlynn, suzanneCollins
                });
                await context.SaveChangesAsync();

                // Seed Books using author references (no need to parse GUIDs!)
                var book1984 = new Book { Id = Guid.NewGuid(), Title = "1984", ISBN = "978-0-452-28423-4", PublishYear = 1949, Price = 15.99m, AuthorId = georgeOrwell.Id };
                var bookPridePrejudice = new Book { Id = Guid.NewGuid(), Title = "Pride and Prejudice", ISBN = "978-0-141-43951-8", PublishYear = 1813, Price = 12.99m, AuthorId = janeAusten.Id };
                var bookOldManSea = new Book { Id = Guid.NewGuid(), Title = "The Old Man and the Sea", ISBN = "978-0-684-80122-3", PublishYear = 1952, Price = 13.99m, AuthorId = ernestHemingway.Id };
                var bookWarPeace = new Book { Id = Guid.NewGuid(), Title = "War and Peace", ISBN = "978-0-140-44793-7", PublishYear = 1869, Price = 24.99m, AuthorId = leoTolstoy.Id };
                var bookTomSawyer = new Book { Id = Guid.NewGuid(), Title = "The Adventures of Tom Sawyer", ISBN = "978-0-486-40077-6", PublishYear = 1876, Price = 11.99m, AuthorId = markTwain.Id };
                var bookGatsby = new Book { Id = Guid.NewGuid(), Title = "The Great Gatsby", ISBN = "978-0-7432-7356-5", PublishYear = 1925, Price = 14.99m, AuthorId = fScottFitzgerald.Id };
                var bookMockingbird = new Book { Id = Guid.NewGuid(), Title = "To Kill a Mockingbird", ISBN = "978-0-061-12008-4", PublishYear = 1960, Price = 16.99m, AuthorId = harperLee.Id };
                var bookHundredYears = new Book { Id = Guid.NewGuid(), Title = "One Hundred Years of Solitude", ISBN = "978-0-060-88328-7", PublishYear = 1967, Price = 17.99m, AuthorId = gabrielGarciaMarquez.Id };
                var bookLOTR = new Book { Id = Guid.NewGuid(), Title = "The Lord of the Rings", ISBN = "978-0-618-34399-6", PublishYear = 1954, Price = 29.99m, AuthorId = jrrTolkien.Id };
                var bookHarryPotter = new Book { Id = Guid.NewGuid(), Title = "Harry Potter and the Philosopher's Stone", ISBN = "978-0-747-53269-9", PublishYear = 1997, Price = 22.99m, AuthorId = jkRowling.Id };
                var bookAmericanGods = new Book { Id = Guid.NewGuid(), Title = "American Gods", ISBN = "978-0-380-97365-0", PublishYear = 2001, Price = 19.99m, AuthorId = neilGaiman.Id };
                var bookWayOfKings = new Book { Id = Guid.NewGuid(), Title = "The Way of Kings", ISBN = "978-0-765-36563-7", PublishYear = 2010, Price = 27.99m, AuthorId = brandonSanderson.Id };
                var bookFoundation = new Book { Id = Guid.NewGuid(), Title = "Foundation", ISBN = "978-0-553-29335-0", PublishYear = 1951, Price = 16.99m, AuthorId = isaacAsimov.Id };
                var bookIRobot = new Book { Id = Guid.NewGuid(), Title = "I, Robot", ISBN = "978-0-553-38256-3", PublishYear = 1950, Price = 15.99m, AuthorId = isaacAsimov.Id };
                var bookMartian = new Book { Id = Guid.NewGuid(), Title = "The Martian", ISBN = "978-0-553-41802-6", PublishYear = 2011, Price = 18.99m, AuthorId = andyWeir.Id };
                var bookOrientExpress = new Book { Id = Guid.NewGuid(), Title = "Murder on the Orient Express", ISBN = "978-0-062-07348-7", PublishYear = 1934, Price = 14.99m, AuthorId = agathaChristie.Id };
                var bookShining = new Book { Id = Guid.NewGuid(), Title = "The Shining", ISBN = "978-0-385-12167-5", PublishYear = 1977, Price = 18.99m, AuthorId = stephenKing.Id };
                var bookIt = new Book { Id = Guid.NewGuid(), Title = "It", ISBN = "978-1-501-14243-3", PublishYear = 1986, Price = 20.99m, AuthorId = stephenKing.Id };
                var bookDaVinci = new Book { Id = Guid.NewGuid(), Title = "The Da Vinci Code", ISBN = "978-0-385-50420-1", PublishYear = 2003, Price = 17.99m, AuthorId = danBrown.Id };
                var bookGoneGirl = new Book { Id = Guid.NewGuid(), Title = "Gone Girl", ISBN = "978-0-307-58836-4", PublishYear = 2012, Price = 16.99m, AuthorId = gillianFlynn.Id };
                var bookHungerGames = new Book { Id = Guid.NewGuid(), Title = "The Hunger Games", ISBN = "978-0-439-02348-1", PublishYear = 2008, Price = 17.99m, AuthorId = suzanneCollins.Id };
                var bookSapiens = new Book { Id = Guid.NewGuid(), Title = "Sapiens: A Brief History of Humankind", ISBN = "978-0-062-31609-7", PublishYear = 2011, Price = 19.99m, AuthorId = yuvalHarari.Id };
                var bookOutliers = new Book { Id = Guid.NewGuid(), Title = "Outliers: The Story of Success", ISBN = "978-0-316-01792-3", PublishYear = 2008, Price = 18.99m, AuthorId = malcolmGladwell.Id };
                var bookBecoming = new Book { Id = Guid.NewGuid(), Title = "Becoming", ISBN = "978-1-524-76313-8", PublishYear = 2018, Price = 24.99m, AuthorId = michelleObama.Id };
                var bookMansSearch = new Book { Id = Guid.NewGuid(), Title = "Man's Search for Meaning", ISBN = "978-0-807-01427-1", PublishYear = 1946, Price = 14.99m, AuthorId = viktorFrankl.Id };
                var bookWinFriends = new Book { Id = Guid.NewGuid(), Title = "How to Win Friends and Influence People", ISBN = "978-0-671-02730-2", PublishYear = 1936, Price = 15.99m, AuthorId = daleCarnegie.Id };
                var bookAlchemist = new Book { Id = Guid.NewGuid(), Title = "The Alchemist", ISBN = "978-0-061-12241-5", PublishYear = 1988, Price = 16.99m, AuthorId = pauloCoelho.Id };

                context.Books.AddRange(new[] {
                    book1984, bookPridePrejudice, bookOldManSea, bookWarPeace, bookTomSawyer,
                    bookGatsby, bookMockingbird, bookHundredYears, bookLOTR, bookHarryPotter,
                    bookAmericanGods, bookWayOfKings, bookFoundation, bookIRobot, bookMartian,
                    bookOrientExpress, bookShining, bookIt, bookDaVinci, bookGoneGirl,
                    bookHungerGames, bookSapiens, bookOutliers, bookBecoming, bookMansSearch,
                    bookWinFriends, bookAlchemist
                });
                await context.SaveChangesAsync();

                // Seed BookGenre relationships using book references
                var bookGenres = new List<BookGenre>
                {
                    // 1984 - Fiction, Science Fiction, Thriller
                    new BookGenre { BookId = book1984.Id, GenreId = 2 },
                    new BookGenre { BookId = book1984.Id, GenreId = 4 },
                    new BookGenre { BookId = book1984.Id, GenreId = 8 },
                    
                    // Pride and Prejudice - Fiction, Romance
                    new BookGenre { BookId = bookPridePrejudice.Id, GenreId = 2 },
                    new BookGenre { BookId = bookPridePrejudice.Id, GenreId = 7 },
                    
                    // Additional books...
                    new BookGenre { BookId = bookOldManSea.Id, GenreId = 2 },
                    new BookGenre { BookId = bookWarPeace.Id, GenreId = 2 },
                    new BookGenre { BookId = bookWarPeace.Id, GenreId = 9 },
                    new BookGenre { BookId = bookTomSawyer.Id, GenreId = 2 },
                    new BookGenre { BookId = bookGatsby.Id, GenreId = 2 },
                    new BookGenre { BookId = bookGatsby.Id, GenreId = 7 },
                    new BookGenre { BookId = bookMockingbird.Id, GenreId = 2 },
                    new BookGenre { BookId = bookMockingbird.Id, GenreId = 9 },
                    new BookGenre { BookId = bookHundredYears.Id, GenreId = 2 },
                    new BookGenre { BookId = bookHundredYears.Id, GenreId = 5 },
                    new BookGenre { BookId = bookLOTR.Id, GenreId = 5 },
                    new BookGenre { BookId = bookLOTR.Id, GenreId = 2 },
                    new BookGenre { BookId = bookHarryPotter.Id, GenreId = 5 },
                    new BookGenre { BookId = bookHarryPotter.Id, GenreId = 2 },
                    new BookGenre { BookId = bookAmericanGods.Id, GenreId = 5 },
                    new BookGenre { BookId = bookAmericanGods.Id, GenreId = 2 },
                    new BookGenre { BookId = bookWayOfKings.Id, GenreId = 5 },
                    new BookGenre { BookId = bookWayOfKings.Id, GenreId = 2 },
                    new BookGenre { BookId = bookFoundation.Id, GenreId = 4 },
                    new BookGenre { BookId = bookFoundation.Id, GenreId = 2 },
                    new BookGenre { BookId = bookIRobot.Id, GenreId = 4 },
                    new BookGenre { BookId = bookIRobot.Id, GenreId = 2 },
                    new BookGenre { BookId = bookMartian.Id, GenreId = 4 },
                    new BookGenre { BookId = bookMartian.Id, GenreId = 2 },
                    new BookGenre { BookId = bookMartian.Id, GenreId = 8 },
                    new BookGenre { BookId = bookOrientExpress.Id, GenreId = 6 },
                    new BookGenre { BookId = bookOrientExpress.Id, GenreId = 8 },
                    new BookGenre { BookId = bookOrientExpress.Id, GenreId = 2 },
                    new BookGenre { BookId = bookShining.Id, GenreId = 8 },
                    new BookGenre { BookId = bookShining.Id, GenreId = 2 },
                    new BookGenre { BookId = bookIt.Id, GenreId = 8 },
                    new BookGenre { BookId = bookIt.Id, GenreId = 2 },
                    new BookGenre { BookId = bookDaVinci.Id, GenreId = 6 },
                    new BookGenre { BookId = bookDaVinci.Id, GenreId = 8 },
                    new BookGenre { BookId = bookDaVinci.Id, GenreId = 2 },
                    new BookGenre { BookId = bookGoneGirl.Id, GenreId = 6 },
                    new BookGenre { BookId = bookGoneGirl.Id, GenreId = 8 },
                    new BookGenre { BookId = bookGoneGirl.Id, GenreId = 2 },
                    new BookGenre { BookId = bookHungerGames.Id, GenreId = 4 },
                    new BookGenre { BookId = bookHungerGames.Id, GenreId = 8 },
                    new BookGenre { BookId = bookHungerGames.Id, GenreId = 2 },
                    new BookGenre { BookId = bookSapiens.Id, GenreId = 3 },
                    new BookGenre { BookId = bookSapiens.Id, GenreId = 9 },
                    new BookGenre { BookId = bookOutliers.Id, GenreId = 3 },
                    new BookGenre { BookId = bookOutliers.Id, GenreId = 11 },
                    new BookGenre { BookId = bookBecoming.Id, GenreId = 10 },
                    new BookGenre { BookId = bookBecoming.Id, GenreId = 3 },
                    new BookGenre { BookId = bookMansSearch.Id, GenreId = 11 },
                    new BookGenre { BookId = bookMansSearch.Id, GenreId = 10 },
                    new BookGenre { BookId = bookMansSearch.Id, GenreId = 3 },
                    new BookGenre { BookId = bookWinFriends.Id, GenreId = 11 },
                    new BookGenre { BookId = bookWinFriends.Id, GenreId = 3 },
                    new BookGenre { BookId = bookAlchemist.Id, GenreId = 2 },
                    new BookGenre { BookId = bookAlchemist.Id, GenreId = 11 }
                };

                context.BookGenres.AddRange(bookGenres);
                await context.SaveChangesAsync();
            }
        }
    }
}
