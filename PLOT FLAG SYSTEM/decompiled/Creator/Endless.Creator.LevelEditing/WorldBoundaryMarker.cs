using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.LevelEditing;

public class WorldBoundaryMarker : MonoBehaviourSingleton<WorldBoundaryMarker>
{
	[SerializeField]
	private GameObject rootContainer;

	[SerializeField]
	private Transform minimumXGrid;

	[SerializeField]
	private Transform maximumXGrid;

	[SerializeField]
	private Transform minimumYGrid;

	[SerializeField]
	private Transform maximumYGrid;

	[SerializeField]
	private Transform minimumZGrid;

	[SerializeField]
	private Transform maximumZGrid;

	private Vector3Int lastMinExtents = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

	private Vector3Int lastMaxExtents = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

	private BoundaryGrid[] boundaryGrids;

	protected override void Awake()
	{
		base.Awake();
		boundaryGrids = GetComponentsInChildren<BoundaryGrid>(includeInactive: true);
	}

	private void Start()
	{
		SetActiveState(active: false);
	}

	public void UpdateTo(Vector3Int minExtents, Vector3Int maxExtents)
	{
		Vector3 position = minExtents - new Vector3(0.5f, 0.5f, 0.5f);
		Vector3 position2 = maxExtents + new Vector3(0.5f, 0.5f, 0.5f);
		Vector3Int vector3Int = maxExtents - minExtents;
		Vector3Int vector3Int2 = new Vector3Int(100, 100, 100) - vector3Int;
		position -= (Vector3)vector3Int2;
		position2 += (Vector3)vector3Int2;
		Vector3Int vector3Int3 = new Vector3Int(100, 100, 100) + vector3Int2 + Vector3Int.one;
		minimumXGrid.position = position;
		minimumXGrid.localScale = vector3Int3;
		minimumYGrid.position = position;
		minimumYGrid.localScale = vector3Int3;
		minimumZGrid.position = position;
		minimumZGrid.localScale = vector3Int3;
		maximumXGrid.position = position2;
		maximumXGrid.localScale = vector3Int3;
		maximumYGrid.position = position2;
		maximumYGrid.localScale = vector3Int3;
		maximumZGrid.position = position2;
		maximumZGrid.localScale = vector3Int3;
	}

	private void Update()
	{
		if (!(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage == null))
		{
			bool flag = false;
			if (lastMinExtents != MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents)
			{
				lastMinExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents;
				flag = true;
			}
			if (lastMaxExtents != MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents)
			{
				lastMaxExtents = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents;
				flag = true;
			}
			if (flag)
			{
				UpdateTo(lastMinExtents, lastMaxExtents);
			}
		}
	}

	public void Track(Transform trackedTransform)
	{
		BoundaryGrid[] array = boundaryGrids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Track(trackedTransform);
		}
	}

	public void Untrack(Transform untrackedTransform)
	{
		BoundaryGrid[] array = boundaryGrids;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Untrack(untrackedTransform);
		}
	}

	public void SetFadeDistance(float fadeDistance)
	{
		BoundaryGrid[] array = boundaryGrids;
		foreach (BoundaryGrid boundaryGrid in array)
		{
			float fadeDistance2 = Mathf.Clamp(fadeDistance, boundaryGrid.FadeDistance, 25f);
			boundaryGrid.SetFadeDistance(fadeDistance2);
		}
	}

	public void SetActiveState(bool active)
	{
		rootContainer.SetActive(active);
	}
}
