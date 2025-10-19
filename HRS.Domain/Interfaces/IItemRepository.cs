using HRS.Domain.Entities;

namespace HRS.Domain.Interfaces;

public interface IItemRepository : ICrudRepository<Item>
{
    Task<IEnumerable<Item>> GetRootItemsAsync();
    Task<Item?> GetByIdWithChildrenAsync(int id);
    Task<IEnumerable<Item>> SearchAsync(string? keyword);
}
