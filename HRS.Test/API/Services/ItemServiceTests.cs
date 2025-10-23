using AutoMapper;
using FluentAssertions;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using HRS.Shared.Core.Dtos;
using HRS.Shared.Core.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ItemResponseDto = HRS.API.Contracts.DTOs.Item.ItemResponseDto;

namespace HRS.Test.API.Services;

public class ItemServiceTests
{
    private readonly IItemRepository _itemRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;
    private readonly ItemService _sut;

    public ItemServiceTests()
    {
        _itemRepository = Substitute.For<IItemRepository>();
        _mapper = Substitute.For<IMapper>();
        _userContextService = Substitute.For<IUserContextService>();
        _sut = new ItemService(_mapper, _userContextService, _itemRepository);
    }

    private static UserResponseDto CreateMockUser(int id = 1)
    {
        return new UserResponseDto
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Role = "Admin"
        };
    }

    #region GetItemAsync Tests

    [Fact]
    public async Task GetItemAsync_WhenItemExists_ShouldReturnMappedDto()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var item = new Item
        {
            Id = itemId,
            Name = "Tent",
            Description = "4-person camping tent",
            Quantity = 10,
            Price = 150.00m,
            CreatedById = 1,
            StoreId = 1
        };
        var expectedDto = new ItemResponseDto
        {
            Id = itemId,
            Name = "Tent",
            Description = "4-person camping tent",
            Quantity = 10,
            Price = 150.00m
        };

        _itemRepository.GetByIdAsync(itemId).Returns(item);
        _mapper.Map<ItemResponseDto>(item).Returns(expectedDto);

        // Act
        var result = await _sut.GetItemAsync(itemId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
        await _itemRepository.Received(1).GetByIdAsync(itemId);
        _mapper.Received(1).Map<ItemResponseDto>(item);
    }

    #endregion

    #region GetRootItemsAsync Tests

    [Fact]
    public async Task GetRootItemsAsync_WhenItemsExist_ShouldReturnMappedDtos()
    {
        // Arrange
        var storeId = 1;
        var items = new List<Item>
        {
            new() { Id = "507f1f77bcf86cd799439011", Name = "Tent", StoreId = storeId },
            new() { Id = "507f1f77bcf86cd799439012", Name = "Backpack", StoreId = storeId }
        };
        var expectedDtos = new List<ItemResponseDto>
        {
            new() { Id = "507f1f77bcf86cd799439011", Name = "Tent" },
            new() { Id = "507f1f77bcf86cd799439012", Name = "Backpack" }
        };

        _itemRepository.GetRootItemsAsync(storeId).Returns(items);
        _mapper.Map<IEnumerable<ItemResponseDto>>(items).Returns(expectedDtos);

        // Act
        var result = await _sut.GetRootItemsAsync(storeId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedDtos);
        await _itemRepository.Received(1).GetRootItemsAsync(storeId);
        _mapper.Received(1).Map<IEnumerable<ItemResponseDto>>(items);
    }

    [Fact]
    public async Task GetRootItemsAsync_WhenNoItems_ShouldReturnEmptyList()
    {
        // Arrange
        var storeId = 1;
        var emptyList = new List<Item>();
        var emptyDtos = new List<ItemResponseDto>();

        _itemRepository.GetRootItemsAsync(storeId).Returns(emptyList);
        _mapper.Map<IEnumerable<ItemResponseDto>>(emptyList).Returns(emptyDtos);

        // Act
        var result = await _sut.GetRootItemsAsync(storeId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        await _itemRepository.Received(1).GetRootItemsAsync(storeId);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateItemAndReturnDto()
    {
        // Arrange
        var user = CreateMockUser();
        var storeId = 1;
        var dto = new AddItemRequestDto
        {
            Name = "Sleeping Bag",
            Description = "Winter sleeping bag",
            Quantity = 5,
            Price = 80.00m
        };
        var entity = new Item
        {
            Id = "507f1f77bcf86cd799439011",
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Price = dto.Price,
            CreatedById = user.Id,
            StoreId = storeId
        };
        var expectedDto = new ItemResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Quantity = entity.Quantity,
            Price = entity.Price
        };

        _userContextService.GetUserAsync().Returns(user);
        _userContextService.GetStoreId().Returns(storeId);
        _mapper.Map<Item>(dto).Returns(entity);
        _itemRepository.AddAsync(Arg.Any<Item>()).Returns(Task.CompletedTask);
        _itemRepository.GetByIdAsync(entity.Id).Returns(entity);
        _mapper.Map<ItemResponseDto>(entity).Returns(expectedDto);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
        await _userContextService.Received(1).GetUserAsync();
        _userContextService.Received(1).GetStoreId();
        await _itemRepository.Received(1).AddAsync(Arg.Is<Item>(i =>
            i.Name == dto.Name &&
            i.CreatedById == user.Id &&
            i.StoreId == storeId));
        await _itemRepository.Received(1).GetByIdAsync(entity.Id);
    }

    [Fact]
    public async Task CreateAsync_WithChildren_ShouldSetChildrenPropertiesAndUpdateQuantity()
    {
        // Arrange
        var user = CreateMockUser();
        var storeId = 1;
        var dto = new AddItemRequestDto
        {
            Name = "Hiking Boots",
            Description = "Waterproof boots",
            Quantity = 10,
            Price = 120.00m,
            Children = new List<ItemRequestDto>
            {
                new() { Name = "Size 9", Description = "Size 9", Quantity = 5, Price = 120.00m },
                new() { Name = "Size 10", Description = "Size 10", Quantity = 5, Price = 120.00m }
            }
        };
        var entity = new Item
        {
            Id = "507f1f77bcf86cd799439011",
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Price = dto.Price,
            StoreId = storeId,
            Children = new List<Item>
            {
                new() { Name = "Size 9", Quantity = 5, Price = 120.00m },
                new() { Name = "Size 10", Quantity = 5, Price = 120.00m }
            }
        };
        var expectedDto = new ItemResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Quantity = entity.Quantity
        };

        _userContextService.GetUserAsync().Returns(user);
        _userContextService.GetStoreId().Returns(storeId);
        _mapper.Map<Item>(dto).Returns(entity);
        _itemRepository.AddAsync(Arg.Any<Item>()).Returns(Task.CompletedTask);
        _itemRepository.GetByIdAsync(entity.Id).Returns(entity);
        _mapper.Map<ItemResponseDto>(entity).Returns(expectedDto);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        await _itemRepository.Received(1).AddAsync(Arg.Is<Item>(i =>
            i.Children.Count == 2 &&
            i.Quantity == 10)); // Should be sum of children quantities
    }

    [Fact]
    public async Task CreateAsync_WithRates_ShouldProcessRatesCorrectly()
    {
        // Arrange
        var user = CreateMockUser();
        var storeId = 1;
        var dto = new AddItemRequestDto
        {
            Name = "Kayak",
            Description = "Single kayak",
            Quantity = 3,
            Price = 200.00m,
            Rates = new List<ItemRateRequestDto>
            {
                new() { MinDays = 1, DailyRate = 50.00m, IsActive = true },
                new() { MinDays = 7, DailyRate = 40.00m, IsActive = true }
            }
        };
        var entity = new Item
        {
            Id = "507f1f77bcf86cd799439011",
            Name = dto.Name,
            StoreId = storeId,
            Rates = new List<ItemRate>()
        };
        var expectedDto = new ItemResponseDto { Id = entity.Id, Name = entity.Name };

        _userContextService.GetUserAsync().Returns(user);
        _userContextService.GetStoreId().Returns(storeId);
        _mapper.Map<Item>(dto).Returns(entity);
        _mapper.Map<ItemRate>(Arg.Any<ItemRateRequestDto>()).Returns(
            x => new ItemRate
            {
                MinDays = x.Arg<ItemRateRequestDto>().MinDays,
                DailyRate = x.Arg<ItemRateRequestDto>().DailyRate,
                IsActive = true
            });
        _itemRepository.AddAsync(Arg.Any<Item>()).Returns(Task.CompletedTask);
        _itemRepository.GetByIdAsync(entity.Id).Returns(entity);
        _mapper.Map<ItemResponseDto>(entity).Returns(expectedDto);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        await _itemRepository.Received(1).AddAsync(Arg.Is<Item>(i =>
            i.Rates.Count == 2 &&
            i.Rates.All(r => r.CreatedById == user.Id && r.IsActive)));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidDto_ShouldUpdateItemAndReturnDto()
    {
        // Arrange
        var user = CreateMockUser();
        var itemId = "507f1f77bcf86cd799439011";
        var existingItem = new Item
        {
            Id = itemId,
            Name = "Old Name",
            Description = "Old Description",
            Quantity = 5,
            Price = 50.00m,
            StoreId = 1,
            Children = new List<Item>()
        };
        var dto = new UpdateItemRequestDto
        {
            Id = itemId,
            Name = "New Name",
            Description = "New Description",
            Quantity = 10,
            Price = 75.00m
        };
        var updatedItem = new Item
        {
            Id = itemId,
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Price = dto.Price,
            StoreId = 1
        };
        var expectedDto = new ItemResponseDto
        {
            Id = itemId,
            Name = dto.Name,
            Description = dto.Description,
            Quantity = dto.Quantity,
            Price = dto.Price
        };

        _userContextService.GetUserAsync().Returns(user);
        _itemRepository.GetByIdAsync(itemId).Returns(existingItem, updatedItem);
        _itemRepository.UpdateAsync(Arg.Any<Item>(), itemId).Returns(Task.CompletedTask);
        _mapper.Map<ItemResponseDto>(updatedItem).Returns(expectedDto);

        // Act
        var result = await _sut.UpdateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
        await _itemRepository.Received(2).GetByIdAsync(itemId);
        await _itemRepository.Received(1).UpdateAsync(Arg.Is<Item>(i =>
            i.Name == dto.Name &&
            i.Description == dto.Description &&
            i.Price == dto.Price &&
            i.UpdatedById == user.Id), itemId);
    }

    [Fact]
    public async Task UpdateAsync_WhenIdIsNull_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateItemRequestDto
        {
            Id = null,
            Name = "Test",
            Description = "Test",
            Quantity = 1,
            Price = 10.00m
        };

        // Act
        var act = async () => await _sut.UpdateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found");
        await _itemRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateAsync_WhenIdIsEmpty_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdateItemRequestDto
        {
            Id = string.Empty,
            Name = "Test",
            Description = "Test",
            Quantity = 1,
            Price = 10.00m
        };

        // Act
        var act = async () => await _sut.UpdateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found");
        await _itemRepository.DidNotReceive().GetByIdAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateAsync_WhenItemNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var dto = new UpdateItemRequestDto
        {
            Id = itemId,
            Name = "Test",
            Description = "Test",
            Quantity = 1,
            Price = 10.00m
        };

        _itemRepository.GetByIdAsync(itemId).Returns((Item?)null);

        // Act
        var act = async () => await _sut.UpdateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found");
        await _itemRepository.Received(1).GetByIdAsync(itemId);
        await _itemRepository.DidNotReceive().UpdateAsync(Arg.Any<Item>(), Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateAsync_WithChildren_ShouldUpdateExistingAndAddNewChildren()
    {
        // Arrange
        var user = CreateMockUser();
        var itemId = "507f1f77bcf86cd799439011";
        var childId1 = "507f1f77bcf86cd799439021";
        var childId2 = "507f1f77bcf86cd799439022";

        var existingItem = new Item
        {
            Id = itemId,
            Name = "Parent",
            StoreId = 1,
            Children = new List<Item>
            {
                new() { Id = childId1, Name = "Child 1", Quantity = 5, Price = 10 },
                new() { Id = childId2, Name = "Child 2", Quantity = 3, Price = 15 }
            }
        };

        var dto = new UpdateItemRequestDto
        {
            Id = itemId,
            Name = "Updated Parent",
            Description = "Updated",
            Quantity = 10,
            Price = 100,
            Children = new List<UpdateItemRequestDto>
            {
                new() { Id = childId1, Name = "Updated Child 1", Description = "Desc", Quantity = 7, Price = 12 },
                new() { Name = "New Child", Description = "New", Quantity = 3, Price = 20 }
            }
        };

        var updatedItem = new Item { Id = itemId, Name = dto.Name };
        var expectedDto = new ItemResponseDto { Id = itemId, Name = dto.Name };

        _userContextService.GetUserAsync().Returns(user);
        _itemRepository.GetByIdAsync(itemId).Returns(existingItem, updatedItem);
        _itemRepository.UpdateAsync(Arg.Any<Item>(), itemId).Returns(Task.CompletedTask);
        _mapper.Map<ItemResponseDto>(updatedItem).Returns(expectedDto);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _itemRepository.Received(1).UpdateAsync(Arg.Is<Item>(i =>
            i.Children.Count == 2 && // One updated, one new
            i.Quantity == 10), itemId); // Sum of children quantities
    }

    [Fact]
    public async Task UpdateAsync_WithRates_ShouldReplaceAllRates()
    {
        // Arrange
        var user = CreateMockUser();
        var itemId = "507f1f77bcf86cd799439011";
        var existingItem = new Item
        {
            Id = itemId,
            Name = "Item",
            StoreId = 1,
            Children = new List<Item>(),
            Rates = new List<ItemRate>
            {
                new() { MinDays = 1, DailyRate = 10, IsActive = true }
            }
        };
        var dto = new UpdateItemRequestDto
        {
            Id = itemId,
            Name = "Item",
            Description = "Updated",
            Quantity = 5,
            Price = 50,
            Rates = new List<ItemRateRequestDto>
            {
                new() { MinDays = 1, DailyRate = 15, IsActive = true },
                new() { MinDays = 7, DailyRate = 12, IsActive = true }
            }
        };
        var updatedItem = new Item { Id = itemId };
        var expectedDto = new ItemResponseDto { Id = itemId };

        _userContextService.GetUserAsync().Returns(user);
        _itemRepository.GetByIdAsync(itemId).Returns(existingItem, updatedItem);
        _mapper.Map<ItemRate>(Arg.Any<ItemRateRequestDto>()).Returns(
            x => new ItemRate
            {
                MinDays = x.Arg<ItemRateRequestDto>().MinDays,
                DailyRate = x.Arg<ItemRateRequestDto>().DailyRate
            });
        _itemRepository.UpdateAsync(Arg.Any<Item>(), itemId).Returns(Task.CompletedTask);
        _mapper.Map<ItemResponseDto>(updatedItem).Returns(expectedDto);

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _itemRepository.Received(1).UpdateAsync(Arg.Is<Item>(i =>
            i.Rates.Count == 2 &&
            i.Rates.All(r => r.CreatedById == user.Id && r.IsActive)), itemId);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenItemExists_ShouldRemoveItem()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var item = new Item
        {
            Id = itemId,
            Name = "Test Item",
            StoreId = 1
        };

        _itemRepository.GetByIdAsync(itemId).Returns(item);
        _itemRepository.RemoveAsync(itemId).Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAsync(itemId);

        // Assert
        await _itemRepository.Received(1).GetByIdAsync(itemId);
        await _itemRepository.Received(1).RemoveAsync(itemId);
    }

    [Fact]
    public async Task DeleteAsync_WhenItemNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        _itemRepository.GetByIdAsync(itemId).Returns((Item?)null);

        // Act
        var act = async () => await _sut.DeleteAsync(itemId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found");
        await _itemRepository.Received(1).GetByIdAsync(itemId);
        await _itemRepository.DidNotReceive().RemoveAsync(Arg.Any<string>());
    }

    #endregion

    #region SearchItemsAsync Tests

    [Fact]
    public async Task SearchItemsAsync_WithKeyword_ShouldReturnMatchingItems()
    {
        // Arrange
        var keyword = "tent";
        var items = new List<Item>
        {
            new() { Id = "507f1f77bcf86cd799439011", Name = "Camping Tent", StoreId = 1 },
            new() { Id = "507f1f77bcf86cd799439012", Name = "Family Tent", StoreId = 1 }
        };
        var expectedDtos = new List<ItemResponseDto>
        {
            new() { Id = "507f1f77bcf86cd799439011", Name = "Camping Tent" },
            new() { Id = "507f1f77bcf86cd799439012", Name = "Family Tent" }
        };

        _itemRepository.SearchAsync(keyword).Returns(items);
        _mapper.Map<IEnumerable<ItemResponseDto>>(items).Returns(expectedDtos);

        // Act
        var result = await _sut.SearchItemsAsync(keyword);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedDtos);
        await _itemRepository.Received(1).SearchAsync(keyword);
    }

    [Fact]
    public async Task SearchItemsAsync_WithNullKeyword_ShouldReturnAllItems()
    {
        // Arrange
        var items = new List<Item>
        {
            new() { Id = "507f1f77bcf86cd799439011", Name = "Item 1" },
            new() { Id = "507f1f77bcf86cd799439012", Name = "Item 2" }
        };
        var expectedDtos = new List<ItemResponseDto>
        {
            new() { Id = "507f1f77bcf86cd799439011", Name = "Item 1" },
            new() { Id = "507f1f77bcf86cd799439012", Name = "Item 2" }
        };

        _itemRepository.SearchAsync(null).Returns(items);
        _mapper.Map<IEnumerable<ItemResponseDto>>(items).Returns(expectedDtos);

        // Act
        var result = await _sut.SearchItemsAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        await _itemRepository.Received(1).SearchAsync(null);
    }

    [Fact]
    public async Task SearchItemsAsync_WhenNoMatch_ShouldReturnEmptyList()
    {
        // Arrange
        var keyword = "nonexistent";
        var emptyList = new List<Item>();
        var emptyDtos = new List<ItemResponseDto>();

        _itemRepository.SearchAsync(keyword).Returns(emptyList);
        _mapper.Map<IEnumerable<ItemResponseDto>>(emptyList).Returns(emptyDtos);

        // Act
        var result = await _sut.SearchItemsAsync(keyword);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        await _itemRepository.Received(1).SearchAsync(keyword);
    }

    #endregion

    #region GetParentItemAsync Tests

    [Fact]
    public async Task GetParentItemAsync_WhenParentExists_ShouldReturnMappedDto()
    {
        // Arrange
        var childId = "507f1f77bcf86cd799439012";
        var parentItem = new Item
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "Parent Item",
            Description = "Parent",
            StoreId = 1
        };
        var expectedDto = new ItemResponseDto
        {
            Id = parentItem.Id,
            Name = parentItem.Name,
            Description = parentItem.Description
        };

        _itemRepository.GetParentItemAsync(childId).Returns(parentItem);
        _mapper.Map<ItemResponseDto>(parentItem).Returns(expectedDto);

        // Act
        var result = await _sut.GetParentItemAsync(childId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
        await _itemRepository.Received(1).GetParentItemAsync(childId);
        _mapper.Received(1).Map<ItemResponseDto>(parentItem);
    }

    #endregion

    #region GetItemRateAsync Tests

    [Fact]
    public async Task GetItemRateAsync_WhenRateExists_ShouldReturnApplicableRate()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var rentalDays = 5;
        var item = new Item
        {
            Id = itemId,
            Name = "Item",
            StoreId = 1,
            Rates = new List<ItemRate>
            {
                new() { MinDays = 1, DailyRate = 50.00m, IsActive = true },
                new() { MinDays = 3, DailyRate = 45.00m, IsActive = true },
                new() { MinDays = 7, DailyRate = 40.00m, IsActive = true }
            }
        };

        _itemRepository.GetByIdAsync(itemId).Returns(item);

        // Act
        var result = await _sut.GetItemRateAsync(itemId, rentalDays);

        // Assert
        result.Should().Be(45.00m); // Should return rate for MinDays = 3 (highest <= 5)
        await _itemRepository.Received(1).GetByIdAsync(itemId);
    }

    [Fact]
    public async Task GetItemRateAsync_WhenItemNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var rentalDays = 5;
        _itemRepository.GetByIdAsync(itemId).Returns((Item?)null);

        // Act
        var act = async () => await _sut.GetItemRateAsync(itemId, rentalDays);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Item not found");
        await _itemRepository.Received(1).GetByIdAsync(itemId);
    }

    [Fact]
    public async Task GetItemRateAsync_WhenNoRatesExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var rentalDays = 5;
        var item = new Item
        {
            Id = itemId,
            Name = "Item",
            StoreId = 1,
            Rates = new List<ItemRate>()
        };

        _itemRepository.GetByIdAsync(itemId).Returns(item);

        // Act
        var act = async () => await _sut.GetItemRateAsync(itemId, rentalDays);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No rates found for this item");
        await _itemRepository.Received(1).GetByIdAsync(itemId);
    }

    [Fact]
    public async Task GetItemRateAsync_WhenNoApplicableRate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var rentalDays = 2;
        var item = new Item
        {
            Id = itemId,
            Name = "Item",
            StoreId = 1,
            Rates = new List<ItemRate>
            {
                new() { MinDays = 3, DailyRate = 45.00m, IsActive = true },
                new() { MinDays = 7, DailyRate = 40.00m, IsActive = true }
            }
        };

        _itemRepository.GetByIdAsync(itemId).Returns(item);

        // Act
        var act = async () => await _sut.GetItemRateAsync(itemId, rentalDays);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No applicable rate found for this item");
        await _itemRepository.Received(1).GetByIdAsync(itemId);
    }

    [Fact]
    public async Task GetItemRateAsync_WhenOnlyInactiveRates_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var rentalDays = 5;
        var item = new Item
        {
            Id = itemId,
            Name = "Item",
            StoreId = 1,
            Rates = new List<ItemRate>
            {
                new() { MinDays = 1, DailyRate = 50.00m, IsActive = false },
                new() { MinDays = 3, DailyRate = 45.00m, IsActive = false }
            }
        };

        _itemRepository.GetByIdAsync(itemId).Returns(item);

        // Act
        var act = async () => await _sut.GetItemRateAsync(itemId, rentalDays);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No applicable rate found for this item");
        await _itemRepository.Received(1).GetByIdAsync(itemId);
    }

    #endregion

    #region UpdateQuantityAsync Tests

    [Fact]
    public async Task UpdateQuantityAsync_WhenItemExists_ShouldUpdateItemQuantity()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var currentQuantity = 10;
        var additionalQuantity = 5;
        var item = new Item
        {
            Id = itemId,
            Name = "Item",
            Quantity = currentQuantity,
            StoreId = 1
        };

        _itemRepository.GetByIdAsync(itemId).Returns(item);
        _itemRepository.UpdateQuantityAsync(itemId, currentQuantity + additionalQuantity)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateQuantityAsync(itemId, additionalQuantity);

        // Assert
        await _itemRepository.Received(1).GetByIdAsync(itemId);
        await _itemRepository.Received(1).UpdateQuantityAsync(itemId, 15);
        await _itemRepository.DidNotReceive().UpdateChildQuantityAsync(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task UpdateQuantityAsync_WhenItemNotExists_ShouldUpdateChildQuantity()
    {
        // Arrange
        var childId = "507f1f77bcf86cd799439012";
        var quantity = 8;

        _itemRepository.GetByIdAsync(childId).Returns((Item?)null);
        _itemRepository.UpdateChildQuantityAsync(childId, quantity).Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateQuantityAsync(childId, quantity);

        // Assert
        await _itemRepository.Received(1).GetByIdAsync(childId);
        await _itemRepository.Received(1).UpdateChildQuantityAsync(childId, quantity);
        await _itemRepository.DidNotReceive().UpdateQuantityAsync(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task UpdateQuantityAsync_WithNegativeQuantity_ShouldDecreaseQuantity()
    {
        // Arrange
        var itemId = "507f1f77bcf86cd799439011";
        var currentQuantity = 10;
        var decreaseQuantity = -3;
        var item = new Item
        {
            Id = itemId,
            Name = "Item",
            Quantity = currentQuantity,
            StoreId = 1
        };

        _itemRepository.GetByIdAsync(itemId).Returns(item);
        _itemRepository.UpdateQuantityAsync(itemId, currentQuantity + decreaseQuantity)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.UpdateQuantityAsync(itemId, decreaseQuantity);

        // Assert
        await _itemRepository.Received(1).GetByIdAsync(itemId);
        await _itemRepository.Received(1).UpdateQuantityAsync(itemId, 7);
    }

    #endregion
}
