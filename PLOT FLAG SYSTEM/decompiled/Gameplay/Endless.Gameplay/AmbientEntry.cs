using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public abstract class AmbientEntry : MonoBehaviour
{
	[HideInInspector]
	public UnityEvent<AmbientEntry> OnActivated = new UnityEvent<AmbientEntry>();

	[HideInInspector]
	public UnityEvent<AmbientEntry> OnDeactivated = new UnityEvent<AmbientEntry>();

	public virtual void Activate()
	{
		OnActivated.Invoke(this);
	}

	public virtual void Deactivate()
	{
		OnDeactivated.Invoke(this);
	}
}
