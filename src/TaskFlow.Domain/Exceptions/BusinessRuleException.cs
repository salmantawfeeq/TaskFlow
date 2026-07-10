namespace TaskFlow.Domain.Exceptions;

/// <summary>
/// Thrown when an operation would violate a business rule (e.g. trying to
/// mark a task Done while it still has an incomplete required checklist,
/// or a non-manager trying to archive a project). Distinct from validation
/// errors (malformed input) - this represents a valid request that is
/// nonetheless not allowed given the current state of the domain.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
