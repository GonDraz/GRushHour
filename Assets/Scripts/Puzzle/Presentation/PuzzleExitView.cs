using GonDraz.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation
{
    /// <summary>
    /// Visual indicator for an exit gate placed on the board border.
    /// Requires a GameObject with an Image component — attach this to the exit indicator prefab.
    /// PuzzleBoardPresenter sizes and positions it automatically at runtime.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PuzzleExitView : MonoBehaviour, IPoolable
    {
        private Image _image;

        private void Awake() => _image = GetComponent<Image>();

        public void SetColor(Color color)
        {
            if (_image == null) _image = GetComponent<Image>();
            _image.color = color;
        }

        public void OnGetFromPool() { }
        public void OnReturnToPool() { }
    }
}
