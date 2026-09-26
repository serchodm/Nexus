namespace Nexus.Domain.Common;

/// <summary>
/// Excepción lanzada cuando una operación viola una regla de negocio o invariante del dominio.
/// </summary>
public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }

    public DomainValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
