using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Rokid.UXR.Interaction
{
    /// <summary>
    /// PointerCanvas allows any IPointable to forward its
    /// events onto an associated Canvas via the IPointableCanvas interface
    /// Requires a PointableCanvasModule present in the scene.
    /// </summary>
    public class PointableCanvas : PointableElement, IPointableCanvas
    {
        [SerializeField]
        private Canvas _canvas;
        public Canvas Canvas => _canvas;

        protected override void Start()
        {
            base.Start();
            Assert.IsNotNull(Canvas);
            Assert.IsNotNull(Canvas.GetComponent<GraphicRaycaster>(),
        "PointableCanvas requires that the Canvas object has an attached GraphicRaycaster.");
        }

        private void Register()
        {
            PointableCanvasModule.RegisterPointableCanvas(this);
        }

        private void Unregister()
        {
            PointableCanvasModule.UnregisterPointableCanvas(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Register();
        }

        protected override void OnDisable()
        {
            Unregister();
            base.OnDisable();
        }
    }
}

