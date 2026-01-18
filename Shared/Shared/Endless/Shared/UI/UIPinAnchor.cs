using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI.Anchors;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000108 RID: 264
	public class UIPinAnchor : MonoBehaviour, IUIAnchor, IPoolableT
	{
		// Token: 0x14000031 RID: 49
		// (add) Token: 0x06000659 RID: 1625 RVA: 0x0001B574 File Offset: 0x00019774
		// (remove) Token: 0x0600065A RID: 1626 RVA: 0x0001B5AC File Offset: 0x000197AC
		public event Action OnClosed;

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x0600065B RID: 1627 RVA: 0x0001B5E1 File Offset: 0x000197E1
		// (set) Token: 0x0600065C RID: 1628 RVA: 0x0001B5E9 File Offset: 0x000197E9
		public Transform Target { get; set; }

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x0600065D RID: 1629 RVA: 0x0001B5F2 File Offset: 0x000197F2
		// (set) Token: 0x0600065E RID: 1630 RVA: 0x0001B5FA File Offset: 0x000197FA
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x0600065F RID: 1631 RVA: 0x0001B604 File Offset: 0x00019804
		public static UIPinAnchor CreateInstance(UIPinAnchor prefab, Transform target, RectTransform container, Color? color = null, Vector3? offset = null)
		{
			UIPinAnchor uipinAnchor = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIPinAnchor>(prefab, default(Vector3), default(Quaternion), null);
			uipinAnchor.transform.SetParent(container, false);
			uipinAnchor.SetTarget(target);
			uipinAnchor.SetColor(color);
			uipinAnchor.SetOffset(offset);
			uipinAnchor.PlayDisplayTween();
			MonoBehaviourSingleton<UIAnchorManager>.Instance.Register(uipinAnchor);
			return uipinAnchor;
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0001B668 File Offset: 0x00019868
		public void SetTarget(Transform target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetTarget", new object[] { target.DebugSafeName(true) });
			}
			if (target == null)
			{
				Debug.LogException(new NullReferenceException("Target cannot be null"));
				this.Close();
				return;
			}
			this.Target = target;
		}

		// Token: 0x06000661 RID: 1633 RVA: 0x0001B6C0 File Offset: 0x000198C0
		public void SetColor(Color? color)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetColor", new object[] { color });
			}
			this.iconImage.color = color ?? Color.white;
		}

		// Token: 0x06000662 RID: 1634 RVA: 0x0001B714 File Offset: 0x00019914
		public void SetOffset(Vector3? offset)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOffset", new object[] { offset });
			}
			this.positioner.Offset = offset ?? Vector3.zero;
		}

		// Token: 0x06000663 RID: 1635 RVA: 0x0001B768 File Offset: 0x00019968
		public void UpdatePosition()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdatePosition", Array.Empty<object>());
			}
			if (!this.Target)
			{
				this.Close();
				return;
			}
			Vector3 vector = this.positioner.GetScreenPosition(this.Target);
			Vector3 vector2;
			Vector3 vector3;
			this.offScreenHandler.GetScreenCenterAndBoundsWithPadding(out vector2, out vector3);
			vector = this.offScreenHandler.ProcessOffScreenPosition(vector, vector2, vector3);
			this.positioner.SetScreenPosition(vector);
		}

		// Token: 0x06000664 RID: 1636 RVA: 0x0001B7E0 File Offset: 0x000199E0
		public void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			if (this.displayAndHideHandler.IsTweeningHide)
			{
				return;
			}
			MonoBehaviourSingleton<UIAnchorManager>.Instance.UnregisterAnchor(this);
			this.displayAndHideHandler.Hide(new Action(this.Despawn));
			Action onClosed = this.OnClosed;
			if (onClosed == null)
			{
				return;
			}
			onClosed();
		}

		// Token: 0x06000665 RID: 1637 RVA: 0x0001B845 File Offset: 0x00019A45
		public void Highlight()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Highlight", Array.Empty<object>());
			}
			this.highlightTweenCollection.Tween();
		}

		// Token: 0x06000666 RID: 1638 RVA: 0x0001B86A File Offset: 0x00019A6A
		public void PlayDisplayTween()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "PlayDisplayTween", Array.Empty<object>());
			}
			this.displayAndHideHandler.Display();
		}

		// Token: 0x06000667 RID: 1639 RVA: 0x0001B88F File Offset: 0x00019A8F
		private void Despawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Despawn", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIPinAnchor>(this);
		}

		// Token: 0x0400039C RID: 924
		[SerializeField]
		private Image iconImage;

		// Token: 0x0400039D RID: 925
		[SerializeField]
		private TweenCollection highlightTweenCollection;

		// Token: 0x0400039E RID: 926
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x0400039F RID: 927
		[SerializeField]
		private UIAnchorPositioner positioner;

		// Token: 0x040003A0 RID: 928
		[SerializeField]
		private UIAnchorOffScreenHandler offScreenHandler;

		// Token: 0x040003A1 RID: 929
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
