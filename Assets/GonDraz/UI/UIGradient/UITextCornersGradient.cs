using UnityEngine;
using UnityEngine.UI;

namespace GonDraz.UI.UIGradient
{
    [AddComponentMenu("UI/GonDraz/Effects/Text 4 Corners Gradient")]
    public class UITextCornersGradient : BaseMeshEffect
    {
        public Color topLeftColor = Color.white;
        public Color topRightColor = Color.white;
        public Color bottomRightColor = Color.white;
        public Color bottomLeftColor = Color.white;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled) return;
            var rect = graphic.rectTransform.rect;

            var vertex = default(UIVertex);
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                var normalizedPosition = UIGradientUtils.VerticePositions[i % 4];
                vertex.color *= UIGradientUtils.Bilerp(bottomLeftColor, bottomRightColor, topLeftColor,
                    topRightColor, normalizedPosition);
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}