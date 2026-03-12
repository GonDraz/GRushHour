using System.Globalization;
using UnityEngine;

namespace GonDraz.PlayerPrefs
{
    /// <summary>
    ///     Centralized PlayerPrefs manager with encryption support
    ///     (Quản lý PlayerPrefs tập trung với hỗ trợ mã hóa)
    /// </summary>
    public static class GPlayerPrefs
    {
        private const string KEY_PREFIX = "";
        private static readonly bool Encrypted = true;

        #region Int Methods

        /// <summary>
        ///     Gets an integer value from PlayerPrefs
        ///     (Lấy giá trị integer từ PlayerPrefs)
        /// </summary>
        public static int GetInt(string key, int defaultValue = 0)
        {
            var fullKey = GetFullKey(key);

            if (!UnityEngine.PlayerPrefs.HasKey(fullKey))
                return defaultValue;

            if (Encrypted)
            {
                var encryptedValue =
                    UnityEngine.PlayerPrefs.GetString(fullKey, defaultValue.ToString(CultureInfo.InvariantCulture));
                var decryptedValue = GPlayerPrefsEncryption.Decrypt(encryptedValue);
                return int.TryParse(decryptedValue, out var result) ? result : defaultValue;
            }

            return UnityEngine.PlayerPrefs.GetInt(fullKey, defaultValue);
        }

        /// <summary>
        ///     Sets an integer value to PlayerPrefs
        ///     (Lưu giá trị integer vào PlayerPrefs)
        /// </summary>
        public static void SetInt(string key, int value)
        {
            var fullKey = GetFullKey(key);

            if (Encrypted)
            {
                var encryptedValue = GPlayerPrefsEncryption.Encrypt(value.ToString(CultureInfo.InvariantCulture));
                UnityEngine.PlayerPrefs.SetString(fullKey, encryptedValue);
            }
            else
            {
                UnityEngine.PlayerPrefs.SetInt(fullKey, value);
            }
        }

        #endregion

        #region Float Methods

        /// <summary>
        ///     Gets a float value from PlayerPrefs
        ///     (Lấy giá trị float từ PlayerPrefs)
        /// </summary>
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            var fullKey = GetFullKey(key);

            if (!UnityEngine.PlayerPrefs.HasKey(fullKey))
                return defaultValue;

            if (Encrypted)
            {
                var encryptedValue =
                    UnityEngine.PlayerPrefs.GetString(fullKey, defaultValue.ToString(CultureInfo.InvariantCulture));
                var decryptedValue = GPlayerPrefsEncryption.Decrypt(encryptedValue);
                return float.TryParse(decryptedValue, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var result)
                    ? result
                    : defaultValue;
            }

            return UnityEngine.PlayerPrefs.GetFloat(fullKey, defaultValue);
        }

        /// <summary>
        ///     Sets a float value to PlayerPrefs
        ///     (Lưu giá trị float vào PlayerPrefs)
        /// </summary>
        public static void SetFloat(string key, float value)
        {
            var fullKey = GetFullKey(key);

            if (Encrypted)
            {
                var encryptedValue = GPlayerPrefsEncryption.Encrypt(value.ToString(CultureInfo.InvariantCulture));
                UnityEngine.PlayerPrefs.SetString(fullKey, encryptedValue);
            }
            else
            {
                UnityEngine.PlayerPrefs.SetFloat(fullKey, value);
            }
        }

        #endregion

        #region String Methods

        /// <summary>
        ///     Gets a string value from PlayerPrefs
        ///     (Lấy giá trị string từ PlayerPrefs)
        /// </summary>
        public static string GetString(string key, string defaultValue = "")
        {
            var fullKey = GetFullKey(key);

            if (!UnityEngine.PlayerPrefs.HasKey(fullKey))
                return defaultValue;

            var value = UnityEngine.PlayerPrefs.GetString(fullKey, defaultValue);

            return Encrypted ? GPlayerPrefsEncryption.Decrypt(value) : value;
        }

        /// <summary>
        ///     Sets a string value to PlayerPrefs
        ///     (Lưu giá trị string vào PlayerPrefs)
        /// </summary>
        public static void SetString(string key, string value)
        {
            var fullKey = GetFullKey(key);

            if (Encrypted) value = GPlayerPrefsEncryption.Encrypt(value);

            UnityEngine.PlayerPrefs.SetString(fullKey, value);
        }

        #endregion

        #region Bool Methods

        /// <summary>
        ///     Gets a boolean value from PlayerPrefs
        ///     (Lấy giá trị boolean từ PlayerPrefs)
        /// </summary>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            return GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        /// <summary>
        ///     Sets a boolean value to PlayerPrefs
        ///     (Lưu giá trị boolean vào PlayerPrefs)
        /// </summary>
        public static void SetBool(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        ///     Checks if a key exists in PlayerPrefs
        ///     (Kiểm tra key có tồn tại trong PlayerPrefs không)
        /// </summary>
        public static bool HasKey(string key)
        {
            return UnityEngine.PlayerPrefs.HasKey(GetFullKey(key));
        }

        /// <summary>
        ///     Deletes a key from PlayerPrefs
        ///     (Xóa key khỏi PlayerPrefs)
        /// </summary>
        public static void DeleteKey(string key)
        {
            UnityEngine.PlayerPrefs.DeleteKey(GetFullKey(key));
        }

        /// <summary>
        ///     Deletes all GonDraz PlayerPrefs keys
        ///     (Xóa tất cả keys GonDraz từ PlayerPrefs)
        /// </summary>
        public static void DeleteAll()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
        }

        /// <summary>
        ///     Saves all changes to PlayerPrefs
        ///     (Lưu tất cả thay đổi vào PlayerPrefs)
        /// </summary>
        public static void Save()
        {
            UnityEngine.PlayerPrefs.Save();
        }

        /// <summary>
        ///     Gets the full key with prefix
        ///     (Lấy key đầy đủ với prefix)
        /// </summary>
        private static string GetFullKey(string key)
        {
            return KEY_PREFIX + key;
        }

        /// <summary>
        ///     Migrates an existing PlayerPrefs key to GonDraz format
        ///     (Di chuyển key PlayerPrefs hiện có sang định dạng GonDraz)
        /// </summary>
        public static void MigrateKey(string oldKey, string newKey, bool deleteOld = true)
        {
            if (!UnityEngine.PlayerPrefs.HasKey(oldKey)) return;

            // Get the old value as string
            var oldValue = UnityEngine.PlayerPrefs.GetString(oldKey, null);

            if (oldValue != null)
            {
                SetString(newKey, oldValue);
            }
            else if (UnityEngine.PlayerPrefs.HasKey(oldKey))
            {
                // Try as int
                var intValue = UnityEngine.PlayerPrefs.GetInt(oldKey, int.MinValue);
                if (intValue != int.MinValue)
                {
                    SetInt(newKey, intValue);
                }
                else
                {
                    // Try as float
                    var floatValue = UnityEngine.PlayerPrefs.GetFloat(oldKey, float.MinValue);
                    if (!float.IsNaN(floatValue) && !Mathf.Approximately(floatValue, float.MinValue))
                        SetFloat(newKey, floatValue);
                }
            }

            if (deleteOld) UnityEngine.PlayerPrefs.DeleteKey(oldKey);

            Save();
        }

        #endregion
    }
}