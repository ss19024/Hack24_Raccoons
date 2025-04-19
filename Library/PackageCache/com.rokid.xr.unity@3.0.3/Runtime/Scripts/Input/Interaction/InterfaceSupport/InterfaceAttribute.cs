using UnityEngine;
using System;

namespace Rokid.UXR.Interaction
{

	/// <summary>
	/// When this attribute is attached to a MonoBehaviour field within a
	/// Unity Object, this allows an interface to be specified in to to
	/// entire only a specific type of MonoBehaviour can be attached.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class InterfaceAttribute : PropertyAttribute
	{
		public Type[] Types = null;
		public string TypeFromFieldName;

		/// <summary>
		/// Creates a new Interface attribute.
		/// </summary>
		/// <param name="type">The type of interface which is allowed.</param>
		/// <param name="types">Extra types of interface which is allowed.</param>
		public InterfaceAttribute(Type type, params Type[] types)
		{
			RKLog.Warning(type.Name + $" {type.Name} needs to be an interface.");
			Debug.Assert(type.IsInterface, $"{type.Name} needs to be an interface.");

			Types = new Type[types.Length + 1];
			Types[0] = type;
			for (int i = 0; i < types.Length; i++)
			{
				Debug.Assert(types[i].IsInterface, $"{types[i].Name} needs to be an interface.");
				Types[i + 1] = types[i];
			}
		}

		public InterfaceAttribute(string typeFromFieldName)
		{
			this.TypeFromFieldName = typeFromFieldName;
		}
	}

}
