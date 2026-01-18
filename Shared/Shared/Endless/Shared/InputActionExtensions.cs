using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Endless.Shared
{
	// Token: 0x0200005E RID: 94
	public static class InputActionExtensions
	{
		// Token: 0x060002E1 RID: 737 RVA: 0x0000E58C File Offset: 0x0000C78C
		public static void Bind(this Dictionary<InputAction, Action<InputAction.CallbackContext>> inputBindings)
		{
			foreach (KeyValuePair<InputAction, Action<InputAction.CallbackContext>> keyValuePair in inputBindings)
			{
				keyValuePair.Key.Bind(keyValuePair.Value);
			}
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x0000E5E8 File Offset: 0x0000C7E8
		public static void Unbind(this Dictionary<InputAction, Action<InputAction.CallbackContext>> inputBindings)
		{
			foreach (KeyValuePair<InputAction, Action<InputAction.CallbackContext>> keyValuePair in inputBindings)
			{
				keyValuePair.Key.Unbind(keyValuePair.Value);
			}
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0000E644 File Offset: 0x0000C844
		public static void Bind(this InputAction inputAction, Action<InputAction.CallbackContext> callback)
		{
			inputAction.performed += callback;
			inputAction.Enable();
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0000E653 File Offset: 0x0000C853
		public static void Unbind(this InputAction inputAction, Action<InputAction.CallbackContext> callback)
		{
			inputAction.performed -= callback;
			inputAction.Disable();
		}
	}
}
