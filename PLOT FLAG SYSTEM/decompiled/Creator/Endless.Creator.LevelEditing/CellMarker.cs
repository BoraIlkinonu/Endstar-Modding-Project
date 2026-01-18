using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing;

public class CellMarker : MonoBehaviourSingleton<CellMarker>
{
	[SerializeField]
	private GameObject rootContainer;

	[SerializeField]
	private BoundaryGrid[] grids;

	[SerializeField]
	private Color[] colors;

	private int colorIndex;

	protected override void Awake()
	{
		base.Awake();
		grids = GetComponentsInChildren<BoundaryGrid>(includeInactive: true);
	}

	private void Start()
	{
		SetActiveState(active: false);
	}

	public void MoveTo(Vector3Int location)
	{
		base.transform.position = location;
		MonoBehaviourSingleton<PropLocationMarker>.Instance.Set3DCursorLocation(location);
	}

	private void Update()
	{
		if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null && (bool)PlayerReferenceManager.LocalInstance && (bool)PlayerReferenceManager.LocalInstance.ApperanceController)
		{
			MonoBehaviourSingleton<WorldBoundaryMarker>.Instance.SetFadeDistance((base.transform.position - PlayerReferenceManager.LocalInstance.ApperanceController.transform.position).magnitude);
		}
	}

	public void SetColor(Color color)
	{
		BoundaryGrid[] array = grids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetLineColor(color);
		}
	}

	public void SetActiveState(bool active)
	{
		rootContainer.SetActive(active);
	}
}
