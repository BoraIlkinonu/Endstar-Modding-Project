using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x020000A1 RID: 161
	public static class GridUtilities
	{
		// Token: 0x0600047F RID: 1151 RVA: 0x0001372C File Offset: 0x0001192C
		public static void DrawDebugCube(Vector3 center, float size, Color color, float duration = 0f, bool topOnly = false, bool depthTest = false)
		{
			float num = size / 2f;
			Debug.DrawLine(center + new Vector3(-num, num, num), center + new Vector3(num, num, num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(num, num, -num), center + new Vector3(num, num, num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-num, num, -num), center + new Vector3(num, num, -num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-num, num, -num), center + new Vector3(-num, num, num), color, duration, depthTest);
			if (topOnly)
			{
				return;
			}
			Debug.DrawLine(center + new Vector3(-num, -num, num), center + new Vector3(num, -num, num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(num, -num, -num), center + new Vector3(num, -num, num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-num, -num, -num), center + new Vector3(num, -num, -num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-num, -num, -num), center + new Vector3(-num, -num, num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-num, -num, -num), center + new Vector3(-num, num, -num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-num, -num, num), center + new Vector3(-num, num, num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(num, -num, -num), center + new Vector3(num, num, -num), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(num, -num, num), center + new Vector3(num, num, num), color, duration, depthTest);
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x00013928 File Offset: 0x00011B28
		public static void DrawDebugCube(Vector3 center, Vector3 size, Color color, float duration = 0f, bool topOnly = false, bool depthTest = false)
		{
			Vector3 vector = size / 2f;
			Debug.DrawLine(center + new Vector3(-vector.x, vector.y, vector.z), center + new Vector3(vector.x, vector.y, vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(vector.x, vector.y, -vector.z), center + new Vector3(vector.x, vector.y, vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-vector.x, vector.y, -vector.z), center + new Vector3(vector.x, vector.y, -vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-vector.x, vector.y, -vector.z), center + new Vector3(-vector.x, vector.y, vector.z), color, duration, depthTest);
			if (topOnly)
			{
				return;
			}
			Debug.DrawLine(center + new Vector3(-vector.x, -vector.y, vector.z), center + new Vector3(vector.x, -vector.y, vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(vector.x, -vector.y, -vector.z), center + new Vector3(vector.x, -vector.y, vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-vector.x, -vector.y, -vector.z), center + new Vector3(vector.x, -vector.y, -vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-vector.x, -vector.y, -vector.z), center + new Vector3(-vector.x, -vector.y, vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-vector.x, -vector.y, -vector.z), center + new Vector3(-vector.x, vector.y, -vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(-vector.x, -vector.y, vector.z), center + new Vector3(-vector.x, vector.y, vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(vector.x, -vector.y, -vector.z), center + new Vector3(vector.x, vector.y, -vector.z), color, duration, depthTest);
			Debug.DrawLine(center + new Vector3(vector.x, -vector.y, vector.z), center + new Vector3(vector.x, vector.y, vector.z), color, duration, depthTest);
		}
	}
}
