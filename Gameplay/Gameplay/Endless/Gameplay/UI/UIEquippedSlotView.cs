using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003A1 RID: 929
	public class UIEquippedSlotView : UIGameObject
	{
		// Token: 0x060017A6 RID: 6054 RVA: 0x0006DE1E File Offset: 0x0006C01E
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIItemView.DragEndAction = (Action<UIItemView>)Delegate.Combine(UIItemView.DragEndAction, new Action<UIItemView>(this.OnItemDragEnd));
		}

		// Token: 0x060017A7 RID: 6055 RVA: 0x0006DE58 File Offset: 0x0006C058
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.occupiedBorderDisplayAndHideHandler.SetToHideEnd(true);
			this.model.OnChanged.AddListener(new UnityAction(this.View));
			this.View();
		}

		// Token: 0x060017A8 RID: 6056 RVA: 0x0006DEAB File Offset: 0x0006C0AB
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UIItemView.DragEndAction = (Action<UIItemView>)Delegate.Remove(UIItemView.DragEndAction, new Action<UIItemView>(this.OnItemDragEnd));
		}

		// Token: 0x060017A9 RID: 6057 RVA: 0x0006DEE5 File Offset: 0x0006C0E5
		public void Initialize(float delay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { delay });
			}
			this.displayAndHideHandler.SetDisplayDelay(delay);
			this.displayAndHideHandler.Display();
		}

		// Token: 0x060017AA RID: 6058 RVA: 0x0006DF20 File Offset: 0x0006C120
		public void View()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", Array.Empty<object>());
			}
			bool flag = this.model.Item == null;
			this.item.View(this.model.Item);
			this.item.gameObject.SetActive(!flag);
			if (flag)
			{
				this.occupiedBorderDisplayAndHideHandler.Hide();
				return;
			}
			this.occupiedBorderDisplayAndHideHandler.Display();
		}

		// Token: 0x060017AB RID: 6059 RVA: 0x0006DF9C File Offset: 0x0006C19C
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

		// Token: 0x060017AC RID: 6060 RVA: 0x0006E005 File Offset: 0x0006C205
		private void OnItemDragEnd(UIItemView item)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemDragEnd", new object[] { item.DebugSafeName(true) });
			}
			this.ViewDropFeedback(false);
		}

		// Token: 0x04001307 RID: 4871
		[SerializeField]
		private UIEquippedSlotModel model;

		// Token: 0x04001308 RID: 4872
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04001309 RID: 4873
		[SerializeField]
		private UIDisplayAndHideHandler occupiedBorderDisplayAndHideHandler;

		// Token: 0x0400130A RID: 4874
		[SerializeField]
		private UIItemView item;

		// Token: 0x0400130B RID: 4875
		[Header("Drop Feedback")]
		[SerializeField]
		private TweenCollection validDropTweenCollection;

		// Token: 0x0400130C RID: 4876
		[SerializeField]
		private TweenCollection invalidDropTweenCollection;

		// Token: 0x0400130D RID: 4877
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400130E RID: 4878
		private bool didTweenValidDropTweenCollection;
	}
}
