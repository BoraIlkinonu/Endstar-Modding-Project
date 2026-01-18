using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine.Events;

namespace Endless.Core.Test
{
	// Token: 0x020000D2 RID: 210
	public class ChunkMergeFpsInfo : BaseFpsInfo
	{
		// Token: 0x17000093 RID: 147
		// (get) Token: 0x060004CA RID: 1226 RVA: 0x0001789A File Offset: 0x00015A9A
		public override bool IsDone
		{
			get
			{
				return !this.merging;
			}
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x060004CB RID: 1227 RVA: 0x00016DC5 File Offset: 0x00014FC5
		protected override FpsTestType TestType
		{
			get
			{
				return FpsTestType.Dynamic;
			}
		}

		// Token: 0x060004CC RID: 1228 RVA: 0x000178A8 File Offset: 0x00015AA8
		public override void StartTest()
		{
			this.chunkManager = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ChunkManager;
			if (this.chunkManager.HasDirtyChunks)
			{
				this.merging = true;
				this.chunkManager.OnMergingComplete.AddListener(new UnityAction(this.HandleMergeCompleted));
				return;
			}
			this.HandleMergeCompleted();
		}

		// Token: 0x060004CD RID: 1229 RVA: 0x00017901 File Offset: 0x00015B01
		private void HandleMergeCompleted()
		{
			this.merging = false;
		}

		// Token: 0x060004CE RID: 1230 RVA: 0x0001790A File Offset: 0x00015B0A
		public override void StopTest()
		{
			if (this.chunkManager != null)
			{
				this.chunkManager.OnMergingComplete.RemoveListener(new UnityAction(this.StopTest));
				this.chunkManager = null;
			}
		}

		// Token: 0x060004CF RID: 1231 RVA: 0x0000229D File Offset: 0x0000049D
		protected override void ProcessFrame_Internal()
		{
		}

		// Token: 0x04000331 RID: 817
		private ChunkManager chunkManager;

		// Token: 0x04000332 RID: 818
		private bool merging;
	}
}
