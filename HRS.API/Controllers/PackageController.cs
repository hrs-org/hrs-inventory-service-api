using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Package;
using HRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRS.Shared.Core.Interfaces;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/packages")]
[Authorize]
public class PackageController : ControllerBase
{
    private readonly IPackageService _packageService;
    private readonly IUserContextService _userContextService;

    public PackageController(IPackageService packageService, IUserContextService userContextService)
    {
        _packageService = packageService;
        _userContextService = userContextService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<PackageResponseDto>>> GetPackagesAsync()
    {
        var res = await _packageService.GetAllAsync(_userContextService.GetStoreId());
        return Ok(ApiResponse<IEnumerable<PackageResponseDto>>.OkResponse(res, "Packages retrieved successfully"));
    }

    [HttpGet("store/{id}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PackageResponseDto>>>> GetPackagesAsync(int id)
    {
        var items = await _packageService.GetAllAsync(id);
        return Ok(ApiResponse<IEnumerable<PackageResponseDto>>.OkResponse(items));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PackageResponseDto>> GetPackageAsync(string id)
    {
        var res = await _packageService.GetByIdAsync(id);
        return Ok(ApiResponse<PackageResponseDto>.OkResponse(res, "Package retrieved successfully"));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<PackageResponseDto>>> CreatePackageAsync([FromBody] AddPackageRequestDto request)
    {
        var created = await _packageService.CreateAsync(request);
        return Ok(ApiResponse<PackageResponseDto>.OkResponse(created, "Package created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> UpdatePackageAsync(string id, [FromBody] UpdatePackageRequestDto request)
    {
        request.Id = id;
        var updated = await _packageService.UpdateAsync(request);
        return Ok(ApiResponse<PackageResponseDto>.OkResponse(updated, "Package updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult> DeletePackageAsync(string id)
    {
        await _packageService.DeleteAsync(id);
        return Ok(ApiResponse<string>.OkResponse(null, $"Package {id} deleted successfully"));
    }
}
