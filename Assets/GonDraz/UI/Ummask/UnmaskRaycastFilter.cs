using UnityEngine;

namespace GonDraz.UI.Ummask
{
    /// <summary>
    ///     Unmask Raycast Filter.
    ///     The ray passes through the unmasked rectangle.
    /// </summary>
    [AddComponentMenu("UI/Unmask/UnmaskRaycastFilter", 2)]
    public class UnmaskRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
    {
        //################################
        // Serialize Members.
        //################################
        [Tooltip("Target unmask component. The ray passes through the unmasked rectangle.")] [SerializeField]
        private Unmask targetUnmask;


        //################################
        // Public Members.
        //################################
        /// <summary>
        ///     Target unmask component. Ray through the unmasked rectangle.
        /// </summary>
        public Unmask TargetUnmask
        {
            get => targetUnmask;
            set => targetUnmask = value;
        }


        //################################
        // Private Members.
        //################################

        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
        }

        /// <summary>
        ///     Given a point and a camera is the raycast valid.
        /// </summary>
        /// <returns>Valid.</returns>
        /// <param name="sp">Screen position.</param>
        /// <param name="eventCamera">Raycast camera.</param>
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            // Skip if deactived.
            if (!isActiveAndEnabled || !targetUnmask || !targetUnmask.isActiveAndEnabled) return true;

            // check inside
            if (eventCamera)
                return !RectTransformUtility.RectangleContainsScreenPoint(targetUnmask.transform as RectTransform, sp,
                    eventCamera);

            return !RectTransformUtility.RectangleContainsScreenPoint(targetUnmask.transform as RectTransform, sp);
        }
    }
}