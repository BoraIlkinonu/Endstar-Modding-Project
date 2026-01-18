using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200037A RID: 890
	public abstract class PropBasedTool : EndlessTool
	{
		// Token: 0x17000288 RID: 648
		// (get) Token: 0x0600110A RID: 4362 RVA: 0x000527E6 File Offset: 0x000509E6
		protected bool IsLoadingProp
		{
			get
			{
				return this.inFlightLoads.Any((PropLibrary.RuntimePropInfo propInfo) => propInfo.PropData.AssetID == this.activeAssetId);
			}
		}

		// Token: 0x17000289 RID: 649
		// (get) Token: 0x0600110B RID: 4363 RVA: 0x000527FF File Offset: 0x000509FF
		protected HashSet<PropLibrary.RuntimePropInfo> InFlightLoads
		{
			get
			{
				return this.inFlightLoads;
			}
		}

		// Token: 0x1700028A RID: 650
		// (get) Token: 0x0600110C RID: 4364 RVA: 0x00052807 File Offset: 0x00050A07
		// (set) Token: 0x0600110D RID: 4365 RVA: 0x0005280F File Offset: 0x00050A0F
		protected bool Rotating { get; set; }

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x0600110E RID: 4366 RVA: 0x00052818 File Offset: 0x00050A18
		// (set) Token: 0x0600110F RID: 4367 RVA: 0x00052820 File Offset: 0x00050A20
		protected Vector3Int PropDimensions { get; set; } = Vector3Int.one;

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06001110 RID: 4368 RVA: 0x00052829 File Offset: 0x00050A29
		protected Transform PropGhostTransform
		{
			get
			{
				return this.propGhostTransform;
			}
		}

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06001111 RID: 4369 RVA: 0x00052831 File Offset: 0x00050A31
		protected SerializableGuid ActiveAssetId
		{
			get
			{
				return this.activeAssetId;
			}
		}

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x06001112 RID: 4370 RVA: 0x00052839 File Offset: 0x00050A39
		protected SerializableGuid PreviousAssetId
		{
			get
			{
				return this.previousAssetId;
			}
		}

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x06001113 RID: 4371 RVA: 0x00052841 File Offset: 0x00050A41
		// (set) Token: 0x06001114 RID: 4372 RVA: 0x00052849 File Offset: 0x00050A49
		protected SerializableGuid LinecastExclusionId { get; set; }

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x06001115 RID: 4373 RVA: 0x00052852 File Offset: 0x00050A52
		protected bool ToolIsPressed
		{
			get
			{
				return this.toolIsPressed;
			}
		}

		// Token: 0x06001116 RID: 4374 RVA: 0x0005285A File Offset: 0x00050A5A
		public override void HandleDeselected()
		{
			base.HandleDeselected();
			MonoBehaviourSingleton<PropLocationMarker>.Instance.ClearAllMarkers();
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			this.invalidOverlaps.Clear();
			this.rotationIndicator.gameObject.SetActive(false);
			this.DestroyPreview();
		}

		// Token: 0x06001117 RID: 4375 RVA: 0x00052898 File Offset: 0x00050A98
		public override void ToolPressed()
		{
			if (this.IsLoadingProp)
			{
				return;
			}
			base.ToolPressed();
			this.initialRayOrigin = base.ActiveRay.origin;
			this.toolIsPressed = true;
			if (!this.propGhostTransform)
			{
				return;
			}
			this.UpdatePropPlacement();
			this.initialPropPlacement = this.propGhostTransform.transform.position;
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			this.originalRotationSamplePoint = this.GetPlaneSampledPosition(activeLineCastResult);
		}

		// Token: 0x06001118 RID: 4376 RVA: 0x00052910 File Offset: 0x00050B10
		public override void ToolHeld()
		{
			if (this.IsLoadingProp || !this.toolIsPressed)
			{
				return;
			}
			base.ToolHeld();
			if (base.IsMobile && !this.propGhostTransform.gameObject.activeSelf)
			{
				this.propGhostTransform.gameObject.SetActive(true);
			}
			if ((this.initialRayOrigin - base.ActiveRay.origin).magnitude < this.toolDragDeadZone)
			{
				return;
			}
			if (this.propGhostTransform == null)
			{
				return;
			}
			this.UpdatePropGhostTransform();
		}

		// Token: 0x06001119 RID: 4377 RVA: 0x000529A0 File Offset: 0x00050BA0
		private void UpdatePropGhostTransform()
		{
			Plane plane = new Plane(global::UnityEngine.Vector3.up, this.propGhostTransform.position);
			float num;
			if (plane.Raycast(base.ActiveRay, out num))
			{
				global::UnityEngine.Vector3 point = base.ActiveRay.GetPoint(num);
				LineCastHit activeLineCastResult = base.ActiveLineCastResult;
				if (!this.Rotating)
				{
					Debug.Log(string.Format("rotation offset: {0}", this.rotationOffset));
					this.rotationIndicator.gameObject.SetActive(true);
					this.originalRotationSamplePoint = this.propGhostTransform.transform.position;
				}
				int num2 = Mathf.RoundToInt(Quaternion.LookRotation(point - this.initialPropPlacement, global::UnityEngine.Vector3.up).eulerAngles.y / 90f);
				this.propGhostTransform.rotation = Quaternion.Euler(0f, (float)(num2 * 90), 0f);
				this.rotationIndicator.rotation = this.propGhostTransform.rotation;
				this.Rotating = true;
				global::UnityEngine.Vector3 vector = this.initialPropPlacement;
				vector = this.ConvertToRoundedPosition(this.propGhostTransform.eulerAngles.y, this.originalRotationSamplePoint, MidpointRounding.ToEven, false);
				this.propGhostTransform.transform.position = vector;
				this.rotationIndicator.transform.position = vector;
			}
		}

		// Token: 0x0600111A RID: 4378 RVA: 0x00052AEB File Offset: 0x00050CEB
		public override void ToolReleased()
		{
			base.ToolReleased();
			if (!this.toolIsPressed)
			{
				return;
			}
			if (this.IsLoadingProp)
			{
				return;
			}
			this.EndRotating();
			this.toolIsPressed = false;
			if (base.IsMobile)
			{
				this.propGhostTransform.gameObject.SetActive(false);
			}
		}

		// Token: 0x0600111B RID: 4379 RVA: 0x00052B2B File Offset: 0x00050D2B
		protected void EndRotating()
		{
			this.Rotating = false;
			this.rotationIndicator.gameObject.SetActive(false);
		}

		// Token: 0x0600111C RID: 4380 RVA: 0x00052B45 File Offset: 0x00050D45
		protected bool NoInvalidOverlapsExist()
		{
			return this.invalidOverlaps.Count == 0;
		}

		// Token: 0x0600111D RID: 4381 RVA: 0x00052B58 File Offset: 0x00050D58
		protected void DestroyPreview()
		{
			if (!this.propGhostTransform)
			{
				return;
			}
			MonoBehaviourSingleton<PropLocationMarker>.Instance.UnmarkPropAndType(this.activeAssetId, PropLocationType.Selected);
			this.previousAssetId = this.activeAssetId;
			this.activeAssetId = SerializableGuid.Empty;
			global::UnityEngine.Object.Destroy(this.propGhostTransform.gameObject);
			this.propGhostTransform = null;
		}

		// Token: 0x0600111E RID: 4382 RVA: 0x00052BB4 File Offset: 0x00050DB4
		public override void UpdateTool()
		{
			base.Update3DCursorLocation(this.LinecastExclusionId);
			if (EndlessInput.GetKeyDown(Key.J))
			{
				this.usePivotStyleRotation = !this.usePivotStyleRotation;
			}
			if (!base.IsMobile && EndlessInput.GetKeyDown(Key.R) && base.ToolState == ToolState.None && this.propGhostTransform)
			{
				float num = this.propGhostTransform.rotation.eulerAngles.y / 90f;
				num = (num + 1f) % 4f;
				this.propGhostTransform.rotation = Quaternion.Euler(0f, num * 90f, 0f);
			}
		}

		// Token: 0x0600111F RID: 4383 RVA: 0x00052C5C File Offset: 0x00050E5C
		protected void UpdatePropPlacement()
		{
			if (!this.propGhostTransform)
			{
				return;
			}
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(this.activeAssetId);
			Vector3Int[] array = Array.Empty<Vector3Int>();
			if (!this.Rotating)
			{
				if (activeLineCastResult.IntersectionOccured)
				{
					if (runtimePropInfo != null)
					{
						global::UnityEngine.Vector3 planeSampledPosition = this.GetPlaneSampledPosition(activeLineCastResult);
						global::UnityEngine.Vector3 vector = this.ConvertToRoundedPosition(this.propGhostTransform.eulerAngles.y, planeSampledPosition, MidpointRounding.ToEven, false);
						if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(runtimePropInfo.PropData, vector, this.propGhostTransform.rotation, this.LinecastExclusionId))
						{
							this.propGhostTransform.transform.position = vector;
						}
						else
						{
							global::UnityEngine.Vector3[] array2 = new global::UnityEngine.Vector3[]
							{
								global::UnityEngine.Vector3.forward,
								global::UnityEngine.Vector3.back,
								global::UnityEngine.Vector3.left,
								global::UnityEngine.Vector3.right,
								global::UnityEngine.Vector3.down,
								global::UnityEngine.Vector3.up
							};
							int num = -1;
							float num2 = float.MinValue;
							global::UnityEngine.Vector3 vector2 = activeLineCastResult.NearestPositionToObject - activeLineCastResult.IntersectedObjectPosition;
							vector2.Normalize();
							for (int i = 0; i < array2.Length; i++)
							{
								float num3 = global::UnityEngine.Vector3.Dot(vector2, array2[i]);
								Cell cellFromCoordinate = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(activeLineCastResult.IntersectedObjectPosition + Vector3Int.FloorToInt(array2[i]));
								if (num3 > num2 && cellFromCoordinate == null)
								{
									num = i;
									num2 = num3;
								}
							}
							global::UnityEngine.Vector3 vector3 = vector;
							if (num >= 0)
							{
								if (EndlessInput.GetKeyDown(Key.V))
								{
									Debug.Log(string.Format("Best: {0}", num));
								}
								vector3 = activeLineCastResult.IntersectedObjectPosition + array2[num] * 0.5f;
								Stage.CellFaces[num].DebugDraw(activeLineCastResult.IntersectedObjectPosition, global::UnityEngine.Color.cyan, 0.1f);
								bool flag = Mathf.RoundToInt(this.propGhostTransform.rotation.eulerAngles.y / 90f) % 2 == 1;
								float num4 = (float)(flag ? this.PropDimensions.z : this.PropDimensions.x) * 0.5f;
								float num5 = (float)(flag ? this.PropDimensions.x : this.PropDimensions.z) * 0.5f;
								switch (num)
								{
								case 0:
									vector3 += array2[num] * num5;
									break;
								case 1:
									vector3 += array2[num] * num5;
									break;
								case 2:
									vector3 += array2[num] * num4;
									break;
								case 3:
									vector3 += array2[num] * num4;
									break;
								case 4:
									vector3 = activeLineCastResult.IntersectedObjectPosition + array2[num] * (float)this.PropDimensions.y;
									break;
								case 5:
									vector3 = planeSampledPosition;
									break;
								}
							}
							this.propGhostTransform.position = this.GetBestPosition(vector3, runtimePropInfo);
						}
					}
				}
				else
				{
					this.propGhostTransform.transform.position = this.ConvertToRoundedPosition(this.propGhostTransform.eulerAngles.y, activeLineCastResult.NearestPositionToObject, MidpointRounding.ToEven, false);
				}
			}
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			array = (from x in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(runtimePropInfo.PropData, this.propGhostTransform.position, this.propGhostTransform.rotation)
				select x.Offset).ToArray<Vector3Int>();
			this.invalidOverlaps.Clear();
			for (int j = 0; j < array.Length; j++)
			{
				Cell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<Cell>(array[j]);
				if (cellFromCoordinateAs != null)
				{
					PropCell propCell = cellFromCoordinateAs as PropCell;
					if (propCell == null || !(propCell.InstanceId == this.LinecastExclusionId))
					{
						this.invalidOverlaps.Add(array[j]);
					}
				}
			}
			MonoBehaviourSingleton<PropLocationMarker>.Instance.MarkCellsForGhostProp(this.ActiveAssetId, this.propGhostTransform.position, this.propGhostTransform.rotation, PropLocationType.Selected);
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinates(this.invalidOverlaps, MarkerType.Erase);
		}

		// Token: 0x06001120 RID: 4384 RVA: 0x00053104 File Offset: 0x00051304
		private global::UnityEngine.Vector3 GetBestPosition(global::UnityEngine.Vector3 source, PropLibrary.RuntimePropInfo metadata)
		{
			global::UnityEngine.Vector3 vector = this.ConvertToRoundedPosition(this.propGhostTransform.eulerAngles.y, source, MidpointRounding.ToEven, false);
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(metadata.PropData, vector, this.propGhostTransform.rotation, this.LinecastExclusionId))
			{
				return vector;
			}
			vector = this.ConvertToRoundedPosition(this.propGhostTransform.eulerAngles.y, source, MidpointRounding.ToEven, true);
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(metadata.PropData, vector, this.propGhostTransform.rotation, this.LinecastExclusionId))
			{
				return vector;
			}
			vector = this.ConvertToRoundedPosition(this.propGhostTransform.eulerAngles.y, source, MidpointRounding.AwayFromZero, false);
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(metadata.PropData, vector, this.propGhostTransform.rotation, this.LinecastExclusionId))
			{
				return vector;
			}
			vector = this.ConvertToRoundedPosition(this.propGhostTransform.eulerAngles.y, source, MidpointRounding.AwayFromZero, true);
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.PlacementIsValidAllowingSelfOverlap(metadata.PropData, vector, this.propGhostTransform.rotation, this.LinecastExclusionId))
			{
				return vector;
			}
			return source;
		}

		// Token: 0x06001121 RID: 4385 RVA: 0x00053228 File Offset: 0x00051428
		protected global::UnityEngine.Vector3 ConvertToRoundedPosition(float yRotationAngles, global::UnityEngine.Vector3 position, MidpointRounding roundingStrategy = MidpointRounding.ToEven, bool flipSign = false)
		{
			int num = Mathf.FloorToInt(yRotationAngles / 90f);
			global::UnityEngine.Vector3 vector = position;
			bool flag = ((num % 2 == 0) ? this.selectedProp.ApplyXOffset : this.selectedProp.ApplyZOffset);
			bool flag2 = ((num % 2 == 0) ? this.selectedProp.ApplyZOffset : this.selectedProp.ApplyXOffset);
			if (flag)
			{
				vector.x = MathF.Round(vector.x + (flipSign ? (-0.5f) : 0.5f), roundingStrategy) + (flipSign ? 0.5f : (-0.5f));
			}
			else
			{
				vector.x = (float)Mathf.RoundToInt(vector.x);
			}
			if (flag2)
			{
				vector.z = MathF.Round(vector.z + (flipSign ? (-0.5f) : 0.5f), roundingStrategy) + (flipSign ? 0.5f : (-0.5f));
			}
			else
			{
				vector.z = (float)Mathf.RoundToInt(vector.z);
			}
			return vector;
		}

		// Token: 0x06001122 RID: 4386 RVA: 0x00053318 File Offset: 0x00051518
		private global::UnityEngine.Vector3 GetPlaneSampledPosition(LineCastHit hit)
		{
			global::UnityEngine.Vector3 normalized = base.ActiveRay.direction.normalized;
			global::UnityEngine.Vector3 vector = ((base.ActiveRay.origin.y > (float)hit.NearestPositionToObject.y) ? (-global::UnityEngine.Vector3.up * 0.5f) : (global::UnityEngine.Vector3.up * 0.5f));
			Plane plane = new Plane(normalized, hit.NearestPositionToObject + vector);
			float num;
			plane.Raycast(base.ActiveRay, out num);
			global::UnityEngine.Vector3 point = base.ActiveRay.GetPoint(num);
			point.y = (float)hit.NearestPositionToObject.y;
			return point;
		}

		// Token: 0x06001123 RID: 4387 RVA: 0x000533E4 File Offset: 0x000515E4
		protected SerializableGuid GetPropUnderCursor()
		{
			LineCastHit activeLineCastResult = base.ActiveLineCastResult;
			if (!activeLineCastResult.IntersectionOccured)
			{
				return SerializableGuid.Empty;
			}
			PropCell cellFromCoordinateAs = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinateAs<PropCell>(activeLineCastResult.IntersectedObjectPosition);
			if (cellFromCoordinateAs == null)
			{
				return SerializableGuid.Empty;
			}
			return cellFromCoordinateAs.InstanceId;
		}

		// Token: 0x06001124 RID: 4388 RVA: 0x00053430 File Offset: 0x00051630
		protected void GeneratePropPreview(PropLibrary.RuntimePropInfo runtimePropInfo, SerializableGuid sourceInstanceId = default(SerializableGuid))
		{
			if (this.activeAssetId != runtimePropInfo.PropData.AssetID)
			{
				if (this.activeAssetId != SerializableGuid.Empty)
				{
					this.DestroyPreview();
				}
				this.SetPropDataFromPropInfo(runtimePropInfo, sourceInstanceId);
				this.SpawnPreview(runtimePropInfo);
				return;
			}
			if (this.propGhostTransform == null)
			{
				this.SpawnPreview(runtimePropInfo);
			}
		}

		// Token: 0x06001125 RID: 4389 RVA: 0x00053494 File Offset: 0x00051694
		protected void SpawnPreview(PropLibrary.RuntimePropInfo runtimePropInfo)
		{
			if (runtimePropInfo.PropData.AssetID != this.activeAssetId)
			{
				Debug.LogError("Attempted to spawn preview with a prop that isn't the selected prop!");
			}
			if (this.propGhostTransform != null)
			{
				global::UnityEngine.Object.Destroy(this.propGhostTransform.gameObject);
				this.propGhostTransform = null;
			}
			this.propGhostTransform = global::UnityEngine.Object.Instantiate<EndlessProp>(runtimePropInfo.EndlessProp).GetComponent<Transform>();
			this.PurgeNonRenderMeshesFromGhost(this.PropGhostTransform.transform);
			this.propGhostTransform.gameObject.SetActive(!base.IsMobile);
		}

		// Token: 0x06001126 RID: 4390 RVA: 0x0005352D File Offset: 0x0005172D
		protected void SetPropDataFromPropInfo(PropLibrary.RuntimePropInfo runtimePropInfo, SerializableGuid sourceInstanceId = default(SerializableGuid))
		{
			this.PropDimensions = runtimePropInfo.PropData.GetBoundingSize();
			this.selectedProp = runtimePropInfo.PropData;
			this.activeAssetId = runtimePropInfo.PropData.AssetID;
			this.LinecastExclusionId = sourceInstanceId;
		}

		// Token: 0x06001127 RID: 4391 RVA: 0x00053569 File Offset: 0x00051769
		protected void ClearLineCastExclusionId()
		{
			this.LinecastExclusionId = SerializableGuid.Empty;
		}

		// Token: 0x06001128 RID: 4392 RVA: 0x00053576 File Offset: 0x00051776
		protected void ClearActiveAssetId()
		{
			this.selectedProp = null;
			this.activeAssetId = SerializableGuid.Empty;
		}

		// Token: 0x06001129 RID: 4393 RVA: 0x0005358C File Offset: 0x0005178C
		private void PurgeNonRenderMeshesFromGhost(Transform targetTransform)
		{
			foreach (Component component in targetTransform.GetComponents<Component>())
			{
				if (component is Collider)
				{
					global::UnityEngine.Object.Destroy(component);
				}
			}
			for (int j = 0; j < targetTransform.childCount; j++)
			{
				this.PurgeNonRenderMeshesFromGhost(targetTransform.GetChild(j));
			}
		}

		// Token: 0x0600112A RID: 4394 RVA: 0x000535DE File Offset: 0x000517DE
		protected void FirePropPlacedAnalyticEvent(SerializableGuid propId, string propName)
		{
			this.FirePropEvent("propPlaced", propId, propName);
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x000535F0 File Offset: 0x000517F0
		protected void FirePropEvent(string eventName, SerializableGuid propId, string propName)
		{
			CustomEvent customEvent = new CustomEvent(eventName)
			{
				{ "propName1", propName },
				{
					"propId1",
					propId.ToString()
				}
			};
			AnalyticsService.Instance.RecordEvent(customEvent);
		}

		// Token: 0x0600112C RID: 4396 RVA: 0x00053633 File Offset: 0x00051833
		protected void ClearPreviousAssetId()
		{
			this.previousAssetId = SerializableGuid.Empty;
		}

		// Token: 0x0600112D RID: 4397 RVA: 0x00053640 File Offset: 0x00051840
		public override void ToolSecondaryPressed()
		{
			base.ToolSecondaryPressed();
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			this.invalidOverlaps.Clear();
			this.rotationIndicator.gameObject.SetActive(false);
			this.DestroyPreview();
		}

		// Token: 0x0600112E RID: 4398 RVA: 0x00053674 File Offset: 0x00051874
		public override void SessionEnded()
		{
			base.SessionEnded();
			this.HandleDeselected();
		}

		// Token: 0x0600112F RID: 4399 RVA: 0x00053682 File Offset: 0x00051882
		public override void CreatorExited()
		{
			base.CreatorExited();
			this.HandleDeselected();
		}

		// Token: 0x06001130 RID: 4400 RVA: 0x00053690 File Offset: 0x00051890
		protected override SerializableGuid GetExcludedAssetId()
		{
			return this.LinecastExclusionId;
		}

		// Token: 0x06001133 RID: 4403 RVA: 0x00053708 File Offset: 0x00051908
		[CompilerGenerated]
		private ValueTuple<global::UnityEngine.Vector3, Vector3Int> <UpdatePropGhostTransform>g__GetOffsetToNearestCell|42_0()
		{
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(this.activeAssetId);
			Vector3Int[] array = (from x in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetRotatedPropLocations(runtimePropInfo.PropData, this.propGhostTransform.position, this.propGhostTransform.rotation)
				select x.Offset).ToArray<Vector3Int>();
			global::UnityEngine.Vector3 vector = new global::UnityEngine.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3Int vector3Int = Vector3Int.zero;
			for (int i = 0; i < array.Length; i++)
			{
				global::UnityEngine.Vector3 vector2 = array[i] - this.propGhostTransform.position;
				if (vector2.sqrMagnitude < vector.sqrMagnitude)
				{
					vector3Int = array[i];
					vector = vector2;
				}
			}
			return new ValueTuple<global::UnityEngine.Vector3, Vector3Int>(vector, vector3Int);
		}

		// Token: 0x06001134 RID: 4404 RVA: 0x000537F0 File Offset: 0x000519F0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001135 RID: 4405 RVA: 0x00049EBA File Offset: 0x000480BA
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x00053806 File Offset: 0x00051A06
		protected internal override string __getTypeName()
		{
			return "PropBasedTool";
		}

		// Token: 0x04000DFA RID: 3578
		[SerializeField]
		private float toolDragDeadZone = 0.01f;

		// Token: 0x04000DFB RID: 3579
		[SerializeField]
		private Transform rotationIndicator;

		// Token: 0x04000DFC RID: 3580
		[SerializeField]
		[Tooltip("Max Void Distance is used when no object is collided against, how far out do we let the user paint. Essentially, in an empty level, how far into the void can they paint with no other props.")]
		private float maxVoidDistance = 10f;

		// Token: 0x04000DFD RID: 3581
		private Transform propGhostTransform;

		// Token: 0x04000DFE RID: 3582
		private global::UnityEngine.Vector3 initialRayOrigin;

		// Token: 0x04000DFF RID: 3583
		private bool toolIsPressed;

		// Token: 0x04000E00 RID: 3584
		private global::UnityEngine.Vector3 initialPropPlacement;

		// Token: 0x04000E01 RID: 3585
		private global::UnityEngine.Vector3 originalRotationSamplePoint;

		// Token: 0x04000E02 RID: 3586
		private List<Vector3Int> invalidOverlaps = new List<Vector3Int>();

		// Token: 0x04000E03 RID: 3587
		private SerializableGuid activeAssetId;

		// Token: 0x04000E04 RID: 3588
		private SerializableGuid previousAssetId;

		// Token: 0x04000E05 RID: 3589
		private HashSet<PropLibrary.RuntimePropInfo> inFlightLoads = new HashSet<PropLibrary.RuntimePropInfo>();

		// Token: 0x04000E06 RID: 3590
		private global::UnityEngine.Vector3 rotationOffset;

		// Token: 0x04000E07 RID: 3591
		private Prop selectedProp;

		// Token: 0x04000E0B RID: 3595
		private bool usePivotStyleRotation = true;
	}
}
