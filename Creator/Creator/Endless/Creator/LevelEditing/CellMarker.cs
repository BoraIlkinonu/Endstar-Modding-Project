using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing
{
	// Token: 0x02000342 RID: 834
	public class CellMarker : MonoBehaviourSingleton<CellMarker>
	{
		// Token: 0x06000F70 RID: 3952 RVA: 0x000477DA File Offset: 0x000459DA
		protected override void Awake()
		{
			base.Awake();
			this.grids = base.GetComponentsInChildren<BoundaryGrid>(true);
		}

		// Token: 0x06000F71 RID: 3953 RVA: 0x000477EF File Offset: 0x000459EF
		private void Start()
		{
			this.SetActiveState(false);
		}

		// Token: 0x06000F72 RID: 3954 RVA: 0x000477F8 File Offset: 0x000459F8
		public void MoveTo(Vector3Int location)
		{
			base.transform.position = location;
			MonoBehaviourSingleton<PropLocationMarker>.Instance.Set3DCursorLocation(location);
		}

		// Token: 0x06000F73 RID: 3955 RVA: 0x00047818 File Offset: 0x00045A18
		private void Update()
		{
			if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null && PlayerReferenceManager.LocalInstance && PlayerReferenceManager.LocalInstance.ApperanceController)
			{
				MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.SetFadeDistance((base.transform.position - PlayerReferenceManager.LocalInstance.ApperanceController.transform.position).magnitude);
			}
		}

		// Token: 0x06000F74 RID: 3956 RVA: 0x0004789C File Offset: 0x00045A9C
		public void SetColor(Color color)
		{
			BoundaryGrid[] array = this.grids;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetLineColor(color);
			}
		}

		// Token: 0x06000F75 RID: 3957 RVA: 0x000478C7 File Offset: 0x00045AC7
		public void SetActiveState(bool active)
		{
			this.rootContainer.SetActive(active);
		}

		// Token: 0x04000CDB RID: 3291
		[SerializeField]
		private GameObject rootContainer;

		// Token: 0x04000CDC RID: 3292
		[SerializeField]
		private BoundaryGrid[] grids;

		// Token: 0x04000CDD RID: 3293
		[SerializeField]
		private Color[] colors;

		// Token: 0x04000CDE RID: 3294
		private int colorIndex;
	}
}
