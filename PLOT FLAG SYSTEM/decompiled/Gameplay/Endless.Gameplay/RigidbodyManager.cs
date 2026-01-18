using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class RigidbodyManager : EndlessBehaviourSingleton<RigidbodyManager>
{
	private UnityEvent BeforeSimulationEvent = new UnityEvent();

	private UnityEvent AfterSimulationEvent = new UnityEvent();

	private List<OfflineRigidbodyController> offlineControllers = new List<OfflineRigidbodyController>(100);

	private void OnEnable()
	{
		Physics.simulationMode = SimulationMode.Script;
	}

	public void AddListener(UnityAction beforeCall, UnityAction afterCall)
	{
		BeforeSimulationEvent.AddListener(beforeCall);
		AfterSimulationEvent.AddListener(afterCall);
	}

	public void RemoveListener(UnityAction beforeCall, UnityAction afterCall)
	{
		BeforeSimulationEvent.RemoveListener(beforeCall);
		AfterSimulationEvent.RemoveListener(afterCall);
	}

	public void AddOffline(OfflineRigidbodyController controller)
	{
		offlineControllers.Add(controller);
	}

	public void RemoveOffline(OfflineRigidbodyController controller)
	{
		offlineControllers.RemoveSwapBack(controller);
	}

	public void BeforeSimulation()
	{
		BeforeSimulationEvent.Invoke();
		foreach (OfflineRigidbodyController offlineController in offlineControllers)
		{
			offlineController.HandleRigidbodySimulationStart();
		}
	}

	public void AfterSimulation()
	{
		AfterSimulationEvent.Invoke();
		foreach (OfflineRigidbodyController offlineController in offlineControllers)
		{
			offlineController.HandleRigidbodySimulationEnd();
		}
	}
}
