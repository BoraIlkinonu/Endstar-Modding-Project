using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class ThrownBomb
{
	private ThrownBombItem thrownBombItem;

	internal ThrownBomb(ThrownBombItem thrownBombItem)
	{
		this.thrownBombItem = thrownBombItem;
	}

	public void SetDamageAtCenter(Context instigator, int damage)
	{
		thrownBombItem.DamageAtCenter = damage;
	}

	public void SetDamageAtEdge(Context instigator, int damage)
	{
		thrownBombItem.DamageAtEdge = damage;
	}

	public void SetCenterRadius(Context instigator, float radius)
	{
		thrownBombItem.CenterRadius = radius;
	}

	public void SetTotalBlastRadius(Context instigator, float radius)
	{
		thrownBombItem.TotalBlastRadius = radius;
	}

	public void SetCenterBlastForce(Context instigator, float force)
	{
		thrownBombItem.CenterBlastForce = force;
	}

	public void SetEdgeBlastForce(Context instigator, float force)
	{
		thrownBombItem.EdgeBlastForce = force;
	}

	public int GetDamageAtCenter()
	{
		return thrownBombItem.DamageAtCenter;
	}

	public int GetDamageAtEdge()
	{
		return thrownBombItem.DamageAtEdge;
	}

	public float GetCenterRadius()
	{
		return thrownBombItem.CenterRadius;
	}

	public float GetTotalBlastRadius()
	{
		return thrownBombItem.TotalBlastRadius;
	}

	public float GetCenterBlastForce()
	{
		return thrownBombItem.CenterBlastForce;
	}

	public float GetEdgeBlastForce()
	{
		return thrownBombItem.EdgeBlastForce;
	}
}
