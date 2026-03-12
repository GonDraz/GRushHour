using GonDraz.Extensions;
using UnityEngine;

namespace GonDraz.Misc
{
    public static class SystemInfo
    {
        private static string uid;

        private static string version;

        public static string Uid
        {
            get
            {
                if (!uid.IsNullOrEmpty()) return uid;
                uid = PlayerPrefs.GetString("UID", UnityEngine.SystemInfo.deviceUniqueIdentifier);
                PlayerPrefs.SetString("UID", uid);
                PlayerPrefs.Save();

                return uid;
            }
        }

        public static string Version
        {
            get
            {
                if (version.IsNullOrEmpty()) version = Application.version;

                return version;
            }
        }
    }
}