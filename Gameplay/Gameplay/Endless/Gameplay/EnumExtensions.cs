using System;

namespace Endless.Gameplay
{
	// Token: 0x02000267 RID: 615
	public static class EnumExtensions
	{
		// Token: 0x06000CA5 RID: 3237 RVA: 0x00044544 File Offset: 0x00042744
		public static bool Contains(this AppearanceIKController.IKMode mode, AppearanceIKController.IKMode otherMode)
		{
			return (mode & otherMode) > AppearanceIKController.IKMode.None;
		}
	}
}
