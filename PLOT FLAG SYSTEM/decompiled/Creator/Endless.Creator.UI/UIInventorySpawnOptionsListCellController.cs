using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInventorySpawnOptionsListCellController : UIBaseListCellController<InventorySpawnOption>
{
	[Header("UIInventorySpawnOptionsListCellController")]
	[SerializeField]
	private UIToggle lockToggle;

	[SerializeField]
	private UIToggle quantityToggle;

	[SerializeField]
	private UIStepper quantityStepper;

	[SerializeField]
	private TweenCollection lockDisplayTweens;

	[SerializeField]
	private TweenCollection lockHideTweens;

	private UIInventorySpawnOptionsListModel TypedListModel => (UIInventorySpawnOptionsListModel)base.ListModel;

	protected override void Start()
	{
		base.Start();
		lockToggle.OnChange.AddListener(ToggleLock);
		quantityToggle.OnChange.AddListener(ToggleChangeAmount);
		quantityStepper.ChangeUnityEvent.AddListener(ChangeAmount);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	private void ToggleLock(bool setToLock)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleLock", setToLock);
		}
		base.Model.LockItem = setToLock;
		TypedListModel.ApplyChanges();
	}

	private void ToggleChangeAmount(bool add)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ChangeAmount", add);
		}
		base.Model.Quantity = (add ? 1 : 0);
		TweenLock();
		TypedListModel.ApplyChanges();
	}

	private void ChangeAmount()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ChangeAmount");
		}
		if (!int.TryParse(quantityStepper.Value, out var result))
		{
			DebugUtility.LogError(this, "ChangeAmount", "Could not parse the Value value of " + quantityStepper.Value + "!");
			return;
		}
		base.Model.Quantity = result;
		TweenLock();
		TypedListModel.ApplyChanges();
	}

	private void TweenLock()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "TweenLock");
		}
		if (base.Model.Quantity == 0)
		{
			lockHideTweens.Tween();
		}
		else
		{
			lockDisplayTweens.Tween();
		}
	}
}
