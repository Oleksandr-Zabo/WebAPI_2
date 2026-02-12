using System.Text.Json;
using WebAPI_2.Abstract;
using WebAPI_2.DAL.Abstracts;
using WebAPI_2.DAL.Entities;
using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Services
{
    public class BookAPIService : IBookAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IGenreRepository _genreRepository;
        private const string BaseUrl = "https://gutendex.com/books";

        public BookAPIService(
            HttpClient httpClient,
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IGenreRepository genreRepository)
        {
            _httpClient = httpClient;
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _genreRepository = genreRepository;
        }

        public async Task<GutendexResponse> SearchBooksAsync(GutendexSearchParams searchParams)
        {
            try
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(searchParams.Search))
                    queryParams.Add($"search={Uri.EscapeDataString(searchParams.Search)}");

                if (searchParams.Page > 1)
                    queryParams.Add($"page={searchParams.Page}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"{BaseUrl}{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GutendexResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? new GutendexResponse { Results = new List<GutendexBook>() };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching Gutendex API: {ex.Message}");
                return new GutendexResponse { Results = new List<GutendexBook>() };
            }
        }

        public async Task<GutendexBook> GetBookByIdAsync(int gutendexId)
        {
            try
            {
                var url = $"{BaseUrl}/{gutendexId}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var book = JsonSerializer.Deserialize<GutendexBook>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return book;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting book from Gutendex: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string ErrorMessage, Guid? BookId)> ImportBookAsync(int gutendexId)
        {
            var gutendexBook = await GetBookByIdAsync(gutendexId);
            
            if (gutendexBook == null)
            {
                return (false, "Book not found in Gutendex API", null);
            }

            return await ImportBookAsync(gutendexBook);
        }

        public async Task<(bool Success, string ErrorMessage, Guid? BookId)> ImportBookAsync(GutendexBook gutendexBook)
        {
            try
            {
                // Import or get existing author (uses first author from list)
                var firstAuthor = gutendexBook.Authors?.FirstOrDefault();
                var author = await ImportOrGetAuthorAsync(firstAuthor);
                
                if (author == null)
                {
                    return (false, "Failed to import author", null);
                }

                // Generate pseudo-ISBN from Gutendex ID
                var isbn = GenerateIsbnFromGutendexId(gutendexBook.Id);

                // Check if book already exists
                var existingBooks = _bookRepository.GetAll();
                var existingBook = existingBooks.FirstOrDefault(b => 
                    b.Title.Equals(gutendexBook.Title, StringComparison.OrdinalIgnoreCase) &&
                    b.AuthorId == author.Id);

                if (existingBook != null)
                {
                    return (true, "Book already exists in database", existingBook.Id);
                }

                // Estimate publish year from author's birth year
                var publishYear = author.BirthDate.Year + 30;

                // Create new book
                var book = new Book
                {
                    Id = Guid.NewGuid(),
                    Title = gutendexBook.Title,
                    ISBN = isbn,
                    PublishYear = publishYear,
                    Price = null, // Gutendex doesn't provide price
                    AuthorId = author.Id
                };

                var success = _bookRepository.AddBook(book);

                if (!success)
                {
                    return (false, "Failed to add book to database", null);
                }

                // Add default Fiction genre
                await AddGenresFromSubjects(book.Id, null);

                return (true, "Book imported successfully", book.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Error importing book: {ex.Message}", null);
            }
        }

        public async Task<List<BookDTO>> SearchBookHybridAsync(string query)
        {
            // Search in local database first
            var localBooks = _bookRepository.GetAll();
            var localResults = localBooks
                .Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           (b.Author != null && (b.Author.FirstName + " " + b.Author.LastName).Contains(query, StringComparison.OrdinalIgnoreCase)))
                .Select(b => new BookDTO
                {
                    Id = b.Id,
                    Title = b.Title,
                    ISBN = b.ISBN,
                    PublishYear = b.PublishYear,
                    Price = b.Price,
                    AuthorId = b.AuthorId,
                    AuthorFullName = b.Author != null ? $"{b.Author.FirstName} {b.Author.LastName}" : "Unknown",
                    GenreIds = b.BookGenres?.Select(bg => bg.GenreId).ToList() ?? new List<int>(),
                    GenreNames = b.BookGenres?.Select(bg => bg.Genre?.Name ?? "Unknown").ToList() ?? new List<string>(),
                    CoverImageUrl = null,
                    DownloadCount = null
                })
                .ToList();

            // Use enrichment with ISBN to get correct covers for imported books
            var enrichmentTasks = localResults.Select(async book => 
            {
                var (coverUrl, downloadCount) = await GetBookEnrichmentDataAsync(book.ISBN);
                book.CoverImageUrl = coverUrl;
                book.DownloadCount = downloadCount;
            });
            await Task.WhenAll(enrichmentTasks);

            // If not enough results, search Gutendex API
            if (localResults.Count < 5)
            {
                var apiResults = await SearchBooksAsync(new GutendexSearchParams
                {
                    Search = query,
                    Page = 1
                });

                var apiBooks = apiResults.Results.Take(10).Select(gb => new BookDTO
                {
                    Id = Guid.Empty,
                    Title = gb.Title,
                    ISBN = GenerateIsbnFromGutendexId(gb.Id),
                    PublishYear = gb.Authors?.FirstOrDefault()?.Birth_Year + 30 ?? 1900,
                    Price = null,
                    AuthorId = Guid.Empty,
                    AuthorFullName = gb.Authors?.FirstOrDefault()?.Name ?? "Unknown",
                    GenreIds = new List<int>(),
                    GenreNames = new List<string>(),
                    CoverImageUrl = GetCoverImageUrl(gb),
                    GutendexId = gb.Id,
                    Summary = gb.Summaries?.FirstOrDefault(),
                    DownloadCount = gb.Download_Count
                }).ToList();

                localResults.AddRange(apiBooks);
            }

            return localResults;
        }

        public async Task<List<BookDTO>> GetAllFullBooksAsync()
        {
            var allBooks = _bookRepository.GetAll();
            var bookDTOs = allBooks.Select(b => new BookDTO
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                PublishYear = b.PublishYear,
                Price = b.Price,
                AuthorId = b.AuthorId,
                AuthorFullName = b.Author != null ? $"{b.Author.FirstName} {b.Author.LastName}" : "Unknown",
                GenreIds = b.BookGenres?.Select(bg => bg.GenreId).ToList() ?? new List<int>(),
                GenreNames = b.BookGenres?.Select(bg => bg.Genre?.Name ?? "Unknown").ToList() ?? new List<string>(),
                CoverImageUrl = null,
                DownloadCount = null
            }).ToList();

            // Fetch cover images for books with Gutendex ISBNs
            var enrichmentTasks = bookDTOs.Select(async book =>
            {
                if (!string.IsNullOrEmpty(book.ISBN))
                {
                    var (coverUrl, downloadCount) = await GetBookEnrichmentDataAsync(book.ISBN);
                    book.CoverImageUrl = coverUrl;
                    book.DownloadCount = downloadCount;
                }
            });
            await Task.WhenAll(enrichmentTasks);

            return bookDTOs;
        }

        public async Task<(string CoverUrl, int? DownloadCount)> GetBookCoverByTitleAsync(string title)
        {
            try
            {
                var searchResults = await SearchBooksAsync(new GutendexSearchParams
                {
                    Search = title,
                    Page = 1
                });

                var firstBook = searchResults.Results.FirstOrDefault();
                if (firstBook != null)
                {
                    return (GetCoverImageUrl(firstBook), firstBook.Download_Count);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cover by title '{title}': {ex.Message}");
                return (null, null);
            }
        }

        public string GetCoverImageUrl(GutendexBook book)
        {
            if (book?.Formats != null && book.Formats.TryGetValue("image/jpeg", out var jpegUrl))
            {
                return jpegUrl;
            }

            return $"https://www.gutenberg.org/cache/epub/{book.Id}/pg{book.Id}.cover.medium.jpg";
        }

        public int GetDownloadCount(GutendexBook book)
        {
            return book?.Download_Count ?? 0;
        }

        private async Task<Author> ImportOrGetAuthorAsync(GutendexAuthor gutendexAuthor)
        {
            if (gutendexAuthor == null)
            {
                gutendexAuthor = new GutendexAuthor
                {
                    Name = "Unknown Author",
                    Birth_Year = 1900,
                    Death_Year = 2000
                };
            }

            var (firstName, lastName) = ParseAuthorName(gutendexAuthor.Name);

            // Check if author already exists to avoid duplicates
            var existingAuthors = _authorRepository.GetAll();
            var existingAuthor = existingAuthors.FirstOrDefault(a =>
                a.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                a.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));

            if (existingAuthor != null)
            {
                return existingAuthor;
            }

            // Create new author if not exists
            var birthDate = gutendexAuthor.Birth_Year.HasValue
                ? new DateTime(gutendexAuthor.Birth_Year.Value, 1, 1)
                : new DateTime(1900, 1, 1);

            var author = new Author
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                BirthDate = birthDate
            };

            var success = _authorRepository.AddAuthor(author);
            return success ? author : null;
        }

        private (string FirstName, string LastName) ParseAuthorName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return ("Unknown", "Author");

            // Remove parentheses content
            fullName = System.Text.RegularExpressions.Regex.Replace(fullName, @"\s*\(.*?\)\s*", " ").Trim();

            // Format: "Lastname, Firstname" (most common in Gutendex)
            if (fullName.Contains(","))
            {
                var parts = fullName.Split(',', 2);
                var lastName = parts[0].Trim();
                var firstName = parts.Length > 1 ? parts[1].Trim() : "Unknown";
                
                return (firstName, lastName);
            }

            // Format: "Firstname Lastname"
            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length >= 2)
            {
                var firstName = string.Join(" ", nameParts.Take(nameParts.Length - 1));
                var lastName = nameParts.Last();
                
                return (firstName, lastName);
            }

            return ("", fullName);
        }

        private string GenerateIsbnFromGutendexId(int gutendexId)
        {
            return $"978-3-16-{gutendexId:D6}-0";
        }

        private async Task<(string CoverUrl, int? DownloadCount)> GetBookEnrichmentDataAsync(string isbn)
        {
            try
            {
                Console.WriteLine($"[DEBUG] GetBookEnrichmentDataAsync called with ISBN: {isbn ?? "null"}");
                
                // Extract Gutendex ID from ISBN (format: 978-3-16-XXXXXX-0, length 17)
                if (!string.IsNullOrEmpty(isbn) && isbn.StartsWith("978-3-16-") && isbn.EndsWith("-0") && isbn.Length == 17)
                {
                    var idPart = isbn.Substring(9, 6);
                    var trimmedId = idPart.TrimStart('0');
                    Console.WriteLine($"[DEBUG] Extracted ID: '{trimmedId}' from ISBN: {isbn}");
                    
                    if (!string.IsNullOrEmpty(trimmedId) && int.TryParse(trimmedId, out int gutendexId))
                    {
                        Console.WriteLine($"[DEBUG] Fetching book with Gutendex ID: {gutendexId}");
                        var book = await GetBookByIdAsync(gutendexId);
                        if (book != null)
                        {
                            Console.WriteLine($"[DEBUG] Successfully fetched: {book.Title}");
                            return (GetCoverImageUrl(book), book.Download_Count);
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG] Book ID {gutendexId} not found on Gutendex");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[DEBUG] ISBN format invalid: {isbn}");
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Enrichment failed for ISBN '{isbn}': {ex.Message}");
                return (null, null);
            }
        }


        private async Task AddGenresFromSubjects(Guid bookId, List<string> subjects)
        {
            var allGenres = _genreRepository.GetAll();
            var fictionGenre = allGenres.FirstOrDefault(g => g.Name == "Fiction");
            
            if (fictionGenre != null)
            {
                _bookRepository.AssignGenresToBook(bookId, new List<int> { fictionGenre.Id });
            }
        }
    }
}
