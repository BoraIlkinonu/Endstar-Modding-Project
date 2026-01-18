using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class ConsoleMessage : INetworkSerializable
{
	private SerializableGuid instanceId;

	private SerializableGuid assetId;

	private SerializableGuid levelId;

	private string message;

	private int lineNumber;

	private LogType logType;

	private DateTime timestamp;

	public SerializableGuid InstanceId => instanceId;

	public SerializableGuid AssetId => assetId;

	public SerializableGuid LevelId => levelId;

	public string Message => message;

	public int LineNumber => lineNumber;

	public LogType LogType => logType;

	public DateTime Timestamp => timestamp;

	public bool ShouldDisplayLineNumber => LineNumber != -1;

	public ConsoleMessage()
	{
	}

	public ConsoleMessage(SerializableGuid instanceId, SerializableGuid assetId, LogType logType, string message, int lineNumber = -1)
	{
		this.instanceId = instanceId;
		this.assetId = assetId;
		this.logType = logType;
		this.message = message;
		this.lineNumber = lineNumber;
		timestamp = DateTime.UtcNow;
		levelId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetID;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref instanceId, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref assetId, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref levelId, default(FastBufferWriter.ForNetworkSerializable));
		if (serializer.IsReader)
		{
			FixedString512Bytes value = default(FixedString512Bytes);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForFixedStrings));
			message = value.ToString();
			lineNumber = Compression.DeserializeInt(serializer);
			logType = (LogType)Compression.DeserializeInt(serializer);
			timestamp = new DateTime(Compression.DeserializeLong(serializer));
		}
		else
		{
			FixedString512Bytes value2 = message.Substring(0, Mathf.Min(message.Length, 255));
			serializer.SerializeValue(ref value2, default(FastBufferWriter.ForFixedStrings));
			Compression.SerializeInt(serializer, lineNumber);
			Compression.SerializeInt(serializer, (int)logType);
			Compression.SerializeLong(serializer, timestamp.Ticks);
		}
		serializer.SerializeValue(ref timestamp, default(FastBufferWriter.ForPrimitives));
	}
}
