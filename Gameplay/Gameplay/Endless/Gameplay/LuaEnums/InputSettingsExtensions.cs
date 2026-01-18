using System;

namespace Endless.Gameplay.LuaEnums
{
	// Token: 0x02000477 RID: 1143
	internal static class InputSettingsExtensions
	{
		// Token: 0x06001C87 RID: 7303 RVA: 0x0007DCB0 File Offset: 0x0007BEB0
		internal static bool CanMove(this InputSettings value)
		{
			return value.HasFlag(InputSettings.Walk) || value.HasFlag(InputSettings.Run);
		}

		// Token: 0x06001C88 RID: 7304 RVA: 0x0007DCD8 File Offset: 0x0007BED8
		internal static bool CanRun(this InputSettings value)
		{
			return value.HasFlag(InputSettings.Run);
		}

		// Token: 0x06001C89 RID: 7305 RVA: 0x0007DCEB File Offset: 0x0007BEEB
		internal static bool CanUseEquipment(this InputSettings value)
		{
			return value.HasFlag(InputSettings.Equipment);
		}

		// Token: 0x06001C8A RID: 7306 RVA: 0x0007DCFE File Offset: 0x0007BEFE
		internal static bool CanInteract(this InputSettings value)
		{
			return value.HasFlag(InputSettings.Interaction);
		}

		// Token: 0x06001C8B RID: 7307 RVA: 0x0007DD12 File Offset: 0x0007BF12
		internal static bool CanJump(this InputSettings value)
		{
			return value.HasFlag(InputSettings.Jump);
		}
	}
}
