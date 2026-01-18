using System;
using Endless.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NetworkTimeUtilUI : MonoBehaviour
{
	public enum MonitorType
	{
		Server,
		Local,
		RollbackFrame,
		ExtrapolationFrame
	}

	[SerializeField]
	private TextMeshProUGUI textMesh;

	[SerializeField]
	private double updateFactor = 1.0;

	[SerializeField]
	private MonitorType monitorType;

	private double runtimeValue;

	private void OnValidate()
	{
		if (textMesh == null)
		{
			textMesh = GetComponent<TextMeshProUGUI>();
		}
	}

	private void Start()
	{
		if (monitorType == MonitorType.Server)
		{
			NetworkTimeUtil instance = MonoBehaviourSingleton<NetworkTimeUtil>.Instance;
			instance.ServerTime = (UnityAction<double>)Delegate.Combine(instance.ServerTime, new UnityAction<double>(HandleUpdate));
		}
		else if (monitorType == MonitorType.Local)
		{
			NetworkTimeUtil instance2 = MonoBehaviourSingleton<NetworkTimeUtil>.Instance;
			instance2.LocalTime = (UnityAction<double>)Delegate.Combine(instance2.LocalTime, new UnityAction<double>(HandleUpdate));
		}
		else if (monitorType == MonitorType.RollbackFrame)
		{
			NetworkTimeUtil instance3 = MonoBehaviourSingleton<NetworkTimeUtil>.Instance;
			instance3.RollbackFrame = (UnityAction<uint>)Delegate.Combine(instance3.RollbackFrame, new UnityAction<uint>(HandleUpdate));
		}
		else if (monitorType == MonitorType.ExtrapolationFrame)
		{
			NetworkTimeUtil instance4 = MonoBehaviourSingleton<NetworkTimeUtil>.Instance;
			instance4.ExtrapolationFrame = (UnityAction<uint>)Delegate.Combine(instance4.ExtrapolationFrame, new UnityAction<uint>(HandleUpdate));
		}
	}

	private void HandleUpdate(double value)
	{
		if (value > runtimeValue + updateFactor)
		{
			runtimeValue = value - value % updateFactor;
		}
		textMesh.text = runtimeValue.ToString();
	}

	private void HandleUpdate(uint value)
	{
		if ((double)value % updateFactor == 0.0)
		{
			textMesh.text = value.ToString();
		}
	}
}
