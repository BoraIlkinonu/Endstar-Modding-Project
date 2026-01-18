using System;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation
{
	// Token: 0x020003B5 RID: 949
	[CreateAssetMenu(menuName = "ScriptableObject/Dynamic Prop Creation/PropCreationPromptData")]
	public class PropCreationPromptData : PropCreationData
	{
		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x06001285 RID: 4741 RVA: 0x0005F5E5 File Offset: 0x0005D7E5
		public string Message
		{
			get
			{
				return this.message;
			}
		}

		// Token: 0x170002AA RID: 682
		// (get) Token: 0x06001286 RID: 4742 RVA: 0x0001BF89 File Offset: 0x0001A189
		public override bool IsSubMenu
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04000F48 RID: 3912
		[TextArea]
		[SerializeField]
		private string message;
	}
}
