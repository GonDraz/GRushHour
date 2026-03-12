using UnityEngine;
using UnityEngine.UI;

namespace GonDraz.UI.UIGradient
{
    [AddComponentMenu("UI/GonDraz/Effects/Text Gradient")]
    public class UITextGradient : BaseMeshEffect
    {
        public Color color1 = Color.white;
        public Color color2 = Color.white;

        [Range(-180f, 180f)] public float angle;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled) return;
            var rect = graphic.rectTransform.rect;
            var dir = UIGradientUtils.RotationDir(angle);
            var localPositionMatrix = UIGradientUtils.LocalPositionMatrix(new Rect(0f, 0f, 1f, 1f), dir);

            var vertex = default(UIVertex);
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                var position = UIGradientUtils.VerticePositions[i % 4];
                var localPosition = localPositionMatrix * position;
                vertex.color *= Color.Lerp(color2, color1, localPosition.y);
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}