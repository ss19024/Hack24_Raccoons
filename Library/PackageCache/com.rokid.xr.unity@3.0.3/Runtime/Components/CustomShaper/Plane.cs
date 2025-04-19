using UnityEngine;

namespace Rokid.UXR.Components
{
    [ExecuteInEditMode]
    public class Plane : BaseShaper
    {
        [SerializeField]
        private float width = 1;
        [SerializeField]
        private float height = 1;

        public override void RefreshMesh()
        {
            verts.Clear();
            indices.Clear();

            verts.Add(new Vector3(-width / 2, -height / 2, 0));
            verts.Add(new Vector3(width / 2, -height / 2, 0));
            verts.Add(new Vector3(-width / 2, height / 2, 0));
            verts.Add(new Vector3(width / 2, height / 2, 0));

            indices.Add(2);
            indices.Add(1);
            indices.Add(0);

            indices.Add(3);
            indices.Add(1);
            indices.Add(2);

            Mesh mesh = new Mesh
            {
                vertices = verts.ToArray(),
                triangles = indices.ToArray()
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshCollider.sharedMesh = mesh;
            meshFilter.mesh = mesh;
        }
    }
}

