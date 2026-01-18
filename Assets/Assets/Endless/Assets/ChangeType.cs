using System;

namespace Endless.Assets
{
	// Token: 0x02000009 RID: 9
	public enum ChangeType
	{
		// Token: 0x04000014 RID: 20
		[ChangeType("created the asset!")]
		AssetCreated,
		// Token: 0x04000015 RID: 21
		[ChangeType("updated asset version to %metastring%")]
		AssetVersionUpdated,
		// Token: 0x04000016 RID: 22
		[ChangeType("updated child asset(s): %metastring%")]
		ChildAssetUpdated,
		// Token: 0x04000017 RID: 23
		AutomaticAssetUpgrade,
		// Token: 0x04000018 RID: 24
		BadDataAutoFix,
		// Token: 0x04000019 RID: 25
		[ChangeType("painted some terrain.")]
		TerrainPainted = 100,
		// Token: 0x0400001A RID: 26
		[ChangeType("erased some terrain.")]
		TerrainErased,
		// Token: 0x0400001B RID: 27
		[ChangeType("erased some props.")]
		PropErase = 200,
		// Token: 0x0400001C RID: 28
		[ChangeType("painted some props.")]
		PropPainted,
		// Token: 0x0400001D RID: 29
		[ChangeType("pasted some copied props.")]
		PropPasted,
		// Token: 0x0400001E RID: 30
		[ChangeType("moved some props around.")]
		PropMoved,
		// Token: 0x0400001F RID: 31
		[ChangeType("renamed some props.")]
		PropLabelChanged,
		// Token: 0x04000020 RID: 32
		[ChangeType("changed some prop values.")]
		PropMemberChanged,
		// Token: 0x04000021 RID: 33
		[ChangeType("created some new wires.")]
		WireCreated = 300,
		// Token: 0x04000022 RID: 34
		[ChangeType("updated some wire details.")]
		WireUpdated,
		// Token: 0x04000023 RID: 35
		[ChangeType("deleted some wires.")]
		WireDeleted,
		// Token: 0x04000024 RID: 36
		[ChangeType("deleted some wire reroutes.")]
		WireRerouteDeleted,
		// Token: 0x04000025 RID: 37
		[ChangeType("added some wire reroutes.")]
		WireRerouteAdded,
		// Token: 0x04000026 RID: 38
		[ChangeType("recolored some wires.")]
		WireColorUpdated,
		// Token: 0x04000027 RID: 39
		[ChangeType("updated the level name.")]
		LevelNameUpdated = 900,
		// Token: 0x04000028 RID: 40
		[ChangeType("updated the level description.")]
		LevelDescriptionUpdated,
		// Token: 0x04000029 RID: 41
		[ChangeType("reordered the spawn points.")]
		LevelUpdatedSpawnPositionOrder,
		// Token: 0x0400002A RID: 42
		[ChangeType("archived the level.")]
		LevelArchived,
		// Token: 0x0400002B RID: 43
		[ChangeType("added screenshots to the level.")]
		LevelScreenshotsAdded,
		// Token: 0x0400002C RID: 44
		[ChangeType("removed screenshots from the level.")]
		LevelScreenshotsRemoved,
		// Token: 0x0400002D RID: 45
		[ChangeType("reordered screenshots for the level.")]
		LevelScreenshotsReorder,
		// Token: 0x0400002E RID: 46
		[ChangeType("updated the game name.")]
		GameNameUpdated = 1000,
		// Token: 0x0400002F RID: 47
		[ChangeType("updated the game description.")]
		GameDescriptionUpdated,
		// Token: 0x04000030 RID: 48
		[ChangeType("updated the game's player count.")]
		GamePlayerCountUpdated,
		// Token: 0x04000031 RID: 49
		[ChangeType("added a new level.")]
		GameNewLevelAdded,
		// Token: 0x04000032 RID: 50
		[ChangeType("added screenshots to the game.")]
		GameScreenshotsAdded,
		// Token: 0x04000033 RID: 51
		[ChangeType("removed screenshots from the game.")]
		GameScreenshotsRemoved,
		// Token: 0x04000034 RID: 52
		[ChangeType("reordered screenshots for the game.")]
		GameScreenshotsReorder,
		// Token: 0x04000035 RID: 53
		[ChangeType("reordered levels for the game.")]
		GameLevelReorder,
		// Token: 0x04000036 RID: 54
		GameLibraryPropVersionChanged = 1500,
		// Token: 0x04000037 RID: 55
		GameLibraryPropAdded = 1510,
		// Token: 0x04000038 RID: 56
		GameLibraryPropRemoved,
		// Token: 0x04000039 RID: 57
		GameLibraryTerrainVersionChanged = 1520,
		// Token: 0x0400003A RID: 58
		GameLibraryTerrainAdded = 1530,
		// Token: 0x0400003B RID: 59
		GameLibraryTerrainRemoved,
		// Token: 0x0400003C RID: 60
		GameLibraryAudioVersionChanged = 1540,
		// Token: 0x0400003D RID: 61
		GameLibraryAudioAdded,
		// Token: 0x0400003E RID: 62
		GameLibraryAudioRemoved,
		// Token: 0x0400003F RID: 63
		GameLibraryUpdateAll = 1550,
		// Token: 0x04000040 RID: 64
		TerrainCreated = 1600,
		// Token: 0x04000041 RID: 65
		TerrainRemoved,
		// Token: 0x04000042 RID: 66
		TerrainUpdated,
		// Token: 0x04000043 RID: 67
		PropCreated = 1700,
		// Token: 0x04000044 RID: 68
		PropRemoved,
		// Token: 0x04000045 RID: 69
		PropUpdated,
		// Token: 0x04000046 RID: 70
		ScriptCreated = 1800,
		// Token: 0x04000047 RID: 71
		ScriptRemoved,
		// Token: 0x04000048 RID: 72
		ScriptUpdated,
		// Token: 0x04000049 RID: 73
		ParticleCreated = 1900,
		// Token: 0x0400004A RID: 74
		ParticleRemoved,
		// Token: 0x0400004B RID: 75
		ParticleUpdated,
		// Token: 0x0400004C RID: 76
		AudioUploaded = 2000,
		// Token: 0x0400004D RID: 77
		AudioFileUpdated
	}
}
