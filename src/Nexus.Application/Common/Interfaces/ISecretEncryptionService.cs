namespace Nexus.Application.Common.Interfaces;

public interface ISecretEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
