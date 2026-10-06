namespace Nexus.Domain.Enums;

public enum BankConnectionStatus
{
    PendingAuthorization = 0,
    Active = 1,
    ExpiringSoon = 2,
    Expired = 3,
    Revoked = 4,
    Error = 5
}
