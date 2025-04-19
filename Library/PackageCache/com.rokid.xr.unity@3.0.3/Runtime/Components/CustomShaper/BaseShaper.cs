using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using UnityEngine.Rendering;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Rokid.UXR.Components
{
    public abstract class BaseShaper : MonoBehaviour, IShaper
    {
        public virtual string ShaperName => this.GetType().Name;
        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        protected MeshCollider meshCollider;
        protected List<Vector3> verts = new List<Vector3>();
        protected List<int> indices = new List<int>();

        private bool Initialized = false;

        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
            Assert.IsNotNull(meshFilter);
            Assert.IsNotNull(meshRenderer);
            Assert.IsNotNull(meshCollider);

#if  UNITY_6000_0_OR_NEWER
            if (GraphicsSettings.defaultRenderPipeline != null)
#else
            if (GraphicsSettings.renderPipelineAsset != null)
#endif
            {
                meshRenderer.material = Resources.Load<Material>("Materials/URP/CustomShaper");
            }
            else
            {
                meshRenderer.material = Resources.Load<Material>("Materials/BuildIn/CustomShaper");
            }
            RefreshMesh();
            Initialized = true;
        }


        public virtual void OnValidate()
        {
            if (Initialized)
                RefreshMesh();
        }

        public virtual void RefreshMesh()
        {

        }

        protected virtual List<T> InverseList<T>(List<T> data)
        {
            int startIndex = 0;
            int endIndex = data.Count - 1;
            for (int i = 0; i < data.Count; i++)
            {
                if (startIndex >= endIndex)
                    break;
                T temp = data[startIndex];
                data[startIndex] = data[endIndex];
                data[endIndex] = temp;
                startIndex++;
                endIndex--;
            }
            return data;
        }


        protected virtual void OnDestroy()
        {

        }
    }
}

