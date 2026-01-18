using System;
using Endless.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x02000014 RID: 20
public class NetworkTimeUtilUI : MonoBehaviour
{
	// Token: 0x06000050 RID: 80 RVA: 0x00002F02 File Offset: 0x00001102
	private void OnValidate()
	{
		if (this.textMesh == null)
		{
			this.textMesh = base.GetComponent<TextMeshProUGUI>();
		}
	}

	// Token: 0x06000051 RID: 81 RVA: 0x00002F20 File Offset: 0x00001120
	private void Start()
	{
		if (this.monitorType == NetworkTimeUtilUI.MonitorType.Server)
		{
			NetworkTimeUtil instance = MonoBehaviourSingleton<NetworkTimeUtil>.Instance;
			instance.ServerTime = (UnityAction<double>)Delegate.Combine(instance.ServerTime, new UnityAction<double>(this.HandleUpdate));
			return;
		}
		if (this.monitorType == NetworkTimeUtilUI.MonitorType.Local)
		{
			NetworkTimeUtil instance2 = MonoBehaviourSingleton<NetworkTimeUtil>.Instance;
			instance2.LocalTime = (UnityAction<double>)Delegate.Combine(instance2.LocalTime, new UnityAction<double>(this.HandleUpdate));
			return;
		}
		if (this.monitorType == NetworkTimeUtilUI.MonitorType.RollbackFrame)
		{
			NetworkTimeUtil instance3 = MonoBehaviourSingleton<NetworkTimeUtil>.Instance;
			instance3.RollbackFrame = (UnityAction<uint>)Delegate.Combine(instance3.RollbackFrame, new UnityAction<uint>(this.HandleUpdate));
			return;
		}
		if (this.monitorType == NetworkTimeUtilUI.MonitorType.ExtrapolationFrame)
		{
			NetworkTimeUtil instance4 = MonoBehaviourSingleton<NetworkTimeUtil>.Instance;
			instance4.ExtrapolationFrame = (UnityAction<uint>)Delegate.Combine(instance4.ExtrapolationFrame, new UnityAction<uint>(this.HandleUpdate));
		}
	}

	// Token: 0x06000052 RID: 82 RVA: 0x00002FEB File Offset: 0x000011EB
	private void HandleUpdate(double value)
	{
		if (value > this.runtimeValue + this.updateFactor)
		{
			this.runtimeValue = value - value % this.updateFactor;
		}
		this.textMesh.text = this.runtimeValue.ToString();
	}

	// Token: 0x06000053 RID: 83 RVA: 0x00003023 File Offset: 0x00001223
	private void HandleUpdate(uint value)
	{
		if (value % this.updateFactor == 0.0)
		{
			this.textMesh.text = value.ToString();
		}
	}

	// Token: 0x04000034 RID: 52
	[SerializeField]
	private TextMeshProUGUI textMesh;

	// Token: 0x04000035 RID: 53
	[SerializeField]
	private double updateFactor = 1.0;

	// Token: 0x04000036 RID: 54
	[SerializeField]
	private NetworkTimeUtilUI.MonitorType monitorType;

	// Token: 0x04000037 RID: 55
	private double runtimeValue;

	// Token: 0x02000015 RID: 21
	public enum MonitorType
	{
		// Token: 0x04000039 RID: 57
		Server,
		// Token: 0x0400003A RID: 58
		Local,
		// Token: 0x0400003B RID: 59
		RollbackFrame,
		// Token: 0x0400003C RID: 60
		ExtrapolationFrame
	}
}
