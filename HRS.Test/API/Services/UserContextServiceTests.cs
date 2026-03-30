using System.Security.Claims;
using FluentAssertions;
using HRS.API.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace HRS.Test.API.Services;

public class UserContextServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly UserContextService _svc;

    public UserContextServiceTests()
    {
        _svc = new UserContextService(_httpContextAccessor);
    }

    // ──────────────────────────── Helpers ────────────────────────────

    private static HttpContext BuildContext(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ctx = new DefaultHttpContext { User = principal };
        return ctx;
    }

    // ──────────────────────────── GetEmail ────────────────────────────

    [Fact]
    public void GetEmail_WithEmailClaim_ReturnsEmail()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([new Claim(ClaimTypes.Email, "user@example.com")]));
        _svc.GetEmail().Should().Be("user@example.com");
    }

    [Fact]
    public void GetEmail_NoEmailClaim_ReturnsNull()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([]));
        _svc.GetEmail().Should().BeNull();
    }

    [Fact]
    public void GetEmail_NullHttpContext_ReturnsNull()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        _svc.GetEmail().Should().BeNull();
    }

    // ──────────────────────────── GetUserId ────────────────────────────

    [Fact]
    public void GetUserId_FromUserIdClaim_ReturnsUserId()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([new Claim("userId", "42")]));
        _svc.GetUserId().Should().Be(42);
    }

    [Fact]
    public void GetUserId_NoMatchingClaim_ReturnsZero()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([]));
        _svc.GetUserId().Should().Be(0);
    }

    [Fact]
    public void GetUserId_NullHttpContext_ReturnsZero()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        _svc.GetUserId().Should().Be(0);
    }

    [Fact]
    public void GetUserId_NonNumericUserIdClaim_ReturnsZero()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([new Claim("userId", "not-a-number")]));
        _svc.GetUserId().Should().Be(0);
    }

    // ──────────────────────────── GetStoreId ────────────────────────────

    [Fact]
    public void GetStoreId_FromStoreIdClaim_ReturnsStoreId()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([new Claim("storeId", "5")]));
        _svc.GetStoreId().Should().Be(5);
    }

    [Fact]
    public void GetStoreId_MissingStoreIdClaim_ThrowsUnauthorizedAccessException()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([]));
        Assert.Throws<UnauthorizedAccessException>(() => _svc.GetStoreId());
    }

    [Fact]
    public void GetStoreId_NonNumericStoreIdClaim_ThrowsUnauthorizedAccessException()
    {
        _httpContextAccessor.HttpContext.Returns(BuildContext([new Claim("storeId", "not-a-number")]));
        Assert.Throws<UnauthorizedAccessException>(() => _svc.GetStoreId());
    }

    [Fact]
    public void GetStoreId_NullHttpContext_ThrowsUnauthorizedAccessException()
    {
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        Assert.Throws<UnauthorizedAccessException>(() => _svc.GetStoreId());
    }

    // ──────────────────────────── GetUserAsync ────────────────────────────

    [Fact]
    public async Task GetUserAsync_WithAllClaims_ReturnsUserResponseDto()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("userId", "42"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        _httpContextAccessor.HttpContext.Returns(BuildContext(claims));

        // Act
        var result = await _svc.GetUserAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(42);
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task GetUserAsync_WithMissingEmail_ReturnsUnknownEmail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("userId", "42"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim(ClaimTypes.Role, "User")
        };
        _httpContextAccessor.HttpContext.Returns(BuildContext(claims));

        // Act
        var result = await _svc.GetUserAsync();

        // Assert
        result.Email.Should().Be("unknown@example.com");
    }

    [Fact]
    public async Task GetUserAsync_WithMissingFirstName_UsesAlternativeClaimOrDefault()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("userId", "42"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim("firstName", "Jane"),
            new Claim(ClaimTypes.Surname, "Smith"),
            new Claim(ClaimTypes.Role, "User")
        };
        _httpContextAccessor.HttpContext.Returns(BuildContext(claims));

        // Act
        var result = await _svc.GetUserAsync();

        // Assert
        result.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task GetUserAsync_WithMissingLastName_UsesAlternativeClaimOrDefault()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("userId", "42"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim("lastName", "Johnson"),
            new Claim(ClaimTypes.Role, "User")
        };
        _httpContextAccessor.HttpContext.Returns(BuildContext(claims));

        // Act
        var result = await _svc.GetUserAsync();

        // Assert
        result.LastName.Should().Be("Johnson");
    }

    [Fact]
    public async Task GetUserAsync_WithMissingRole_UsesAlternativeClaimOrDefault()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("userId", "42"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim("role", "Manager")
        };
        _httpContextAccessor.HttpContext.Returns(BuildContext(claims));

        // Act
        var result = await _svc.GetUserAsync();

        // Assert
        result.Role.Should().Be("Manager");
    }

    [Fact]
    public async Task GetUserAsync_WithAllMissingOptionalClaims_ReturnsDefaults()
    {
        // Arrange
        var claims = new[] { new Claim("userId", "0") };
        _httpContextAccessor.HttpContext.Returns(BuildContext(claims));

        // Act
        var result = await _svc.GetUserAsync();

        // Assert
        result.Id.Should().Be(0);
        result.Email.Should().Be("unknown@example.com");
        result.FirstName.Should().Be("Unknown");
        result.LastName.Should().Be("User");
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task GetUserAsync_WithNullHttpContext_ReturnsDefaults()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = await _svc.GetUserAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(0);
        result.Email.Should().Be("unknown@example.com");
        result.FirstName.Should().Be("Unknown");
        result.LastName.Should().Be("User");
        result.Role.Should().Be("User");
    }
}
