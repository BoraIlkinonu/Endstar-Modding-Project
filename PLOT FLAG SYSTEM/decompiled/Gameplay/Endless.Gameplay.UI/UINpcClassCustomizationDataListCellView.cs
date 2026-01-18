using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UINpcClassCustomizationDataListCellView : UIBaseListCellView<NpcClassCustomizationData>
{
	[Header("UINpcClassCustomizationDataListCellView")]
	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private UIButton removeButton;

	private IUIPresentable presentable;

	public override void View(UIBaseListView<NpcClassCustomizationData> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!IsAddButton)
		{
			presentable = MonoBehaviourSingleton<UIPoolableViewPresenterSpawner>.Instance.SpawnObjectModelWithDefaultStyle(base.Model, container, null);
			removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		}
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		if (presentable != null)
		{
			presentable.ReturnToPool();
			presentable = null;
		}
	}
}
