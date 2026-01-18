using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIItemView : UIGameObject, IUIViewable<Item>, IClearable, IDragInstance
{
	public static Action<UIItemView> InstanceDraggedAction;

	public static Action<UIItemView> DragEndAction;

	[SerializeField]
	private UIDisplayAndHideHandler sourceItemDragStateDisplayAndHideHandler;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private UIDisplayAndHideHandler stackCountDisplayAndHideHandler;

	[SerializeField]
	private UIText stackCountText;

	[SerializeField]
	private UIContentSizeFitter stackCountContentSizeFitter;

	[SerializeField]
	private UIDragInstanceHandler dragInstanceHandler;

	[SerializeField]
	private TweenCollection onDragStartTweenCollection;

	[SerializeField]
	private TweenCollection onDragEndTweenCollection;

	[SerializeField]
	private TweenCollection onViewTweenCollection;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIItemView instance;

	public Item Model { get; private set; }

	public bool IsEmpty { get; private set; } = true;

	public UIDragHandler DragHandler => dragInstanceHandler.DragHandler;

	public bool HasDragInstance => instance != null;

	public void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		dragInstanceHandler.OnInstantiateUnityEvent.AddListener(OnDragInstanceInstantiated);
		dragInstanceHandler.DragHandler.DragUnityEvent.AddListener(OnInstanceDragged);
		onDragEndTweenCollection.OnAllTweenCompleted.AddListener(Destroy);
	}

	public void Initialize()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize");
		}
		stackCountDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
	}

	public void View(Item model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", JsonUtility.ToJson(model));
		}
		if (model == null || model.AssetID.IsEmpty)
		{
			Clear();
			return;
		}
		bool flag = Model != model;
		if (flag)
		{
			onViewTweenCollection.Tween();
			RangedWeaponItem rangedWeaponItem = ((Model != null) ? (Model as RangedWeaponItem) : null);
			if (rangedWeaponItem != null)
			{
				rangedWeaponItem.RemoveAmmoCountHandler(OnAmmoCountChanged);
			}
		}
		Model = model;
		IsEmpty = false;
		RangedWeaponItem rangedWeaponItem2 = model as RangedWeaponItem;
		if (rangedWeaponItem2 != null)
		{
			int ammoCount = rangedWeaponItem2.AmmoCount;
			stackCountText.Value = StringUtility.AbbreviateQuantity(ammoCount);
			stackCountDisplayAndHideHandler.Display();
			stackCountContentSizeFitter.RequestLayout();
			if (flag)
			{
				rangedWeaponItem2.RegisterAmmoCountHandler(OnAmmoCountChanged);
			}
		}
		else
		{
			int stackCount = model.StackCount;
			stackCountText.Value = StringUtility.AbbreviateQuantity(stackCount);
			if (stackCount > 1)
			{
				stackCountDisplayAndHideHandler.Display();
				stackCountContentSizeFitter.RequestLayout();
			}
			else
			{
				stackCountDisplayAndHideHandler.Hide();
			}
		}
		InventoryUsableDefinition usableDefinitionFromItemAssetID = RuntimeDatabase.GetUsableDefinitionFromItemAssetID(model.AssetID);
		iconImage.sprite = usableDefinitionFromItemAssetID.Sprite;
		iconImage.gameObject.SetActive(value: true);
	}

	private void OnAmmoCountChanged(int previousValue, int newValue)
	{
		stackCountText.Value = StringUtility.AbbreviateQuantity(newValue);
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		RangedWeaponItem rangedWeaponItem = ((Model != null) ? (Model as RangedWeaponItem) : null);
		if (rangedWeaponItem != null)
		{
			rangedWeaponItem.RemoveAmmoCountHandler(OnAmmoCountChanged);
		}
		IsEmpty = true;
		Model = null;
		iconImage.sprite = null;
		iconImage.gameObject.SetActive(value: false);
		stackCountDisplayAndHideHandler.Hide();
	}

	public void SetDisplayDelay(float delay)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetDisplayDelay", delay);
		}
		stackCountDisplayAndHideHandler.SetDisplayDelay(delay);
	}

	public void OnDragStart()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDragStart");
		}
		onDragStartTweenCollection.Tween();
	}

	public void OnDragEnd()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDragEnd");
		}
		onDragEndTweenCollection.Tween();
		DragEndAction?.Invoke(this);
	}

	private void OnDragInstanceInstantiated(GameObject dragInstance)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDragInstanceInstantiated", dragInstance.DebugSafeName());
		}
		sourceItemDragStateDisplayAndHideHandler.Hide();
		if (!dragInstance.TryGetComponent<UIItemView>(out instance))
		{
			DebugUtility.LogError(this, "OnDragInstanceInstantiated", string.Format("Could not get {0} from {1}!", instance.GetType(), "instance"), dragInstance.DebugSafeName());
		}
		instance.View(Model);
		instance.OnDragStart();
		instance.DragHandler.EndDragUnityEvent.AddListener(instance.OnDragEnd);
		instance.DragHandler.EndDragUnityEvent.AddListener(FadeIn);
	}

	private void OnInstanceDragged()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnInstanceDragged");
		}
		InstanceDraggedAction?.Invoke(this);
	}

	private void FadeIn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "FadeIn");
		}
		sourceItemDragStateDisplayAndHideHandler.Display();
	}

	private void Destroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Destroy");
		}
		MonoBehaviourSingleton<TweenManager>.Instance.CancelAllTweens(base.gameObject);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
