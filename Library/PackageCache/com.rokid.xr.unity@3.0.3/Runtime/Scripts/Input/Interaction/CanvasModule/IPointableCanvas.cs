using UnityEngine;

namespace Rokid.UXR.Interaction {
	public interface IPointableCanvas : IPointableElement
	{
	    Canvas Canvas { get; }
	}
}
