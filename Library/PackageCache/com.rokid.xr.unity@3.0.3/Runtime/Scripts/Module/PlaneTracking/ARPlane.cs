using System.Text;
using UnityEngine;
using UnityEngine.Rokid.XR.ARFoundation;
using Unity.Collections;
using Rokid.UXR.Interaction;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.EventSystems;


namespace Rokid.UXR.Module
{
    public class ARPlane : MonoBehaviour, IRayPointerDown
    {
        [SerializeField]
        protected bool drawPoint;
        [SerializeField]
        protected bool drawAxis;
        [SerializeField]
        protected Transform axis;
        [SerializeField]
        protected Transform point;
        protected Mesh mesh;
        protected MeshFilter meshFilter;
        protected MeshCollider meshCollider;
        public BoundedPlane boundedPlane { get; set; }

        protected List<Transform> pointList = new List<Transform>();

        private void Start()
        {

        }

        public virtual void Init(ref BoundedPlane boundedPlane)
        {
            this.boundedPlane = boundedPlane;
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
            mesh = new Mesh();
            Assert.IsNotNull(meshFilter);
            Assert.IsNotNull(meshCollider);
            Assert.IsNotNull(axis);
            Assert.IsNotNull(point);
            transform.SetPose(boundedPlane.pose);
            gameObject.SetActive(true);
            GenerateMesh(ref boundedPlane);
            axis.gameObject.SetActive(drawAxis);
            if (drawPoint)
            {
                for (int i = 0; i < boundedPlane.boundary3D.Length; i++)
                {
                    GameObject go = GameObject.Instantiate(point).gameObject;
                    go.transform.position = boundedPlane.boundary3D[i];
                    go.transform.parent = transform;
                    go.SetActive(true);
                    pointList.Add(go.transform);
                }
            }
        }

        protected virtual void GenerateMesh(ref BoundedPlane boundedPlane)
        {
            if (ARPlaneMeshGenerators.GenerateMesh(mesh, Pose.identity, boundedPlane.boundary))
            {
                meshFilter.sharedMesh = mesh;
                meshCollider.sharedMesh = mesh;
            }
            else
            {
                RKLog.Error($"====ARPlane====: Failed To Create Mesh {LogBoundaryInfo(boundedPlane.boundary)}");
            }
        }

        public virtual void UpdatePlane(ref BoundedPlane boundedPlane)
        {
            this.boundedPlane = boundedPlane;
            transform.SetPose(boundedPlane.pose);
            if (ARPlaneMeshGenerators.GenerateMesh(mesh, Pose.identity, boundedPlane.boundary))
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

        protected string LogBoundaryInfo(Vector2[] boundary)
        {
            StringBuilder msg = new StringBuilder();
            int length = boundary.Length;
            for (int i = 0; i < length; i++)
            {
                msg.Append($"{boundary[i][0]},{boundary[i][1]} & ");
            }
            return msg.ToString();
        }

        public void DestroyPlane(ref BoundedPlane boundedPlane)
        {
            Destroy(this.gameObject);
            boundedPlane.release();
        }

        public void OnRayPointerDown(PointerEventData eventData)
        {
            // Used to flag global events, making global event events 
        }
    }
}
