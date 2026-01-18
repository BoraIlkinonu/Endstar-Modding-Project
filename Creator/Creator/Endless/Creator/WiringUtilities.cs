using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Creator
{
	// Token: 0x02000096 RID: 150
	public static class WiringUtilities
	{
		// Token: 0x0600024C RID: 588 RVA: 0x00010AE8 File Offset: 0x0000ECE8
		public static WireBundle GetWireBundle(SerializableGuid emitter, SerializableGuid receiver)
		{
			SerializableGuid[] array = new SerializableGuid[] { emitter, receiver };
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetWiresUsingProps(array);
			foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
			{
				if (wireBundle.EmitterInstanceId == emitter && wireBundle.ReceiverInstanceId == receiver)
				{
					return wireBundle;
				}
			}
			return null;
		}

		// Token: 0x0600024D RID: 589 RVA: 0x00010B88 File Offset: 0x0000ED88
		public static SerializableGuid GetWiringId(SerializableGuid emitter, string emitterMemberName, SerializableGuid receiver, string receiverMemberName)
		{
			foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
			{
				if (wireBundle.EmitterInstanceId == emitter && wireBundle.ReceiverInstanceId == receiver)
				{
					foreach (WireEntry wireEntry in wireBundle.Wires)
					{
						if (wireEntry.EmitterMemberName == emitterMemberName && wireEntry.ReceiverMemberName == receiverMemberName)
						{
							return wireEntry.WireId;
						}
					}
				}
			}
			return SerializableGuid.Empty;
		}

		// Token: 0x0600024E RID: 590 RVA: 0x00010C64 File Offset: 0x0000EE64
		public static WireEntry GetWireEntry(SerializableGuid emitter, string emitterMemberName, SerializableGuid receiver, string receiverMemberName)
		{
			WireBundle wireBundle = WiringUtilities.GetWireBundle(emitter, receiver);
			if (wireBundle != null)
			{
				foreach (WireEntry wireEntry in wireBundle.Wires)
				{
					if (wireEntry.EmitterMemberName == emitterMemberName && wireEntry.ReceiverMemberName == receiverMemberName)
					{
						return wireEntry;
					}
				}
			}
			return null;
		}

		// Token: 0x0600024F RID: 591 RVA: 0x00010CE0 File Offset: 0x0000EEE0
		public static IReadOnlyList<WireBundle> GetWiresEmittingFrom(SerializableGuid emitter, string emitterMemberName = null)
		{
			List<WireBundle> list = new List<WireBundle>();
			foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
			{
				if (wireBundle.EmitterInstanceId == emitter)
				{
					if (string.IsNullOrEmpty(emitterMemberName))
					{
						list.Add(wireBundle);
					}
					else
					{
						using (List<WireEntry>.Enumerator enumerator2 = wireBundle.Wires.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								if (enumerator2.Current.EmitterMemberName == emitterMemberName)
								{
									list.Add(wireBundle);
								}
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x06000250 RID: 592 RVA: 0x00010DA8 File Offset: 0x0000EFA8
		public static IReadOnlyList<WireBundle> GetWiresWithAReceiverOf(SerializableGuid receiver, string receiverMemberName = null)
		{
			List<WireBundle> list = new List<WireBundle>();
			foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
			{
				if (wireBundle.ReceiverInstanceId == receiver)
				{
					if (string.IsNullOrEmpty(receiverMemberName))
					{
						list.Add(wireBundle);
					}
					else
					{
						using (List<WireEntry>.Enumerator enumerator2 = wireBundle.Wires.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								if (enumerator2.Current.ReceiverMemberName == receiverMemberName)
								{
									list.Add(wireBundle);
								}
							}
						}
					}
				}
			}
			return list;
		}
	}
}
