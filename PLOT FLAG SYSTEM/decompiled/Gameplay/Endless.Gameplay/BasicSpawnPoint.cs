using System;
using Endless.Gameplay.Screenshotting;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class BasicSpawnPoint : EndlessBehaviour, IAwakeSubscriber, ISpawnPoint, IBaseType, IComponentBase, IScriptInjector
{
	[SerializeField]
	[HideInInspector]
	private EndlessScriptComponent scriptComponent;

	[SerializeField]
	[HideInInspector]
	private SpawnPointReferences references;

	private Context context;

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(SpawnPointReferences);

	public Context Context => context ?? (context = new Context(WorldObject));

	public void EndlessAwake()
	{
		SetVisuals(enabled: false);
	}

	private void SetVisuals(bool enabled)
	{
		foreach (Renderer creatorOnlyRenderer in references.CreatorOnlyRenderers)
		{
			creatorOnlyRenderer.enabled = enabled;
		}
	}

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.AddListener(ScreenshotStarted);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(ScreenshotFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.RemoveListener(ScreenshotStarted);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.RemoveListener(ScreenshotFinished);
	}

	private void ScreenshotStarted()
	{
		SetVisuals(enabled: false);
	}

	private void ScreenshotFinished()
	{
		SetVisuals(enabled: true);
	}

	public Transform GetSpawnPosition(int index)
	{
		if (references.SpawnPoints.Length == 0)
		{
			return base.transform;
		}
		return references.SpawnPoints[index % references.SpawnPoints.Length];
	}

	public void ConfigurePlayer(GameplayPlayerReferenceManager playerReferenceManager)
	{
		scriptComponent.TryExecuteFunction("HandleNewPlayerSpawned", out var _, playerReferenceManager.PlayerContext);
	}

	public void HandlePlayerEnteredLevel(GameplayPlayerReferenceManager playerReferenceManager)
	{
		scriptComponent.TryExecuteFunction("HandlePlayerLevelChange", out var _, playerReferenceManager.PlayerContext);
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = referenceBase as SpawnPointReferences;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}
}
