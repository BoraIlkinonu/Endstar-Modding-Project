using System.Collections;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public class AmbientManager : EndlessBehaviourSingleton<AmbientManager>, IGameEndSubscriber
{
	[SerializeField]
	private AmbientEntry defaultEntry;

	[SerializeField]
	private ReflectionProbe reflectionProbe;

	private AmbientEntry currentEntry;

	public void EndlessGameEnd()
	{
		ResetSky();
	}

	protected override void Awake()
	{
		base.Awake();
		SetAmbientEntry(defaultEntry);
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<GameplayManager>.Instance.OnLevelLoaded.AddListener(OnLevelLoaded);
	}

	private void OnLevelLoaded(SerializableGuid arg0)
	{
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.HasDefaultEnvironmentSet)
		{
			ResetSky();
		}
	}

	private void ResetSky()
	{
		SetAmbientEntry(defaultEntry);
	}

	internal void SetAmbientEntry(AmbientEntry newEntry)
	{
		if ((bool)currentEntry)
		{
			currentEntry.Deactivate();
		}
		if (newEntry != null)
		{
			currentEntry = newEntry;
		}
		else
		{
			currentEntry = defaultEntry;
		}
		currentEntry.Activate();
		StartCoroutine(RebuildReflectionProbeRoutine());
	}

	private IEnumerator RebuildReflectionProbeRoutine()
	{
		yield return null;
		reflectionProbe.RenderProbe();
	}
}
