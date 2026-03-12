using TMPro;
using UnityEngine;

namespace GonDraz.Misc
{
    public class SystemInfoText : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private void Start()
        {
            text.SetText($"UID: {SystemInfo.Uid} | {SystemInfo.Version}");
        }
    }
}