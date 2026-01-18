using System.Collections.Generic;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UISpawnPointSelectionModalView : UIEscapableModalView
{
	[Header("UISpawnPointSelectionModalView")]
	[SerializeField]
	private UISpawnPointListModel spawnPointListModel;

	public UILevelDestinationPresenter LevelDestinationPresenter { get; private set; }

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		LevelDestinationPresenter = modalData[0] as UILevelDestinationPresenter;
		List<UISpawnPoint> list = new List<UISpawnPoint>();
		List<UISpawnPoint> list2 = new List<UISpawnPoint>(LevelDestinationPresenter.AllSpawnPoints);
		HashSet<UISpawnPoint> hashSet = GetHashSet(LevelDestinationPresenter.TargetSpawnPoints);
		for (int i = 0; i < list2.Count; i++)
		{
			UISpawnPoint item = list2[i];
			if (!hashSet.Contains(item))
			{
				list.Add(item);
			}
		}
		spawnPointListModel.Set(list, triggerEvents: true);
	}

	public override void Close()
	{
		base.Close();
		spawnPointListModel.Clear(triggerEvents: true);
	}

	private static HashSet<UISpawnPoint> GetHashSet(IReadOnlyList<UISpawnPoint> spawnPoints)
	{
		HashSet<UISpawnPoint> hashSet = new HashSet<UISpawnPoint>();
		foreach (UISpawnPoint spawnPoint in spawnPoints)
		{
			hashSet.Add(spawnPoint);
		}
		return hashSet;
	}
}
