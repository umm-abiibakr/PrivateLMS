using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{

    public class AuthorService : IAuthorService
    {
        private readonly LibraryDbContext _context;

        public AuthorService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Author>> GetAllAuthorsAsync()
        {
            try
            {
                return await _context.Authors
                    .Include(a => a.Books)
                    .AsNoTracking()
                    .ToListAsync() ?? new List<Author>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllAuthorsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<AuthorViewModel?> GetAuthorByIdAsync(int authorId)
        {
            try
            {
                var author = await _context.Authors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AuthorId == authorId);

                if (author == null)
                {
                    return null;
                }

                var books = await _context.Books
                    .Where(b => b.AuthorId == authorId)
                    .Select(b => b.Title)
                    .ToListAsync();

                return new AuthorViewModel
                {
                    AuthorId = author.AuthorId,
                    Name = author.Name,
                    Biography = author.Biography,
                    BirthDate = author.BirthDate,
                    DeathDate = author.DeathDate,
                    BookCount = books.Count,
                    Books = books ?? new List<string>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAuthorByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateAuthorAsync(Author author)
        {
            try
            {
                // Ensure Books is initialized
                author.Books ??= new List<Book>();
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateAuthorAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAuthorAsync(int id, Author author)
        {
            try
            {
                var existingAuthor = await _context.Authors.FindAsync(id);
                if (existingAuthor == null) return false;

                existingAuthor.Name = author.Name;
                existingAuthor.Biography = author.Biography;
                existingAuthor.BirthDate = author.BirthDate;
                existingAuthor.DeathDate = author.DeathDate;
                // Preserve existing Books if not updated
                existingAuthor.Books ??= new List<Book>();

                _context.Update(existingAuthor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateAuthorAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAuthorAsync(int id)
        {
            try
            {
                var author = await _context.Authors
                    .Include(a => a.Books)
                    .FirstOrDefaultAsync(a => a.AuthorId == id);

                if (author == null) return false;

                if (author.Books.Any())
                {
                    return false;
                }

                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteAuthorAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<PagedResultViewModel<Author>> GetPagedAuthorsAsync(int page, int pageSize)
        {
            try
            {
                var totalItems = await _context.Authors.CountAsync();
                var authors = await _context.Authors
                    .Include(a => a.Books)
                    .OrderBy(a => a.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResultViewModel<Author>
                {
                    Items = authors,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPagedAuthorsAsync: {ex.Message}");
                throw;
            }
        }
    }
}