
namespace Rokid.UXR.Interaction {
	/// 可交互的元素+WhenPoitnerRaised 
	/// </summary>
	public interface IPointableElement : IPointable
	{
	    void ProcessPointerEvent(PointerEvent evt);
	}
}
