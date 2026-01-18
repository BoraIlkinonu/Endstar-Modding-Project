using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000210 RID: 528
	public class UIAudioReferencePresenter : UIBaseAssetLibraryReferenceClassPresenter<AudioReference>
	{
		// Token: 0x170000FE RID: 254
		// (get) Token: 0x06000870 RID: 2160 RVA: 0x0002950D File Offset: 0x0002770D
		protected override IEnumerable<object> SelectionOptions
		{
			get
			{
				return MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.GetLoadedAudioAsAudioReferences();
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x06000871 RID: 2161 RVA: 0x0002951E File Offset: 0x0002771E
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select an Audio Reference";
			}
		}

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x06000872 RID: 2162 RVA: 0x00029525 File Offset: 0x00027725
		protected override Dictionary<Type, Enum> TypeStyleOverrideDictionary { get; } = new Dictionary<Type, Enum> { 
		{
			typeof(AudioReference),
			UIAudioReferenceView.Styles.ReadOnly
		} };

		// Token: 0x06000873 RID: 2163 RVA: 0x00029530 File Offset: 0x00027730
		protected override void SetSelection(List<object> selection)
		{
			List<object> list = new List<object>();
			foreach (object obj in selection)
			{
				AudioAsset audioAsset = obj as AudioAsset;
				if (audioAsset == null)
				{
					AudioReference audioReference = obj as AudioReference;
					if (audioReference == null)
					{
						DebugUtility.LogException(new InvalidCastException("selection's element type must be of type AudioAsset or AudioReference"), this);
					}
					else
					{
						list.Add(audioReference);
					}
				}
				else
				{
					AudioReference audioReference2 = ReferenceFactory.CreateAudioReference(new SerializableGuid(audioAsset.AssetID));
					list.Add(audioReference2);
				}
			}
			base.SetSelection(list);
		}

		// Token: 0x06000874 RID: 2164 RVA: 0x000295D4 File Offset: 0x000277D4
		protected override AudioReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreateDefaultModel", Array.Empty<object>());
			}
			return ReferenceFactory.CreateAudioReference(SerializableGuid.Empty);
		}
	}
}
