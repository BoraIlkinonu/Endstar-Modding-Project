using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Endless.Shared
{
	// Token: 0x02000062 RID: 98
	public static class EndlessInput
	{
		// Token: 0x060002FC RID: 764 RVA: 0x0000E920 File Offset: 0x0000CB20
		public static bool GetKey(Key key)
		{
			Keyboard current = Keyboard.current;
			return current != null && current[key].IsPressed(0f);
		}

		// Token: 0x060002FD RID: 765 RVA: 0x0000E94C File Offset: 0x0000CB4C
		public static bool GetKeyDown(Key key)
		{
			Keyboard current = Keyboard.current;
			return current != null && current[key].wasPressedThisFrame;
		}

		// Token: 0x060002FE RID: 766 RVA: 0x0000E970 File Offset: 0x0000CB70
		public static bool GetKeyUp(Key key)
		{
			Keyboard current = Keyboard.current;
			return current != null && current[key].wasReleasedThisFrame;
		}

		// Token: 0x060002FF RID: 767 RVA: 0x0000E994 File Offset: 0x0000CB94
		public unsafe static Vector2 MousePosition()
		{
			Mouse current = Mouse.current;
			Vector2? vector;
			if (current == null)
			{
				vector = null;
			}
			else
			{
				Vector2Control position = current.position;
				vector = ((position != null) ? new Vector2?(*position.value) : null);
			}
			Vector2? vector2 = vector;
			if (vector2 == null)
			{
				return Vector2.zero;
			}
			return vector2.GetValueOrDefault();
		}
	}
}
