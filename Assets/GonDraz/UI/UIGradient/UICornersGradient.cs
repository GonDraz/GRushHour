using UnityEngine;
using UnityEngine.UI;

namespace GonDraz.UI.UIGradient
{
    [AddComponentMenu("UI/GonDraz/Effects/4 Corners Gradient")]
    public class UICornersGradient : BaseMeshEffect
    {
        public Color topLeftColor = Color.white;
        public Color topRightColor = Color.white;
        public Color bottomRightColor = Color.white;
        public Color bottomLeftColor = Color.white;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled) return;
            var rect = graphic.rectTransform.rect;
            var localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, Vector2.right);

            var vertex = default(UIVertex);
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                var normalizedPosition = localPositionMatrix * vertex.position;
                vertex.color *= UIGradientUtils.Bilerp(bottomLeftColor, bottomRightColor, topLeftColor,
                    topRightColor, normalizedPosition);
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}