using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level;

public static class TerrainChangeSerializationExtensions
{
	public static void ReadValueSafe(this FastBufferReader reader, out TerrainChange[] terrainChange)
	{
		int value = 0;
		reader.ReadValueSafe(out value, default(FastBufferWriter.ForPrimitives));
		terrainChange = new TerrainChange[value];
		for (int i = 0; i < value; i++)
		{
			TerrainChange terrainChange2 = new TerrainChange();
			reader.ReadValueSafe(out terrainChange2.TilesetIndex, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out terrainChange2.Erased, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out terrainChange2.Coordinates);
			terrainChange[i] = terrainChange2;
		}
	}

	public static void WriteValueSafe(this FastBufferWriter writer, in TerrainChange[] terrainChange)
	{
		writer.WriteValueSafe<int>(terrainChange.Length, default(FastBufferWriter.ForPrimitives));
		for (int i = 0; i < terrainChange.Length; i++)
		{
			writer.WriteValueSafe(in terrainChange[i].TilesetIndex, default(FastBufferWriter.ForPrimitives));
			writer.WriteValueSafe(in terrainChange[i].Erased, default(FastBufferWriter.ForPrimitives));
			writer.WriteValueSafe(terrainChange[i].Coordinates);
		}
	}

	public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref TerrainChange terrainChange) where TReaderWriter : IReaderWriter
	{
		if (serializer.IsReader)
		{
			terrainChange = new TerrainChange();
		}
		serializer.SerializeValue(ref terrainChange.TilesetIndex, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref terrainChange.Erased, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref terrainChange.Coordinates);
	}
}
