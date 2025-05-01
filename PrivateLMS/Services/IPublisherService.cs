using PrivateLMS.Models;
using PrivateLMS.ViewModels;

namespace PrivateLMS.Services
{
    public interface IPublisherService
    {
        Task<List<PublisherViewModel>> GetAllPublishersAsync();
        Task<PublisherViewModel?> GetPublisherDetailsAsync(int publisherId);
        Task<bool> CreatePublisherAsync(PublisherViewModel model, string? logoImagePath);
        Task<bool> UpdatePublisherAsync(int id, PublisherViewModel model, string? logoImagePath);
        Task<bool> DeletePublisherAsync(int id);
        Task<PagedResultViewModel<PublisherViewModel>> GetPagedPublishersAsync(int page, int pageSize);
    }
}