using UnityEngine;

namespace Rokid.UXR.Components
{
    [ExecuteInEditMode]
    public class Capsule : BaseShaper
    {
        [SerializeField, Tooltip("球体垂直方向的分割数"), Range(5, 20)]
        int verticalSegments = 5;
        [SerializeField, Tooltip("球体水平方向的分割数"), Range(5, 20)]
        int horizontalSegments = 20;
        [SerializeField, Tooltip("半径"), Range(0.5f, 10)]
        float radius = 2;
        [SerializeField, Tooltip("高度"), Range(0, 10)]
        float height = 2;
        [SerializeField, Tooltip("球体高度压缩"), Range(0f, 1.0f)]
        float radiusHeightRate = 1;
        [SerializeField, Tooltip("发现是否逆反")]
        bool normalInverse = false;

        public override void RefreshMesh()
        {
            verts.Clear();
            indices.Clear();
            // 生成上半球的顶点
            for (int i = 0; i <= verticalSegments; i++)
            {
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float vSegment = (float)i / verticalSegments;
                    float hSegment = (float)j / horizontalSegments;
                    float alpha = vSegment * Mathf.PI / 2;
                    float beta = hSegment * 2.0f * Mathf.PI;
                    float xPos = radius * Mathf.Cos(beta) * Mathf.Sin(alpha);
                    float yPos = radius * Mathf.Cos(alpha);
                    float zPos = radius * Mathf.Sin(beta) * Mathf.Sin(alpha);
                    verts.Add(new Vector3(xPos, yPos * radiusHeightRate + height * 0.5f, zPos));
                }
            }

            // 生成下半球的顶点
            for (int i = 0; i <= verticalSegments; i++)
            {
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float vSegment = (float)i / verticalSegments;
                    float hSegment = (float)j / horizontalSegments;
                    float alpha = vSegment * Mathf.PI / 2 + Mathf.PI / 2;
                    float beta = hSegment * 2.0f * Mathf.PI;
                    float xPos = radius * Mathf.Cos(beta) * Mathf.Sin(alpha);
                    float yPos = radius * Mathf.Cos(alpha);
                    float zPos = radius * Mathf.Sin(beta) * Mathf.Sin(alpha);
                    verts.Add(new Vector3(xPos, yPos * radiusHeightRate - height * 0.5f, zPos));
                }
            }

            // 根据球面上每一点的坐标，去构造一个三角形顶点数组
            for (int i = 0; i < verticalSegments * 2 + 1; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    indices.Add(i * (horizontalSegments + 1) + j);
                    indices.Add((i + 1) * (horizontalSegments + 1) + j);
                    indices.Add((i + 1) * (horizontalSegments + 1) + j + 1);

                    indices.Add(i * (horizontalSegments + 1) + j);
                    indices.Add((i + 1) * (horizontalSegments + 1) + j + 1);
                    indices.Add(i * (horizontalSegments + 1) + j + 1);
                }
            }

            Mesh mesh = new Mesh
            {
                vertices = verts.ToArray(),
                triangles = normalInverse ? indices.ToArray() : InverseList(indices).ToArray()
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshCollider.sharedMesh = mesh;
            meshFilter.mesh = mesh;
        }
    }
}
