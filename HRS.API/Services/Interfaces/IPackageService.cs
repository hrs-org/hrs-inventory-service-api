using HRS.API.Contracts.DTOs.Package;

namespace HRS.API.Services.Interfaces;

public interface IPackageService
{
    Task<IEnumerable<PackageResponseDto>> GetAllAsync(string storeId);
    Task<PackageResponseDto> GetByIdAsync(string id);
    Task<PackageResponseDto> CreateAsync(AddPackageRequestDto dto);
    Task<PackageResponseDto> UpdateAsync(UpdatePackageRequestDto dto);
    Task DeleteAsync(string id);
}
