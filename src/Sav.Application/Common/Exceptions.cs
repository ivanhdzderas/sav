namespace Sav.Application.Common;

/// <summary>The requested record does not exist.</summary>
public sealed class NotFoundException(string entity, object key)
    : Exception($"{entity} '{key}' was not found.");

/// <summary>The operation clashes with existing data, such as a duplicated code.</summary>
public sealed class ConflictException(string message) : Exception(message);
