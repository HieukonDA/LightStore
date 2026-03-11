using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using TheLightStore.Application.Interfaces;

namespace TheLightStore.Infrastructure.ExternalServices;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"] 
            ?? throw new ArgumentNullException("Encryption:Key", "Encryption key not configured");
        
        if (keyString.Length != 32)
            throw new ArgumentException("Encryption key must be exactly 32 characters (256-bit)");
        
        _key = Encoding.UTF8.GetBytes(keyString);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            
            // Write IV at the beginning
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);
            
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt, Encoding.UTF8))
            {
                swEncrypt.Write(plainText);
            } // ✅ StreamWriter và CryptoStream dispose đúng thứ tự

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Encryption failed", ex);
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            
            if (fullCipher.Length < 16)
                throw new ArgumentException("Invalid cipher text - too short");

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            // Extract IV from first 16 bytes
            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, 16);
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(fullCipher, 16, fullCipher.Length - 16);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8);

            return srDecrypt.ReadToEnd();
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Invalid cipher text - not valid Base64");
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Decryption failed - possibly wrong key or corrupted data", ex);
        }
    }
}
