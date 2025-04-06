using PrivateLMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public interface IBookService
    {
        Task<List<BookViewModel>> GetAllBooksAsync();
        Task<BookViewModel> GetBookDetailsAsync(int bookId);
        Task<bool> CreateBookAsync(BookViewModel model, string coverImagePath);
        Task<bool> UpdateBookAsync(int id, BookViewModel model, string coverImagePath);
        Task<Book> GetBookByIdAsync(int bookId);
        Task<List<Publisher>> GetAllPublishersAsync();
        Task<List<Category>> GetAllCategoriesAsync();
        Task<bool> DeleteBookAsync(int id);
        Task<List<BookViewModel>> SearchBooksAsync(string searchTerm);
    }
}