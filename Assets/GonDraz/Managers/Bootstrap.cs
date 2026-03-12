using GonDraz.Scene;
using PrimeTween;
using UnityEngine;

namespace Managers
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private SceneField scene;

        private void Awake()
        {
            // Calculate optimal FPS: use divisors to get 60-75fps range for battery efficiency
            var refreshRate = (int)Screen.currentResolution.refreshRateRatio.numerator;
            var targetFPS = CalculateOptimalFPS(refreshRate);
            Application.targetFrameRate = targetFPS;
            // Application.targetFrameRate = 60;

            Debug.Log($"Screen Refresh Rate: {refreshRate}Hz -> Target FPS: {targetFPS}");

            PrimeTweenConfig.SetTweensCapacity(1600);

            Debug.Log("Bootstrap");
            _ = scene.LoadSceneAsync();
        }

        private static int CalculateOptimalFPS(int refreshRate)
        {
            // Strategy: Find divisor that results in 60-75 fps range
            // 60Hz   -> 60fps   (no division)
            // 90Hz   -> 60fps   (÷1.5)
            // 120Hz  -> 60fps   (÷2)
            // 144Hz  -> 72fps   (÷2)
            // 165Hz  -> 55fps   (÷3) -> Fallback to 60fps
            // 180Hz  -> 60fps   (÷3)
            // 240Hz  -> 60fps   (÷4)

            if (refreshRate <= 60)
                return 60;

            // Try integer divisors first
            for (var divisor = 2; divisor <= refreshRate / 55; divisor++)
            {
                var fps = refreshRate / divisor;
                if (fps >= 60 && fps <= 75)
                    return fps;
            }

            // If no good divisor found, use 60fps as fallback
            return 60;
        }
    }
}