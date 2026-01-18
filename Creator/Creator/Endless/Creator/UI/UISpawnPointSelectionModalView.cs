using System;
using System.Collections.Generic;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001DA RID: 474
	public class UISpawnPointSelectionModalView : UIEscapableModalView
	{
		// Token: 0x170000CA RID: 202
		// (get) Token: 0x0600071F RID: 1823 RVA: 0x00023EEE File Offset: 0x000220EE
		// (set) Token: 0x06000720 RID: 1824 RVA: 0x00023EF6 File Offset: 0x000220F6
		public UILevelDestinationPresenter LevelDestinationPresenter { get; private set; }

		// Token: 0x06000721 RID: 1825 RVA: 0x00023F00 File Offset: 0x00022100
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.LevelDestinationPresenter = modalData[0] as UILevelDestinationPresenter;
			List<UISpawnPoint> list = new List<UISpawnPoint>();
			List<UISpawnPoint> list2 = new List<UISpawnPoint>(this.LevelDestinationPresenter.AllSpawnPoints);
			HashSet<UISpawnPoint> hashSet = UISpawnPointSelectionModalView.GetHashSet(this.LevelDestinationPresenter.TargetSpawnPoints);
			for (int i = 0; i < list2.Count; i++)
			{
				UISpawnPoint uispawnPoint = list2[i];
				if (!hashSet.Contains(uispawnPoint))
				{
					list.Add(uispawnPoint);
				}
			}
			this.spawnPointListModel.Set(list, true);
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x00023F83 File Offset: 0x00022183
		public override void Close()
		{
			base.Close();
			this.spawnPointListModel.Clear(true);
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x00023F98 File Offset: 0x00022198
		private static HashSet<UISpawnPoint> GetHashSet(IReadOnlyList<UISpawnPoint> spawnPoints)
		{
			HashSet<UISpawnPoint> hashSet = new HashSet<UISpawnPoint>();
			foreach (UISpawnPoint uispawnPoint in spawnPoints)
			{
				hashSet.Add(uispawnPoint);
			}
			return hashSet;
		}

		// Token: 0x04000673 RID: 1651
		[Header("UISpawnPointSelectionModalView")]
		[SerializeField]
		private UISpawnPointListModel spawnPointListModel;
	}
}
