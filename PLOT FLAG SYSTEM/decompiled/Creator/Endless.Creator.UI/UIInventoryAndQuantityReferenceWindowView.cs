using System;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIInventoryAndQuantityReferenceWindowView : UIBaseWindowView
{
	private const int MIN_QUANTITY = 1;

	private const int MAX_STACKABLE_QUANTITY = 2000000000;

	private const int MAX_NON_STACKABLE_QUANTITY = 10;

	[Header("UIInventoryAndQuantityReferenceWindowView")]
	[SerializeField]
	private UIIEnumerablePresenter iEnumerablePresenter;

	[SerializeField]
	private GameObject stackableQuantityContainer;

	[SerializeField]
	private UIIntPresenter stackableQuantity;

	[SerializeField]
	private UIButton confirmButton;

	[SerializeField]
	private GameObject[] setActiveIfNotStackable = Array.Empty<GameObject>();

	[SerializeField]
	private LayoutElement stackableInputField;

	private TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference;

	private Action<TradeInfo.InventoryAndQuantityReference> onConfirm;

	private bool isStackable;

	public static UIInventoryAndQuantityReferenceWindowView Display(TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference, Action<TradeInfo.InventoryAndQuantityReference> onConfirm, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{ "inventoryAndQuantityReference", inventoryAndQuantityReference },
			{ "onConfirm", onConfirm }
		};
		return (UIInventoryAndQuantityReferenceWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIInventoryAndQuantityReferenceWindowView>(parent, supplementalData);
	}

	protected override void Start()
	{
		base.Start();
		iEnumerablePresenter.OnSelectionChanged += SetInspectorReference;
		stackableQuantity.OnModelChanged += SetQuantity;
		confirmButton.onClick.AddListener(ConfirmAndClose);
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		iEnumerablePresenter.OnSelectionChanged -= SetInspectorReference;
		stackableQuantity.OnModelChanged -= SetQuantity;
		confirmButton.onClick.RemoveListener(ConfirmAndClose);
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		iEnumerablePresenter.Clear();
		stackableQuantity.Clear();
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		inventoryAndQuantityReference = supplementalData["inventoryAndQuantityReference"] as TradeInfo.InventoryAndQuantityReference;
		if ((object)inventoryAndQuantityReference == null)
		{
			inventoryAndQuantityReference = ReferenceFactory.CreateInventoryAndQuantityReference(SerializableGuid.Empty);
		}
		onConfirm = supplementalData["onConfirm"] as Action<TradeInfo.InventoryAndQuantityReference>;
		List<PropLibrary.RuntimePropInfo> selectionOptions = GetSelectionOptions();
		iEnumerablePresenter.SetModel(selectionOptions, triggerOnModelChanged: false);
		if (!inventoryAndQuantityReference.IsReferenceEmpty())
		{
			SerializableGuid id = InspectorReferenceUtility.GetId(inventoryAndQuantityReference);
			foreach (PropLibrary.RuntimePropInfo item in selectionOptions)
			{
				if ((SerializableGuid)item.PropData.AssetID == id)
				{
					List<object> selectedItems = new List<object> { item };
					iEnumerablePresenter.SetSelected(selectedItems, updateSelectionVisuals: true, invokeOnModelChangedAndOnSelectionChanged: false);
					break;
				}
			}
		}
		stackableQuantityContainer.SetActive(iEnumerablePresenter.SelectedItemsList.Count > 0);
		stackableQuantity.SetModel(inventoryAndQuantityReference.Quantity, triggerOnModelChanged: false);
		if (base.VerboseLogging)
		{
			Debug.Log("inventoryAndQuantityReference: " + JsonUtility.ToJson(inventoryAndQuantityReference), this);
		}
		UpdateIsStackable();
		HandleStackableVisibility();
		UpdateStackableQuantityMinMax();
	}

	private void SetInspectorReference(object item)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInspectorReference", item);
		}
		List<object> list = item as List<object>;
		bool flag = list != null && list.Count > 0;
		stackableQuantityContainer.SetActive(flag);
		if (flag)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo = list[0] as PropLibrary.RuntimePropInfo;
			InspectorReferenceUtility.SetId(inventoryAndQuantityReference, runtimePropInfo.PropData.AssetID);
		}
		else
		{
			inventoryAndQuantityReference = ReferenceFactory.CreateInventoryAndQuantityReference(SerializableGuid.Empty);
		}
		UpdateIsStackable();
		HandleStackableVisibility();
		UpdateStackableQuantityMinMax();
	}

	private void SetQuantity(object item)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetQuantity", item);
		}
		int num = (int)item;
		int num2 = ClampStackableQuantity(num, stackableQuantity.Max);
		if (base.VerboseLogging)
		{
			Debug.Log(string.Format("{0}: {1}", "newQuantity", num), this);
			Debug.Log(string.Format("{0}: {1}", "clampedQuantity", num2), this);
		}
		stackableQuantity.SetModel(num2, triggerOnModelChanged: false);
		inventoryAndQuantityReference.Quantity = num2;
	}

	private void ConfirmAndClose()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ConfirmAndClose");
		}
		if (base.VerboseLogging)
		{
			Debug.Log("inventoryAndQuantityReference: " + JsonUtility.ToJson(inventoryAndQuantityReference), this);
		}
		onConfirm?.Invoke(inventoryAndQuantityReference);
		Close();
	}

	private void UpdateIsStackable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateIsStackable");
		}
		isStackable = false;
		if (inventoryAndQuantityReference.IsReferenceEmpty())
		{
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "isStackable", isStackable), this);
			}
			return;
		}
		SerializableGuid id = InspectorReferenceUtility.GetId(inventoryAndQuantityReference);
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(id, out var metadata))
		{
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "isStackable", isStackable), this);
			}
			return;
		}
		if (metadata.EndlessProp.ReferenceFilter.HasFlag(ReferenceFilter.Resource))
		{
			isStackable = true;
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "isStackable", isStackable), this);
			}
			return;
		}
		if (metadata.GetBaseTypeDefinition().ComponentBase is Item item)
		{
			isStackable = item.IsStackable;
		}
		if (base.VerboseLogging)
		{
			Debug.Log(string.Format("{0}: {1}", "isStackable", isStackable), this);
		}
	}

	private void UpdateStackableQuantityMinMax()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateStackableQuantityMinMax");
		}
		int num = (isStackable ? 2000000000 : 10);
		stackableQuantity.SetMinMax(0, 1, num);
		inventoryAndQuantityReference.Quantity = ClampStackableQuantity(inventoryAndQuantityReference.Quantity, num);
		if (base.VerboseLogging)
		{
			Debug.Log(string.Format("{0}: {1}", "max", num), this);
			Debug.Log(string.Format("{0}: {1}", "Quantity", inventoryAndQuantityReference.Quantity), this);
		}
	}

	private void HandleStackableVisibility()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleStackableVisibility");
		}
		GameObject[] array = setActiveIfNotStackable;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(!isStackable);
		}
		stackableInputField.flexibleWidth = (isStackable ? 1 : (-1));
	}

	private static List<PropLibrary.RuntimePropInfo> GetSelectionOptions()
	{
		List<PropLibrary.RuntimePropInfo> list = new List<PropLibrary.RuntimePropInfo>();
		foreach (AssetReference propReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propReference, out var metadata) && !metadata.IsMissingObject && metadata.EndlessProp.ReferenceFilter.HasFlag(ReferenceFilter.InventoryItem))
			{
				list.Add(metadata);
			}
		}
		return list;
	}

	private static int ClampStackableQuantity(int quantity, int max)
	{
		return Mathf.Clamp(quantity, 1, max);
	}
}
