using UnityEngine;

namespace GonDraz.Misc
{
    public class AutoRotate : MonoBehaviour
    {
        public Vector3 speed = Vector3.zero;

        private void LateUpdate()
        {
            transform.Rotate(
                speed.x * Time.deltaTime,
                speed.y * Time.deltaTime,
                speed.z * Time.deltaTime
            );
        }
    }
}