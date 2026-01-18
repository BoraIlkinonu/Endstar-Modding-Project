using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime;

public abstract class PropBasedTool : EndlessTool
{
	[SerializeField]
	private float toolDragDeadZone = 0.01f;

	[SerializeField]
	private Transform rotationIndicator;

	[SerializeField]
	[Tooltip("Max Void Distance is used when no object is collided against, how far out do we let the user paint. Essentially, in an empty level, how far into the void can they paint with no other props.")]
	private float maxVoidDistance = 10f;

	private Transform propGhostTransform;

	private Vector3 initialRayOrigin;

	private bool toolIsPressed;

	private Vector3 initialPropPlacement;

	private Vector3 originalRotationSamplePoint;

	private List<Vector3Int> invalidOverlaps = new List<Vector3Int>();

	private SerializableGuid activeAssetId;

	private SerializableGuid previousAssetId;

	private HashSet<PropLibrary.RuntimePropInfo> inFlightLoads = new HashSet<PropLibrary.RuntimePropInfo>();

	private Vector3 rotationOffset;

	private Prop selectedProp;

	private bool usePivotStyleRotation = true;

	protected bool IsLoadingProp => inFlightLoads.Any((PropLibrary.RuntimePropInfo propInfo) => (SerializableGuid)propInfo.PropData.AssetID == activeAssetId);

	protected HashSet<PropLibrary.RuntimePropInfo> InFlightLoads => inFlightLoads;

	protected bool Rotating { get; set; }

	protected Vector3Int PropDimensions { get; set; } = Vector3Int.one;

	protected Transform PropGhostTransform => propGhostTransform;

	protected SerializableGuid ActiveAssetId => activeAssetId;

	protected SerializableGuid PreviousAssetId => previousAssetId;

	protected SerializableGuid LinecastExclusionId { get; set; }

	protected bool ToolIsPressed => toolIsPressed;

	public override void HandleDeselected()
	{
		base.HandleDeselected();
		MonoBehaviourSingleton<PropLocationMarker>.Instance.ClearAllMarkers();
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		invalidOverlaps.Clear();
		rotationIndicator.gameObject.SetActive(value: false);
		DestroyPreview();
	}

	public override void ToolPressed()
	{
		if (!IsLoadingProp)
		{
			base.ToolPressed();
			initialRayOrigin = base.ActiveRay.origin;
			toolIsPressed = true;
			if ((bool)propGhostTransform)
			{
				UpdatePropPlacement();
				initialPropPlacement = propGhostTransform.transform.position;
				LineCastHit activeLineCastResult = base.ActiveLineCastResult;
				originalRotationSamplePoint = GetPlaneSampledPosition(activeLineCastResult);
			}
		}
	}

	public override void ToolHeld()
	{
		if (!IsLoadingProp && toolIsPressed)
		{
			base.ToolHeld();
			if (base.IsMobile && !propGhostTransform.gameObject.activeSelf)
			{
				propGhostTransform.gameObject.SetActive(value: true);
			}
			if (!((initialRayOrigin - base.ActiveRay.origin).magnitude < toolDragDeadZone) && !(propGhostTransform == null))
			{
				UpdatePropGhostTransform();
			}
		}
	}

	private void UpdatePropGhostTransform()
	{
		if (new Plane(Vector3.up, propGhostTransform.position).Raycast(base.ActiveRay, out var enter))
		{
			Vector3 point = base.ActiveRay.GetPoint(enter);
			_ = base.ActiveLineCastResult;
			if (!Rotating)
			{
				Debug.Log($"rotation offset: {rotationOffset}");
				rotationIndicator.gameObject.SetActive(value: true);
				originalRotationSamplePoint = propGhostTransform.transform.position;
			}
			int num = Mathf.RoundToInt(Quaternion.LookRotation(point - initialPropPlacement, Vector3.up).eulerAngles.y / 90f);
			propGhostTransform.rotation = Quaternion.Euler(0f, num * 90, 0f);
			rotationIndicator.rotation = propGhostTransform.rotation;
			Rotating = true;
			Vector3 vector = initialPropPlacement;
			vector = ConvertToRoundedPosition(propGhostTransform.eulerAngles.y, originalRotationSamplePoint);
			propGhostTransform.transform.position = vector;
			rotationIndicator.transform.position = vector;
		}
	}

	public override void ToolReleased()
	{
		base.ToolReleased();
		if (toolIsPressed && !IsLoadingProp)
		{
			EndRotating();
			toolIsPressed = false;
			if (base.IsMobile)
			{
				propGhostTransform.gameObject.SetActive(value: false);
			}
		}
	}

	protected void EndRotating()
	{
		Rotating = false;
		rotationIndicator.gameObject.SetActive(value: false);
	}

	protected bool NoInvalidOverlapsExist()
	{
		return invalidOverlaps.Count == 0;
	}

	protected void DestroyPreview()
	{
		if ((bool)propGhostTransform)
		{
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(activeAssetId, PropLocationType.Selected);
			previousAssetId = activeAssetId;
			activeAssetId = SerializableGuid.Empty;
			UnityEngine.Object.Destroy(propGhostTransform.gameObject);
			propGhostTransform = null;
		}
	}

	public override void UpdateTool()
	{
		Update3DCursorLocation(LinecastExclusionId);
		if (EndlessInput.GetKeyDown(Key.J))
		{
			usePivotStyleRotation = !usePivotStyleRotation;
		}
		if (!base.IsMobile && EndlessInput.GetKeyDown(Key.R) && base.ToolState == ToolState.None && (bool)propGhostTransform)
		{
			float num = propGhostTransform.rotation.eulerAngles.y / 90f;
			num = (num + 1f) % 4f;
			propGhostTransform.rotation = Quaternion.Euler(0f, num * 90f, 0f);
		}
	}

	protected void UpdatePropPlacement()
	{
		if (!propGhostTransform)
		{
			return;
		}
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(activeAssetId);
		Vector3Int[] array = Array.Empty<Vector3Int>();
		if (!Rotating)
		{
			if (activeLineCastResult.IntersectionOccured)
			{
				if (runtimePropInfo != null)
				{
					Vector3 planeSampledPosition = GetPlaneSampledPosition(activeLineCastResult);
					Vector3 vector = ConvertToRoundedPosition(propGhostTransform.eulerAngles.y, planeSampledPosition);
					if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(runtimePropInfo.PropData, vector, propGhostTransform.rotation, LinecastExclusionId))
					{
						propGhostTransform.transform.position = vector;
					}
					else
					{
						Vector3[] array2 = new Vector3[6]
						{
							Vector3.forward,
							Vector3.back,
							Vector3.left,
							Vector3.right,
							Vector3.down,
							Vector3.up
						};
						int num = -1;
						float num2 = float.MinValue;
						Vector3 lhs = activeLineCastResult.NearestPositionToObject - activeLineCastResult.IntersectedObjectPosition;
						lhs.Normalize();
						for (int i = 0; i < array2.Length; i++)
						{
							float num3 = Vector3.Dot(lhs, array2[i]);
							Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition + Vector3Int.FloorToInt(array2[i]));
							if (num3 > num2 && cellFromCoordinate == null)
							{
								num = i;
								num2 = num3;
							}
						}
						Vector3 source = vector;
						if (num >= 0)
						{
							if (EndlessInput.GetKeyDown(Key.V))
							{
								Debug.Log($"Best: {num}");
							}
							source = activeLineCastResult.IntersectedObjectPosition + array2[num] * 0.5f;
							Stage.CellFaces[num].DebugDraw(activeLineCastResult.IntersectedObjectPosition, Color.cyan, 0.1f);
							bool num4 = Mathf.RoundToInt(propGhostTransform.rotation.eulerAngles.y / 90f) % 2 == 1;
							float num5 = (float)(num4 ? PropDimensions.z : PropDimensions.x) * 0.5f;
							float num6 = (float)(num4 ? PropDimensions.x : PropDimensions.z) * 0.5f;
							switch (num)
							{
							case 0:
								source += array2[num] * num6;
								break;
							case 1:
								source += array2[num] * num6;
								break;
							case 2:
								source += array2[num] * num5;
								break;
							case 3:
								source += array2[num] * num5;
								break;
							case 4:
								source = activeLineCastResult.IntersectedObjectPosition + array2[num] * PropDimensions.y;
								break;
							case 5:
								source = planeSampledPosition;
								break;
							}
						}
						propGhostTransform.position = GetBestPosition(source, runtimePropInfo);
					}
				}
			}
			else
			{
				propGhostTransform.transform.position = ConvertToRoundedPosition(propGhostTransform.eulerAngles.y, activeLineCastResult.NearestPositionToObject);
			}
		}
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		array = (from x in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(runtimePropInfo.PropData, propGhostTransform.position, propGhostTransform.rotation)
			select x.Offset).ToArray();
		invalidOverlaps.Clear();
		for (int num7 = 0; num7 < array.Length; num7++)
		{
			Cell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<Cell>(array[num7]);
			if (cellFromCoordinateAs != null && (!(cellFromCoordinateAs is PropCell propCell) || !(propCell.InstanceId == LinecastExclusionId)))
			{
				invalidOverlaps.Add(array[num7]);
			}
		}
		MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForGhostProp(ActiveAssetId, propGhostTransform.position, propGhostTransform.rotation, PropLocationType.Selected);
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(invalidOverlaps, MarkerType.Erase);
	}

	private Vector3 GetBestPosition(Vector3 source, PropLibrary.RuntimePropInfo metadata)
	{
		Vector3 vector = ConvertToRoundedPosition(propGhostTransform.eulerAngles.y, source);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(metadata.PropData, vector, propGhostTransform.rotation, LinecastExclusionId))
		{
			return vector;
		}
		vector = ConvertToRoundedPosition(propGhostTransform.eulerAngles.y, source, MidpointRounding.ToEven, flipSign: true);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(metadata.PropData, vector, propGhostTransform.rotation, LinecastExclusionId))
		{
			return vector;
		}
		vector = ConvertToRoundedPosition(propGhostTransform.eulerAngles.y, source, MidpointRounding.AwayFromZero);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(metadata.PropData, vector, propGhostTransform.rotation, LinecastExclusionId))
		{
			return vector;
		}
		vector = ConvertToRoundedPosition(propGhostTransform.eulerAngles.y, source, MidpointRounding.AwayFromZero, flipSign: true);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(metadata.PropData, vector, propGhostTransform.rotation, LinecastExclusionId))
		{
			return vector;
		}
		return source;
	}

	protected Vector3 ConvertToRoundedPosition(float yRotationAngles, Vector3 position, MidpointRounding roundingStrategy = MidpointRounding.ToEven, bool flipSign = false)
	{
		int num = Mathf.FloorToInt(yRotationAngles / 90f);
		Vector3 result = position;
		bool flag = ((num % 2 == 0) ? selectedProp.ApplyXOffset : selectedProp.ApplyZOffset);
		bool num2 = ((num % 2 == 0) ? selectedProp.ApplyZOffset : selectedProp.ApplyXOffset);
		if (flag)
		{
			result.x = MathF.Round(result.x + (flipSign ? (-0.5f) : 0.5f), roundingStrategy) + (flipSign ? 0.5f : (-0.5f));
		}
		else
		{
			result.x = Mathf.RoundToInt(result.x);
		}
		if (num2)
		{
			result.z = MathF.Round(result.z + (flipSign ? (-0.5f) : 0.5f), roundingStrategy) + (flipSign ? 0.5f : (-0.5f));
		}
		else
		{
			result.z = Mathf.RoundToInt(result.z);
		}
		return result;
	}

	private Vector3 GetPlaneSampledPosition(LineCastHit hit)
	{
		Vector3 normalized = base.ActiveRay.direction.normalized;
		Vector3 vector = ((base.ActiveRay.origin.y > (float)hit.NearestPositionToObject.y) ? (-Vector3.up * 0.5f) : (Vector3.up * 0.5f));
		new Plane(normalized, hit.NearestPositionToObject + vector).Raycast(base.ActiveRay, out var enter);
		Vector3 point = base.ActiveRay.GetPoint(enter);
		point.y = hit.NearestPositionToObject.y;
		return point;
	}

	protected SerializableGuid GetPropUnderCursor()
	{
		LineCastHit activeLineCastResult = base.ActiveLineCastResult;
		if (!activeLineCastResult.IntersectionOccured)
		{
			return SerializableGuid.Empty;
		}
		return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition)?.InstanceId ?? SerializableGuid.Empty;
	}

	protected void GeneratePropPreview(PropLibrary.RuntimePropInfo runtimePropInfo, SerializableGuid sourceInstanceId = default(SerializableGuid))
	{
		if (activeAssetId != runtimePropInfo.PropData.AssetID)
		{
			if (activeAssetId != SerializableGuid.Empty)
			{
				DestroyPreview();
			}
			SetPropDataFromPropInfo(runtimePropInfo, sourceInstanceId);
			SpawnPreview(runtimePropInfo);
		}
		else if (propGhostTransform == null)
		{
			SpawnPreview(runtimePropInfo);
		}
	}

	protected void SpawnPreview(PropLibrary.RuntimePropInfo runtimePropInfo)
	{
		if ((SerializableGuid)runtimePropInfo.PropData.AssetID != activeAssetId)
		{
			Debug.LogError("Attempted to spawn preview with a prop that isn't the selected prop!");
		}
		if (propGhostTransform != null)
		{
			UnityEngine.Object.Destroy(propGhostTransform.gameObject);
			propGhostTransform = null;
		}
		propGhostTransform = UnityEngine.Object.Instantiate(runtimePropInfo.EndlessProp).GetComponent<Transform>();
		PurgeNonRenderMeshesFromGhost(PropGhostTransform.transform);
		propGhostTransform.gameObject.SetActive(!base.IsMobile);
	}

	protected void SetPropDataFromPropInfo(PropLibrary.RuntimePropInfo runtimePropInfo, SerializableGuid sourceInstanceId = default(SerializableGuid))
	{
		PropDimensions = runtimePropInfo.PropData.GetBoundingSize();
		selectedProp = runtimePropInfo.PropData;
		activeAssetId = runtimePropInfo.PropData.AssetID;
		LinecastExclusionId = sourceInstanceId;
	}

	protected void ClearLineCastExclusionId()
	{
		LinecastExclusionId = SerializableGuid.Empty;
	}

	protected void ClearActiveAssetId()
	{
		selectedProp = null;
		activeAssetId = SerializableGuid.Empty;
	}

	private void PurgeNonRenderMeshesFromGhost(Transform targetTransform)
	{
		Component[] components = targetTransform.GetComponents<Component>();
		foreach (Component component in components)
		{
			if (component is Collider)
			{
				UnityEngine.Object.Destroy(component);
			}
		}
		for (int j = 0; j < targetTransform.childCount; j++)
		{
			PurgeNonRenderMeshesFromGhost(targetTransform.GetChild(j));
		}
	}

	protected void FirePropPlacedAnalyticEvent(SerializableGuid propId, string propName)
	{
		FirePropEvent("propPlaced", propId, propName);
	}

	protected void FirePropEvent(string eventName, SerializableGuid propId, string propName)
	{
		CustomEvent e = new CustomEvent(eventName)
		{
			{ "propName1", propName },
			{
				"propId1",
				propId.ToString()
			}
		};
		AnalyticsService.Instance.RecordEvent(e);
	}

	protected void ClearPreviousAssetId()
	{
		previousAssetId = SerializableGuid.Empty;
	}

	public override void ToolSecondaryPressed()
	{
		base.ToolSecondaryPressed();
		MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
		invalidOverlaps.Clear();
		rotationIndicator.gameObject.SetActive(value: false);
		DestroyPreview();
	}

	public override void SessionEnded()
	{
		base.SessionEnded();
		HandleDeselected();
	}

	public override void CreatorExited()
	{
		base.CreatorExited();
		HandleDeselected();
	}

	protected override SerializableGuid GetExcludedAssetId()
	{
		return LinecastExclusionId;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "PropBasedTool";
	}
}
