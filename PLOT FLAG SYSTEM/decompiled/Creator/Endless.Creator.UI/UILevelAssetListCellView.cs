using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelAssetListCellView : UIBaseListCellView<LevelAsset>
{
	[Header("UILevelAssetListCellView")]
	[SerializeField]
	private UILevelAssetView levelAssetView;

	[SerializeField]
	private UIDragInstanceHandler dragInstanceHandler;

	protected override void Start()
	{
		base.Start();
		if ((bool)dragInstanceHandler)
		{
			dragInstanceHandler.OnInstantiateUnityEvent.AddListener(OnInstantiate);
		}
	}

	public override void View(UIBaseListView<LevelAsset> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!IsAddButton && base.Model != null)
		{
			levelAssetView.View(base.Model);
		}
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		levelAssetView.Clear();
	}

	private void OnInstantiate(GameObject instantiation)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnInstantiate", instantiation.DebugSafeName());
		}
		if (instantiation.TryGetComponent<UILevelAssetListCellView>(out var component))
		{
			component.View(base.ListView, base.DataIndex);
		}
	}
}
