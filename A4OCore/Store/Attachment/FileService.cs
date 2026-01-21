using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace A4OCore.Store.Attachment
{
    public class FileService : IFileService
    {
        private readonly string _storagePath;
        private readonly string _tempPath;
        private readonly byte[] _encryptionKey;

        public FileService(IConfiguration configuration)
        {
            _storagePath = configuration["AttachmentStoragePath"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Attachments");
            _tempPath = Path.Combine(_storagePath, "Temp");

            var keyString = configuration["AttachmentEncryptionKey"];
            if (string.IsNullOrEmpty(keyString))
            {
                // Fallback key for development - DO NOT USE IN PRODUCTION
                // In a real scenario, force exception if key is missing
                _encryptionKey = new byte[32]; // 32 bytes = 256 bits
            }
            else
            {
                // Ensure key is valid length or hash it to fit
                using (var sha256 = SHA256.Create())
                {
                    _encryptionKey = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(keyString));
                }
            }
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
        {
            var guid = Guid.NewGuid().ToString();
            var folderPath = Path.Combine(_storagePath, guid);
            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);

            // Encrypt and save
            using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create))
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = new byte[16]; // Use a zero IV for simplicity if unique salt not stored, OR generate and prepend IV to file.
                // Better security: Generate random IV, write it to beginning of file.
                aes.GenerateIV();
                fileStreamOutput.Write(aes.IV, 0, aes.IV.Length);

                using (var cryptoStream = new CryptoStream(fileStreamOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    await fileStream.CopyToAsync(cryptoStream);
                }
            }

            // Return relative path: {guid}/{fileName}
            return $"{guid}/{fileName}";
        }

        public async Task<(Stream FileStream, string ContentType, string FileName)> GetFileAsync(string relativePath)
        {
            var fullPath = Path.Combine(_storagePath, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found", relativePath);
            }

            var fileName = Path.GetFileName(fullPath);
            var memoryStream = new MemoryStream();

            using (var fileStreamInput = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                // Read IV
                var iv = new byte[16];
                await fileStreamInput.ReadAsync(iv, 0, iv.Length);

                using (var aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.IV = iv;

                    using (var cryptoStream = new CryptoStream(fileStreamInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        await cryptoStream.CopyToAsync(memoryStream);
                    }
                }
            }

            memoryStream.Position = 0;
            var contentType = GetContentType(fileName);
            return (memoryStream, contentType, fileName);
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private System.Collections.Generic.Dictionary<string, string> GetMimeTypes()
        {
            return new System.Collections.Generic.Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }
        /// <summary>
        /// Salva un file nella cartella temporanea
        /// </summary>
        public async Task<string> SaveTempFileAsync(Stream fileStream, string fileName)
        {
            var guid = Guid.NewGuid().ToString();
            var folderPath = Path.Combine(_tempPath, guid);
            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);

            // Encrypt and save
            using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create))
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.GenerateIV();
                fileStreamOutput.Write(aes.IV, 0, aes.IV.Length);

                using (var cryptoStream = new CryptoStream(fileStreamOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    await fileStream.CopyToAsync(cryptoStream);
                }
            }

            // Return temp path: Temp/{guid}/{fileName}
            return $"Temp/{guid}/{fileName}";
        }

        /// <summary>
        /// Sposta un file dalla cartella temporanea a quella permanente
        /// </summary>
        public async Task<string> MoveTempToPermanentAsync(string tempPath)
        {
            var tempFullPath = Path.Combine(_storagePath, tempPath);

            if (!File.Exists(tempFullPath))
            {
                throw new FileNotFoundException("File temporaneo non trovato", tempPath);
            }

            var guid = Guid.NewGuid().ToString();
            var fileName = Path.GetFileName(tempPath);
            var permanentPath = Path.Combine(_storagePath, guid);
            Directory.CreateDirectory(permanentPath);

            var permanentFullPath = Path.Combine(permanentPath, fileName);

            // Sposta il file
            await Task.Run(() => File.Move(tempFullPath, permanentFullPath));

            // Rimuovi la cartella temporanea se vuota
            var tempDir = Path.GetDirectoryName(tempFullPath);
            if (Directory.Exists(tempDir) && !Directory.EnumerateFileSystemEntries(tempDir).Any())
            {
                Directory.Delete(tempDir);
            }

            // Return permanent path: {guid}/{fileName}
            return $"{guid}/{fileName}";
        }

        /// <summary>
        /// Pulisce i file temporanei più vecchi di un certo tempo
        /// </summary>
        public async Task CleanupTempFilesAsync(TimeSpan olderThan)
        {
            if (!Directory.Exists(_tempPath))
            {
                return;
            }

            var cutoffTime = DateTime.Now - olderThan;

            await Task.Run(() =>
            {
                foreach (var dir in Directory.GetDirectories(_tempPath))
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        if (dirInfo.CreationTime < cutoffTime)
                        {
                            Directory.Delete(dir, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other directories
                        Console.WriteLine($"Errore durante la pulizia della cartella {dir}: {ex.Message}");
                    }
                }
            });
        }

        public string GetStoragePath()
        {
            return _storagePath;
        }
    }
}
