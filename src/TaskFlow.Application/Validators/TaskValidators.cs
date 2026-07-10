using FluentValidation;
using TaskFlow.Application.DTOs.Tasks;

namespace TaskFlow.Application.Validators;

public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public class MoveTaskDtoValidator : AbstractValidator<MoveTaskDto>
{
    public MoveTaskDtoValidator()
    {
        RuleFor(x => x.TaskId).GreaterThan(0);
        RuleFor(x => x.NewStatus).IsInEnum();
        RuleFor(x => x.ChangedByUserId).NotEmpty();
    }
}
