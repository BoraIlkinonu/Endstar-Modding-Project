using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIAudioReferenceView : UIBaseAssetLibraryReferenceClassView<AudioReference, UIAudioReferenceView.Styles>
{
	public enum Styles
	{
		Default,
		ReadOnly
	}

	[field: Header("UIAudioReferenceView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	protected override string GetReferenceName(AudioReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetReferenceName", model);
		}
		if (model.IsReferenceEmpty())
		{
			return "None";
		}
		SerializableGuid id = InspectorReferenceUtility.GetId(model);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(id, out var metadata))
		{
			return metadata.AudioAsset.Name;
		}
		return "Missing";
	}
}
