using FluentValidation;
using HRS.API.Contracts.DTOs.Item;

namespace HRS.API.Validators.Item;

public class AddItemRequestDtoValidator : AbstractValidator<AddItemRequestDto>
{
    public AddItemRequestDtoValidator()
    {
        ItemRequestValidatorHelper.AddCommonRules(this);

        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store Id is required");

        RuleForEach(x => x.Rates)
            .SetValidator(new ItemRateRequestDtoValidator());

        RuleForEach(x => x.Children)
            .SetValidator(new ItemChildRequestDtoValidator());
    }
}

public class UpdateItemRequestDtoValidator : AbstractValidator<UpdateItemRequestDto>
{
    public UpdateItemRequestDtoValidator()
    {
        ItemRequestValidatorHelper.AddCommonRules(this);
        ParentItemRequestValidatorHelper.AddCommonRules(this);

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item Id is required")
            .Matches(@"^[0-9a-fA-F]{24}$").WithMessage("Item Id must be a valid ObjectId");

        RuleForEach(x => x.Children)
            .SetValidator(new ItemChildRequestDtoValidator());
    }
}

public static class ItemRequestValidatorHelper
{
    public static void AddCommonRules<T>(AbstractValidator<T> validator) where T : ItemRequestDto
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required");

        validator.RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Item Quantity is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Quantity cannot be negative");

        validator.RuleFor(x => x.Price)
            .NotNull().WithMessage("Item Price is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Price cannot be negative");
    }
}

public static class ParentItemRequestValidatorHelper
{
    public static void AddCommonRules<T>(AbstractValidator<T> validator) where T : ParentItemRequestDto
    {
        validator.RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store Id is required");

        validator.RuleForEach(x => x.Rates)
            .SetValidator(new ItemRateRequestDtoValidator());
    }
}

public class ItemChildRequestDtoValidator : AbstractValidator<ItemRequestDto>
{
    public ItemChildRequestDtoValidator()
    {
        // For children, only validate the essential fields
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required");

        RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Item Quantity is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Quantity cannot be negative");

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Item Price is required")
            .GreaterThanOrEqualTo(0).WithMessage("Item Price cannot be negative");
    }
}

public class ItemRateRequestDtoValidator : AbstractValidator<ItemRateRequestDto>
{
    public ItemRateRequestDtoValidator()
    {
        RuleFor(x => x.MinDays)
            .NotEmpty().WithMessage("MinDays is required")
            .GreaterThan(0).WithMessage("MinDays must be greater than 0");
        RuleFor(x => x.DailyRate)
            .NotEmpty().WithMessage("DailyRate is required")
            .GreaterThan(0).WithMessage("DailyRate must be non-negative");
    }
}
