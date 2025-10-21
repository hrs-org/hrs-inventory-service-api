using AutoMapper;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using HRS.Shared.Core.Interfaces;

namespace HRS.API.Services;

public class ItemService : IItemService
{
    private const string ItemNotFound = "Item not found";
    private readonly IItemRepository _itemRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;

    public ItemService(IMapper mapper, IUserContextService userContextService, IItemRepository itemRepository)
    {
        _mapper = mapper;
        _userContextService = userContextService;
        _itemRepository = itemRepository;
    }

    public async Task<ItemResponseDto> GetItemAsync(string id)
    {
        var item = await _itemRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException(ItemNotFound);
        return _mapper.Map<ItemResponseDto>(item);
    }

    public async Task<IEnumerable<ItemResponseDto>> GetRootItemsAsync(int storeId)
    {
        var items = await _itemRepository.GetRootItemsAsync(storeId);
        return _mapper.Map<IEnumerable<ItemResponseDto>>(items);
    }

    private static void ProcessChildren(Item entity, int userId)
    {
        if (entity.Children.Count <= 0) return;

        entity.Quantity = entity.Children.Sum(c => c.Quantity);

        foreach (var child in entity.Children)
        {
            child.CreatedById = userId;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedById = userId;
            child.UpdatedAt = DateTime.UtcNow;
            child.StoreId = entity.StoreId;
            child.ParentId = entity.Id;
        }
    }

    private void ProcessRates(Item entity, ICollection<ItemRateRequestDto>? rates, int userId)
    {
        if (rates == null || rates.Count <= 0) return;

        entity.Rates = rates.Select(rateDto =>
        {
            var rate = _mapper.Map<ItemRate>(rateDto);
            rate.CreatedById = userId;
            rate.CreatedAt = DateTime.UtcNow;
            rate.UpdatedById = userId;
            rate.UpdatedAt = DateTime.UtcNow;
            rate.IsActive = true;
            return rate;
        }).ToList();
    }

    public async Task<ItemResponseDto> CreateAsync(AddItemRequestDto dto)
    {
        var user = await _userContextService.GetUserAsync();
        var entity = _mapper.Map<Item>(dto);
        entity.CreatedById = user.Id;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedById = user.Id;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.StoreId = _userContextService.GetStoreId();
        entity.ParentId = entity.Id;

        ProcessChildren(entity, user.Id);
        ProcessRates(entity, dto.Rates, user.Id);

        await _itemRepository.AddAsync(entity);
        var createdItem = await _itemRepository.GetByIdAsync(entity.Id);
        return _mapper.Map<ItemResponseDto>(createdItem);
    }

    public async Task<ItemResponseDto> UpdateAsync(UpdateItemRequestDto dto)
    {
        if (string.IsNullOrEmpty(dto.Id)) throw new KeyNotFoundException(ItemNotFound);
        var user = await _userContextService.GetUserAsync();
        var item = await _itemRepository.GetByIdAsync(dto.Id)
                   ?? throw new KeyNotFoundException(ItemNotFound);
        item.Name = dto.Name;
        item.Description = dto.Description;
        item.Price = dto.Price;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedById = user.Id;
        if (dto.Children != null)
        {
            var existingChildDict = dto.Children.Where(c => c.Id is not null)
                .ToDictionary(c => c.Id!);
            var newChildren = dto.Children.Where(c => c.Id is null).ToList();
            foreach (var child in item.Children.ToList())
            {
                if (existingChildDict.TryGetValue(child.Id, out var dtoChild))
                {
                    child.Name = dtoChild.Name;
                    child.Description = dtoChild.Description;
                    child.Quantity = dtoChild.Quantity;
                    child.Price = dtoChild.Price;
                    child.UpdatedAt = DateTime.UtcNow;
                    child.UpdatedById = user.Id;
                    existingChildDict.Remove(child.Id);
                }
                else
                {
                    item.Children.Remove(child);
                }
            }
            foreach (var dtoChild in newChildren)
            {
                var newChild = new Item
                {
                    Name = dtoChild.Name,
                    Description = dtoChild.Description,
                    Quantity = dtoChild.Quantity,
                    Price = dtoChild.Price,
                    CreatedById = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedById = user.Id,
                    UpdatedAt = DateTime.UtcNow,
                    ParentId = item.Id,
                    StoreId = item.StoreId
                };
                item.Children.Add(newChild);
            }
            item.Quantity = item.Children.Count > 0 ? item.Children.Sum(c => c.Quantity) : 0;
        }
        if (dto.Rates != null && dto.Rates.Count > 0)
        {
            item.Rates = dto.Rates.Select(rateDto =>
            {
                var rate = _mapper.Map<ItemRate>(rateDto);
                rate.CreatedById = user.Id;
                rate.CreatedAt = DateTime.UtcNow;
                rate.IsActive = true;
                return rate;
            }).ToList();
        }
        await _itemRepository.UpdateAsync(item, item.Id);
        var updatedItem = await _itemRepository.GetByIdAsync(item.Id);
        return _mapper.Map<ItemResponseDto>(updatedItem);
    }

    public async Task UpdateQuantityAsync(string id, int quantity)
    {
        var item = await _itemRepository.GetByIdAsync(id);
        if (item == null)
            await _itemRepository.UpdateChildQuantityAsync(id, quantity);
        else
            await _itemRepository.UpdateQuantityAsync(id, item.Quantity + quantity);
    }

    public async Task DeleteAsync(string id)
    {
        var item = await _itemRepository.GetByIdAsync(id) ?? throw new KeyNotFoundException(ItemNotFound);
        await _itemRepository.RemoveAsync(item.Id);
    }

    public async Task<IEnumerable<ItemResponseDto>> SearchItemsAsync(string? keyword)
    {
        var items = await _itemRepository.SearchAsync(keyword);
        return _mapper.Map<IEnumerable<ItemResponseDto>>(items);
    }

    public async Task<ItemResponseDto> GetParentItemAsync(string childId)
    {
        var parentItem = await _itemRepository.GetParentItemAsync(childId) ??
            throw new KeyNotFoundException("Parent item not found");
        return _mapper.Map<ItemResponseDto>(parentItem);
    }

    public async Task<decimal> GetItemRateAsync(string itemId, int rentalDays)
    {
        var item = await _itemRepository.GetByIdAsync(itemId) ?? throw new KeyNotFoundException(ItemNotFound);
        if (item.Rates == null || item.Rates.Count == 0)
            throw new InvalidOperationException("No rates found for this item");
        var rate = item.Rates
            .Where(r => r.IsActive && r.MinDays <= rentalDays)
            .OrderByDescending(r => r.MinDays)
            .FirstOrDefault();
        if (rate == null)
            throw new InvalidOperationException("No applicable rate found for this item");
        return rate.DailyRate;
    }
}
