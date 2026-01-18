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
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020002D6 RID: 726
	public class UIInventoryAndQuantityReferenceWindowView : UIBaseWindowView
	{
		// Token: 0x06000C49 RID: 3145 RVA: 0x0003AA78 File Offset: 0x00038C78
		public static UIInventoryAndQuantityReferenceWindowView Display(TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference, Action<TradeInfo.InventoryAndQuantityReference> onConfirm, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "inventoryAndQuantityReference", inventoryAndQuantityReference },
				{ "onConfirm", onConfirm }
			};
			return (UIInventoryAndQuantityReferenceWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIInventoryAndQuantityReferenceWindowView>(parent, dictionary);
		}

		// Token: 0x06000C4A RID: 3146 RVA: 0x0003AAB4 File Offset: 0x00038CB4
		protected override void Start()
		{
			base.Start();
			this.iEnumerablePresenter.OnSelectionChanged += new Action<IReadOnlyList<object>>(this.SetInspectorReference);
			this.stackableQuantity.OnModelChanged += this.SetQuantity;
			this.confirmButton.onClick.AddListener(new UnityAction(this.ConfirmAndClose));
		}

		// Token: 0x06000C4B RID: 3147 RVA: 0x0003AB14 File Offset: 0x00038D14
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.iEnumerablePresenter.OnSelectionChanged -= new Action<IReadOnlyList<object>>(this.SetInspectorReference);
			this.stackableQuantity.OnModelChanged -= this.SetQuantity;
			this.confirmButton.onClick.RemoveListener(new UnityAction(this.ConfirmAndClose));
		}

		// Token: 0x06000C4C RID: 3148 RVA: 0x0003AB83 File Offset: 0x00038D83
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.iEnumerablePresenter.Clear();
			this.stackableQuantity.Clear();
		}

		// Token: 0x06000C4D RID: 3149 RVA: 0x0003ABA4 File Offset: 0x00038DA4
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.inventoryAndQuantityReference = supplementalData["inventoryAndQuantityReference"] as TradeInfo.InventoryAndQuantityReference;
			if (this.inventoryAndQuantityReference == null)
			{
				this.inventoryAndQuantityReference = ReferenceFactory.CreateInventoryAndQuantityReference(SerializableGuid.Empty);
			}
			this.onConfirm = supplementalData["onConfirm"] as Action<TradeInfo.InventoryAndQuantityReference>;
			List<PropLibrary.RuntimePropInfo> selectionOptions = UIInventoryAndQuantityReferenceWindowView.GetSelectionOptions();
			this.iEnumerablePresenter.SetModel(selectionOptions, false);
			if (!this.inventoryAndQuantityReference.IsReferenceEmpty())
			{
				SerializableGuid id = InspectorReferenceUtility.GetId(this.inventoryAndQuantityReference);
				foreach (PropLibrary.RuntimePropInfo runtimePropInfo in selectionOptions)
				{
					if (runtimePropInfo.PropData.AssetID == id)
					{
						List<object> list = new List<object> { runtimePropInfo };
						this.iEnumerablePresenter.SetSelected(list, true, false);
						break;
					}
				}
			}
			this.stackableQuantityContainer.SetActive(this.iEnumerablePresenter.SelectedItemsList.Count > 0);
			this.stackableQuantity.SetModel(this.inventoryAndQuantityReference.Quantity, false);
			if (base.VerboseLogging)
			{
				Debug.Log("inventoryAndQuantityReference: " + JsonUtility.ToJson(this.inventoryAndQuantityReference), this);
			}
			this.UpdateIsStackable();
			this.HandleStackableVisibility();
			this.UpdateStackableQuantityMinMax();
		}

		// Token: 0x06000C4E RID: 3150 RVA: 0x0003AD04 File Offset: 0x00038F04
		private void SetInspectorReference(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInspectorReference", new object[] { item });
			}
			List<object> list = item as List<object>;
			bool flag = list != null && list.Count > 0;
			this.stackableQuantityContainer.SetActive(flag);
			if (flag)
			{
				PropLibrary.RuntimePropInfo runtimePropInfo = list[0] as PropLibrary.RuntimePropInfo;
				InspectorReferenceUtility.SetId(this.inventoryAndQuantityReference, runtimePropInfo.PropData.AssetID);
			}
			else
			{
				this.inventoryAndQuantityReference = ReferenceFactory.CreateInventoryAndQuantityReference(SerializableGuid.Empty);
			}
			this.UpdateIsStackable();
			this.HandleStackableVisibility();
			this.UpdateStackableQuantityMinMax();
		}

		// Token: 0x06000C4F RID: 3151 RVA: 0x0003ADA0 File Offset: 0x00038FA0
		private void SetQuantity(object item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetQuantity", new object[] { item });
			}
			int num = (int)item;
			int num2 = UIInventoryAndQuantityReferenceWindowView.ClampStackableQuantity(num, this.stackableQuantity.Max);
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "newQuantity", num), this);
				Debug.Log(string.Format("{0}: {1}", "clampedQuantity", num2), this);
			}
			this.stackableQuantity.SetModel(num2, false);
			this.inventoryAndQuantityReference.Quantity = num2;
		}

		// Token: 0x06000C50 RID: 3152 RVA: 0x0003AE3C File Offset: 0x0003903C
		private void ConfirmAndClose()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmAndClose", Array.Empty<object>());
			}
			if (base.VerboseLogging)
			{
				Debug.Log("inventoryAndQuantityReference: " + JsonUtility.ToJson(this.inventoryAndQuantityReference), this);
			}
			Action<TradeInfo.InventoryAndQuantityReference> action = this.onConfirm;
			if (action != null)
			{
				action(this.inventoryAndQuantityReference);
			}
			this.Close();
		}

		// Token: 0x06000C51 RID: 3153 RVA: 0x0003AEA4 File Offset: 0x000390A4
		private void UpdateIsStackable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateIsStackable", Array.Empty<object>());
			}
			this.isStackable = false;
			if (this.inventoryAndQuantityReference.IsReferenceEmpty())
			{
				if (base.VerboseLogging)
				{
					Debug.Log(string.Format("{0}: {1}", "isStackable", this.isStackable), this);
				}
				return;
			}
			SerializableGuid id = InspectorReferenceUtility.GetId(this.inventoryAndQuantityReference);
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(id, out runtimePropInfo))
			{
				if (base.VerboseLogging)
				{
					Debug.Log(string.Format("{0}: {1}", "isStackable", this.isStackable), this);
				}
				return;
			}
			if (runtimePropInfo.EndlessProp.ReferenceFilter.HasFlag(ReferenceFilter.Resource))
			{
				this.isStackable = true;
				if (base.VerboseLogging)
				{
					Debug.Log(string.Format("{0}: {1}", "isStackable", this.isStackable), this);
				}
				return;
			}
			Item item = runtimePropInfo.GetBaseTypeDefinition().ComponentBase as Item;
			if (item != null)
			{
				this.isStackable = item.IsStackable;
			}
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "isStackable", this.isStackable), this);
			}
		}

		// Token: 0x06000C52 RID: 3154 RVA: 0x0003AFE8 File Offset: 0x000391E8
		private void UpdateStackableQuantityMinMax()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateStackableQuantityMinMax", Array.Empty<object>());
			}
			int num = (this.isStackable ? 2000000000 : 10);
			this.stackableQuantity.SetMinMax(0, 1, num);
			this.inventoryAndQuantityReference.Quantity = UIInventoryAndQuantityReferenceWindowView.ClampStackableQuantity(this.inventoryAndQuantityReference.Quantity, num);
			if (base.VerboseLogging)
			{
				Debug.Log(string.Format("{0}: {1}", "max", num), this);
				Debug.Log(string.Format("{0}: {1}", "Quantity", this.inventoryAndQuantityReference.Quantity), this);
			}
		}

		// Token: 0x06000C53 RID: 3155 RVA: 0x0003B094 File Offset: 0x00039294
		private void HandleStackableVisibility()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleStackableVisibility", Array.Empty<object>());
			}
			GameObject[] array = this.setActiveIfNotStackable;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(!this.isStackable);
			}
			this.stackableInputField.flexibleWidth = (float)(this.isStackable ? 1 : (-1));
		}

		// Token: 0x06000C54 RID: 3156 RVA: 0x0003B0F8 File Offset: 0x000392F8
		private static List<PropLibrary.RuntimePropInfo> GetSelectionOptions()
		{
			List<PropLibrary.RuntimePropInfo> list = new List<PropLibrary.RuntimePropInfo>();
			foreach (AssetReference assetReference in MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences)
			{
				PropLibrary.RuntimePropInfo runtimePropInfo;
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetReference, out runtimePropInfo) && !runtimePropInfo.IsMissingObject && runtimePropInfo.EndlessProp.ReferenceFilter.HasFlag(ReferenceFilter.InventoryItem))
				{
					list.Add(runtimePropInfo);
				}
			}
			return list;
		}

		// Token: 0x06000C55 RID: 3157 RVA: 0x0003B194 File Offset: 0x00039394
		private static int ClampStackableQuantity(int quantity, int max)
		{
			return Mathf.Clamp(quantity, 1, max);
		}

		// Token: 0x04000A99 RID: 2713
		private const int MIN_QUANTITY = 1;

		// Token: 0x04000A9A RID: 2714
		private const int MAX_STACKABLE_QUANTITY = 2000000000;

		// Token: 0x04000A9B RID: 2715
		private const int MAX_NON_STACKABLE_QUANTITY = 10;

		// Token: 0x04000A9C RID: 2716
		[Header("UIInventoryAndQuantityReferenceWindowView")]
		[SerializeField]
		private UIIEnumerablePresenter iEnumerablePresenter;

		// Token: 0x04000A9D RID: 2717
		[SerializeField]
		private GameObject stackableQuantityContainer;

		// Token: 0x04000A9E RID: 2718
		[SerializeField]
		private UIIntPresenter stackableQuantity;

		// Token: 0x04000A9F RID: 2719
		[SerializeField]
		private UIButton confirmButton;

		// Token: 0x04000AA0 RID: 2720
		[SerializeField]
		private GameObject[] setActiveIfNotStackable = Array.Empty<GameObject>();

		// Token: 0x04000AA1 RID: 2721
		[SerializeField]
		private LayoutElement stackableInputField;

		// Token: 0x04000AA2 RID: 2722
		private TradeInfo.InventoryAndQuantityReference inventoryAndQuantityReference;

		// Token: 0x04000AA3 RID: 2723
		private Action<TradeInfo.InventoryAndQuantityReference> onConfirm;

		// Token: 0x04000AA4 RID: 2724
		private bool isStackable;
	}
}
