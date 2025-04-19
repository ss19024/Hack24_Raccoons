using System;
using System.Collections.Generic;

namespace Rokid.UXR.Interaction {
	/// <summary>
	/// MAction can be used in place of Action. This allows
	/// for interfaces with Actions of generic covariant types
	/// to be subscribed to by multiple types of delegates.
	/// </summary>
	public interface MAction<out T>
	{
	    event Action<T> Action;
	}
	
	/// <summary>
	/// Classes that implement an interface that has MActions
	/// can use MultiAction as their MAction implementation to
	/// allow for multiple types of delegates to subscribe to the
	/// generic type.
	/// </summary>
	public class MultiAction<T> : MAction<T>
	{
	    protected HashSet<Action<T>> actions = new HashSet<Action<T>>();
	
	    public event Action<T> Action
	    {
	        add
	        {
	            actions.Add(value);
	        }
	        remove
	        {
	            actions.Remove(value);
	        }
	    }
	
	    public void Invoke(T t)
	    {
	        foreach (Action<T> action in actions)
	        {
	            action(t);
	        }
	    }
	}
}
