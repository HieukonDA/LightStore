namespace TheLightStore.Application.Interfaces.Infrastructures;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
