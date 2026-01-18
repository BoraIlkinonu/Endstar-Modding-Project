using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Core.UI
{
	// Token: 0x0200005C RID: 92
	public class UIMainMenuGameModelListController : UIBaseLocalFilterableListController<MainMenuGameModel>
	{
		// Token: 0x060001AD RID: 429 RVA: 0x0000A653 File Offset: 0x00008853
		protected override void Start()
		{
			base.Start();
			base.View.ListCellSizeTypeChangedUnityEvent.AddListener(new UnityAction<ListCellSizeTypes>(this.OnListCellSizeTypeChanged));
			this.OnListCellSizeTypeChanged(base.View.ListCellSizeType);
		}

		// Token: 0x060001AE RID: 430 RVA: 0x0000A688 File Offset: 0x00008888
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			ValueTween.CancelAndNull(ref this.preferredHeightTween);
		}

		// Token: 0x060001AF RID: 431 RVA: 0x0000A6A8 File Offset: 0x000088A8
		protected override bool IncludeInFilteredResults(MainMenuGameModel item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
			}
			if (item == null)
			{
				DebugUtility.LogError("item was null!", this);
				return false;
			}
			string text = item.Name;
			if (!base.CaseSensitive)
			{
				text = text.ToLower();
			}
			string text2 = base.StringFilter;
			if (!base.CaseSensitive)
			{
				text2 = text2.ToLower();
			}
			return text.Contains(text2);
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000A718 File Offset: 0x00008918
		private void OnListCellSizeTypeChanged(ListCellSizeTypes listCellSizeType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnListCellSizeTypeChanged", new object[] { listCellSizeType });
			}
			this.from = this.layoutElement.preferredHeight;
			this.to = this.listCellSizeTypesFloatDictionary[listCellSizeType];
			ValueTween.CancelAndNull(ref this.preferredHeightTween);
			this.preferredHeightTween = MonoBehaviourSingleton<TweenManager>.Instance.TweenValue(0.25f, new Action<float>(this.TweenPreferredHeight), delegate
			{
				this.CompletePreferredHeightTween();
			}, TweenTimeMode.Scaled, null);
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x0000A7A4 File Offset: 0x000089A4
		private void TweenPreferredHeight(float interpolation)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenPreferredHeight", new object[] { interpolation });
			}
			this.layoutElement.PreferredHeightLayoutDimension.ExplicitValue = Mathf.Lerp(this.from, this.to, interpolation);
			LayoutRebuilder.MarkLayoutForRebuild(this.layoutElement.RectTransform);
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0000A805 File Offset: 0x00008A05
		private void CompletePreferredHeightTween()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CompletePreferredHeightTween", Array.Empty<object>());
			}
			this.layoutElement.PreferredHeightLayoutDimension.ExplicitValue = this.to;
			LayoutRebuilder.MarkLayoutForRebuild(this.layoutElement.RectTransform);
		}

		// Token: 0x04000135 RID: 309
		[Header("UIMainMenuGameModelListController")]
		[SerializeField]
		private ListCellSizeTypesFloatDictionary listCellSizeTypesFloatDictionary;

		// Token: 0x04000136 RID: 310
		[SerializeField]
		private UILayoutElement layoutElement;

		// Token: 0x04000137 RID: 311
		private float from;

		// Token: 0x04000138 RID: 312
		private float to;

		// Token: 0x04000139 RID: 313
		private ValueTween preferredHeightTween;
	}
}
