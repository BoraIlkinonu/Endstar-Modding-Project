using System;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000213 RID: 531
	public class UIAudioReferenceView : UIBaseAssetLibraryReferenceClassView<AudioReference, UIAudioReferenceView.Styles>
	{
		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06000883 RID: 2179 RVA: 0x0002998C File Offset: 0x00027B8C
		// (set) Token: 0x06000884 RID: 2180 RVA: 0x00029994 File Offset: 0x00027B94
		public override UIAudioReferenceView.Styles Style { get; protected set; }

		// Token: 0x06000885 RID: 2181 RVA: 0x000299A0 File Offset: 0x00027BA0
		protected override string GetReferenceName(AudioReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetReferenceName", new object[] { model });
			}
			if (model.IsReferenceEmpty())
			{
				return "None";
			}
			SerializableGuid id = InspectorReferenceUtility.GetId(model);
			RuntimeAudioInfo runtimeAudioInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(id, out runtimeAudioInfo))
			{
				return runtimeAudioInfo.AudioAsset.Name;
			}
			return "Missing";
		}

		// Token: 0x02000214 RID: 532
		public enum Styles
		{
			// Token: 0x04000764 RID: 1892
			Default,
			// Token: 0x04000765 RID: 1893
			ReadOnly
		}
	}
}
