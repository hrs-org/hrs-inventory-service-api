using HRS.Domain.Entities;

namespace HRS.Domain.Interfaces;

public interface IItemRepository : ICrudRepository<Item>
{
    Task<Item?> GetByIdWithParentAsync(string id);
    Task<IEnumerable<Item>> GetRootItemsAsync();
    Task<Item?> GetByIdWithChildrenAsync(string id);
    Task RemoveItem(Item entity);
    Task<IEnumerable<Item>> SearchAsync(string? keyword);
}
