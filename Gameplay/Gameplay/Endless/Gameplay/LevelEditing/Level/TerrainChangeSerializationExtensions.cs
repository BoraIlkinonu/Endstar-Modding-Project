using System;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200058C RID: 1420
	public static class TerrainChangeSerializationExtensions
	{
		// Token: 0x0600224B RID: 8779 RVA: 0x0009DDE0 File Offset: 0x0009BFE0
		public static void ReadValueSafe(this FastBufferReader reader, out TerrainChange[] terrainChange)
		{
			int num = 0;
			reader.ReadValueSafe<int>(out num, default(FastBufferWriter.ForPrimitives));
			terrainChange = new TerrainChange[num];
			for (int i = 0; i < num; i++)
			{
				TerrainChange terrainChange2 = new TerrainChange();
				reader.ReadValueSafe<int>(out terrainChange2.TilesetIndex, default(FastBufferWriter.ForPrimitives));
				reader.ReadValueSafe<bool>(out terrainChange2.Erased, default(FastBufferWriter.ForPrimitives));
				reader.ReadValueSafe(out terrainChange2.Coordinates);
				terrainChange[i] = terrainChange2;
			}
		}

		// Token: 0x0600224C RID: 8780 RVA: 0x0009DE5C File Offset: 0x0009C05C
		public static void WriteValueSafe(this FastBufferWriter writer, in TerrainChange[] terrainChange)
		{
			int num = terrainChange.Length;
			writer.WriteValueSafe<int>(in num, default(FastBufferWriter.ForPrimitives));
			for (int i = 0; i < terrainChange.Length; i++)
			{
				writer.WriteValueSafe<int>(in terrainChange[i].TilesetIndex, default(FastBufferWriter.ForPrimitives));
				writer.WriteValueSafe<bool>(in terrainChange[i].Erased, default(FastBufferWriter.ForPrimitives));
				writer.WriteValueSafe(terrainChange[i].Coordinates);
			}
		}

		// Token: 0x0600224D RID: 8781 RVA: 0x0009DED4 File Offset: 0x0009C0D4
		public static void SerializeValue<TReaderWriter>(this BufferSerializer<TReaderWriter> serializer, ref TerrainChange terrainChange) where TReaderWriter : IReaderWriter
		{
			if (serializer.IsReader)
			{
				terrainChange = new TerrainChange();
			}
			serializer.SerializeValue<int>(ref terrainChange.TilesetIndex, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref terrainChange.Erased, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref terrainChange.Coordinates);
		}
	}
}
