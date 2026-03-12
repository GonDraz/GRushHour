using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using SystemInfo = GonDraz.Misc.SystemInfo;

namespace GonDraz.PlayerPrefs
{
    /// <summary>
    ///     Provides encryption/decryption for PlayerPrefs data using device-specific Uid
    ///     (Cung cấp mã hóa/giải mã cho dữ liệu PlayerPrefs sử dụng Uid thiết bị)
    /// </summary>
    public static class GPlayerPrefsEncryption
    {
        private static byte[] _keyCache;
        private static byte[] _ivCache;

        /// <summary>
        ///     Encrypts a string value using AES encryption with device Uid as key
        ///     (Mã hóa chuỗi sử dụng AES với Uid thiết bị làm khóa)
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            try
            {
                InitializeKeyAndIv();

                using (var aes = Aes.Create())
                {
                    aes.Key = _keyCache;
                    aes.IV = _ivCache;

                    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }

                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GPlayerPrefsEncryption] Encryption failed: {ex.Message}");
                return plainText; // Fallback to plain text
            }
        }

        /// <summary>
        ///     Decrypts an encrypted string value
        ///     (Giải mã chuỗi đã mã hóa)
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                InitializeKeyAndIv();

                var buffer = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = _keyCache;
                aes.IV = _ivCache;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using var msDecrypt = new MemoryStream(buffer);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GPlayerPrefsEncryption] Decryption failed: {ex.Message}");
                return cipherText; // Fallback to cipher text
            }
        }

        /// <summary>
        ///     Initializes encryption key and IV from device Uid
        ///     (Khởi tạo khóa mã hóa và IV từ Uid thiết bị)
        /// </summary>
        private static void InitializeKeyAndIv()
        {
            if (_keyCache != null && _ivCache != null) return;

            var uid = SystemInfo.Uid;

            // Create a deterministic key from Uid
            using var sha256 = SHA256.Create();
            var uidBytes = Encoding.UTF8.GetBytes(uid);
            var hash = sha256.ComputeHash(uidBytes);

            // Use first 32 bytes for AES-256 key
            _keyCache = new byte[32];
            Array.Copy(hash, _keyCache, 32);

            // Create IV from hash
            var hash2 = sha256.ComputeHash(hash);
            _ivCache = new byte[16];
            Array.Copy(hash2, _ivCache, 16);
        }

        /// <summary>
        ///     Clears the cached encryption keys (useful for testing)
        ///     (Xóa cache khóa mã hóa - hữu ích cho testing)
        /// </summary>
        public static void ClearCache()
        {
            _keyCache = null;
            _ivCache = null;
        }
    }
}