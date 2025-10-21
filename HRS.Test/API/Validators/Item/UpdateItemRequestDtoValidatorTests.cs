using FluentValidation.TestHelper;
using HRS.API.Contracts.DTOs.Item;
using HRS.API.Validators.Item;

namespace HRS.Test.API.Validators.Item;

public class UpdateItemRequestDtoValidatorTests
{
    private readonly UpdateItemRequestDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Model_Is_Not_Valid()
    {
        var model = new UpdateItemRequestDto
        {
            Id = "",
            Name = "",
            Description = "",
            Quantity = -1,
            Price = -1,
            StoreId = ""
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_Have_Error_When_Child_Has_Invalid_Fields()
    {
        var child = new UpdateItemRequestDto
        {
            Name = "",
            Description = "",
            Quantity = -1,
            Price = -1,
            StoreId = ""
        };
        var model = new UpdateItemRequestDto
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "Parent",
            Description = "Parent Desc",
            Quantity = 1,
            Price = 1,
            Children = new List<UpdateItemRequestDto>
            {
                child
            },
            StoreId = "1"
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Children[0].Name");
        result.ShouldHaveValidationErrorFor("Children[0].Quantity");
        result.ShouldHaveValidationErrorFor("Children[0].Price");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var child = new UpdateItemRequestDto
        {
            Name = "Child",
            Description = "Child Desc",
            Quantity = 1,
            Price = 1,
            StoreId = "1"
        };
        var model = new UpdateItemRequestDto
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "Parent",
            Description = "Parent Desc",
            Quantity = 1,
            Price = 1,
            Children = new List<UpdateItemRequestDto>
            {
                child
            },
            StoreId = "1"
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
