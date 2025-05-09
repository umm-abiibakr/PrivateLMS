﻿using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public interface IAuthorService
    {
        Task<List<Author>> GetAllAuthorsAsync();
        Task<AuthorViewModel?> GetAuthorByIdAsync(int authorId); 
        Task<bool> CreateAuthorAsync(Author author);
        Task<bool> UpdateAuthorAsync(int id, Author author);
        Task<bool> DeleteAuthorAsync(int id);
        Task<PagedResultViewModel<Author>> GetPagedAuthorsAsync(int page, int pageSize);
    }
}