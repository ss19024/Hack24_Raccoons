using Rokid.UXR.Interaction;
using UnityEngine;
using UnityEngine.Rokid.XR.ARFoundation;



namespace Rokid.UXR.Module
{
    public class ARGridPlane : ARPlane
    {
        [SerializeField]
        private float gradientRadius;

        private void Start()
        {

        }

        protected override void GenerateMesh(ref BoundedPlane boundedPlane)
        {
            if (ARPlaneMeshGenerators.GenerateGradientMesh(mesh, Pose.identity, boundedPlane.boundary, gradientRadius))
            {
                meshFilter.sharedMesh = mesh;
                meshCollider.sharedMesh = mesh;
            }
            else
            {
                RKLog.Error($"====ARPlane====: Failed To Create Mesh {LogBoundaryInfo(boundedPlane.boundary)}");
            }
        }

        public override void UpdatePlane(ref BoundedPlane boundedPlane)
        {
            this.boundedPlane = boundedPlane;
            transform.SetPose(boundedPlane.pose);
            if (ARPlaneMeshGenerators.GenerateGradientMesh(mesh, Pose.identity, boundedPlane.boundary, gradientRadius))
            {
                meshFilter.sharedMesh = mesh;
                meshCollider.sharedMesh = mesh;
            }
            else
            {
                RKLog.Error($"====ARPlane====: Failed To Create Mesh {LogBoundaryInfo(boundedPlane.boundary)}");
            }
            if (drawPoint)
            {
                if (pointList.Count < boundedPlane.boundary3D.Length)
                {
                    // add need point
                    int addCount = boundedPlane.boundary3D.Length - pointList.Count;
                    for (int i = 0; i < addCount; i++)
                    {
                        GameObject go = GameObject.Instantiate(point).gameObject;
                        go.transform.parent = transform;
                        go.SetActive(true);
                        pointList.Add(go.transform);
                    }
                }

                if (pointList.Count > boundedPlane.boundary3D.Length)
                {
                    // remove unended point
                    int removeCount = pointList.Count - boundedPlane.boundary3D.Length;
                    for (int i = 0; i < removeCount; i++)
                    {
                        GameObject go = pointList[i].gameObject;
                        pointList.RemoveAt(i);
                        Destroy(go);
                    }
                }

                for (int i = 0; i < boundedPlane.boundary3D.Length; i++)
                {
                    pointList[i].transform.position = boundedPlane.boundary3D[i];
                }
            }
        }

    }
}
