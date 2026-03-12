using GonDraz.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation
{
    /// <summary>
    /// Generic poolable coloured rect used for grid-cell backgrounds and border walls.
    /// Attach to a prefab that has a RectTransform + Image.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class PuzzleImageView : MonoBehaviour, IPoolable
    {
        private Image _image;

        private void Awake() => _image = GetComponent<Image>();

        public void SetColor(Color color)
        {
            if (_image == null) _image = GetComponent<Image>();
            _image.color = color;
        }

        // Resets visual state when returned to pool so it is clean for the next borrow
        public void OnGetFromPool() { }
        public void OnReturnToPool() => SetColor(Color.white);
    }
}

