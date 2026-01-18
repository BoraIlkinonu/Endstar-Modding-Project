using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000237 RID: 567
	[CreateAssetMenu(menuName = "ScriptableObject/UI/Shared/Arrays/Presenter Array", fileName = "Presenter Array")]
	public class UIPresenterScriptableObjectArray : ScriptableObjectArray<InterfaceReference<IUIPresentable>>
	{
		// Token: 0x06000E59 RID: 3673 RVA: 0x0003ED80 File Offset: 0x0003CF80
		private void SetArrayToPrefabsInDirectory()
		{
			DebugUtility.LogWarning("SetArrayToPrefabsInDirectory is only available in the Unity Editor.", this);
		}
	}
}
