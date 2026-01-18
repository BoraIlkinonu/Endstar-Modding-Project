using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UIAudioReferencePresenter : UIBaseAssetLibraryReferenceClassPresenter<AudioReference>
{
	protected override IEnumerable<object> SelectionOptions => MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.GetLoadedAudioAsAudioReferences();

	protected override string IEnumerableWindowTitle => "Select an Audio Reference";

	protected override Dictionary<Type, Enum> TypeStyleOverrideDictionary { get; } = new Dictionary<Type, Enum> { 
	{
		typeof(AudioReference),
		UIAudioReferenceView.Styles.ReadOnly
	} };

	protected override void SetSelection(List<object> selection)
	{
		List<object> list = new List<object>();
		foreach (object item3 in selection)
		{
			if (!(item3 is AudioAsset audioAsset))
			{
				if (item3 is AudioReference item)
				{
					list.Add(item);
				}
				else
				{
					DebugUtility.LogException(new InvalidCastException("selection's element type must be of type AudioAsset or AudioReference"), this);
				}
			}
			else
			{
				AudioReference item2 = ReferenceFactory.CreateAudioReference(new SerializableGuid(audioAsset.AssetID));
				list.Add(item2);
			}
		}
		base.SetSelection(list);
	}

	protected override AudioReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateDefaultModel");
		}
		return ReferenceFactory.CreateAudioReference(SerializableGuid.Empty);
	}
}
