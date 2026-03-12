using UnityEngine;
using UnityEngine.UI;

namespace GonDraz.UI.UIGradient
{
    public class SetDirty : MonoBehaviour
    {
        private Graphic graphic;

        private Graphic Graphic
        {
            get
            {
                if (!graphic) graphic = GetComponent<Graphic>();
                return graphic;
            }
            set => graphic = value;
        }

        // Use this for initialization
        private void Reset()
        {
            Graphic = GetComponent<Graphic>();
        }

        // Update is called once per frame
        private void Update()
        {
            Graphic.SetVerticesDirty();
        }
    }
}