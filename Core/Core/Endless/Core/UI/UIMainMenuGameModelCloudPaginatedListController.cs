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
	// Token: 0x0200005B RID: 91
	public class UIMainMenuGameModelCloudPaginatedListController : UIBaseCloudPaginatedListController<MainMenuGameModel>
	{
		// Token: 0x060001A5 RID: 421 RVA: 0x0000A4BD File Offset: 0x000086BD
		protected override void Start()
		{
			base.Start();
			base.View.ListCellSizeTypeChangedUnityEvent.AddListener(new UnityAction<ListCellSizeTypes>(this.OnListCellSizeTypeChanged));
			this.OnListCellSizeTypeChanged(base.View.ListCellSizeType);
		}

		// Token: 0x060001A6 RID: 422 RVA: 0x0000A4F2 File Offset: 0x000086F2
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			ValueTween.CancelAndNull(ref this.preferredHeightTween);
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x0000A512 File Offset: 0x00008712
		public void SetGameNameStringFilter(string toFilterBy)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetStringFilter", new object[] { toFilterBy });
			}
			this.gameNameStringFilterInputField.text = toFilterBy;
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x0000A540 File Offset: 0x00008740
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

		// Token: 0x060001A9 RID: 425 RVA: 0x0000A5CC File Offset: 0x000087CC
		private void TweenPreferredHeight(float interpolation)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenPreferredHeight", new object[] { interpolation });
			}
			this.layoutElement.preferredHeight = Mathf.Lerp(this.from, this.to, interpolation);
		}

		// Token: 0x060001AA RID: 426 RVA: 0x0000A618 File Offset: 0x00008818
		private void CompletePreferredHeightTween()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CompletePreferredHeightTween", Array.Empty<object>());
			}
			this.layoutElement.preferredHeight = this.to;
		}

		// Token: 0x0400012F RID: 303
		[Header("UIMainMenuGameModelCloudPaginatedListController")]
		[SerializeField]
		private UIInputField gameNameStringFilterInputField;

		// Token: 0x04000130 RID: 304
		[SerializeField]
		private ListCellSizeTypesFloatDictionary listCellSizeTypesFloatDictionary;

		// Token: 0x04000131 RID: 305
		[SerializeField]
		private LayoutElement layoutElement;

		// Token: 0x04000132 RID: 306
		private float from;

		// Token: 0x04000133 RID: 307
		private float to;

		// Token: 0x04000134 RID: 308
		private ValueTween preferredHeightTween;
	}
}
