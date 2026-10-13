using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WpfApiWrapperExample
{
    /// <summary>
    /// Remembers the last API key used, encrypted with Windows DPAPI for the current Windows user,
    /// in %LOCALAPPDATA%\HHDM\WpfApiWrapperExample\apikey.dat.
    /// </summary>
    internal static class ApiKeyStore
    {
        private static readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HHDM", "WpfApiWrapperExample", "apikey.dat");

        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("HHDM.WpfApiWrapperExample.ApiKey");

        public static string Load()
        {
            try
            {
                if (File.Exists(_filePath) == false)
                {
                    return null;
                }

                var decrypted = ProtectedData.Unprotect(File.ReadAllBytes(_filePath), _entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is CryptographicException)
            {
                Debug.WriteLine($"Could not read the saved API key: {ex.Message}");
                return null;
            }
        }

        public static void Save(string apiKey)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                var encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(apiKey), _entropy, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(_filePath, encrypted);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is CryptographicException)
            {
                Debug.WriteLine($"Could not save the API key: {ex.Message}");
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.WriteLine($"Could not delete the saved API key: {ex.Message}");
            }
        }
    }
}
