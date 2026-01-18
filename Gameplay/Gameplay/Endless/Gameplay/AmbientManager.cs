using System;
using System.Collections;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000AC RID: 172
	public class AmbientManager : EndlessBehaviourSingleton<AmbientManager>, IGameEndSubscriber
	{
		// Token: 0x060002EE RID: 750 RVA: 0x0000FD09 File Offset: 0x0000DF09
		public void EndlessGameEnd()
		{
			this.ResetSky();
		}

		// Token: 0x060002EF RID: 751 RVA: 0x0000FD11 File Offset: 0x0000DF11
		protected override void Awake()
		{
			base.Awake();
			this.SetAmbientEntry(this.defaultEntry);
		}

		// Token: 0x060002F0 RID: 752 RVA: 0x0000FD25 File Offset: 0x0000DF25
		protected override void Start()
		{
			base.Start();
			MonoBehaviourSingleton<GameplayManager>.Instance.OnLevelLoaded.AddListener(new UnityAction<SerializableGuid>(this.OnLevelLoaded));
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x0000FD48 File Offset: 0x0000DF48
		private void OnLevelLoaded(SerializableGuid arg0)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.HasDefaultEnvironmentSet)
			{
				this.ResetSky();
			}
		}

		// Token: 0x060002F2 RID: 754 RVA: 0x0000FD66 File Offset: 0x0000DF66
		private void ResetSky()
		{
			this.SetAmbientEntry(this.defaultEntry);
		}

		// Token: 0x060002F3 RID: 755 RVA: 0x0000FD74 File Offset: 0x0000DF74
		internal void SetAmbientEntry(AmbientEntry newEntry)
		{
			if (this.currentEntry)
			{
				this.currentEntry.Deactivate();
			}
			if (newEntry != null)
			{
				this.currentEntry = newEntry;
			}
			else
			{
				this.currentEntry = this.defaultEntry;
			}
			this.currentEntry.Activate();
			base.StartCoroutine(this.RebuildReflectionProbeRoutine());
		}

		// Token: 0x060002F4 RID: 756 RVA: 0x0000FDCF File Offset: 0x0000DFCF
		private IEnumerator RebuildReflectionProbeRoutine()
		{
			yield return null;
			this.reflectionProbe.RenderProbe();
			yield break;
		}

		// Token: 0x040002C0 RID: 704
		[SerializeField]
		private AmbientEntry defaultEntry;

		// Token: 0x040002C1 RID: 705
		[SerializeField]
		private ReflectionProbe reflectionProbe;

		// Token: 0x040002C2 RID: 706
		private AmbientEntry currentEntry;
	}
}
