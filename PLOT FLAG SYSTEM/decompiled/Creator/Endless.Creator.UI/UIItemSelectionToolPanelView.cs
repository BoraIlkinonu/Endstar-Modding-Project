using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIItemSelectionToolPanelView<T, TItemType> : UIBaseToolPanelView<T> where T : EndlessTool
{
	[Header("Detail")]
	[SerializeField]
	private UIDisplayAndHideHandler detailDisplayAndHideHandler;

	[SerializeField]
	private InterfaceReference<IUIViewable<TItemType>> detailView;

	[SerializeField]
	private InterfaceReference<IUIDetailControllable> detailController;

	[Header("Panel Docking")]
	[SerializeField]
	private TweenCollection dockTweenCollection;

	[SerializeField]
	private TweenCollection undockTweenCollection;

	[Header("Floating Selected Item")]
	[SerializeField]
	private TweenCollection floatingSelectedItemContainerDisplayTweenCollection;

	[SerializeField]
	private TweenCollection floatingSelectedItemContainerHideTweenCollection;

	[SerializeField]
	private InterfaceReference<IUIViewable<TItemType>> floatingSelectedItemView;

	protected bool IsMobile;

	protected UIBaseListView<TItemType> ListView { get; private set; }

	protected abstract bool HasSelectedItem { get; }

	protected abstract bool CanViewDetail { get; }

	protected override void Start()
	{
		base.Start();
		detailDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		detailDisplayAndHideHandler.OnHideComplete.AddListener(detailView.Interface.Clear);
		IsMobile = MobileUtility.IsMobile;
		if (IsMobile)
		{
			detailController.Interface.OnHide.AddListener(Dock);
		}
	}

	public override void Display()
	{
		base.Display();
		if (HasSelectedItem && IsMobile)
		{
			Dock();
		}
	}

	public override void Hide()
	{
		base.Hide();
		floatingSelectedItemContainerHideTweenCollection.Tween();
	}

	public void ViewSelectedItem(TItemType itemType)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewSelectedItem", "itemType", itemType), this);
		}
		if (CanViewDetail)
		{
			detailView.Interface.View(itemType);
			detailDisplayAndHideHandler.Display();
		}
		if (IsMobile)
		{
			floatingSelectedItemView.Interface.View(itemType);
			Dock();
		}
	}

	public void Dock()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Dock", this);
		}
		dockTweenCollection.Tween();
		if (IsMobile && (!MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed || !(MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed is UIScriptWindowView)))
		{
			floatingSelectedItemContainerDisplayTweenCollection.Tween();
		}
	}

	public void Undock()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Undock", this);
		}
		undockTweenCollection.Tween();
		if (IsMobile)
		{
			floatingSelectedItemContainerHideTweenCollection.Tween();
		}
		ListView.SetDataToAllVisibleCells();
	}

	protected void OnItemSelectionEmpty()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnItemSelectionEmpty", this);
		}
		if (IsMobile)
		{
			Undock();
		}
		detailDisplayAndHideHandler.Hide();
	}
}
