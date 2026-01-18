using System;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000ED RID: 237
	public class UICreatorPlayerListModel : UIPlayerReferenceManagerListModel
	{
		// Token: 0x060003EF RID: 1007 RVA: 0x00018FC4 File Offset: 0x000171C4
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(base.Initialize));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(base.Uninitialize));
		}
	}
}
