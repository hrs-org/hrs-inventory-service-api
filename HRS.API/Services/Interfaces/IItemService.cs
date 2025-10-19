using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Services.Interfaces;

public interface IItemService
{
    Task<ItemResponseDto> GetItemAsync(string id);
    Task<IEnumerable<ItemResponseDto>> GetRootItemsAsync();
    Task<ItemResponseDto> CreateAsync(AddItemRequestDto dto);
    Task<ItemResponseDto> UpdateAsync(UpdateItemRequestDto dto);
    Task DeleteAsync(string id);
    Task<decimal> GetItemRateAsync(string itemId, int rentalDays);
    Task<IEnumerable<ItemResponseDto>> SearchItemsAsync(string? keyword);
}
