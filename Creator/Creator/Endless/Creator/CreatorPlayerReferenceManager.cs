using System;
using Endless.Creator.LevelEditing;
using Endless.Gameplay;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator
{
	// Token: 0x02000093 RID: 147
	public class CreatorPlayerReferenceManager : PlayerReferenceManager
	{
		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600023E RID: 574 RVA: 0x00010912 File Offset: 0x0000EB12
		public override PlayerGhostController PlayerGhostController
		{
			get
			{
				return this.playerGhostController;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600023F RID: 575 RVA: 0x0001091A File Offset: 0x0000EB1A
		public CreatorGunController CreatorGunController
		{
			get
			{
				return this.creatorGunController;
			}
		}

		// Token: 0x06000240 RID: 576 RVA: 0x00010922 File Offset: 0x0000EB22
		protected override void Track()
		{
			base.Track();
			Debug.Log("Telling World Boundary Marker to track us");
			MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.Track(base.transform);
		}

		// Token: 0x06000241 RID: 577 RVA: 0x00010944 File Offset: 0x0000EB44
		protected override void Untrack()
		{
			base.Untrack();
			Debug.Log("Telling World Boundary Marker to untrack us");
			MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.Untrack(base.transform);
		}

		// Token: 0x06000243 RID: 579 RVA: 0x00010970 File Offset: 0x0000EB70
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000244 RID: 580 RVA: 0x00010986 File Offset: 0x0000EB86
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000245 RID: 581 RVA: 0x00010990 File Offset: 0x0000EB90
		protected internal override string __getTypeName()
		{
			return "CreatorPlayerReferenceManager";
		}

		// Token: 0x0400029B RID: 667
		[SerializeField]
		private PlayerGhostController playerGhostController;

		// Token: 0x0400029C RID: 668
		[SerializeField]
		private CreatorGunController creatorGunController;
	}
}
