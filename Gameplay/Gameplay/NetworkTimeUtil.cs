using System;
using Endless.Gameplay;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine.Events;

// Token: 0x02000013 RID: 19
public class NetworkTimeUtil : MonoBehaviourSingleton<NetworkTimeUtil>
{
	// Token: 0x0600004C RID: 76 RVA: 0x00002DB4 File Offset: 0x00000FB4
	private void Update()
	{
		if (!this.registered && NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkTickSystem != null)
		{
			NetworkManager.Singleton.NetworkTickSystem.Tick += this.NetworkUpdate;
			this.registered = true;
		}
	}

	// Token: 0x0600004D RID: 77 RVA: 0x00002E04 File Offset: 0x00001004
	private void OnDisable()
	{
		if (this.registered && NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkTickSystem != null)
		{
			NetworkManager.Singleton.NetworkTickSystem.Tick -= this.NetworkUpdate;
		}
		this.registered = false;
	}

	// Token: 0x0600004E RID: 78 RVA: 0x00002E54 File Offset: 0x00001054
	private void NetworkUpdate()
	{
		UnityAction<double> serverTime = this.ServerTime;
		if (serverTime != null)
		{
			serverTime(NetworkManager.Singleton.ServerTime.FixedTime);
		}
		UnityAction<double> localTime = this.LocalTime;
		if (localTime != null)
		{
			localTime(NetworkManager.Singleton.LocalTime.FixedTime);
		}
		UnityAction<uint> rollbackFrame = this.RollbackFrame;
		if (rollbackFrame != null)
		{
			rollbackFrame(NetClock.GetFrameFromTime(NetworkManager.Singleton.ServerTime.FixedTime) - 2U);
		}
		UnityAction<uint> extrapolationFrame = this.ExtrapolationFrame;
		if (extrapolationFrame == null)
		{
			return;
		}
		extrapolationFrame(NetClock.GetFrameFromTime(NetworkManager.Singleton.ServerTime.FixedTime) - 2U);
	}

	// Token: 0x0400002F RID: 47
	public UnityAction<double> ServerTime;

	// Token: 0x04000030 RID: 48
	public UnityAction<double> LocalTime;

	// Token: 0x04000031 RID: 49
	public UnityAction<uint> RollbackFrame;

	// Token: 0x04000032 RID: 50
	public UnityAction<uint> ExtrapolationFrame;

	// Token: 0x04000033 RID: 51
	private bool registered;
}
