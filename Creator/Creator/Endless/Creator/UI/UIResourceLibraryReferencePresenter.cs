using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200023B RID: 571
	public class UIResourceLibraryReferencePresenter : UIInspectorPropReferencePresenter<ResourceLibraryReference>
	{
		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000939 RID: 2361 RVA: 0x0002B680 File Offset: 0x00029880
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select a Resource";
			}
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x0600093A RID: 2362 RVA: 0x0001BF89 File Offset: 0x0001A189
		protected override SelectionType SelectionType
		{
			get
			{
				return SelectionType.Select0To1;
			}
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x0002B687 File Offset: 0x00029887
		protected override ResourceLibraryReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateDefaultModel", Array.Empty<object>());
			}
			return ReferenceFactory.CreateResourceLibraryReference(SerializableGuid.Empty);
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x0002B6AC File Offset: 0x000298AC
		protected override void SetSelection(List<object> selection)
		{
			List<object> list = new List<object>();
			foreach (object obj in selection)
			{
				ResourceLibraryReference resourceLibraryReference = ReferenceFactory.CreateResourceLibraryReference((obj as PropLibrary.RuntimePropInfo).PropData.AssetID);
				list.Add(resourceLibraryReference);
			}
			base.SetSelection(list);
		}
	}
}
