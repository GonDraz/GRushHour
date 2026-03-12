using GonDraz.Events;
using GonDraz.Observable;

namespace GonDraz.PlayerPrefs
{
    /// <summary>
    ///     Helper class for creating observable PlayerPrefs values with auto-save
    ///     (Class hỗ trợ tạo giá trị PlayerPrefs observable với auto-save)
    /// </summary>
    public static class GObservablePlayerPrefs
    {
        /// <summary>
        ///     Creates an observable int value that automatically saves to PlayerPrefs
        ///     (Tạo giá trị int observable tự động lưu vào PlayerPrefs)
        /// </summary>
        public static GObservableValue<int> CreateObservableInt(
            string key,
            int defaultValue = 0,
            GEvent<int, int> additionalEvents = null)
        {
            var initialValue = GPlayerPrefs.GetInt(key, defaultValue);

            var saveEvent = new GEvent<int, int>(
                $"{key}_Changed",
                (preValue, curValue) =>
                {
                    GPlayerPrefs.SetInt(key, curValue);
                    GPlayerPrefs.Save();
                });

            // Combine save event with additional events if provided
            if (additionalEvents != null) saveEvent += additionalEvents;

            return new GObservableValue<int>(initialValue, saveEvent);
        }

        /// <summary>
        ///     Creates an observable float value that automatically saves to PlayerPrefs
        ///     (Tạo giá trị float observable tự động lưu vào PlayerPrefs)
        /// </summary>
        public static GObservableValue<float> CreateObservableFloat(
            string key,
            float defaultValue = 0f,
            GEvent<float, float> additionalEvents = null)
        {
            var initialValue = GPlayerPrefs.GetFloat(key, defaultValue);

            var saveEvent = new GEvent<float, float>(
                $"{key}_Changed",
                (preValue, curValue) =>
                {
                    GPlayerPrefs.SetFloat(key, curValue);
                    GPlayerPrefs.Save();
                });

            if (additionalEvents != null) saveEvent += additionalEvents;

            return new GObservableValue<float>(initialValue, saveEvent);
        }

        /// <summary>
        ///     Creates an observable string value that automatically saves to PlayerPrefs
        ///     (Tạo giá trị string observable tự động lưu vào PlayerPrefs)
        /// </summary>
        public static GObservableValue<string> CreateObservableString(
            string key,
            string defaultValue = "",
            GEvent<string, string> additionalEvents = null)
        {
            var initialValue = GPlayerPrefs.GetString(key, defaultValue);

            var saveEvent = new GEvent<string, string>(
                $"{key}_Changed",
                (preValue, curValue) =>
                {
                    GPlayerPrefs.SetString(key, curValue);
                    GPlayerPrefs.Save();
                });

            if (additionalEvents != null) saveEvent += additionalEvents;

            return new GObservableValue<string>(initialValue, saveEvent);
        }

        /// <summary>
        ///     Creates an observable bool value that automatically saves to PlayerPrefs
        ///     (Tạo giá trị bool observable tự động lưu vào PlayerPrefs)
        /// </summary>
        public static GObservableValue<bool> CreateObservableBool(
            string key,
            bool defaultValue = false,
            GEvent<bool, bool> additionalEvents = null)
        {
            var initialValue = GPlayerPrefs.GetBool(key, defaultValue);

            var saveEvent = new GEvent<bool, bool>(
                $"{key}_Changed",
                (preValue, curValue) =>
                {
                    GPlayerPrefs.SetBool(key, curValue);
                    GPlayerPrefs.Save();
                });

            if (additionalEvents != null) saveEvent += additionalEvents;

            return new GObservableValue<bool>(initialValue, saveEvent);
        }
    }
}