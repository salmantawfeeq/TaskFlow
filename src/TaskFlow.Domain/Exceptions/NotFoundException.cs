namespace TaskFlow.Domain.Exceptions;

/// <summary>
/// Thrown when a lookup by Id (or other key) fails to find an entity that
/// the caller expected to exist. Caught specifically by the global exception
/// handling middleware to return a proper 404 response instead of a generic 500.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" with key ({key}) was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}
