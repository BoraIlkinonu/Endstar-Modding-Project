using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000232 RID: 562
	public class UIInventoryLibraryReferencePresenter : UIBasePropLibraryReferencePresenter<InventoryLibraryReference>
	{
		// Token: 0x17000120 RID: 288
		// (get) Token: 0x0600091D RID: 2333 RVA: 0x0002B452 File Offset: 0x00029652
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select an Inventory Item";
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x0600091E RID: 2334 RVA: 0x0001BF89 File Offset: 0x0001A189
		protected override SelectionType SelectionType
		{
			get
			{
				return SelectionType.Select0To1;
			}
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x0002B45C File Offset: 0x0002965C
		protected override void SetSelection(List<object> selection)
		{
			if (selection.Any<object>())
			{
				List<object> list = new List<object>();
				foreach (object obj in selection)
				{
					PropLibrary.RuntimePropInfo runtimePropInfo = obj as PropLibrary.RuntimePropInfo;
					if (runtimePropInfo == null)
					{
						PropEntry propEntry = obj as PropEntry;
						if (propEntry == null)
						{
							DebugUtility.LogException(new InvalidCastException("selection's element type must be of type RuntimePropInfo or PropEntry"), this);
						}
						else
						{
							InventoryLibraryReference inventoryLibraryReference = ReferenceFactory.CreateInventoryLibraryReference(propEntry.AssetId);
							list.Add(inventoryLibraryReference);
						}
					}
					else
					{
						InventoryLibraryReference inventoryLibraryReference2 = ReferenceFactory.CreateInventoryLibraryReference(runtimePropInfo.PropData.AssetID);
						list.Add(inventoryLibraryReference2);
					}
				}
				base.SetSelection(list);
				return;
			}
			base.SetSelection(selection);
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x0002B524 File Offset: 0x00029724
		protected override InventoryLibraryReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("CreateDefaultModel", this);
			}
			return ReferenceFactory.CreateInventoryLibraryReference(SerializableGuid.Empty);
		}
	}
}
