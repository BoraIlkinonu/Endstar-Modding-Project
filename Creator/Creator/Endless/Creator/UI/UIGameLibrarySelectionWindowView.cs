using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020002D5 RID: 725
	public class UIGameLibrarySelectionWindowView : UIBaseWindowView
	{
		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000C43 RID: 3139 RVA: 0x0003A941 File Offset: 0x00038B41
		// (set) Token: 0x06000C44 RID: 3140 RVA: 0x0003A949 File Offset: 0x00038B49
		public Action<SerializableGuid> OnSelected { get; private set; }

		// Token: 0x06000C45 RID: 3141 RVA: 0x0003A954 File Offset: 0x00038B54
		public static UIGameLibrarySelectionWindowView Display(UIGameAssetTypes filter, SerializableGuid selected, Action<SerializableGuid> onSelected, Transform parent = null)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{ "filter", filter },
				{ "selected", selected },
				{ "OnSelected", onSelected }
			};
			return (UIGameLibrarySelectionWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIGameLibrarySelectionWindowView>(parent, dictionary);
		}

		// Token: 0x06000C46 RID: 3142 RVA: 0x0003A9A8 File Offset: 0x00038BA8
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			UIGameAssetTypes uigameAssetTypes = (UIGameAssetTypes)supplementalData["filter"];
			SerializableGuid serializableGuid = (SerializableGuid)supplementalData["selected"];
			this.OnSelected = supplementalData["OnSelected"] as Action<SerializableGuid>;
			this.gameLibraryListModel.SetAssetTypeFilter(uigameAssetTypes, true);
			int num = -1;
			if (!serializableGuid.IsEmpty)
			{
				for (int i = 0; i < this.gameLibraryListModel.FilteredList.Count; i++)
				{
					if (!(this.gameLibraryListModel.FilteredList[i].AssetID != serializableGuid))
					{
						num = i;
						break;
					}
				}
			}
			if (num > -1)
			{
				this.gameLibraryListModel.Select(num, true);
			}
		}

		// Token: 0x06000C47 RID: 3143 RVA: 0x0003AA61 File Offset: 0x00038C61
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.gameLibraryListModel.Clear(true);
		}

		// Token: 0x04000A97 RID: 2711
		[Header("UIGameLibrarySelectionWindowView")]
		[SerializeField]
		private UIGameLibraryListModel gameLibraryListModel;
	}
}
