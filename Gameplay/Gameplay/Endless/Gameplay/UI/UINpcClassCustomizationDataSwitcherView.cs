using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003E8 RID: 1000
	public class UINpcClassCustomizationDataSwitcherView : UIBaseView<NpcClassCustomizationData, UINpcClassCustomizationDataSwitcherView.Styles>, IUIInteractable
	{
		// Token: 0x1700051F RID: 1311
		// (get) Token: 0x06001917 RID: 6423 RVA: 0x000742C7 File Offset: 0x000724C7
		// (set) Token: 0x06001918 RID: 6424 RVA: 0x000742CF File Offset: 0x000724CF
		public override UINpcClassCustomizationDataSwitcherView.Styles Style { get; protected set; }

		// Token: 0x06001919 RID: 6425 RVA: 0x000742D8 File Offset: 0x000724D8
		private void Awake()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x0600191A RID: 6426 RVA: 0x00074300 File Offset: 0x00072500
		public override void View(NpcClassCustomizationData model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			Type type = model.GetType();
			foreach (KeyValuePair<Type, GameObject> keyValuePair in this.typeToViewDictionary)
			{
				bool flag = type == keyValuePair.Key;
				keyValuePair.Value.SetActive(flag);
			}
		}

		// Token: 0x0600191B RID: 6427 RVA: 0x0007439C File Offset: 0x0007259C
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
			foreach (KeyValuePair<Type, GameObject> keyValuePair in this.typeToViewDictionary)
			{
				keyValuePair.Value.SetActive(false);
			}
		}

		// Token: 0x0600191C RID: 6428 RVA: 0x0007441C File Offset: 0x0007261C
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.blankNpcCustomizationDataView.SetInteractable(interactable);
			this.gruntNpcCustomizationDataView.SetInteractable(interactable);
			this.riflemanNpcCustomizationDataView.SetInteractable(interactable);
			this.zombieNpcCustomizationDataView.SetInteractable(interactable);
		}

		// Token: 0x0600191D RID: 6429 RVA: 0x0007447C File Offset: 0x0007267C
		private void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (this.initialized)
			{
				return;
			}
			this.typeToViewDictionary = new Dictionary<Type, GameObject>
			{
				{
					typeof(BlankNpcCustomizationData),
					this.blankNpcCustomizationDataView.gameObject
				},
				{
					typeof(GruntNpcCustomizationData),
					this.gruntNpcCustomizationDataView.gameObject
				},
				{
					typeof(RiflemanNpcCustomizationData),
					this.riflemanNpcCustomizationDataView.gameObject
				},
				{
					typeof(ZombieNpcCustomizationData),
					this.zombieNpcCustomizationDataView.gameObject
				}
			};
			this.initialized = true;
		}

		// Token: 0x0400141E RID: 5150
		[SerializeField]
		private UIBlankNpcCustomizationDataView blankNpcCustomizationDataView;

		// Token: 0x0400141F RID: 5151
		[SerializeField]
		private UIGruntNpcCustomizationDataView gruntNpcCustomizationDataView;

		// Token: 0x04001420 RID: 5152
		[SerializeField]
		private UIRiflemanNpcCustomizationDataView riflemanNpcCustomizationDataView;

		// Token: 0x04001421 RID: 5153
		[SerializeField]
		private UIZombieNpcCustomizationDataView zombieNpcCustomizationDataView;

		// Token: 0x04001422 RID: 5154
		private Dictionary<Type, GameObject> typeToViewDictionary;

		// Token: 0x04001423 RID: 5155
		private bool initialized;

		// Token: 0x020003E9 RID: 1001
		public enum Styles
		{
			// Token: 0x04001425 RID: 5157
			Default
		}
	}
}
