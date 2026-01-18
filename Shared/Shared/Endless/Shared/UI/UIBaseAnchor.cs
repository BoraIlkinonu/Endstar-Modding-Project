using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI.Anchors;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000107 RID: 263
	public abstract class UIBaseAnchor : MonoBehaviour, IUIAnchor, IPoolableT
	{
		// Token: 0x17000107 RID: 263
		// (get) Token: 0x0600064C RID: 1612 RVA: 0x0001B33D File Offset: 0x0001953D
		// (set) Token: 0x0600064D RID: 1613 RVA: 0x0001B345 File Offset: 0x00019545
		public Transform Target { get; set; }

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x0600064E RID: 1614 RVA: 0x0001B34E File Offset: 0x0001954E
		// (set) Token: 0x0600064F RID: 1615 RVA: 0x0001B356 File Offset: 0x00019556
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x06000650 RID: 1616 RVA: 0x0001B360 File Offset: 0x00019560
		protected static T CreateAndInitialize<T>(T prefab, Transform target, RectTransform container, Vector3? offset = null) where T : UIBaseAnchor
		{
			T t = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<T>(prefab, default(Vector3), default(Quaternion), null);
			t.displayAndHideHandler.SetToDisplayStart(false);
			t.transform.SetParent(container, false);
			t.SetTarget(target);
			t.SetOffset(offset);
			t.PlayDisplayTween();
			MonoBehaviourSingleton<UIAnchorManager>.Instance.Register(t);
			return t;
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x0001B3E4 File Offset: 0x000195E4
		public void SetTarget(Transform target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("SetTarget ( target: " + target.DebugSafeName(true) + " )", this);
			}
			if (target == null)
			{
				Debug.LogException(new NullReferenceException("Target cannot be null"));
				this.Close();
				return;
			}
			this.Target = target;
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x0001B43C File Offset: 0x0001963C
		public void SetOffset(Vector3? offset)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetOffset", "offset", offset), this);
			}
			this.positioner.Offset = offset ?? Vector3.zero;
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x0001B495 File Offset: 0x00019695
		public virtual void UpdatePosition()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("UpdatePosition", this);
			}
			if (!this.Target)
			{
				this.Close();
				return;
			}
			this.positioner.UpdatePosition(this.Target);
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x0001B4D0 File Offset: 0x000196D0
		public virtual void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Close", this);
			}
			if (this.displayAndHideHandler.IsTweeningHide)
			{
				return;
			}
			this.UnregisterAnchorAndHide();
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x0001B4F9 File Offset: 0x000196F9
		protected virtual void UnregisterAnchorAndHide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("UnregisterAnchorAndHide", this);
			}
			MonoBehaviourSingleton<UIAnchorManager>.Instance.UnregisterAnchor(this);
			this.displayAndHideHandler.Hide(new Action(this.Despawn));
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x0001B531 File Offset: 0x00019731
		public void PlayDisplayTween()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("PlayDisplayTween", this);
			}
			this.displayAndHideHandler.Display();
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x0001B551 File Offset: 0x00019751
		protected virtual void Despawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Despawn", this);
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIBaseAnchor>(this);
		}

		// Token: 0x04000396 RID: 918
		[SerializeField]
		protected UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04000397 RID: 919
		[SerializeField]
		protected UIAnchorPositioner positioner;

		// Token: 0x04000398 RID: 920
		[Header("Debugging")]
		[SerializeField]
		protected bool verboseLogging;
	}
}
