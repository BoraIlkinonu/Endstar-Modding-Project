using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003B5 RID: 949
	public class UIItemView : UIGameObject, IUIViewable<Item>, IClearable, IDragInstance
	{
		// Token: 0x170004FF RID: 1279
		// (get) Token: 0x0600183D RID: 6205 RVA: 0x00070AEE File Offset: 0x0006ECEE
		// (set) Token: 0x0600183E RID: 6206 RVA: 0x00070AF6 File Offset: 0x0006ECF6
		public Item Model { get; private set; }

		// Token: 0x17000500 RID: 1280
		// (get) Token: 0x0600183F RID: 6207 RVA: 0x00070AFF File Offset: 0x0006ECFF
		// (set) Token: 0x06001840 RID: 6208 RVA: 0x00070B07 File Offset: 0x0006ED07
		public bool IsEmpty { get; private set; } = true;

		// Token: 0x17000501 RID: 1281
		// (get) Token: 0x06001841 RID: 6209 RVA: 0x00070B10 File Offset: 0x0006ED10
		public UIDragHandler DragHandler
		{
			get
			{
				return this.dragInstanceHandler.DragHandler;
			}
		}

		// Token: 0x17000502 RID: 1282
		// (get) Token: 0x06001842 RID: 6210 RVA: 0x00070B1D File Offset: 0x0006ED1D
		public bool HasDragInstance
		{
			get
			{
				return this.instance != null;
			}
		}

		// Token: 0x06001843 RID: 6211 RVA: 0x00070B2C File Offset: 0x0006ED2C
		public void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dragInstanceHandler.OnInstantiateUnityEvent.AddListener(new UnityAction<GameObject>(this.OnDragInstanceInstantiated));
			this.dragInstanceHandler.DragHandler.DragUnityEvent.AddListener(new UnityAction(this.OnInstanceDragged));
			this.onDragEndTweenCollection.OnAllTweenCompleted.AddListener(new UnityAction(this.Destroy));
		}

		// Token: 0x06001844 RID: 6212 RVA: 0x00070BAA File Offset: 0x0006EDAA
		public void Initialize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			this.stackCountDisplayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x06001845 RID: 6213 RVA: 0x00070BD0 File Offset: 0x0006EDD0
		public void View(Item model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { JsonUtility.ToJson(model) });
			}
			if (model == null || model.AssetID.IsEmpty)
			{
				this.Clear();
				return;
			}
			bool flag = this.Model != model;
			if (flag)
			{
				this.onViewTweenCollection.Tween();
				RangedWeaponItem rangedWeaponItem = ((this.Model != null) ? (this.Model as RangedWeaponItem) : null);
				if (rangedWeaponItem != null)
				{
					rangedWeaponItem.RemoveAmmoCountHandler(new NetworkVariable<int>.OnValueChangedDelegate(this.OnAmmoCountChanged));
				}
			}
			this.Model = model;
			this.IsEmpty = false;
			RangedWeaponItem rangedWeaponItem2 = model as RangedWeaponItem;
			if (rangedWeaponItem2 != null)
			{
				int ammoCount = rangedWeaponItem2.AmmoCount;
				this.stackCountText.Value = StringUtility.AbbreviateQuantity(ammoCount);
				this.stackCountDisplayAndHideHandler.Display();
				this.stackCountContentSizeFitter.RequestLayout();
				if (flag)
				{
					rangedWeaponItem2.RegisterAmmoCountHandler(new NetworkVariable<int>.OnValueChangedDelegate(this.OnAmmoCountChanged));
				}
			}
			else
			{
				int stackCount = model.StackCount;
				this.stackCountText.Value = StringUtility.AbbreviateQuantity(stackCount);
				if (stackCount > 1)
				{
					this.stackCountDisplayAndHideHandler.Display();
					this.stackCountContentSizeFitter.RequestLayout();
				}
				else
				{
					this.stackCountDisplayAndHideHandler.Hide();
				}
			}
			InventoryUsableDefinition usableDefinitionFromItemAssetID = RuntimeDatabase.GetUsableDefinitionFromItemAssetID(model.AssetID);
			this.iconImage.sprite = usableDefinitionFromItemAssetID.Sprite;
			this.iconImage.gameObject.SetActive(true);
		}

		// Token: 0x06001846 RID: 6214 RVA: 0x00070D4A File Offset: 0x0006EF4A
		private void OnAmmoCountChanged(int previousValue, int newValue)
		{
			this.stackCountText.Value = StringUtility.AbbreviateQuantity(newValue);
		}

		// Token: 0x06001847 RID: 6215 RVA: 0x00070D60 File Offset: 0x0006EF60
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			RangedWeaponItem rangedWeaponItem = ((this.Model != null) ? (this.Model as RangedWeaponItem) : null);
			if (rangedWeaponItem != null)
			{
				rangedWeaponItem.RemoveAmmoCountHandler(new NetworkVariable<int>.OnValueChangedDelegate(this.OnAmmoCountChanged));
			}
			this.IsEmpty = true;
			this.Model = null;
			this.iconImage.sprite = null;
			this.iconImage.gameObject.SetActive(false);
			this.stackCountDisplayAndHideHandler.Hide();
		}

		// Token: 0x06001848 RID: 6216 RVA: 0x00070DF3 File Offset: 0x0006EFF3
		public void SetDisplayDelay(float delay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDisplayDelay", new object[] { delay });
			}
			this.stackCountDisplayAndHideHandler.SetDisplayDelay(delay);
		}

		// Token: 0x06001849 RID: 6217 RVA: 0x00070E23 File Offset: 0x0006F023
		public void OnDragStart()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDragStart", Array.Empty<object>());
			}
			this.onDragStartTweenCollection.Tween();
		}

		// Token: 0x0600184A RID: 6218 RVA: 0x00070E48 File Offset: 0x0006F048
		public void OnDragEnd()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDragEnd", Array.Empty<object>());
			}
			this.onDragEndTweenCollection.Tween();
			Action<UIItemView> dragEndAction = UIItemView.DragEndAction;
			if (dragEndAction == null)
			{
				return;
			}
			dragEndAction(this);
		}

		// Token: 0x0600184B RID: 6219 RVA: 0x00070E80 File Offset: 0x0006F080
		private void OnDragInstanceInstantiated(GameObject dragInstance)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDragInstanceInstantiated", new object[] { dragInstance.DebugSafeName(true) });
			}
			this.sourceItemDragStateDisplayAndHideHandler.Hide();
			if (!dragInstance.TryGetComponent<UIItemView>(out this.instance))
			{
				DebugUtility.LogError(this, "OnDragInstanceInstantiated", string.Format("Could not get {0} from {1}!", this.instance.GetType(), "instance"), new object[] { dragInstance.DebugSafeName(true) });
			}
			this.instance.View(this.Model);
			this.instance.OnDragStart();
			this.instance.DragHandler.EndDragUnityEvent.AddListener(new UnityAction(this.instance.OnDragEnd));
			this.instance.DragHandler.EndDragUnityEvent.AddListener(new UnityAction(this.FadeIn));
		}

		// Token: 0x0600184C RID: 6220 RVA: 0x00070F61 File Offset: 0x0006F161
		private void OnInstanceDragged()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInstanceDragged", Array.Empty<object>());
			}
			Action<UIItemView> instanceDraggedAction = UIItemView.InstanceDraggedAction;
			if (instanceDraggedAction == null)
			{
				return;
			}
			instanceDraggedAction(this);
		}

		// Token: 0x0600184D RID: 6221 RVA: 0x00070F8B File Offset: 0x0006F18B
		private void FadeIn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "FadeIn", Array.Empty<object>());
			}
			this.sourceItemDragStateDisplayAndHideHandler.Display();
		}

		// Token: 0x0600184E RID: 6222 RVA: 0x00070FB0 File Offset: 0x0006F1B0
		private void Destroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Destroy", Array.Empty<object>());
			}
			MonoBehaviourSingleton<TweenManager>.Instance.CancelAllTweens(base.gameObject);
			global::UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x04001377 RID: 4983
		public static Action<UIItemView> InstanceDraggedAction;

		// Token: 0x04001378 RID: 4984
		public static Action<UIItemView> DragEndAction;

		// Token: 0x04001379 RID: 4985
		[SerializeField]
		private UIDisplayAndHideHandler sourceItemDragStateDisplayAndHideHandler;

		// Token: 0x0400137A RID: 4986
		[SerializeField]
		private Image iconImage;

		// Token: 0x0400137B RID: 4987
		[SerializeField]
		private UIDisplayAndHideHandler stackCountDisplayAndHideHandler;

		// Token: 0x0400137C RID: 4988
		[SerializeField]
		private UIText stackCountText;

		// Token: 0x0400137D RID: 4989
		[SerializeField]
		private UIContentSizeFitter stackCountContentSizeFitter;

		// Token: 0x0400137E RID: 4990
		[SerializeField]
		private UIDragInstanceHandler dragInstanceHandler;

		// Token: 0x0400137F RID: 4991
		[SerializeField]
		private TweenCollection onDragStartTweenCollection;

		// Token: 0x04001380 RID: 4992
		[SerializeField]
		private TweenCollection onDragEndTweenCollection;

		// Token: 0x04001381 RID: 4993
		[SerializeField]
		private TweenCollection onViewTweenCollection;

		// Token: 0x04001382 RID: 4994
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04001383 RID: 4995
		private UIItemView instance;
	}
}
