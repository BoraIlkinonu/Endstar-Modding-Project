using System;
using Endless.Props;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002F0 RID: 752
	public class UITransformIdentifierSelectionWindowController : UIWindowController
	{
		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06000CFD RID: 3325 RVA: 0x0003E5B8 File Offset: 0x0003C7B8
		private UITransformIdentifierSelectionWindowView TypedWindowView
		{
			get
			{
				return (UITransformIdentifierSelectionWindowView)this.BaseWindowView;
			}
		}

		// Token: 0x06000CFE RID: 3326 RVA: 0x0003E5C5 File Offset: 0x0003C7C5
		protected override void Start()
		{
			base.Start();
			this.transformIdentifierListModel.SelectionChangedUnityEvent.AddListener(new UnityAction<int, bool>(this.OnSelect));
		}

		// Token: 0x06000CFF RID: 3327 RVA: 0x0003E5EC File Offset: 0x0003C7EC
		private void OnSelect(int index, bool selected)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelect", new object[] { index, selected });
			}
			Action<TransformIdentifier> onTransformIdentifierSelect = this.TypedWindowView.OnTransformIdentifierSelect;
			if (onTransformIdentifierSelect != null)
			{
				onTransformIdentifierSelect(this.transformIdentifierListModel[index].TransformIdentifier);
			}
			this.TypedWindowView.Close();
		}

		// Token: 0x04000B2F RID: 2863
		[Header("UITransformIdentifierSelectionWindowController")]
		[SerializeField]
		private UITransformIdentifierListModel transformIdentifierListModel;
	}
}
