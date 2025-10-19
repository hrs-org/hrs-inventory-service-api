using HRS.Domain.Entities;

namespace HRS.Domain.Interfaces;

public interface IPackageRepository : ICrudRepository<Package>
{
    Task<IEnumerable<Package>> GetAllWithDetailsAsync();
    Task<Package?> GetByIdWithDetailsAsync(string id);
    Task<Package?> GetByIdWithItemsAsync(string id);
    Task<object> UpdateAsync(Package package, string id);
    Task<object> RemoveAsync(string id);
}
