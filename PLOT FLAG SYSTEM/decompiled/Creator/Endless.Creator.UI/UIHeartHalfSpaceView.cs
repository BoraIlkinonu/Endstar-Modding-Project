using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIHeartHalfSpaceView : UIPoolableGameObject
{
	[Header("UIHeartHalfSpaceView")]
	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	public override void OnSpawn()
	{
		base.OnSpawn();
		displayAndHideHandler.SetToDisplayStart(triggerUnityEvent: false);
		displayAndHideHandler.Display();
	}

	public void HideAndDespawnOnComplete()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideAndDespawnOnComplete");
		}
		displayAndHideHandler.Hide(Despawn);
	}

	private void Despawn()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Despawn");
		}
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
	}
}
