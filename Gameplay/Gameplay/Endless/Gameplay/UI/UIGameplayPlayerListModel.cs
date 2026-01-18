using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003C0 RID: 960
	public class UIGameplayPlayerListModel : UIPlayerReferenceManagerListModel
	{
		// Token: 0x06001875 RID: 6261 RVA: 0x00071B08 File Offset: 0x0006FD08
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(new UnityAction(base.Initialize));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.AddListener(new UnityAction(base.Uninitialize));
		}
	}
}
