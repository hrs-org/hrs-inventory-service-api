using AutoMapper;
using FluentAssertions;
using HRS.API.Contracts.DTOs.Package;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using HRS.Shared.Core.Dtos;
using HRS.Shared.Core.Interfaces;
using NSubstitute;
using PackageResponseDto = HRS.API.Contracts.DTOs.Package.PackageResponseDto;

namespace HRS.Test.API.Services;

public class PackageServiceTests
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;
    private readonly PackageService _sut;

    public PackageServiceTests()
    {
        _packageRepository = Substitute.For<IPackageRepository>();
        _mapper = Substitute.For<IMapper>();
        _userContextService = Substitute.For<IUserContextService>();
        _sut = new PackageService(_mapper, _userContextService, _packageRepository);
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

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedPackages()
    {
        // Arrange
        var storeId = 1;
        var packages = new List<Package>
        {
            new() { Id = "507f1f77bcf86cd799439011", Name = "Package 1", StoreId = storeId },
            new() { Id = "507f1f77bcf86cd799439012", Name = "Package 2", StoreId = storeId }
        };
        var expectedDtos = new List<PackageResponseDto>
        {
            new() { Id = "507f1f77bcf86cd799439011", Name = "Package 1" },
            new() { Id = "507f1f77bcf86cd799439012", Name = "Package 2" }
        };

        _packageRepository.GetAllWithDetailsAsync(storeId).Returns(packages);
        _mapper.Map<IEnumerable<PackageResponseDto>>(packages).Returns(expectedDtos);

        // Act
        var result = await _sut.GetAllAsync(storeId);

        // Assert
        result.Should().BeEquivalentTo(expectedDtos);
        await _packageRepository.Received(1).GetAllWithDetailsAsync(storeId);
        _mapper.Received(1).Map<IEnumerable<PackageResponseDto>>(packages);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenPackageExists_ShouldReturnMappedDto()
    {
        // Arrange
        var packageId = "507f1f77bcf86cd799439011";
        var package = new Package { Id = packageId, Name = "Test Package", StoreId = 1 };
        var expectedDto = new PackageResponseDto { Id = packageId, Name = "Test Package" };

        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns(package);
        _mapper.Map<PackageResponseDto>(package).Returns(expectedDto);

        // Act
        var result = await _sut.GetByIdAsync(packageId);

        // Assert
        result.Should().BeEquivalentTo(expectedDto);
        await _packageRepository.Received(1).GetByIdWithDetailsAsync(packageId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPackageNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var packageId = "507f1f77bcf86cd799439011";
        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns((Package?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(packageId));
        await _packageRepository.Received(1).GetByIdWithDetailsAsync(packageId);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateAndReturnPackage()
    {
        // Arrange
        var user = CreateMockUser();
        var storeId = 1;
        var dto = new AddPackageRequestDto
        {
            Name = "New Package",
            Description = "Description",
            BasePrice = 100.00m
        };
        var entity = new Package
        {
            Id = "507f1f77bcf86cd799439011",
            Name = dto.Name,
            PackageItems = new List<PackageItem>(),
            PackageRates = new List<PackageRate>()
        };
        var expectedDto = new PackageResponseDto { Id = entity.Id, Name = entity.Name };

        _userContextService.GetUserAsync().Returns(user);
        _userContextService.GetStoreId().Returns(storeId);
        _mapper.Map<Package>(dto).Returns(entity);
        _packageRepository.AddAsync(Arg.Any<Package>()).Returns(Task.CompletedTask);
        _mapper.Map<PackageResponseDto>(entity).Returns(expectedDto);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().BeEquivalentTo(expectedDto);
        await _packageRepository.Received(1).AddAsync(Arg.Is<Package>(p =>
            p.CreatedById == user.Id &&
            p.UpdatedById == user.Id &&
            p.StoreId == storeId));
    }

    [Fact]
    public async Task CreateAsync_WithRates_ShouldInitializeRatesWithUserId()
    {
        // Arrange
        var user = CreateMockUser();
        var storeId = 1;
        var dto = new AddPackageRequestDto
        {
            Name = "Package",
            BasePrice = 100.00m
        };
        var entity = new Package
        {
            Id = "507f1f77bcf86cd799439011",
            PackageItems = new List<PackageItem>(),
            PackageRates = new List<PackageRate>
            {
                new() { MinDays = 1, DailyRate = 50.00m },
                new() { MinDays = 7, DailyRate = 40.00m }
            }
        };
        var expectedDto = new PackageResponseDto { Id = entity.Id };

        _userContextService.GetUserAsync().Returns(user);
        _userContextService.GetStoreId().Returns(storeId);
        _mapper.Map<Package>(dto).Returns(entity);
        _packageRepository.AddAsync(Arg.Any<Package>()).Returns(Task.CompletedTask);
        _mapper.Map<PackageResponseDto>(entity).Returns(expectedDto);

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        await _packageRepository.Received(1).AddAsync(Arg.Is<Package>(p =>
            p.PackageRates.All(r => r.CreatedById == user.Id && r.IsActive)));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidDto_ShouldUpdatePackage()
    {
        // Arrange
        var user = CreateMockUser();
        var packageId = "507f1f77bcf86cd799439011";
        var existingPackage = new Package
        {
            Id = packageId,
            Name = "Old",
            StoreId = 1,
            PackageItems = new List<PackageItem>(),
            PackageRates = new List<PackageRate>()
        };
        var dto = new UpdatePackageRequestDto
        {
            Id = packageId,
            Name = "New",
            Description = "Updated",
            BasePrice = 150.00m
        };
        var expectedDto = new PackageResponseDto { Id = packageId, Name = "New" };

        _userContextService.GetUserAsync().Returns(user);
        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns(existingPackage);
        _packageRepository.UpdateAsync(Arg.Any<Package>(), packageId).Returns(Task.FromResult<object>(null!));
        _mapper.Map<PackageResponseDto>(Arg.Any<Package>()).Returns(expectedDto);

        // Act
        var result = await _sut.UpdateAsync(dto);

        // Assert
        result.Should().BeEquivalentTo(expectedDto);
        await _packageRepository.Received(1).UpdateAsync(Arg.Is<Package>(p =>
            p.Name == dto.Name &&
            p.Description == dto.Description &&
            p.BasePrice == dto.BasePrice &&
            p.UpdatedById == user.Id), packageId);
    }

    [Fact]
    public async Task UpdateAsync_WhenPackageNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var dto = new UpdatePackageRequestDto
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "Test",
            BasePrice = 100.00m
        };

        _packageRepository.GetByIdWithDetailsAsync(dto.Id).Returns((Package?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(dto));
        await _packageRepository.DidNotReceive().UpdateAsync(Arg.Any<Package>(), Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateAsync_WithNullItems_ShouldClearAllItems()
    {
        // Arrange
        var user = CreateMockUser();
        var packageId = "507f1f77bcf86cd799439011";
        var existingPackage = new Package
        {
            Id = packageId,
            Name = "Package",
            StoreId = 1,
            PackageItems = new List<PackageItem>
            {
                new() { ItemId = "item1", Quantity = 1 }
            },
            PackageRates = new List<PackageRate>()
        };
        var dto = new UpdatePackageRequestDto
        {
            Id = packageId,
            Name = "Updated",
            BasePrice = 100.00m,
            Items = null
        };

        _userContextService.GetUserAsync().Returns(user);
        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns(existingPackage);
        _packageRepository.UpdateAsync(Arg.Any<Package>(), packageId).Returns(Task.FromResult<object>(null!));
        _mapper.Map<PackageResponseDto>(Arg.Any<Package>()).Returns(new PackageResponseDto());

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _packageRepository.Received(1).UpdateAsync(Arg.Is<Package>(p =>
            p.PackageItems.Count == 0), packageId);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyItems_ShouldClearAllItems()
    {
        // Arrange
        var user = CreateMockUser();
        var packageId = "507f1f77bcf86cd799439011";
        var existingPackage = new Package
        {
            Id = packageId,
            Name = "Package",
            StoreId = 1,
            PackageItems = new List<PackageItem>
            {
                new() { ItemId = "item1", Quantity = 1 }
            },
            PackageRates = new List<PackageRate>()
        };
        var dto = new UpdatePackageRequestDto
        {
            Id = packageId,
            Name = "Updated",
            BasePrice = 100.00m,
            Items = new List<PackageItemRequestDto>()
        };

        _userContextService.GetUserAsync().Returns(user);
        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns(existingPackage);
        _packageRepository.UpdateAsync(Arg.Any<Package>(), packageId).Returns(Task.FromResult<object>(null!));
        _mapper.Map<PackageResponseDto>(Arg.Any<Package>()).Returns(new PackageResponseDto());

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _packageRepository.Received(1).UpdateAsync(Arg.Is<Package>(p =>
            p.PackageItems.Count == 0), packageId);
    }

    [Fact]
    public async Task UpdateAsync_WithItems_ShouldUpdateExistingAndAddNew()
    {
        // Arrange
        var user = CreateMockUser();
        var packageId = "507f1f77bcf86cd799439011";
        var item1 = "item1";
        var item2 = "item2";
        var item3 = "item3";

        var existingPackage = new Package
        {
            Id = packageId,
            Name = "Package",
            StoreId = 1,
            PackageItems = new List<PackageItem>
            {
                new() { ItemId = item1, Quantity = 1 },
                new() { ItemId = item2, Quantity = 2 }
            },
            PackageRates = new List<PackageRate>()
        };
        var dto = new UpdatePackageRequestDto
        {
            Id = packageId,
            Name = "Updated",
            BasePrice = 100.00m,
            Items = new List<PackageItemRequestDto>
            {
                new() { ItemId = item1, Quantity = 5 }, // Updated
                new() { ItemId = item3, Quantity = 3 }  // New (item2 removed)
            }
        };

        _userContextService.GetUserAsync().Returns(user);
        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns(existingPackage);
        _packageRepository.UpdateAsync(Arg.Any<Package>(), packageId).Returns(Task.FromResult<object>(null!));
        _mapper.Map<PackageResponseDto>(Arg.Any<Package>()).Returns(new PackageResponseDto());

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _packageRepository.Received(1).UpdateAsync(Arg.Is<Package>(p =>
            p.PackageItems.Count == 2 &&
            p.PackageItems.Any(pi => pi.ItemId == item1 && pi.Quantity == 5) &&
            p.PackageItems.Any(pi => pi.ItemId == item3 && pi.Quantity == 3) &&
            !p.PackageItems.Any(pi => pi.ItemId == item2)), packageId);
    }

    [Fact]
    public async Task UpdateAsync_WithNullRates_ShouldClearAllRates()
    {
        // Arrange
        var user = CreateMockUser();
        var packageId = "507f1f77bcf86cd799439011";
        var existingPackage = new Package
        {
            Id = packageId,
            Name = "Package",
            StoreId = 1,
            PackageItems = new List<PackageItem>(),
            PackageRates = new List<PackageRate>
            {
                new() { MinDays = 1, DailyRate = 50.00m }
            }
        };
        var dto = new UpdatePackageRequestDto
        {
            Id = packageId,
            Name = "Updated",
            BasePrice = 100.00m,
            Rates = null
        };

        _userContextService.GetUserAsync().Returns(user);
        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns(existingPackage);
        _packageRepository.UpdateAsync(Arg.Any<Package>(), packageId).Returns(Task.FromResult<object>(null!));
        _mapper.Map<PackageResponseDto>(Arg.Any<Package>()).Returns(new PackageResponseDto());

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _packageRepository.Received(1).UpdateAsync(Arg.Is<Package>(p =>
            p.PackageRates.Count == 0), packageId);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyRates_ShouldClearAllRates()
    {
        // Arrange
        var user = CreateMockUser();
        var packageId = "507f1f77bcf86cd799439011";
        var existingPackage = new Package
        {
            Id = packageId,
            Name = "Package",
            StoreId = 1,
            PackageItems = new List<PackageItem>(),
            PackageRates = new List<PackageRate>
            {
                new() { MinDays = 1, DailyRate = 50.00m }
            }
        };
        var dto = new UpdatePackageRequestDto
        {
            Id = packageId,
            Name = "Updated",
            BasePrice = 100.00m,
            Rates = new List<PackageRateRequestDto>()
        };

        _userContextService.GetUserAsync().Returns(user);
        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns(existingPackage);
        _packageRepository.UpdateAsync(Arg.Any<Package>(), packageId).Returns(Task.FromResult<object>(null!));
        _mapper.Map<PackageResponseDto>(Arg.Any<Package>()).Returns(new PackageResponseDto());

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _packageRepository.Received(1).UpdateAsync(Arg.Is<Package>(p =>
            p.PackageRates.Count == 0), packageId);
    }

    [Fact]
    public async Task UpdateAsync_WithRates_ShouldUpdateExistingAndAddNew()
    {
        // Arrange
        var user = CreateMockUser();
        var packageId = "507f1f77bcf86cd799439011";
        var existingPackage = new Package
        {
            Id = packageId,
            Name = "Package",
            StoreId = 1,
            PackageItems = new List<PackageItem>(),
            PackageRates = new List<PackageRate>
            {
                new() { MinDays = 1, DailyRate = 50.00m, IsActive = true },
                new() { MinDays = 3, DailyRate = 45.00m, IsActive = true }
            }
        };
        var dto = new UpdatePackageRequestDto
        {
            Id = packageId,
            Name = "Updated",
            BasePrice = 100.00m,
            Rates = new List<PackageRateRequestDto>
            {
                new() { MinDays = 1, DailyRate = 55.00m, IsActive = false }, // Updated
                new() { MinDays = 7, DailyRate = 40.00m, IsActive = true }   // New (MinDays=3 removed)
            }
        };

        _userContextService.GetUserAsync().Returns(user);
        _packageRepository.GetByIdWithDetailsAsync(packageId).Returns(existingPackage);
        _packageRepository.UpdateAsync(Arg.Any<Package>(), packageId).Returns(Task.FromResult<object>(null!));
        _mapper.Map<PackageResponseDto>(Arg.Any<Package>()).Returns(new PackageResponseDto());

        // Act
        await _sut.UpdateAsync(dto);

        // Assert
        await _packageRepository.Received(1).UpdateAsync(Arg.Is<Package>(p =>
            p.PackageRates.Count == 2 &&
            p.PackageRates.Any(r => r.MinDays == 1 && r.DailyRate == 55.00m && !r.IsActive) &&
            p.PackageRates.Any(r => r.MinDays == 7 && r.DailyRate == 40.00m && r.IsActive) &&
            !p.PackageRates.Any(r => r.MinDays == 3)), packageId);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenPackageExists_ShouldRemovePackage()
    {
        // Arrange
        var packageId = "507f1f77bcf86cd799439011";
        var package = new Package { Id = packageId, Name = "Test", StoreId = 1 };

        _packageRepository.GetByIdAsync(packageId).Returns(package);
        _packageRepository.RemoveAsync(packageId).Returns(Task.FromResult<object>(null!));

        // Act
        await _sut.DeleteAsync(packageId);

        // Assert
        await _packageRepository.Received(1).GetByIdAsync(packageId);
        await _packageRepository.Received(1).RemoveAsync(packageId);
    }

    [Fact]
    public async Task DeleteAsync_WhenPackageNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var packageId = "507f1f77bcf86cd799439011";
        _packageRepository.GetByIdAsync(packageId).Returns((Package?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(packageId));
        await _packageRepository.Received(1).GetByIdAsync(packageId);
        await _packageRepository.DidNotReceive().RemoveAsync(Arg.Any<string>());
    }

    #endregion
}
