using HRS.Domain.Entities;

namespace HRS.Domain.Interfaces;

public interface IItemRepository : ICrudRepository<Item>
{
    Task<IEnumerable<Item>> GetRootItemsAsync(int storeId);
    Task<Item?> GetParentItemAsync(string childId);
    Task RemoveItem(Item entity);
    Task<IEnumerable<Item>> SearchAsync(string? keyword);
}
