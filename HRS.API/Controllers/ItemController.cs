using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRS.Shared.Core.Interfaces;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/items")]
public class ItemController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly IUserContextService _userContextService;

    public ItemController(IItemService itemService, IUserContextService userContextService)
    {
        _itemService = itemService;
        _userContextService = userContextService;
    }

    [HttpGet]
    [Authorize(Policy = "read:item")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ItemResponseDto>>>> GetRootItems()
    {
        var items = await _itemService.GetRootItemsAsync(_userContextService.GetStoreId());
        return Ok(ApiResponse<IEnumerable<ItemResponseDto>>.OkResponse(items));
    }

    [HttpGet("store/{id}")]
    [Authorize(Policy = "read:item")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ItemResponseDto>>>> GetRootItems(int id)
    {
        var items = await _itemService.GetRootItemsAsync(id);
        return Ok(ApiResponse<IEnumerable<ItemResponseDto>>.OkResponse(items));
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "read:item")]
    public async Task<ActionResult<ItemResponseDto>> GetItemAsync(string id)
    {
        var res = await _itemService.GetItemAsync(id);
        return Ok(ApiResponse<ItemResponseDto>.OkResponse(res));
    }

    [HttpGet("{id}/parent")]
    [Authorize(Policy = "read:item")]
    public async Task<ActionResult<ItemResponseDto>> GetParentItemAsync(string id)
    {
        var res = await _itemService.GetParentItemAsync(id);
        return Ok(ApiResponse<ItemResponseDto>.OkResponse(res));
    }

    [HttpPost]
    [Authorize(Policy = "write:item")]
    public async Task<ActionResult> CreateItemAsync([FromBody] AddItemRequestDto request)
    {
        var createdItem = await _itemService.CreateAsync(request);

        return CreatedAtAction(
            "GetItem",
            new { id = createdItem.Id },
            ApiResponse<ItemResponseDto>.OkResponse(createdItem)
        );
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "update:item")]
    public async Task<ActionResult> UpdateItemAsync(string id, [FromBody] UpdateItemRequestDto request)
    {
        request.Id = id;
        await _itemService.UpdateAsync(request);
        return Ok(ApiResponse<object>.OkResponse(null, "Item updated successfully"));
    }

    [HttpPut("{id}/quantity")]
    [Authorize(Policy = "update:item")]
    public async Task<ActionResult> UpdateItemQuantityAsync(string id, [FromQuery] int quantity)
    {
        await _itemService.UpdateQuantityAsync(id, quantity);
        return Ok(ApiResponse<object>.OkResponse(null, "Item quantity updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "delete:item")]
    public async Task<ActionResult> DeleteItemAsync(string id)
    {
        await _itemService.DeleteAsync(id);
        return Ok(ApiResponse<object>.OkResponse(null, "Item deleted successfully"));
    }

    [HttpGet("search")]
    [Authorize(Policy = "read:item")]
    public async Task<ActionResult<List<ItemResponseDto>>> SearchItemsAsync([FromQuery] string? keyword)
    {
        var res = await _itemService.SearchItemsAsync(keyword);
        return Ok(ApiResponse<List<ItemResponseDto>>.OkResponse(res.ToList()));
    }
}
