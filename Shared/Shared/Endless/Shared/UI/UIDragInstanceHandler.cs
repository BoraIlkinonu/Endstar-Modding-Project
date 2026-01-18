using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200012E RID: 302
	public class UIDragInstanceHandler : UIGameObject, IValidatable
	{
		// Token: 0x1700013E RID: 318
		// (get) Token: 0x0600075F RID: 1887 RVA: 0x0001EF91 File Offset: 0x0001D191
		// (set) Token: 0x06000760 RID: 1888 RVA: 0x0001EF99 File Offset: 0x0001D199
		public UIDragHandler DragHandler { get; private set; }

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000761 RID: 1889 RVA: 0x0001EFA2 File Offset: 0x0001D1A2
		public UnityEvent<RectTransform> InstanceBeginDragUnityEvent { get; } = new UnityEvent<RectTransform>();

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000762 RID: 1890 RVA: 0x0001EFAA File Offset: 0x0001D1AA
		public UnityEvent<RectTransform> InstanceEndDragUnityEvent { get; } = new UnityEvent<RectTransform>();

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000763 RID: 1891 RVA: 0x0001EFB2 File Offset: 0x0001D1B2
		public UnityEvent InstancePositionResetTweenCompleteUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000764 RID: 1892 RVA: 0x0001EFBA File Offset: 0x0001D1BA
		public UnityEvent<GameObject> OnInstantiateUnityEvent { get; } = new UnityEvent<GameObject>();

		// Token: 0x06000765 RID: 1893 RVA: 0x0001EFC4 File Offset: 0x0001D1C4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!this.instanceSource)
			{
				this.instanceSource = base.gameObject;
			}
			this.DragHandler.BeginDragUnityEvent.AddListener(new UnityAction(this.OnBeginDrag));
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x0001F01E File Offset: 0x0001D21E
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugIsNull("interactionBlockerSource", this.interactionBlockerSource, this);
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x0001F04C File Offset: 0x0001D24C
		private void OnBeginDrag()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBeginDrag", Array.Empty<object>());
			}
			if (this.instance)
			{
				if (this.verboseLogging)
				{
					DebugUtility.LogMethodWithAppension(this, "OnBeginDrag", "returning due to instance already existing", Array.Empty<object>());
				}
				return;
			}
			Canvas canvas = ((this.parentCanvasLevels <= -1) ? base.gameObject.transform.GetTopMostComponentInParent<Canvas>() : base.gameObject.GetComponentInParent<Canvas>());
			int i = this.parentCanvasLevels - 1;
			while (i > 0)
			{
				i--;
				canvas = canvas.GetComponentInParent<Canvas>();
			}
			this.instance = global::UnityEngine.Object.Instantiate<GameObject>(this.instanceSource, base.transform.parent);
			this.instance.transform.position = base.transform.position;
			this.instance.transform.SetParent(canvas.transform);
			UIDragInterceptAndPassToParentScrollRect uidragInterceptAndPassToParentScrollRect;
			if (MobileUtility.IsMobile && this.instance.TryGetComponent<UIDragInterceptAndPassToParentScrollRect>(out uidragInterceptAndPassToParentScrollRect))
			{
				global::UnityEngine.Object.Destroy(uidragInterceptAndPassToParentScrollRect);
			}
			if (!this.instance.TryGetComponent<UIDragHandler>(out this.instanceDragHandler))
			{
				DebugUtility.LogError("Could not get instanceDragHandler from instance!", this);
				return;
			}
			UIDragPositionController uidragPositionController;
			if (!this.instance.TryGetComponent<UIDragPositionController>(out uidragPositionController))
			{
				DebugUtility.LogError("Could not get dragPositionController from dragPositionController!", this);
				return;
			}
			uidragPositionController.enabled = true;
			this.instanceDragHandler.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, base.RectTransform.rect.width);
			this.instanceDragHandler.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, base.RectTransform.rect.height);
			this.instanceDragHandler.EndDragUnityEvent.AddListener(new UnityAction(this.OnInstanceEndDrag));
			this.DragHandler.SetIntercepter(this.instanceDragHandler);
			if (!this.addDragPositionResetterToInstance)
			{
				this.OnInstantiateUnityEvent.Invoke(this.instance);
				return;
			}
			UIDragPositionResetter uidragPositionResetter = this.instance.AddComponent<UIDragPositionResetter>();
			uidragPositionResetter.SetInteractionBlockerSource(this.interactionBlockerSource);
			uidragPositionResetter.OnDragStarted();
			uidragPositionResetter.TweenPosition.OnTweenComplete.AddListener(new UnityAction(this.OnInstancePositionResetTweenComplete));
			PoolManagerT poolManagerT = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIPooledInteractionBlocker uipooledInteractionBlocker = this.interactionBlockerSource;
			Transform parent = base.transform.parent;
			this.spawnedInteractionBlocker = poolManagerT.Spawn<UIPooledInteractionBlocker>(uipooledInteractionBlocker, default(Vector3), default(Quaternion), parent);
			this.instance.AddComponent<UIListCellInstanceUpdateHandler>();
			Action<RectTransform> instanceBeginDragAction = UIDragInstanceHandler.InstanceBeginDragAction;
			if (instanceBeginDragAction != null)
			{
				instanceBeginDragAction(this.instanceDragHandler.RectTransform);
			}
			this.InstanceBeginDragUnityEvent.Invoke(this.instanceDragHandler.RectTransform);
			this.OnInstantiateUnityEvent.Invoke(this.instance);
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x0001F2D8 File Offset: 0x0001D4D8
		private void OnInstanceEndDrag()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInstanceEndDrag", Array.Empty<object>());
			}
			Action<RectTransform> instanceEndDragAction = UIDragInstanceHandler.InstanceEndDragAction;
			if (instanceEndDragAction != null)
			{
				instanceEndDragAction(this.instanceDragHandler.RectTransform);
			}
			this.InstanceEndDragUnityEvent.Invoke(this.instanceDragHandler.RectTransform);
			this.instanceDragHandler.EndDragUnityEvent.RemoveListener(new UnityAction(this.OnInstanceEndDrag));
			this.DragHandler.SetIntercepter(null);
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x0001F358 File Offset: 0x0001D558
		private void OnInstancePositionResetTweenComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInstancePositionResetTweenComplete", Array.Empty<object>());
			}
			this.InstancePositionResetTweenCompleteUnityEvent.Invoke();
			global::UnityEngine.Object.Destroy(this.instance);
			if (this.spawnedInteractionBlocker == null)
			{
				return;
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIPooledInteractionBlocker>(this.spawnedInteractionBlocker);
			this.spawnedInteractionBlocker = null;
		}

		// Token: 0x0400044E RID: 1102
		public static Action<RectTransform> InstanceBeginDragAction;

		// Token: 0x0400044F RID: 1103
		public static Action<RectTransform> InstanceEndDragAction;

		// Token: 0x04000451 RID: 1105
		[SerializeField]
		private GameObject instanceSource;

		// Token: 0x04000452 RID: 1106
		[Min(-1f)]
		[SerializeField]
		private int parentCanvasLevels = -1;

		// Token: 0x04000453 RID: 1107
		[SerializeField]
		private bool addDragPositionResetterToInstance = true;

		// Token: 0x04000454 RID: 1108
		[SerializeField]
		private UIPooledInteractionBlocker interactionBlockerSource;

		// Token: 0x04000455 RID: 1109
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000456 RID: 1110
		private GameObject instance;

		// Token: 0x04000457 RID: 1111
		private UIDragHandler instanceDragHandler;

		// Token: 0x04000458 RID: 1112
		private UIPooledInteractionBlocker spawnedInteractionBlocker;
	}
}
