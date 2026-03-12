using UnityEngine;
using UnityEngine.UI;

namespace GonDraz.UI.UIGradient
{
    [AddComponentMenu("UI/GonDraz/Effects/Gradient")]
    public class UIGradient : BaseMeshEffect
    {
        public Color color1 = Color.white;
        public Color color2 = Color.white;

        [Range(-180f, 180f)] public float angle;

        public bool ignoreRatio = true;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!enabled) return;
            var rect = graphic.rectTransform.rect;
            var dir = UIGradientUtils.RotationDir(angle);

            if (!ignoreRatio)
                dir = UIGradientUtils.CompensateAspectRatio(rect, dir);

            var localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

            var vertex = default(UIVertex);
            for (var i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                var localPosition = localPositionMatrix * vertex.position;
                vertex.color *= Color.Lerp(color2, color1, localPosition.y);
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}