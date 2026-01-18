using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200039D RID: 925
	public class UIEquipmentView : UIMonoBehaviourSingleton<UIEquipmentView>
	{
		// Token: 0x170004DA RID: 1242
		// (get) Token: 0x06001788 RID: 6024 RVA: 0x0006D767 File Offset: 0x0006B967
		public IReadOnlyList<UIEquippedSlotController> EquippedSlotControllers
		{
			get
			{
				return this.equippedSlotControllers;
			}
		}

		// Token: 0x06001789 RID: 6025 RVA: 0x0006D770 File Offset: 0x0006B970
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (MobileUtility.IsMobile)
			{
				base.gameObject.SetActive(false);
				return;
			}
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(new UnityAction(this.View));
		}

		// Token: 0x0600178A RID: 6026 RVA: 0x0006D7C4 File Offset: 0x0006B9C4
		public void RegisterEquippedSlotController(UIEquippedSlotController equippedSlotController)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RegisterEquippedSlotController", new object[] { equippedSlotController });
			}
			this.equippedSlotControllers.Add(equippedSlotController);
		}

		// Token: 0x0600178B RID: 6027 RVA: 0x0006D7F0 File Offset: 0x0006B9F0
		private void View()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", Array.Empty<object>());
			}
			for (int i = 0; i < this.equippedSlots.Length; i++)
			{
				float num = (float)(i + 1) * 0.25f;
				this.equippedSlots[i].Initialize(num);
			}
		}

		// Token: 0x040012F3 RID: 4851
		[SerializeField]
		private UIEquippedSlotView[] equippedSlots;

		// Token: 0x040012F4 RID: 4852
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040012F5 RID: 4853
		private readonly List<UIEquippedSlotController> equippedSlotControllers = new List<UIEquippedSlotController>();
	}
}
