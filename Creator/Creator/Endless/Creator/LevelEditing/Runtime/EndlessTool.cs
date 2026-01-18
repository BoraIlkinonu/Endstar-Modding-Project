using System;
using Endless.Creator.UI;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200034D RID: 845
	public abstract class EndlessTool : NetworkBehaviour
	{
		// Token: 0x17000265 RID: 613
		// (get) Token: 0x06000FCB RID: 4043 RVA: 0x00049ECB File Offset: 0x000480CB
		public float MaxSelectionDistance
		{
			get
			{
				return this.maxSelectionDistance;
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x06000FCC RID: 4044 RVA: 0x00049ED3 File Offset: 0x000480D3
		public Sprite Icon
		{
			get
			{
				return this.icon;
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x06000FCD RID: 4045 RVA: 0x00049EDB File Offset: 0x000480DB
		// (set) Token: 0x06000FCE RID: 4046 RVA: 0x00049EE3 File Offset: 0x000480E3
		public Ray ActiveRay { get; set; }

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x06000FCF RID: 4047
		public abstract ToolType ToolType { get; }

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x06000FD0 RID: 4048 RVA: 0x00049EEC File Offset: 0x000480EC
		public string ToolTypeName
		{
			get
			{
				return base.GetType().Name;
			}
		}

		// Token: 0x1700026A RID: 618
		// (get) Token: 0x06000FD1 RID: 4049 RVA: 0x00049EF9 File Offset: 0x000480F9
		protected UIToolPrompterManager UIToolPrompter
		{
			get
			{
				return MonoBehaviourSingleton<UIToolPrompterManager>.Instance;
			}
		}

		// Token: 0x1700026B RID: 619
		// (get) Token: 0x06000FD2 RID: 4050 RVA: 0x00049F00 File Offset: 0x00048100
		public bool IsActive
		{
			get
			{
				return MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool == this;
			}
		}

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x06000FD3 RID: 4051 RVA: 0x00049F12 File Offset: 0x00048112
		protected bool IsMobile
		{
			get
			{
				return MobileUtility.IsMobile;
			}
		}

		// Token: 0x1700026D RID: 621
		// (get) Token: 0x06000FD4 RID: 4052 RVA: 0x00049F19 File Offset: 0x00048119
		public virtual bool PerformsLineCast { get; } = true;

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06000FD5 RID: 4053 RVA: 0x00049F21 File Offset: 0x00048121
		// (set) Token: 0x06000FD6 RID: 4054 RVA: 0x00049F29 File Offset: 0x00048129
		protected ToolState ToolState { get; set; }

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x06000FD7 RID: 4055 RVA: 0x00049F32 File Offset: 0x00048132
		// (set) Token: 0x06000FD8 RID: 4056 RVA: 0x00049F3A File Offset: 0x0004813A
		private protected LineCastHit ActiveLineCastResult { protected get; private set; }

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000FD9 RID: 4057 RVA: 0x00049F43 File Offset: 0x00048143
		// (set) Token: 0x06000FDA RID: 4058 RVA: 0x00049F4B File Offset: 0x0004814B
		public bool AutoPlace3DCursor { get; set; } = true;

		// Token: 0x06000FDB RID: 4059 RVA: 0x00049F54 File Offset: 0x00048154
		public virtual void HandleSelected()
		{
			this.ToolState = ToolState.None;
			if (!this.PerformsLineCast)
			{
				MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue));
			}
		}

		// Token: 0x06000FDC RID: 4060 RVA: 0x00049F83 File Offset: 0x00048183
		public virtual void HandleDeselected()
		{
			this.ToolState = ToolState.None;
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x00049F8C File Offset: 0x0004818C
		public virtual void ToolPressed()
		{
			this.ToolState = ToolState.Pressed;
		}

		// Token: 0x06000FDE RID: 4062 RVA: 0x000056F3 File Offset: 0x000038F3
		public virtual void ToolSecondaryPressed()
		{
		}

		// Token: 0x06000FDF RID: 4063 RVA: 0x00049F95 File Offset: 0x00048195
		public virtual void ToolHeld()
		{
			this.ToolState = ToolState.Held;
		}

		// Token: 0x06000FE0 RID: 4064 RVA: 0x00049F83 File Offset: 0x00048183
		public virtual void ToolReleased()
		{
			this.ToolState = ToolState.None;
		}

		// Token: 0x06000FE1 RID: 4065 RVA: 0x00049FA0 File Offset: 0x000481A0
		public virtual void UpdateTool()
		{
			if (this.PerformsLineCast)
			{
				this.Update3DCursorLocation(default(SerializableGuid));
			}
		}

		// Token: 0x06000FE2 RID: 4066 RVA: 0x00049FC4 File Offset: 0x000481C4
		protected void Update3DCursorLocation(SerializableGuid excludedId = default(SerializableGuid))
		{
			if (this.AutoPlace3DCursor)
			{
				if (EventSystem.current.IsPointerOverGameObject())
				{
					MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue));
					return;
				}
				LineCastHit activeLineCastResult = this.ActiveLineCastResult;
				if (this.useIntersectionFor3DCursor)
				{
					if (activeLineCastResult.IntersectionOccured)
					{
						MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(activeLineCastResult.IntersectedObjectPosition);
						return;
					}
					MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(activeLineCastResult.NearestPositionToObject);
					return;
				}
				else
				{
					MonoBehaviourSingleton<CellMarker>.Instance.MoveTo(activeLineCastResult.NearestPositionToObject);
				}
			}
		}

		// Token: 0x06000FE3 RID: 4067 RVA: 0x0004A051 File Offset: 0x00048251
		private bool RayOriginIsOutOfBounds(Vector3 activeRayOrigin)
		{
			return !MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CanPaintCellPosition(Stage.WorldSpacePointToGridCoordinate(activeRayOrigin));
		}

		// Token: 0x06000FE4 RID: 4068 RVA: 0x000056F3 File Offset: 0x000038F3
		public virtual void CreatorExited()
		{
		}

		// Token: 0x06000FE5 RID: 4069 RVA: 0x000056F3 File Offset: 0x000038F3
		public virtual void SessionEnded()
		{
		}

		// Token: 0x06000FE6 RID: 4070 RVA: 0x0004A06B File Offset: 0x0004826B
		public void PerformAndCacheLineCast()
		{
			this.ActiveLineCastResult = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Linecast(this.ActiveRay, this.maxSelectionDistance, this.lineCastScalar, this.fallbackVoidDistance, this.GetExcludedAssetId());
		}

		// Token: 0x06000FE7 RID: 4071 RVA: 0x0004A0A0 File Offset: 0x000482A0
		protected virtual SerializableGuid GetExcludedAssetId()
		{
			return default(SerializableGuid);
		}

		// Token: 0x06000FE8 RID: 4072 RVA: 0x0004A0B6 File Offset: 0x000482B6
		protected bool PerformRaycast(out RaycastHit hit, int layerMask)
		{
			return Physics.Raycast(this.ActiveRay, out hit, this.maxSelectionDistance, layerMask);
		}

		// Token: 0x06000FE9 RID: 4073 RVA: 0x000056F3 File Offset: 0x000038F3
		public virtual void Reset()
		{
		}

		// Token: 0x06000FEA RID: 4074 RVA: 0x0004A0CB File Offset: 0x000482CB
		public void Set3DCursorUsesIntersection(bool val)
		{
			this.useIntersectionFor3DCursor = val;
		}

		// Token: 0x06000FEC RID: 4076 RVA: 0x0004A0FC File Offset: 0x000482FC
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000FED RID: 4077 RVA: 0x0004A112 File Offset: 0x00048312
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000FEE RID: 4078 RVA: 0x0004A11C File Offset: 0x0004831C
		protected internal override string __getTypeName()
		{
			return "EndlessTool";
		}

		// Token: 0x04000D13 RID: 3347
		[SerializeField]
		private Sprite icon;

		// Token: 0x04000D14 RID: 3348
		[SerializeField]
		private float maxSelectionDistance;

		// Token: 0x04000D15 RID: 3349
		[SerializeField]
		private float lineCastScalar;

		// Token: 0x04000D16 RID: 3350
		[SerializeField]
		private bool useIntersectionFor3DCursor = true;

		// Token: 0x04000D17 RID: 3351
		[SerializeField]
		private float fallbackVoidDistance = 10f;
	}
}
