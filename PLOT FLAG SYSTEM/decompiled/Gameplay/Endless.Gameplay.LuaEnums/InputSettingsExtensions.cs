namespace Endless.Gameplay.LuaEnums;

internal static class InputSettingsExtensions
{
	internal static bool CanMove(this InputSettings value)
	{
		if (!value.HasFlag(InputSettings.Walk))
		{
			return value.HasFlag(InputSettings.Run);
		}
		return true;
	}

	internal static bool CanRun(this InputSettings value)
	{
		return value.HasFlag(InputSettings.Run);
	}

	internal static bool CanUseEquipment(this InputSettings value)
	{
		return value.HasFlag(InputSettings.Equipment);
	}

	internal static bool CanInteract(this InputSettings value)
	{
		return value.HasFlag(InputSettings.Interaction);
	}

	internal static bool CanJump(this InputSettings value)
	{
		return value.HasFlag(InputSettings.Jump);
	}
}
