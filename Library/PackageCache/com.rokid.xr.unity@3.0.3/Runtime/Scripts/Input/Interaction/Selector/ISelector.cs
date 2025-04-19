
namespace Rokid.UXR.Interaction
{
	using System;
	/// <summary>
	/// ISelector defines an input abstraction that can broadcast
	/// select and release events
	/// </summary>
	public interface ISelector
	{
		event Action WhenSelected;
		event Action WhenUnselected;
		bool Selecting { get; }
		bool PointerDown { get; }
		bool PointerUp { get; }
	}
}
