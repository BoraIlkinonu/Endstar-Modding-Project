using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;

namespace Endless.Creator.LevelEditing.Runtime;

public class ServerCopyHistoryEntry
{
	public PropEntry Prop { get; set; }

	public List<WireBundle> EmitterWireBundles { get; set; }

	public List<WireBundle> ReceiverWireBundles { get; set; }

	public WireBundle[] CopyEmitterBundles(SerializableGuid newInstanceId)
	{
		WireBundle[] array = new WireBundle[EmitterWireBundles.Count];
		for (int i = 0; i < EmitterWireBundles.Count; i++)
		{
			WireBundle wireBundle = EmitterWireBundles[i].Copy(generateNewUniqueIds: true);
			wireBundle.EmitterInstanceId = newInstanceId;
			array[i] = wireBundle;
		}
		return array;
	}

	public WireBundle[] CopyReceiverBundles(SerializableGuid newInstanceId)
	{
		WireBundle[] array = new WireBundle[ReceiverWireBundles.Count];
		for (int i = 0; i < ReceiverWireBundles.Count; i++)
		{
			WireBundle wireBundle = ReceiverWireBundles[i].Copy(generateNewUniqueIds: true);
			wireBundle.ReceiverInstanceId = newInstanceId;
			array[i] = wireBundle;
		}
		return array;
	}
}
