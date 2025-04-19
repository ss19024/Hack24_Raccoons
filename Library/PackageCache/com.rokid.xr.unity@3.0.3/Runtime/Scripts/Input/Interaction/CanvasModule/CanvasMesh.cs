
namespace Rokid.UXR.Interaction {
	
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
	
	[DisallowMultipleComponent]
	public abstract class CanvasMesh : MonoBehaviour
	{
	    [SerializeField]
	    protected CanvasRenderTexture _canvasRenderTexture;
	
	    [SerializeField]
	    protected MeshFilter _meshFilter;
	
	    [SerializeField, Optional]
	    protected MeshCollider _meshCollider = null;
	
	    protected abstract Vector3 MeshInverseTransform(Vector3 localPosition);
	
	    protected abstract void GenerateMesh(out List<Vector3> verts, out List<int> tris, out List<Vector2> uvs);
	
	    /// <summary>
	    /// Transform a position in world space relative to the imposter to an associated position relative
	    /// to the original canvas in world space.
	    /// </summary>
	    public Vector3 ImposterToCanvasTransformPoint(Vector3 worldPosition)
	    {
	        Vector3 localToImposter =
	            _meshFilter.transform.InverseTransformPoint(worldPosition);
	        Vector3 canvasLocalPosition = MeshInverseTransform(localToImposter) /
	                                      _canvasRenderTexture.transform.localScale.x;
	        Vector3 transformedWorldPosition = _canvasRenderTexture.transform.TransformPoint(canvasLocalPosition);
	        return transformedWorldPosition;//
	    }
	
	    protected virtual void Start()
	    {
	        Assert.IsNotNull(_meshFilter);
	        Assert.IsNotNull(_canvasRenderTexture);
	    }
	
	    protected virtual void OnEnable()
	    {
	        UpdateImposter();
	
	        _canvasRenderTexture.OnUpdateRenderTexture += HandleUpdateRenderTexture;
	        if (_canvasRenderTexture.Texture != null)
	        {
	            HandleUpdateRenderTexture(_canvasRenderTexture.Texture);
	        }
	    }
	
	    protected virtual void OnDisable()
	    {
	        _canvasRenderTexture.OnUpdateRenderTexture -= HandleUpdateRenderTexture;
	    }
	
	    protected virtual void HandleUpdateRenderTexture(Texture texture)
	    {
	        UpdateImposter();
	    }
	
	    public virtual void UpdateImposter()
	    {
	        Profiler.BeginSample("InterfaceRenderer.UpdateImposter");
	        try
	        {
	            GenerateMesh(out List<Vector3> verts, out List<int> tris, out List<Vector2> uvs);
	
	            Mesh mesh = new Mesh();
	            mesh.SetVertices(verts);
	            mesh.SetUVs(0, uvs);
	            mesh.SetTriangles(tris, 0);
	
	            mesh.RecalculateBounds();
	            mesh.RecalculateNormals();
	
	            _meshFilter.mesh = mesh;
	            if (_meshCollider != null)
	            {
	                _meshCollider.sharedMesh = _meshFilter.sharedMesh;
	            }
	        }
	        finally
	        {
	            Profiler.EndSample();
	        }
	    }
	
	}
}
