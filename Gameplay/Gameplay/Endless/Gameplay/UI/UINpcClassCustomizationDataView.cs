using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003E1 RID: 993
	public abstract class UINpcClassCustomizationDataView<TModel> : UIBaseView<TModel, UINpcClassCustomizationDataView<TModel>.Styles>, IUIInteractable, IUINpcClassCustomizationDataViewable where TModel : NpcClassCustomizationData
	{
		// Token: 0x14000034 RID: 52
		// (add) Token: 0x06001901 RID: 6401 RVA: 0x00073E80 File Offset: 0x00072080
		// (remove) Token: 0x06001902 RID: 6402 RVA: 0x00073EB8 File Offset: 0x000720B8
		public event Action<NpcClass> OnNpcClassChanged;

		// Token: 0x1700051C RID: 1308
		// (get) Token: 0x06001903 RID: 6403 RVA: 0x00073EED File Offset: 0x000720ED
		// (set) Token: 0x06001904 RID: 6404 RVA: 0x00073EF5 File Offset: 0x000720F5
		public override UINpcClassCustomizationDataView<TModel>.Styles Style { get; protected set; }

		// Token: 0x06001905 RID: 6405 RVA: 0x00073F00 File Offset: 0x00072100
		protected virtual void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			List<int> list = new List<int>();
			if (!EndlessCloudService.CanHaveGrunt())
			{
				list.Add(1);
			}
			if (!EndlessCloudService.CanHaveRiflemen())
			{
				list.Add(2);
			}
			if (!EndlessCloudService.CanHaveZombies())
			{
				list.Add(3);
			}
			this.npcClassDropdown.SetOptionIndexesToHide(list.ToArray());
			this.npcClassDropdown.OnEnumValueChanged.AddListener(new UnityAction<Enum>(this.OnNpcClassDropdownChanged));
		}

		// Token: 0x06001906 RID: 6406 RVA: 0x00073F80 File Offset: 0x00072180
		public override void View(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
			}
			this.npcClassDropdown.SetEnumValue(model.NpcClass, false);
		}

		// Token: 0x06001907 RID: 6407 RVA: 0x00073FD1 File Offset: 0x000721D1
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.npcClassDropdown.SetIsInteractable(interactable);
		}

		// Token: 0x06001908 RID: 6408 RVA: 0x00074004 File Offset: 0x00072204
		private void OnNpcClassDropdownChanged(Enum npcClassAsEnum)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnNpcClassDropdownChanged", "npcClassAsEnum", npcClassAsEnum), this);
			}
			NpcClass npcClass = (NpcClass)npcClassAsEnum;
			Action<NpcClass> onNpcClassChanged = this.OnNpcClassChanged;
			if (onNpcClassChanged == null)
			{
				return;
			}
			onNpcClassChanged(npcClass);
		}

		// Token: 0x04001413 RID: 5139
		[SerializeField]
		private UIDropdownEnum npcClassDropdown;

		// Token: 0x020003E2 RID: 994
		public enum Styles
		{
			// Token: 0x04001416 RID: 5142
			Default
		}
	}
}
