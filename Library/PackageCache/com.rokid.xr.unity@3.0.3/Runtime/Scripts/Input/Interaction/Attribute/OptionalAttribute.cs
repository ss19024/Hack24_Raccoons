using UnityEngine;

namespace Rokid.UXR.Interaction {
	/// <summary>
	/// Used on a SerializedField surfaces the expectation that this field can remain empty.
	/// </summary>
	public class OptionalAttribute : PropertyAttribute
	{
	    public OptionalAttribute() { }
	}
}
