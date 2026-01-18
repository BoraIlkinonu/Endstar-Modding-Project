using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001FF RID: 511
	public class UIIEnumerableView : UIBaseIEnumerableView
	{
		// Token: 0x06000D57 RID: 3415 RVA: 0x0003A9F4 File Offset: 0x00038BF4
		public override void View(IEnumerable model)
		{
			base.View(model);
			int num = this.GetCount();
			for (int i = 0; i < num; i++)
			{
				object obj = this.GetItem(i);
				UIIEnumerableItem uiienumerableItem = this.SpawnItem(obj, i, i, this.scrollRect.content);
				if (!uiienumerableItem)
				{
					DebugUtility.LogException(new InvalidCastException("Could not cast iEnumerableItemSource to UIIEnumerableItem!"), this);
				}
				else
				{
					this.iEnumerableItems.Add(uiienumerableItem);
				}
			}
			if (base.IsReview)
			{
				this.scrollRect.normalizedPosition = base.CachedNormalizedPosition;
			}
		}

		// Token: 0x06000D58 RID: 3416 RVA: 0x0003AA84 File Offset: 0x00038C84
		public override void ReviewSelectedStatus()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ReviewSelectedStatus", Array.Empty<object>());
			}
			foreach (UIIEnumerableItem uiienumerableItem in this.iEnumerableItems)
			{
				bool flag = this.GetIsSelected(uiienumerableItem.ModelAsObject);
				uiienumerableItem.ViewSelectedStatus(this, flag);
			}
		}

		// Token: 0x06000D59 RID: 3417 RVA: 0x0003AB04 File Offset: 0x00038D04
		public override void SetInteractable(bool interactable)
		{
			base.SetInteractable(interactable);
			foreach (UIIEnumerableItem uiienumerableItem in this.iEnumerableItems)
			{
				uiienumerableItem.SetInteractable(interactable);
			}
		}

		// Token: 0x06000D5A RID: 3418 RVA: 0x0003AB5C File Offset: 0x00038D5C
		public override void SetChildIEnumerableCanAddAndRemoveItems(bool canAddAndRemoveItems)
		{
			base.SetChildIEnumerableCanAddAndRemoveItems(canAddAndRemoveItems);
			foreach (UIIEnumerableItem uiienumerableItem in this.iEnumerableItems)
			{
				uiienumerableItem.OnChildIEnumerableCanAddAndRemoveItemsChanged(canAddAndRemoveItems);
			}
		}

		// Token: 0x06000D5B RID: 3419 RVA: 0x0003ABB4 File Offset: 0x00038DB4
		protected override void ResetViewState()
		{
			base.ResetViewState();
			foreach (UIIEnumerableItem uiienumerableItem in this.iEnumerableItems)
			{
				this.DespawnItem(uiienumerableItem);
			}
			this.iEnumerableItems.Clear();
		}

		// Token: 0x06000D5C RID: 3420 RVA: 0x0003AC18 File Offset: 0x00038E18
		public void JumpToDataIndex(int dataIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "JumpToDataIndex", new object[] { dataIndex });
			}
			if (dataIndex < 0 || dataIndex >= this.GetCount())
			{
				DebugUtility.LogWarning(string.Format("Invalid dataIndex: {0}. Must be between 0 and {1}", dataIndex, this.GetCount() - 1), this);
				return;
			}
			Canvas.ForceUpdateCanvases();
			UIGameObject uigameObject = this.iEnumerableItems[dataIndex];
			this.itemCorners = new Vector3[4];
			uigameObject.RectTransform.GetWorldCorners(this.itemCorners);
			this.contentCorners = new Vector3[4];
			this.scrollRect.content.GetWorldCorners(this.contentCorners);
			if (base.Vertical)
			{
				float height = this.scrollRect.content.rect.height;
				float height2 = this.scrollRect.viewport.rect.height;
				if (height > height2)
				{
					float num = this.contentCorners[1].y - this.itemCorners[1].y;
					float num2 = 1f - num / (height - height2);
					this.scrollRect.verticalNormalizedPosition = Mathf.Clamp01(num2);
					return;
				}
			}
			else
			{
				float width = this.scrollRect.content.rect.width;
				float width2 = this.scrollRect.viewport.rect.width;
				if (width > width2)
				{
					float num3 = (this.itemCorners[0].x - this.contentCorners[0].x) / (width - width2);
					this.scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(num3);
				}
			}
		}

		// Token: 0x06000D5D RID: 3421 RVA: 0x0003ADCD File Offset: 0x00038FCD
		protected override void OnScroll(Vector2 scrollRectValue)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScroll", Array.Empty<object>());
			}
			base.ProcessItemVisibility(this.iEnumerableItems, this.scrollRect.LastScrollDirection);
		}

		// Token: 0x040008AF RID: 2223
		private readonly List<UIIEnumerableItem> iEnumerableItems = new List<UIIEnumerableItem>();

		// Token: 0x040008B0 RID: 2224
		private Vector3[] itemCorners;

		// Token: 0x040008B1 RID: 2225
		private Vector3[] contentCorners;
	}
}
