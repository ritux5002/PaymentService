using FluentValidation;

namespace PaymentService.Application.Features.Payments.GetPaymentById;

public class GetPaymentByIdQueryValidator : AbstractValidator<GetPaymentByIdQuery>
{
    public GetPaymentByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
