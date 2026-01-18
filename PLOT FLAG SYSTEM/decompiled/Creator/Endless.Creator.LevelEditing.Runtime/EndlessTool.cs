using Endless.Creator.UI;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Creator.LevelEditing.Runtime;

public abstract class EndlessTool : NetworkBehaviour
{
	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private float maxSelectionDistance;

	[SerializeField]
	private float lineCastScalar;

	[SerializeField]
	private bool useIntersectionFor3DCursor = true;

	[SerializeField]
	private float fallbackVoidDistance = 10f;

	public float MaxSelectionDistance => maxSelectionDistance;

	public Sprite Icon => icon;

	public Ray ActiveRay { get; set; }

	public abstract ToolType ToolType { get; }

	public string ToolTypeName => GetType().Name;

	protected UIToolPrompterManager UIToolPrompter => MonoBehaviourSingleton<UIToolPrompterManager>.Instance;

	public bool IsActive => MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool == this;

	protected bool IsMobile => MobileUtility.IsMobile;

	public virtual bool PerformsLineCast { get; } = true;

	protected ToolState ToolState { get; set; }

	protected LineCastHit ActiveLineCastResult { get; private set; }

	public bool AutoPlace3DCursor { get; set; } = true;

	public virtual void HandleSelected()
	{
		ToolState = ToolState.None;
		if (!PerformsLineCast)
		{
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue));
		}
	}

	public virtual void HandleDeselected()
	{
		ToolState = ToolState.None;
	}

	public virtual void ToolPressed()
	{
		ToolState = ToolState.Pressed;
	}

	public virtual void ToolSecondaryPressed()
	{
	}

	public virtual void ToolHeld()
	{
		ToolState = ToolState.Held;
	}

	public virtual void ToolReleased()
	{
		ToolState = ToolState.None;
	}

	public virtual void UpdateTool()
	{
		if (PerformsLineCast)
		{
			Update3DCursorLocation();
		}
	}

	protected void Update3DCursorLocation(SerializableGuid excludedId = default(SerializableGuid))
	{
		if (!AutoPlace3DCursor)
		{
			return;
		}
		if (EventSystem.current.IsPointerOverGameObject())
		{
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue));
			return;
		}
		LineCastHit activeLineCastResult = ActiveLineCastResult;
		if (useIntersectionFor3DCursor)
		{
			if (activeLineCastResult.IntersectionOccured)
			{
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(activeLineCastResult.IntersectedObjectPosition);
			}
			else
			{
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(activeLineCastResult.NearestPositionToObject);
			}
		}
		else
		{
			MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(activeLineCastResult.NearestPositionToObject);
		}
	}

	private bool RayOriginIsOutOfBounds(Vector3 activeRayOrigin)
	{
		return !MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanPaintCellPosition(Stage.WorldSpacePointToGridCoordinate(activeRayOrigin));
	}

	public virtual void CreatorExited()
	{
	}

	public virtual void SessionEnded()
	{
	}

	public void PerformAndCacheLineCast()
	{
		ActiveLineCastResult = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Linecast(ActiveRay, maxSelectionDistance, lineCastScalar, fallbackVoidDistance, GetExcludedAssetId());
	}

	protected virtual SerializableGuid GetExcludedAssetId()
	{
		return default(SerializableGuid);
	}

	protected bool PerformRaycast(out RaycastHit hit, int layerMask)
	{
		return Physics.Raycast(ActiveRay, out hit, maxSelectionDistance, layerMask);
	}

	public virtual void Reset()
	{
	}

	public void Set3DCursorUsesIntersection(bool val)
	{
		useIntersectionFor3DCursor = val;
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
		return "EndlessTool";
	}
}
