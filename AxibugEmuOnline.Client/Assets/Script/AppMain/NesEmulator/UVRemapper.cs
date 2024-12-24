using UnityEngine;
using UnityEngine.UI;

public class UVRemapper : BaseMeshEffect
{
    public Vector2 uvMin = new Vector2(0, 0);
    public Vector2 uvMax = new Vector2(1, 1);

    public override void ModifyMesh(VertexHelper vh)
    {
        UIVertex vertex = new UIVertex();
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            switch (i % 4)
            {
                case 0:
                    vertex.uv0 = uvMin;
                    break;
                case 1:
                    vertex.uv0 = new Vector2(uvMin.x, uvMax.y);
                    break;
                case 2:
                    vertex.uv0 = uvMax;
                    break;
                case 3:
                    vertex.uv0 = new Vector2(uvMax.x, uvMin.y);
                    break;
            }

            vh.SetUIVertex(vertex, i);
        }
    }
}
