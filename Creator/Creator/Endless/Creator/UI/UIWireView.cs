using System;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Creator.UI
{
	// Token: 0x02000303 RID: 771
	[RequireComponent(typeof(UILineRenderer))]
	public class UIWireView : UIGameObject, IPoolableT
	{
		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000D6F RID: 3439 RVA: 0x00040656 File Offset: 0x0003E856
		// (set) Token: 0x06000D70 RID: 3440 RVA: 0x0004065E File Offset: 0x0003E85E
		public SerializableGuid WireId { get; private set; } = SerializableGuid.Empty;

		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06000D71 RID: 3441 RVA: 0x00040667 File Offset: 0x0003E867
		// (set) Token: 0x06000D72 RID: 3442 RVA: 0x0004066F File Offset: 0x0003E86F
		public UIWireNodeView EmitterNode { get; private set; }

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x06000D73 RID: 3443 RVA: 0x00040678 File Offset: 0x0003E878
		// (set) Token: 0x06000D74 RID: 3444 RVA: 0x00040680 File Offset: 0x0003E880
		public UIWireNodeView ReceiverNode { get; private set; }

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x06000D75 RID: 3445 RVA: 0x00040689 File Offset: 0x0003E889
		// (set) Token: 0x06000D76 RID: 3446 RVA: 0x00040691 File Offset: 0x0003E891
		public WireColor CurrentWireColor { get; private set; }

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x06000D77 RID: 3447 RVA: 0x0004069A File Offset: 0x0003E89A
		public UILineRenderer LineRenderer
		{
			get
			{
				if (!this.lineRenderer)
				{
					base.TryGetComponent<UILineRenderer>(out this.lineRenderer);
				}
				return this.lineRenderer;
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000D78 RID: 3448 RVA: 0x000406BC File Offset: 0x0003E8BC
		// (set) Token: 0x06000D79 RID: 3449 RVA: 0x000406C4 File Offset: 0x0003E8C4
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000D7A RID: 3450 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000D7B RID: 3451 RVA: 0x000406CD File Offset: 0x0003E8CD
		private void Update()
		{
			this.UpdateLineRendererPoints();
		}

		// Token: 0x06000D7C RID: 3452 RVA: 0x000406D5 File Offset: 0x0003E8D5
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x000406EF File Offset: 0x0003E8EF
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			this.SetColor(WireColor.NoColor);
			this.lightenTweens.SetToEnd();
			base.enabled = false;
		}

		// Token: 0x06000D7E RID: 3454 RVA: 0x00040724 File Offset: 0x0003E924
		public void Initialize(SerializableGuid wireId, UIWireNodeView emitterNode, UIWireNodeView receiverNode, bool flowLeftToRight)
		{
			if (this.verboseLogging)
			{
				if (emitterNode)
				{
					string memberName = emitterNode.MemberName;
				}
				if (receiverNode)
				{
					string memberName2 = receiverNode.MemberName;
				}
				DebugUtility.LogMethod(this, "Initialize", new object[]
				{
					wireId,
					emitterNode.DebugSafeName(true),
					receiverNode.DebugSafeName(true),
					flowLeftToRight
				});
			}
			this.WireId = wireId;
			this.EmitterNode = emitterNode;
			this.ReceiverNode = receiverNode;
			this.flowLeftToRight = flowLeftToRight;
			WireEntry wireEntry = WiringUtilities.GetWireEntry(emitterNode.InspectedObjectId, emitterNode.MemberName, receiverNode.InspectedObjectId, receiverNode.MemberName);
			if (wireEntry != null)
			{
				this.SetColor((WireColor)wireEntry.WireColor);
			}
			if (!this.container)
			{
				base.RectTransform.parent.TryGetComponent<RectTransform>(out this.container);
			}
			this.UpdateLineRendererPoints();
		}

		// Token: 0x06000D7F RID: 3455 RVA: 0x00040803 File Offset: 0x0003EA03
		public void Darken()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Darken", Array.Empty<object>());
			}
			this.darkenTweens.Tween();
		}

		// Token: 0x06000D80 RID: 3456 RVA: 0x00040828 File Offset: 0x0003EA28
		public void Lighten()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Lighten", Array.Empty<object>());
			}
			this.lightenTweens.Tween();
		}

		// Token: 0x06000D81 RID: 3457 RVA: 0x00040850 File Offset: 0x0003EA50
		public void SetColor(WireColor color)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetColor", new object[] { color });
			}
			this.LineRenderer.material = this.wireColorDictionary[color].UIMaterial;
			this.CurrentWireColor = color;
		}

		// Token: 0x06000D82 RID: 3458 RVA: 0x000408A4 File Offset: 0x0003EAA4
		public void UpdateLineRendererPoints()
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateLineRendererPoints", Array.Empty<object>());
			}
			Vector2 vector = new Vector2(-100f, 0f);
			Vector2 vector2 = new Vector2(100f, 0f);
			if (this.EmitterNode)
			{
				vector = this.container.InverseTransformPoint(this.EmitterNode.WirePoint);
			}
			if (this.ReceiverNode)
			{
				vector2 = this.container.InverseTransformPoint(this.ReceiverNode.WirePoint);
			}
			else
			{
				vector2 = vector + (this.flowLeftToRight ? new Vector2(100f, 0f) : new Vector2(-100f, 0f));
			}
			if (!this.EmitterNode)
			{
				vector = vector2 + (this.flowLeftToRight ? new Vector2(100f, 0f) : new Vector2(-100f, 0f));
			}
			Vector2[] array = new Vector2[] { vector, vector2 };
			this.LineRenderer.SetPoints(array);
			this.LineRenderer.SetTiling((this.EmitterNode && this.ReceiverNode) ? this.tilingIfConnected : this.tilingIfNotConnected);
		}

		// Token: 0x04000B94 RID: 2964
		[SerializeField]
		private int tilingIfNotConnected = 5;

		// Token: 0x04000B95 RID: 2965
		[SerializeField]
		private int tilingIfConnected = 25;

		// Token: 0x04000B96 RID: 2966
		[FormerlySerializedAs("darkenTweems")]
		[SerializeField]
		private TweenCollection darkenTweens;

		// Token: 0x04000B97 RID: 2967
		[SerializeField]
		private TweenCollection lightenTweens;

		// Token: 0x04000B98 RID: 2968
		[SerializeField]
		private WireColorDictionary wireColorDictionary;

		// Token: 0x04000B99 RID: 2969
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000B9A RID: 2970
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000B9B RID: 2971
		private RectTransform container;

		// Token: 0x04000B9C RID: 2972
		private bool flowLeftToRight = true;

		// Token: 0x04000B9D RID: 2973
		private UILineRenderer lineRenderer;
	}
}
