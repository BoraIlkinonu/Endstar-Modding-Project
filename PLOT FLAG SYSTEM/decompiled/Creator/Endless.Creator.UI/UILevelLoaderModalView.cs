using Endless.Gameplay.LevelEditing.Level;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelLoaderModalView : UIEscapableModalView
{
	[SerializeField]
	private TextMeshProUGUI levelNameText;

	[SerializeField]
	private TextMeshProUGUI levelDescriptionText;

	public SerializableGuid LevelId { get; private set; } = SerializableGuid.Empty;

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		if (MatchmakingClientController.Instance.ActiveGameId == SerializableGuid.Empty)
		{
			DebugUtility.LogError(this, "OnSpawn", "MatchmakingClientController's ActiveGameId has no value!");
			return;
		}
		if (MatchmakingClientController.Instance.LocalMatch == null)
		{
			DebugUtility.LogError(this, "OnSpawn", "MatchmakingClientController's LocalMatch has no value!");
			return;
		}
		LevelAsset levelAsset = (LevelAsset)modalData[0];
		levelNameText.text = levelAsset.Name;
		levelDescriptionText.text = levelAsset.Description;
		LevelId = levelAsset.AssetID;
		if (levelDescriptionText.text.IsNullOrEmptyOrWhiteSpace())
		{
			levelDescriptionText.text = "<color=#B5B5B5>(There is none)</color>";
		}
	}
}
