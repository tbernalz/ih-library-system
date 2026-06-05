namespace IH.LibrarySystem.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested resource cannot be found.
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    public NotFoundException(string entityName, object key, Exception innerException)
        : base($"Entity '{entityName}' with key '{key}' was not found.", innerException)
    {
        EntityName = entityName;
        Key = key;
    }

    public string EntityName { get; }
    public object Key { get; }
}
