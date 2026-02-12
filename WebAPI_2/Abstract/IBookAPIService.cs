using WebAPI_2.DTOs;
using WebAPI_2.Models;

namespace WebAPI_2.Abstract
{
    public interface IBookAPIService
    {
        Task<GutendexResponse> SearchBooksAsync(GutendexSearchParams searchParams);

        Task<GutendexBook> GetBookByIdAsync(int gutendexId);

        Task<(bool Success, string ErrorMessage, Guid? BookId)> ImportBookAsync(int gutendexId);

        Task<(bool Success, string ErrorMessage, Guid? BookId)> ImportBookAsync(GutendexBook gutendexBook);

        Task<List<BookDTO>> SearchBookHybridAsync(string query);

        Task<List<BookDTO>> GetAllFullBooksAsync();

        Task<(string CoverUrl, int? DownloadCount)> GetBookCoverByTitleAsync(string title);

        string GetCoverImageUrl(GutendexBook book);

        int GetDownloadCount(GutendexBook book);
    }
}

