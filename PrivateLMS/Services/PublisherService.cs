using Microsoft.EntityFrameworkCore;
using PrivateLMS.Data;
using PrivateLMS.Models;
using PrivateLMS.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly LibraryDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PublisherService(LibraryDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<PublisherViewModel>> GetAllPublishersAsync()
        {
            var pagedResult = await GetPagedPublishersAsync(1, int.MaxValue);
            return pagedResult.Items;
        }

        public async Task<PublisherViewModel?> GetPublisherDetailsAsync(int publisherId)
        {
            var publisher = await _context.Publishers
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PublisherId == publisherId);

            if (publisher == null)
            {
                return null;
            }

            return new PublisherViewModel
            {
                PublisherId = publisher.PublisherId,
                PublisherName = publisher.PublisherName,
                Location = publisher.Location,
                LogoImagePath = publisher.LogoImagePath
            };
        }

        public async Task<bool> CreatePublisherAsync(PublisherViewModel model, string? logoImagePath)
        {
            var publisher = new Publisher
            {
                PublisherName = model.PublisherName,
                Location = model.Location,
                LogoImagePath = logoImagePath
            };

            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePublisherAsync(int id, PublisherViewModel model, string? logoImagePath)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return false;
            }

            publisher.PublisherName = model.PublisherName;
            publisher.Location = model.Location;
            if (!string.IsNullOrEmpty(logoImagePath))
            {
                if (!string.IsNullOrEmpty(publisher.LogoImagePath))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, publisher.LogoImagePath.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }
                publisher.LogoImagePath = logoImagePath;
            }

            _context.Update(publisher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePublisherAsync(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(publisher.LogoImagePath))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, publisher.LogoImagePath.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResultViewModel<PublisherViewModel>> GetPagedPublishersAsync(int page, int pageSize)
        {
            var query = _context.Publishers
                .AsNoTracking();

            var totalItems = await query.CountAsync();
            var publishers = await query
                .OrderBy(p => p.PublisherName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PublisherViewModel
                {
                    PublisherId = p.PublisherId,
                    PublisherName = p.PublisherName,
                    Location = p.Location,
                    LogoImagePath = p.LogoImagePath
                })
                .ToListAsync();

            return new PagedResultViewModel<PublisherViewModel>
            {
                Items = publishers,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }
    }
}