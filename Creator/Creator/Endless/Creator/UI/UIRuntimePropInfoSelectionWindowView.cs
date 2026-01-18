using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020002DC RID: 732
	public class UIRuntimePropInfoSelectionWindowView : UIBaseWindowView
	{
		// Token: 0x17000199 RID: 409
		// (get) Token: 0x06000C73 RID: 3187 RVA: 0x0003B800 File Offset: 0x00039A00
		// (set) Token: 0x06000C74 RID: 3188 RVA: 0x0003B808 File Offset: 0x00039A08
		public Action<IReadOnlyList<PropLibrary.RuntimePropInfo>> OnConfirm { get; private set; }

		// Token: 0x06000C75 RID: 3189 RVA: 0x0003B814 File Offset: 0x00039A14
		public static UIRuntimePropInfoSelectionWindowView Display(SerializableGuid selectedInstanceId, bool restrictSelectionCountTo1, ReferenceFilter referenceFilter, IReadOnlyList<PropLibrary.RuntimePropInfo> propsToIgnore, Action<IReadOnlyList<PropLibrary.RuntimePropInfo>> onConfirm, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "selectedInstanceId", selectedInstanceId },
				{ "restrictSelectionCountTo1", restrictSelectionCountTo1 },
				{ "referenceFilter", referenceFilter },
				{ "propsToIgnore", propsToIgnore },
				{
					"onConfirm".CapitalizeFirstCharacter(),
					onConfirm
				}
			};
			return (UIRuntimePropInfoSelectionWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIRuntimePropInfoSelectionWindowView>(parent, dictionary);
		}

		// Token: 0x06000C76 RID: 3190 RVA: 0x0003B88C File Offset: 0x00039A8C
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			SerializableGuid selectedInstanceId = (SerializableGuid)supplementalData["selectedInstanceId"];
			bool flag = (bool)supplementalData["restrictSelectionCountTo1"];
			ReferenceFilter referenceFilter = (ReferenceFilter)supplementalData["referenceFilter"];
			IReadOnlyList<PropLibrary.RuntimePropInfo> readOnlyList = (IReadOnlyList<PropLibrary.RuntimePropInfo>)supplementalData["propsToIgnore"];
			this.OnConfirm = (Action<IReadOnlyList<PropLibrary.RuntimePropInfo>>)supplementalData["OnConfirm"];
			this.runtimePropInfoListModel.SetRestrictSelectionCountTo1(flag, false);
			this.runtimePropInfoListModel.ClearSelected(false);
			this.runtimePropInfoListModel.Synchronize(referenceFilter, readOnlyList);
			if (selectedInstanceId == SerializableGuid.Empty)
			{
				return;
			}
			int num = this.runtimePropInfoListModel.FilteredList.ToList<PropLibrary.RuntimePropInfo>().FindIndex((PropLibrary.RuntimePropInfo item) => item.PropData.AssetID == selectedInstanceId);
			this.runtimePropInfoListModel.Select(num, true);
		}

		// Token: 0x04000AB8 RID: 2744
		[SerializeField]
		private UIRuntimePropInfoListModel runtimePropInfoListModel;

		// Token: 0x04000AB9 RID: 2745
		[SerializeField]
		private UIButton confirmButton;

		// Token: 0x04000ABA RID: 2746
		[Header("Debugging")]
		[SerializeField]
		private bool superVerboseLogging;
	}
}
