using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.LevelEditing.Level;

public class LevelSwitcher : EndlessNetworkBehaviour
{
	[SerializeField]
	private TextAsset targetLevel;

	[SerializeField]
	private UnityEvent<SerializedLevel> onLoadLevel;

	public void Activate()
	{
		if (base.IsServer)
		{
			onLoadLevel.Invoke(JsonUtility.FromJson<SerializedLevel>(targetLevel.text));
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "LevelSwitcher";
	}
}
