using System;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000356 RID: 854
	public class ResizableVolume : MonoBehaviour, IComponentBase
	{
		// Token: 0x17000477 RID: 1143
		// (get) Token: 0x06001570 RID: 5488 RVA: 0x00066289 File Offset: 0x00064489
		// (set) Token: 0x06001571 RID: 5489 RVA: 0x00066294 File Offset: 0x00064494
		public global::UnityEngine.Vector3 Offset
		{
			get
			{
				return this.offset;
			}
			set
			{
				value.x = Mathf.Clamp(value.x, -1f, 1f);
				value.y = Mathf.Clamp(value.y, -1f, 1f);
				value.z = Mathf.Clamp(value.z, -1f, 1f);
				this.offset = value;
				this.RepositionVolume();
			}
		}

		// Token: 0x17000478 RID: 1144
		// (get) Token: 0x06001572 RID: 5490 RVA: 0x00066302 File Offset: 0x00064502
		// (set) Token: 0x06001573 RID: 5491 RVA: 0x0006630A File Offset: 0x0006450A
		public int Forward
		{
			get
			{
				return this.forward;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.forward = value;
				this.RecalculateZAxis();
			}
		}

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x06001574 RID: 5492 RVA: 0x00066322 File Offset: 0x00064522
		// (set) Token: 0x06001575 RID: 5493 RVA: 0x0006632A File Offset: 0x0006452A
		public int Backward
		{
			get
			{
				return this.backward;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.backward = value;
				this.RecalculateZAxis();
			}
		}

		// Token: 0x1700047A RID: 1146
		// (get) Token: 0x06001576 RID: 5494 RVA: 0x00066342 File Offset: 0x00064542
		// (set) Token: 0x06001577 RID: 5495 RVA: 0x0006634A File Offset: 0x0006454A
		public int Left
		{
			get
			{
				return this.left;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.left = value;
				this.RecalculateXAxis();
			}
		}

		// Token: 0x1700047B RID: 1147
		// (get) Token: 0x06001578 RID: 5496 RVA: 0x00066362 File Offset: 0x00064562
		// (set) Token: 0x06001579 RID: 5497 RVA: 0x0006636A File Offset: 0x0006456A
		public int Right
		{
			get
			{
				return this.right;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.right = value;
				this.RecalculateXAxis();
			}
		}

		// Token: 0x1700047C RID: 1148
		// (get) Token: 0x0600157A RID: 5498 RVA: 0x00066382 File Offset: 0x00064582
		// (set) Token: 0x0600157B RID: 5499 RVA: 0x0006638A File Offset: 0x0006458A
		public int Up
		{
			get
			{
				return this.up;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.up = value;
				this.RecalculateYAxis();
			}
		}

		// Token: 0x1700047D RID: 1149
		// (get) Token: 0x0600157C RID: 5500 RVA: 0x000663A2 File Offset: 0x000645A2
		// (set) Token: 0x0600157D RID: 5501 RVA: 0x000663AA File Offset: 0x000645AA
		public int Down
		{
			get
			{
				return this.down;
			}
			set
			{
				value = Mathf.Max(value, 0);
				this.down = value;
				this.RecalculateYAxis();
			}
		}

		// Token: 0x0600157E RID: 5502 RVA: 0x000663C4 File Offset: 0x000645C4
		private void RecalculateYAxis()
		{
			int num = this.up + this.down + 1;
			float num2 = (float)(this.up - this.down) / 2f;
			this.scaleTransform.localScale = new global::UnityEngine.Vector3(this.scaleTransform.localScale.x, (float)num, this.scaleTransform.localScale.z);
			this.scaleTransform.localPosition = new global::UnityEngine.Vector3(this.scaleTransform.localPosition.x, num2, this.scaleTransform.localPosition.z);
		}

		// Token: 0x0600157F RID: 5503 RVA: 0x0006645C File Offset: 0x0006465C
		private void RecalculateXAxis()
		{
			int num = this.right + this.left + 1;
			float num2 = (float)(this.right - this.left) / 2f;
			this.scaleTransform.localScale = new global::UnityEngine.Vector3((float)num, this.scaleTransform.localScale.y, this.scaleTransform.localScale.z);
			this.scaleTransform.localPosition = new global::UnityEngine.Vector3(num2, this.scaleTransform.localPosition.y, this.scaleTransform.localPosition.z);
		}

		// Token: 0x06001580 RID: 5504 RVA: 0x000664F4 File Offset: 0x000646F4
		private void RecalculateZAxis()
		{
			int num = this.forward + this.backward + 1;
			float num2 = (float)(this.forward - this.backward) / 2f;
			this.scaleTransform.localScale = new global::UnityEngine.Vector3(this.scaleTransform.localScale.x, this.scaleTransform.localScale.y, (float)num);
			this.scaleTransform.localPosition = new global::UnityEngine.Vector3(this.scaleTransform.localPosition.x, this.scaleTransform.localPosition.y, num2);
		}

		// Token: 0x06001581 RID: 5505 RVA: 0x00066589 File Offset: 0x00064789
		private void RepositionVolume()
		{
			this.offsetTransform.localPosition = this.offset;
		}

		// Token: 0x06001582 RID: 5506 RVA: 0x0006659C File Offset: 0x0006479C
		protected virtual void Awake()
		{
			this.endlessProp.OnInspectionStateChanged.AddListener(new UnityAction<bool>(this.HandleInspectionStateChanged));
		}

		// Token: 0x06001583 RID: 5507 RVA: 0x000665BA File Offset: 0x000647BA
		private void HandleInspectionStateChanged(bool isInspected)
		{
			this.volumeVisuals.enabled = isInspected;
		}

		// Token: 0x1700047E RID: 1150
		// (get) Token: 0x06001584 RID: 5508 RVA: 0x000665C8 File Offset: 0x000647C8
		// (set) Token: 0x06001585 RID: 5509 RVA: 0x000665D0 File Offset: 0x000647D0
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700047F RID: 1151
		// (get) Token: 0x06001586 RID: 5510 RVA: 0x000665D9 File Offset: 0x000647D9
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(ResizableVolumeReferences);
			}
		}

		// Token: 0x06001587 RID: 5511 RVA: 0x000665E5 File Offset: 0x000647E5
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001588 RID: 5512 RVA: 0x000665F0 File Offset: 0x000647F0
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.endlessProp = endlessProp;
			BoxCollider[] collidersToScale = (referenceBase as ResizableVolumeReferences).CollidersToScale;
			for (int i = 0; i < collidersToScale.Length; i++)
			{
				collidersToScale[i].transform.SetParent(this.scaleTransform);
			}
		}

		// Token: 0x0400118A RID: 4490
		[SerializeField]
		private Transform scaleTransform;

		// Token: 0x0400118B RID: 4491
		[SerializeField]
		private Transform offsetTransform;

		// Token: 0x0400118C RID: 4492
		[SerializeField]
		private Renderer volumeVisuals;

		// Token: 0x0400118D RID: 4493
		private global::UnityEngine.Vector3 offset;

		// Token: 0x0400118E RID: 4494
		private int forward;

		// Token: 0x0400118F RID: 4495
		private int backward;

		// Token: 0x04001190 RID: 4496
		private int left;

		// Token: 0x04001191 RID: 4497
		private int right;

		// Token: 0x04001192 RID: 4498
		private int up;

		// Token: 0x04001193 RID: 4499
		private int down;

		// Token: 0x04001195 RID: 4501
		[SerializeField]
		[HideInInspector]
		private EndlessProp endlessProp;
	}
}
