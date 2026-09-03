using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PrintSaaS.Engine;

public interface IFileEncryptionService
{
    Task<byte[]> EncryptFileAsync(Stream inputStream);
    Stream DecryptToStream(byte[] encryptedData);
    Task EncryptAndSaveAsync(Stream inputStream, string outputPath);
    Stream DecryptFileToStream(string encryptedFilePath);
}

public class FileEncryptionService : IFileEncryptionService
{
    private readonly byte[] _key;
    private readonly ILogger<FileEncryptionService> _logger;

    public FileEncryptionService(IConfiguration config, ILogger<FileEncryptionService> logger)
    {
        _logger = logger;
        var keyString = config["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key not configured");
        _key = Convert.FromBase64String(keyString);

        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption key must be 256-bit (32 bytes)");
    }

    public async Task<byte[]> EncryptFileAsync(Stream inputStream)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var outputStream = new MemoryStream();

        // Write IV first (16 bytes)
        await outputStream.WriteAsync(aes.IV);

        using var cryptoStream = new CryptoStream(
            outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await inputStream.CopyToAsync(cryptoStream);
        await cryptoStream.FlushFinalBlockAsync();

        _logger.LogInformation("File encrypted, {Size} bytes", outputStream.Length);
        return outputStream.ToArray();
    }

    public Stream DecryptToStream(byte[] encryptedData)
    {
        // Never write decrypted files to disk — decrypt in memory only
        var iv = new byte[16];
        Array.Copy(encryptedData, 0, iv, 0, 16);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;

        var ciphertext = new byte[encryptedData.Length - 16];
        Array.Copy(encryptedData, 16, ciphertext, 0, ciphertext.Length);

        var outputStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(
            new MemoryStream(ciphertext), aes.CreateDecryptor(), CryptoStreamMode.Read))
        {
            cryptoStream.CopyTo(outputStream);
        }

        outputStream.Position = 0;
        return outputStream;
    }

    public async Task EncryptAndSaveAsync(Stream inputStream, string outputPath)
    {
        var encrypted = await EncryptFileAsync(inputStream);
        await File.WriteAllBytesAsync(outputPath, encrypted);
        _logger.LogInformation("Encrypted file saved to {Path}", outputPath);
    }

    public Stream DecryptFileToStream(string encryptedFilePath)
    {
        // Read encrypted file, decrypt in memory, never write plaintext to disk
        var encryptedData = File.ReadAllBytes(encryptedFilePath);
        return DecryptToStream(encryptedData);
    }
}
