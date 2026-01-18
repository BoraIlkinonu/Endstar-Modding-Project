using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003B1 RID: 945
	public sealed class UIInventorySlotView : UIGameObject, IPoolableT
	{
		// Token: 0x170004F7 RID: 1271
		// (get) Token: 0x0600181F RID: 6175 RVA: 0x0007046E File Offset: 0x0006E66E
		// (set) Token: 0x06001820 RID: 6176 RVA: 0x00070476 File Offset: 0x0006E676
		public InventorySlot Model { get; private set; }

		// Token: 0x170004F8 RID: 1272
		// (get) Token: 0x06001821 RID: 6177 RVA: 0x0007047F File Offset: 0x0006E67F
		// (set) Token: 0x06001822 RID: 6178 RVA: 0x00070487 File Offset: 0x0006E687
		public int Index { get; private set; }

		// Token: 0x170004F9 RID: 1273
		// (get) Token: 0x06001823 RID: 6179 RVA: 0x00070490 File Offset: 0x0006E690
		// (set) Token: 0x06001824 RID: 6180 RVA: 0x00070498 File Offset: 0x0006E698
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170004FA RID: 1274
		// (get) Token: 0x06001825 RID: 6181 RVA: 0x00017586 File Offset: 0x00015786
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170004FB RID: 1275
		// (get) Token: 0x06001826 RID: 6182 RVA: 0x000704A1 File Offset: 0x0006E6A1
		public Vector3 ItemPosition
		{
			get
			{
				return this.item.transform.position;
			}
		}

		// Token: 0x06001827 RID: 6183 RVA: 0x000704B3 File Offset: 0x0006E6B3
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!this.waitFrameAndViewBorderOnEnable)
			{
				return;
			}
			base.StartCoroutine(this.WaitFrameAndViewBorder());
		}

		// Token: 0x06001828 RID: 6184 RVA: 0x000704E3 File Offset: 0x0006E6E3
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayAndHideHandler.OnHideComplete.AddListener(new UnityAction(this.Despawn));
		}

		// Token: 0x06001829 RID: 6185 RVA: 0x00070519 File Offset: 0x0006E719
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x0600182A RID: 6186 RVA: 0x00070533 File Offset: 0x0006E733
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			this.displayAndHideHandler.SetToHideEnd(false);
			this.waitFrameAndViewBorderOnEnable = false;
			this.hotKeyDisplayAndHideHandler.SetToHideEnd(false);
		}

		// Token: 0x0600182B RID: 6187 RVA: 0x0007056C File Offset: 0x0006E76C
		public void Initialize(InventorySlot model, float delay, int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { model, delay, index });
			}
			this.View(model, index);
			if (this.isFirstDisplay)
			{
				this.displayAndHideHandler.SetToHideEnd(false);
				this.item.Initialize();
				this.hotKeyDisplayAndHideHandler.SetToHideEnd(false);
				this.isFirstDisplay = false;
			}
			this.SetSetDisplayDelay(this.isFirstDisplay ? delay : 0f);
			this.displayAndHideHandler.Display();
			if (index <= 9)
			{
				this.hotKeyDisplayAndHideHandler.Display();
			}
		}

		// Token: 0x0600182C RID: 6188 RVA: 0x00070618 File Offset: 0x0006E818
		public void View(InventorySlot model, int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[]
				{
					JsonUtility.ToJson(model.Item),
					index
				});
			}
			this.Model = model;
			bool flag = model.Item == null;
			this.item.View(model.Item);
			this.item.gameObject.SetActive(!flag);
			this.Index = index;
			if (model.Locked)
			{
				this.lockedDisplayAndHideHandler.Display();
			}
			else
			{
				this.lockedDisplayAndHideHandler.Hide();
			}
			if (base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(this.WaitFrameAndViewBorder());
			}
			else
			{
				this.waitFrameAndViewBorderOnEnable = true;
			}
			this.hotKeyText.text = (index + 1).ToString();
		}

		// Token: 0x0600182D RID: 6189 RVA: 0x000706F1 File Offset: 0x0006E8F1
		public void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x0600182E RID: 6190 RVA: 0x00070718 File Offset: 0x0006E918
		public void SetSetDisplayDelay(float delay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSetDisplayDelay", new object[] { delay });
			}
			this.displayAndHideHandler.SetDisplayDelay(delay);
			this.item.SetDisplayDelay(delay + (float)(this.isFirstDisplay ? 1 : 0));
		}

		// Token: 0x0600182F RID: 6191 RVA: 0x00070770 File Offset: 0x0006E970
		public void ViewDropFeedback(bool dropIsValid)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewDropFeedback", new object[] { dropIsValid });
			}
			if (dropIsValid)
			{
				if (this.didTweenValidDropTweenCollection)
				{
					return;
				}
				this.validDropTweenCollection.Tween();
				this.didTweenValidDropTweenCollection = true;
				return;
			}
			else
			{
				if (!this.didTweenValidDropTweenCollection)
				{
					return;
				}
				this.invalidDropTweenCollection.Tween();
				this.didTweenValidDropTweenCollection = false;
				return;
			}
		}

		// Token: 0x06001830 RID: 6192 RVA: 0x000707D9 File Offset: 0x0006E9D9
		private IEnumerator WaitFrameAndViewBorder()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitFrameAndViewBorder", Array.Empty<object>());
			}
			yield return new WaitForEndOfFrame();
			bool flag = this.Model.Item == null;
			UIInventorySlotView.States states = UIInventorySlotView.States.InventorySlotUnoccupied;
			if (flag)
			{
				this.inventorySlotUnoccupiedBorderImage.color = new Color(1f, 1f, 1f, this.inventorySlotUnoccupiedBorderImage.color.a);
			}
			else
			{
				states = ((MonoBehaviourSingleton<UIInventoryView>.Instance.Inventory.GetEquippedSlotIndex(this.Index) > -1) ? UIInventorySlotView.States.EquipmentSlotOccupied : UIInventorySlotView.States.InventorySlotOccupied);
				Color color = ((this.Model.Item.InventorySlot == Item.InventorySlotType.Major) ? this.majorEquipmentColor.Value : this.minorEquipmentColor.Value);
				Graphic[] array = this.occupiedBorderGraphics;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = color;
				}
				this.inventorySlotUnoccupiedBorderImage.color = new Color(color.r, color.g, color.b, this.inventorySlotUnoccupiedBorderImage.color.a);
			}
			switch (states)
			{
			case UIInventorySlotView.States.EquipmentSlotOccupied:
				this.inventorySlotOccupiedBorderImage.sprite = this.equipmentSlotOccupiedBorderSpriteVariable.Value;
				this.inventorySlotUnoccupiedBorderImage.sprite = this.equipmentSlotUnoccupiedBorderSpriteVariable.Value;
				this.occupiedBorderDisplayAndHideHandler.Display();
				break;
			case UIInventorySlotView.States.InventorySlotOccupied:
				this.inventorySlotOccupiedBorderImage.sprite = this.inventorySlotOccupiedBorderSpriteVariable.Value;
				this.inventorySlotUnoccupiedBorderImage.sprite = this.inventorySlotUnoccupiedBorderSpriteVariable.Value;
				this.occupiedBorderDisplayAndHideHandler.Display();
				break;
			case UIInventorySlotView.States.InventorySlotUnoccupied:
				this.inventorySlotOccupiedBorderImage.sprite = this.inventorySlotUnoccupiedBorderSpriteVariable.Value;
				this.inventorySlotUnoccupiedBorderImage.sprite = this.inventorySlotUnoccupiedBorderSpriteVariable.Value;
				this.occupiedBorderDisplayAndHideHandler.Hide();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x06001831 RID: 6193 RVA: 0x000707E8 File Offset: 0x0006E9E8
		private void Despawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Despawn", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIInventorySlotView>(this);
		}

		// Token: 0x04001354 RID: 4948
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04001355 RID: 4949
		[SerializeField]
		private UIDisplayAndHideHandler occupiedBorderDisplayAndHideHandler;

		// Token: 0x04001356 RID: 4950
		[SerializeField]
		private UIDisplayAndHideHandler lockedDisplayAndHideHandler;

		// Token: 0x04001357 RID: 4951
		[Header("Equipment Type Coloring")]
		[SerializeField]
		private Graphic[] occupiedBorderGraphics = Array.Empty<Graphic>();

		// Token: 0x04001358 RID: 4952
		[SerializeField]
		private ColorVariable majorEquipmentColor;

		// Token: 0x04001359 RID: 4953
		[SerializeField]
		private ColorVariable minorEquipmentColor;

		// Token: 0x0400135A RID: 4954
		[Header("Drop Feedback")]
		[SerializeField]
		private TweenCollection validDropTweenCollection;

		// Token: 0x0400135B RID: 4955
		[SerializeField]
		private TweenCollection invalidDropTweenCollection;

		// Token: 0x0400135C RID: 4956
		[Header("Borders")]
		[SerializeField]
		private Image inventorySlotOccupiedBorderImage;

		// Token: 0x0400135D RID: 4957
		[SerializeField]
		private SpriteVariable inventorySlotOccupiedBorderSpriteVariable;

		// Token: 0x0400135E RID: 4958
		[SerializeField]
		private SpriteVariable equipmentSlotOccupiedBorderSpriteVariable;

		// Token: 0x0400135F RID: 4959
		[SerializeField]
		private Image inventorySlotUnoccupiedBorderImage;

		// Token: 0x04001360 RID: 4960
		[SerializeField]
		private SpriteVariable inventorySlotUnoccupiedBorderSpriteVariable;

		// Token: 0x04001361 RID: 4961
		[SerializeField]
		private SpriteVariable equipmentSlotUnoccupiedBorderSpriteVariable;

		// Token: 0x04001362 RID: 4962
		[Header("Hot Key")]
		[SerializeField]
		private TextMeshProUGUI hotKeyText;

		// Token: 0x04001363 RID: 4963
		[SerializeField]
		private UIDisplayAndHideHandler hotKeyDisplayAndHideHandler;

		// Token: 0x04001364 RID: 4964
		[SerializeField]
		private UIItemView item;

		// Token: 0x04001365 RID: 4965
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04001366 RID: 4966
		private bool isFirstDisplay = true;

		// Token: 0x04001367 RID: 4967
		private bool didTweenValidDropTweenCollection;

		// Token: 0x04001368 RID: 4968
		private bool waitFrameAndViewBorderOnEnable;

		// Token: 0x020003B2 RID: 946
		private enum States
		{
			// Token: 0x0400136D RID: 4973
			EquipmentSlotOccupied,
			// Token: 0x0400136E RID: 4974
			InventorySlotOccupied,
			// Token: 0x0400136F RID: 4975
			InventorySlotUnoccupied
		}
	}
}
