using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000123 RID: 291
	public class UIInventorySpawnOptionsListCellController : UIBaseListCellController<InventorySpawnOption>
	{
		// Token: 0x1700006F RID: 111
		// (get) Token: 0x0600048E RID: 1166 RVA: 0x0001A6FB File Offset: 0x000188FB
		private UIInventorySpawnOptionsListModel TypedListModel
		{
			get
			{
				return (UIInventorySpawnOptionsListModel)base.ListModel;
			}
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x0001A708 File Offset: 0x00018908
		protected override void Start()
		{
			base.Start();
			this.lockToggle.OnChange.AddListener(new UnityAction<bool>(this.ToggleLock));
			this.quantityToggle.OnChange.AddListener(new UnityAction<bool>(this.ToggleChangeAmount));
			this.quantityStepper.ChangeUnityEvent.AddListener(new UnityAction(this.ChangeAmount));
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x0001A76F File Offset: 0x0001896F
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x06000491 RID: 1169 RVA: 0x0001A78E File Offset: 0x0001898E
		private void ToggleLock(bool setToLock)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ToggleLock", new object[] { setToLock });
			}
			base.Model.LockItem = setToLock;
			this.TypedListModel.ApplyChanges();
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x0001A7CC File Offset: 0x000189CC
		private void ToggleChangeAmount(bool add)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ChangeAmount", new object[] { add });
			}
			base.Model.Quantity = (add ? 1 : 0);
			this.TweenLock();
			this.TypedListModel.ApplyChanges();
		}

		// Token: 0x06000493 RID: 1171 RVA: 0x0001A820 File Offset: 0x00018A20
		private void ChangeAmount()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ChangeAmount", Array.Empty<object>());
			}
			int num;
			if (!int.TryParse(this.quantityStepper.Value, out num))
			{
				DebugUtility.LogError(this, "ChangeAmount", "Could not parse the Value value of " + this.quantityStepper.Value + "!", Array.Empty<object>());
				return;
			}
			base.Model.Quantity = num;
			this.TweenLock();
			this.TypedListModel.ApplyChanges();
		}

		// Token: 0x06000494 RID: 1172 RVA: 0x0001A8A1 File Offset: 0x00018AA1
		private void TweenLock()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenLock", Array.Empty<object>());
			}
			if (base.Model.Quantity == 0)
			{
				this.lockHideTweens.Tween();
				return;
			}
			this.lockDisplayTweens.Tween();
		}

		// Token: 0x04000451 RID: 1105
		[Header("UIInventorySpawnOptionsListCellController")]
		[SerializeField]
		private UIToggle lockToggle;

		// Token: 0x04000452 RID: 1106
		[SerializeField]
		private UIToggle quantityToggle;

		// Token: 0x04000453 RID: 1107
		[SerializeField]
		private UIStepper quantityStepper;

		// Token: 0x04000454 RID: 1108
		[SerializeField]
		private TweenCollection lockDisplayTweens;

		// Token: 0x04000455 RID: 1109
		[SerializeField]
		private TweenCollection lockHideTweens;
	}
}
