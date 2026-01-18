using System;
using Endless.Gameplay.Screenshotting;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020002FB RID: 763
	public class BasicSpawnPoint : EndlessBehaviour, IAwakeSubscriber, ISpawnPoint, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x06001145 RID: 4421 RVA: 0x00056833 File Offset: 0x00054A33
		public void EndlessAwake()
		{
			this.SetVisuals(false);
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x0005683C File Offset: 0x00054A3C
		private void SetVisuals(bool enabled)
		{
			foreach (Renderer renderer in this.references.CreatorOnlyRenderers)
			{
				renderer.enabled = enabled;
			}
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x00056894 File Offset: 0x00054A94
		protected override void Start()
		{
			base.Start();
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.AddListener(new UnityAction(this.ScreenshotStarted));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(new UnityAction(this.ScreenshotFinished));
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x000568D2 File Offset: 0x00054AD2
		protected override void OnDestroy()
		{
			base.OnDestroy();
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.RemoveListener(new UnityAction(this.ScreenshotStarted));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.RemoveListener(new UnityAction(this.ScreenshotFinished));
		}

		// Token: 0x06001149 RID: 4425 RVA: 0x00056833 File Offset: 0x00054A33
		private void ScreenshotStarted()
		{
			this.SetVisuals(false);
		}

		// Token: 0x0600114A RID: 4426 RVA: 0x00056910 File Offset: 0x00054B10
		private void ScreenshotFinished()
		{
			this.SetVisuals(true);
		}

		// Token: 0x0600114B RID: 4427 RVA: 0x00056919 File Offset: 0x00054B19
		public Transform GetSpawnPosition(int index)
		{
			if (this.references.SpawnPoints.Length == 0)
			{
				return base.transform;
			}
			return this.references.SpawnPoints[index % this.references.SpawnPoints.Length];
		}

		// Token: 0x0600114C RID: 4428 RVA: 0x0005694C File Offset: 0x00054B4C
		public void ConfigurePlayer(GameplayPlayerReferenceManager playerReferenceManager)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("HandleNewPlayerSpawned", out array, new object[] { playerReferenceManager.PlayerContext });
		}

		// Token: 0x0600114D RID: 4429 RVA: 0x0005697C File Offset: 0x00054B7C
		public void HandlePlayerEnteredLevel(GameplayPlayerReferenceManager playerReferenceManager)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("HandlePlayerLevelChange", out array, new object[] { playerReferenceManager.PlayerContext });
		}

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x0600114E RID: 4430 RVA: 0x000569AB File Offset: 0x00054BAB
		// (set) Token: 0x0600114F RID: 4431 RVA: 0x000569B3 File Offset: 0x00054BB3
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000369 RID: 873
		// (get) Token: 0x06001150 RID: 4432 RVA: 0x000569BC File Offset: 0x00054BBC
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(SpawnPointReferences);
			}
		}

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x06001151 RID: 4433 RVA: 0x000569C8 File Offset: 0x00054BC8
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x06001152 RID: 4434 RVA: 0x000569F3 File Offset: 0x00054BF3
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = referenceBase as SpawnPointReferences;
		}

		// Token: 0x06001153 RID: 4435 RVA: 0x00056A01 File Offset: 0x00054C01
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x06001154 RID: 4436 RVA: 0x00056A0A File Offset: 0x00054C0A
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x04000EE2 RID: 3810
		[SerializeField]
		[HideInInspector]
		private EndlessScriptComponent scriptComponent;

		// Token: 0x04000EE3 RID: 3811
		[SerializeField]
		[HideInInspector]
		private SpawnPointReferences references;

		// Token: 0x04000EE4 RID: 3812
		private Context context;
	}
}
