using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B1 RID: 1201
	public class ConsoleMessage : INetworkSerializable
	{
		// Token: 0x170005BC RID: 1468
		// (get) Token: 0x06001DBE RID: 7614 RVA: 0x00082AF1 File Offset: 0x00080CF1
		public SerializableGuid InstanceId
		{
			get
			{
				return this.instanceId;
			}
		}

		// Token: 0x170005BD RID: 1469
		// (get) Token: 0x06001DBF RID: 7615 RVA: 0x00082AF9 File Offset: 0x00080CF9
		public SerializableGuid AssetId
		{
			get
			{
				return this.assetId;
			}
		}

		// Token: 0x170005BE RID: 1470
		// (get) Token: 0x06001DC0 RID: 7616 RVA: 0x00082B01 File Offset: 0x00080D01
		public SerializableGuid LevelId
		{
			get
			{
				return this.levelId;
			}
		}

		// Token: 0x170005BF RID: 1471
		// (get) Token: 0x06001DC1 RID: 7617 RVA: 0x00082B09 File Offset: 0x00080D09
		public string Message
		{
			get
			{
				return this.message;
			}
		}

		// Token: 0x170005C0 RID: 1472
		// (get) Token: 0x06001DC2 RID: 7618 RVA: 0x00082B11 File Offset: 0x00080D11
		public int LineNumber
		{
			get
			{
				return this.lineNumber;
			}
		}

		// Token: 0x170005C1 RID: 1473
		// (get) Token: 0x06001DC3 RID: 7619 RVA: 0x00082B19 File Offset: 0x00080D19
		public LogType LogType
		{
			get
			{
				return this.logType;
			}
		}

		// Token: 0x170005C2 RID: 1474
		// (get) Token: 0x06001DC4 RID: 7620 RVA: 0x00082B21 File Offset: 0x00080D21
		public DateTime Timestamp
		{
			get
			{
				return this.timestamp;
			}
		}

		// Token: 0x170005C3 RID: 1475
		// (get) Token: 0x06001DC5 RID: 7621 RVA: 0x00082B29 File Offset: 0x00080D29
		public bool ShouldDisplayLineNumber
		{
			get
			{
				return this.LineNumber != -1;
			}
		}

		// Token: 0x06001DC6 RID: 7622 RVA: 0x000030D2 File Offset: 0x000012D2
		public ConsoleMessage()
		{
		}

		// Token: 0x06001DC7 RID: 7623 RVA: 0x00082B38 File Offset: 0x00080D38
		public ConsoleMessage(SerializableGuid instanceId, SerializableGuid assetId, LogType logType, string message, int lineNumber = -1)
		{
			this.instanceId = instanceId;
			this.assetId = assetId;
			this.logType = logType;
			this.message = message;
			this.lineNumber = lineNumber;
			this.timestamp = DateTime.UtcNow;
			this.levelId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID;
		}

		// Token: 0x06001DC8 RID: 7624 RVA: 0x00082B9C File Offset: 0x00080D9C
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<SerializableGuid>(ref this.instanceId, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<SerializableGuid>(ref this.assetId, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<SerializableGuid>(ref this.levelId, default(FastBufferWriter.ForNetworkSerializable));
			if (serializer.IsReader)
			{
				FixedString512Bytes fixedString512Bytes = default(FixedString512Bytes);
				serializer.SerializeValue<FixedString512Bytes>(ref fixedString512Bytes, default(FastBufferWriter.ForFixedStrings));
				this.message = fixedString512Bytes.ToString();
				this.lineNumber = Compression.DeserializeInt<T>(serializer);
				this.logType = (LogType)Compression.DeserializeInt<T>(serializer);
				this.timestamp = new DateTime(Compression.DeserializeLong<T>(serializer));
			}
			else
			{
				FixedString512Bytes fixedString512Bytes2 = this.message.Substring(0, Mathf.Min(this.message.Length, 255));
				serializer.SerializeValue<FixedString512Bytes>(ref fixedString512Bytes2, default(FastBufferWriter.ForFixedStrings));
				Compression.SerializeInt<T>(serializer, this.lineNumber);
				Compression.SerializeInt<T>(serializer, (int)this.logType);
				Compression.SerializeLong<T>(serializer, this.timestamp.Ticks);
			}
			serializer.SerializeValue<DateTime>(ref this.timestamp, default(FastBufferWriter.ForPrimitives));
		}

		// Token: 0x04001740 RID: 5952
		private SerializableGuid instanceId;

		// Token: 0x04001741 RID: 5953
		private SerializableGuid assetId;

		// Token: 0x04001742 RID: 5954
		private SerializableGuid levelId;

		// Token: 0x04001743 RID: 5955
		private string message;

		// Token: 0x04001744 RID: 5956
		private int lineNumber;

		// Token: 0x04001745 RID: 5957
		private LogType logType;

		// Token: 0x04001746 RID: 5958
		private DateTime timestamp;
	}
}
