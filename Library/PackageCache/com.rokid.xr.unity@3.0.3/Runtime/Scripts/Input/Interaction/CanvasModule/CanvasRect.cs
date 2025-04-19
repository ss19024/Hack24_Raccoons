using System.Collections.Generic;

using UnityEngine;
namespace Rokid.UXR.Interaction
{
    public class CanvasRect : CanvasMesh
    {
        protected override Vector3 MeshInverseTransform(Vector3 localPosition)
        {
            return localPosition;
        }

        protected override void GenerateMesh(out List<Vector3> verts,
                                             out List<int> tris,
                                             out List<Vector2> uvs)
        {
            verts = new List<Vector3>();
            tris = new List<int>();
            uvs = new List<Vector2>();

            var resolution = _canvasRenderTexture.GetBaseResolutionToUse();
            Vector2 worldSize = new Vector2(
                _canvasRenderTexture.PixelsToUnits(Mathf.RoundToInt(resolution.x)),
                _canvasRenderTexture.PixelsToUnits(Mathf.RoundToInt(resolution.y))
                ) / transform.lossyScale;

            float xPos = worldSize.x * 0.5f;
            float xNeg = -xPos;

            float yPos = worldSize.y * 0.5f;
            float yNeg = -yPos;

            verts.Add(new Vector3(xNeg, yNeg, 0));
            verts.Add(new Vector3(xNeg, yPos, 0));
            verts.Add(new Vector3(xPos, yPos, 0));
            verts.Add(new Vector3(xPos, yNeg, 0));

            tris.Add(0);
            tris.Add(1);
            tris.Add(2);

            tris.Add(0);
            tris.Add(2);
            tris.Add(3);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
        }
    }

}
