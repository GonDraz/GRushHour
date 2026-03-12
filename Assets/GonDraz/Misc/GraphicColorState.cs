using UnityEngine;
using UnityEngine.UI;

namespace GonDraz.Misc
{
    public class GraphicColorState : MonoBehaviour
    {
        [SerializeField] private Graphic graphic;
        [SerializeField] private Color colorActive = Color.white;
        [SerializeField] private Color colorInactive = Color.darkGray;

        public void SetActive(bool isActive)
        {
            graphic.color = isActive ? colorActive : colorInactive;
        }
    }
}