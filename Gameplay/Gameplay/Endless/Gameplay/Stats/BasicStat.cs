using System;
using System.Data;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.Stats
{
	// Token: 0x02000383 RID: 899
	public class BasicStat : StatBase, INetworkSerializable
	{
		// Token: 0x060016FD RID: 5885 RVA: 0x0006B72C File Offset: 0x0006992C
		public void SetPlayerContext(Context playerContext)
		{
			if (!playerContext.IsPlayer())
			{
				throw new ArgumentException("Context provided to SetPlayerContext was not a player");
			}
			if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(playerContext.WorldObject.NetworkObject.OwnerClientId, out this.UserId))
			{
				throw new DataException("Provided player context to SetPlayerContext was not registered yet.");
			}
		}

		// Token: 0x060016FE RID: 5886 RVA: 0x0006B77C File Offset: 0x0006997C
		public void SetNumericValue(Context instigator, float value, int displayFormat)
		{
			this.Value = StatBase.GetFormattedString(value, (NumericDisplayFormat)displayFormat);
		}

		// Token: 0x060016FF RID: 5887 RVA: 0x0006B798 File Offset: 0x00069998
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref this.Identifier, false);
			serializer.SerializeValue<LocalizedString>(ref this.Message, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<int>(ref this.Order, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.UserId, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsWriter && this.Value == null)
			{
				this.Value = string.Empty;
			}
			serializer.SerializeValue(ref this.Value, false);
		}

		// Token: 0x06001700 RID: 5888 RVA: 0x0006B81F File Offset: 0x00069A1F
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		// Token: 0x06001701 RID: 5889 RVA: 0x0006B828 File Offset: 0x00069A28
		public void LoadFromString(string stringData)
		{
			BasicStat basicStat = JsonConvert.DeserializeObject<BasicStat>(stringData);
			base.CopyFrom(basicStat);
			this.UserId = basicStat.UserId;
		}

		// Token: 0x04001272 RID: 4722
		[JsonProperty]
		internal int UserId = -1;

		// Token: 0x04001273 RID: 4723
		public string Value = string.Empty;
	}
}
