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
	// Token: 0x0200022F RID: 559
	public class UIKeyLibraryReferencePresenter : UIBaseInventoryLibraryReferencePresenter<KeyLibraryReference>
	{
		// Token: 0x1700011C RID: 284
		// (get) Token: 0x0600090B RID: 2315 RVA: 0x0002B16E File Offset: 0x0002936E
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select a Key";
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x0600090C RID: 2316 RVA: 0x0001BF89 File Offset: 0x0001A189
		protected override SelectionType SelectionType
		{
			get
			{
				return SelectionType.Select0To1;
			}
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x0002B175 File Offset: 0x00029375
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as UIKeyLibraryReferenceView).OnLockedChanged += this.SetLocked;
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x0002B1A0 File Offset: 0x000293A0
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
							KeyLibraryReference keyLibraryReference = ReferenceFactory.CreateKeyLibraryReference(propEntry.AssetId);
							list.Add(keyLibraryReference);
						}
					}
					else
					{
						KeyLibraryReference keyLibraryReference2 = ReferenceFactory.CreateKeyLibraryReference(runtimePropInfo.PropData.AssetID);
						list.Add(keyLibraryReference2);
					}
				}
				base.SetSelection(list);
				return;
			}
			base.SetSelection(selection);
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x0002B268 File Offset: 0x00029468
		protected override KeyLibraryReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateDefaultModel", Array.Empty<object>());
			}
			return ReferenceFactory.CreateKeyLibraryReference(SerializableGuid.Empty);
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x0002B28C File Offset: 0x0002948C
		private void SetLocked(bool locked)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLocked", new object[] { locked });
			}
			SerializableGuid serializableGuid = (locked ? "0e5eb3a8-f10b-4ee1-b0db-02400a34f63e" : SerializableGuid.Empty);
			InspectorReferenceUtility.SetId(base.Model, serializableGuid);
			base.SetModelAndTriggerOnModelChanged(base.Model);
		}
	}
}
