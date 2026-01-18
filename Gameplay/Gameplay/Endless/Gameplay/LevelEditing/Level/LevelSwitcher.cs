using System;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000559 RID: 1369
	public class LevelSwitcher : EndlessNetworkBehaviour
	{
		// Token: 0x060020F3 RID: 8435 RVA: 0x0009496C File Offset: 0x00092B6C
		public void Activate()
		{
			if (base.IsServer)
			{
				this.onLoadLevel.Invoke(JsonUtility.FromJson<SerializedLevel>(this.targetLevel.text));
			}
		}

		// Token: 0x060020F5 RID: 8437 RVA: 0x00094994 File Offset: 0x00092B94
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060020F6 RID: 8438 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060020F7 RID: 8439 RVA: 0x000949AA File Offset: 0x00092BAA
		protected internal override string __getTypeName()
		{
			return "LevelSwitcher";
		}

		// Token: 0x04001A42 RID: 6722
		[SerializeField]
		private TextAsset targetLevel;

		// Token: 0x04001A43 RID: 6723
		[SerializeField]
		private UnityEvent<SerializedLevel> onLoadLevel;
	}
}
