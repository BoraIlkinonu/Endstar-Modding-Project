using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200012B RID: 299
	public class NpcBlackboard
	{
		// Token: 0x060006C4 RID: 1732 RVA: 0x000212EC File Offset: 0x0001F4EC
		public NpcBlackboard()
		{
			this.typeDictionary.Add(typeof(int), new Dictionary<NpcBlackboard.Key, int>());
			this.typeDictionary.Add(typeof(float), new Dictionary<NpcBlackboard.Key, float>());
			this.typeDictionary.Add(typeof(bool), new Dictionary<NpcBlackboard.Key, bool>());
			this.typeDictionary.Add(typeof(Vector3), new Dictionary<NpcBlackboard.Key, Vector3>());
			this.typeDictionary.Add(typeof(Quaternion), new Dictionary<NpcBlackboard.Key, Quaternion>());
			this.typeDictionary.Add(typeof(uint), new Dictionary<NpcBlackboard.Key, uint>());
			this.typeDictionary.Add(typeof(object), new Dictionary<NpcBlackboard.Key, object>());
		}

		// Token: 0x060006C5 RID: 1733 RVA: 0x000213C0 File Offset: 0x0001F5C0
		internal void Set<T>(NpcBlackboard.Key key, T value)
		{
			IDictionary dictionary;
			if (!this.typeDictionary.TryGetValue(typeof(T), out dictionary))
			{
				throw new ArgumentException("Unsupported type");
			}
			((Dictionary<NpcBlackboard.Key, T>)dictionary)[key] = value;
		}

		// Token: 0x060006C6 RID: 1734 RVA: 0x00021400 File Offset: 0x0001F600
		public T Get<T>(NpcBlackboard.Key key)
		{
			IDictionary dictionary;
			if (!this.typeDictionary.TryGetValue(typeof(T), out dictionary))
			{
				throw new ArgumentException("Unsupported type");
			}
			return ((Dictionary<NpcBlackboard.Key, T>)dictionary)[key];
		}

		// Token: 0x060006C7 RID: 1735 RVA: 0x00021440 File Offset: 0x0001F640
		public bool TryGet<T>(NpcBlackboard.Key key, out T value)
		{
			IDictionary dictionary;
			if (!this.typeDictionary.TryGetValue(typeof(T), out dictionary))
			{
				throw new ArgumentException("Unsupported type");
			}
			return ((Dictionary<NpcBlackboard.Key, T>)dictionary).TryGetValue(key, out value);
		}

		// Token: 0x060006C8 RID: 1736 RVA: 0x00021480 File Offset: 0x0001F680
		public T GetValueOrDefault<T>(NpcBlackboard.Key key, T defaultValue)
		{
			IDictionary dictionary;
			if (!this.typeDictionary.TryGetValue(typeof(T), out dictionary))
			{
				throw new ArgumentException("Unsupported type");
			}
			return ((Dictionary<NpcBlackboard.Key, T>)dictionary).GetValueOrDefault(key, defaultValue);
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x000214C0 File Offset: 0x0001F6C0
		public void Clear<T>(NpcBlackboard.Key key)
		{
			IDictionary dictionary;
			if (!this.typeDictionary.TryGetValue(typeof(T), out dictionary))
			{
				throw new ArgumentException("Unsupported type");
			}
			((Dictionary<NpcBlackboard.Key, T>)dictionary).Remove(key);
		}

		// Token: 0x04000573 RID: 1395
		private readonly Dictionary<Type, IDictionary> typeDictionary = new Dictionary<Type, IDictionary>();

		// Token: 0x0200012C RID: 300
		public enum Key
		{
			// Token: 0x04000575 RID: 1397
			LastPatrol,
			// Token: 0x04000576 RID: 1398
			RunSpeed,
			// Token: 0x04000577 RID: 1399
			LastWander,
			// Token: 0x04000578 RID: 1400
			CanFidget,
			// Token: 0x04000579 RID: 1401
			IsRangedAttacker,
			// Token: 0x0400057A RID: 1402
			CloseDistance,
			// Token: 0x0400057B RID: 1403
			OptimalAttackDistance,
			// Token: 0x0400057C RID: 1404
			MeleeDistance,
			// Token: 0x0400057D RID: 1405
			NearDistance,
			// Token: 0x0400057E RID: 1406
			AroundDistance,
			// Token: 0x0400057F RID: 1407
			MaxRangedAttackDistance,
			// Token: 0x04000580 RID: 1408
			MaxPerceptionDistance,
			// Token: 0x04000581 RID: 1409
			MaxViewAngle,
			// Token: 0x04000582 RID: 1410
			ProximityDistance,
			// Token: 0x04000583 RID: 1411
			AwarenessGainRate,
			// Token: 0x04000584 RID: 1412
			AwarenessLossRate,
			// Token: 0x04000585 RID: 1413
			DestinationTolerance,
			// Token: 0x04000586 RID: 1414
			JumpPosition,
			// Token: 0x04000587 RID: 1415
			BehaviorDestination,
			// Token: 0x04000588 RID: 1416
			CommandDestination,
			// Token: 0x04000589 RID: 1417
			Rotation,
			// Token: 0x0400058A RID: 1418
			TeleportType,
			// Token: 0x0400058B RID: 1419
			TeleportPosition,
			// Token: 0x0400058C RID: 1420
			TeleportRotation,
			// Token: 0x0400058D RID: 1421
			LastIdleFrame,
			// Token: 0x0400058E RID: 1422
			OverrideSpeed,
			// Token: 0x0400058F RID: 1423
			BoredomTime
		}
	}
}
