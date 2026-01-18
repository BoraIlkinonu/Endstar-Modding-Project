using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x0200049B RID: 1179
	public class NpcManager
	{
		// Token: 0x170005A2 RID: 1442
		// (get) Token: 0x06001CF1 RID: 7409 RVA: 0x0007ECDC File Offset: 0x0007CEDC
		internal static NpcManager Instance
		{
			get
			{
				NpcManager npcManager;
				if ((npcManager = NpcManager.instance) == null)
				{
					npcManager = (NpcManager.instance = new NpcManager());
				}
				return npcManager;
			}
		}

		// Token: 0x06001CF2 RID: 7410 RVA: 0x0007ECF4 File Offset: 0x0007CEF4
		public Context SpawnNpc(Context instigator, CellReference cellReference, NpcConfiguration config, Context fallbackContextForPosition)
		{
			Vector3 vector;
			Quaternion quaternion;
			if (cellReference.HasValue)
			{
				vector = cellReference.GetCellPosition();
				quaternion = ((cellReference.Rotation != null) ? Quaternion.Euler(0f, cellReference.Rotation.Value, 0f) : Quaternion.Euler(0f, fallbackContextForPosition.GetYAxisRotation(), 0f));
			}
			else
			{
				vector = fallbackContextForPosition.GetPosition().RoundToVector3Int();
				quaternion = Quaternion.Euler(0f, fallbackContextForPosition.GetYAxisRotation(), 0f);
			}
			return MonoBehaviourSingleton<NpcManager>.Instance.SpawnNpc(vector, quaternion, config);
		}

		// Token: 0x06001CF3 RID: 7411 RVA: 0x0007ED88 File Offset: 0x0007CF88
		public Context SpawnNpc(Context instigator, Vector3 position, float yAxisRotation, NpcConfiguration config)
		{
			return MonoBehaviourSingleton<NpcManager>.Instance.SpawnNpc(position, Quaternion.Euler(0f, yAxisRotation, 0f), config);
		}

		// Token: 0x06001CF4 RID: 7412 RVA: 0x0007EDA8 File Offset: 0x0007CFA8
		public Context SpawnNpc(Context instigator, Vector3 position, NpcConfiguration config, Context rotationContext)
		{
			Quaternion quaternion = Quaternion.Euler(0f, rotationContext.GetYAxisRotation(), 0f);
			return MonoBehaviourSingleton<NpcManager>.Instance.SpawnNpc(position, quaternion, config);
		}

		// Token: 0x06001CF5 RID: 7413 RVA: 0x0007EDD9 File Offset: 0x0007CFD9
		public Context GetNpcInGroupByIndex(int group, int index)
		{
			return MonoBehaviourSingleton<NpcManager>.Instance.GetNpcInGroupByIndex((NpcGroup)group, index);
		}

		// Token: 0x06001CF6 RID: 7414 RVA: 0x0007EDE7 File Offset: 0x0007CFE7
		public int GetNumNpcsByGroup(int group)
		{
			return MonoBehaviourSingleton<NpcManager>.Instance.GetNumNpcsInGroup((NpcGroup)group);
		}

		// Token: 0x06001CF7 RID: 7415 RVA: 0x0007EDF4 File Offset: 0x0007CFF4
		public NpcConfiguration CreateNewConfiguration()
		{
			return new NpcConfiguration();
		}

		// Token: 0x06001CF8 RID: 7416 RVA: 0x0007EDFC File Offset: 0x0007CFFC
		public NpcConfiguration CopyNpcConfiguration(Context context)
		{
			if (!context.IsNpc())
			{
				return new NpcConfiguration();
			}
			NpcEntity userComponent = context.WorldObject.GetUserComponent<NpcEntity>();
			if (userComponent.IsConfigured)
			{
				return new NpcConfiguration(userComponent);
			}
			return new NpcConfiguration();
		}

		// Token: 0x040016CF RID: 5839
		private static NpcManager instance;
	}
}
