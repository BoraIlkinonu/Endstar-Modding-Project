using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIHeartsView : UIBaseIntView
{
	[Header("UIHeartsView")]
	[SerializeField]
	private RectTransform heartHalfContainer;

	[SerializeField]
	private UIHeartHalfView heartHalfSource;

	[SerializeField]
	private UIHeartHalfSpaceView heartHalfSpaceSource;

	private readonly List<UIPoolableGameObject> poolableObjects = new List<UIPoolableGameObject>();

	private int halfHeartCount;

	public override void View(int model)
	{
		base.View(model);
		DisableSurplus();
		base.NumericFieldViews[0].SetValue(model, triggerOnValueChanged: false);
		if (model > halfHeartCount)
		{
			SpawnHeartsUpTo(model);
		}
		else if (model < halfHeartCount)
		{
			DespawnHeartsDownTo(model);
		}
		ViewSurplus(model);
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		foreach (UIPoolableGameObject poolableObject in poolableObjects)
		{
			poolableObject.ReturnToPool();
		}
		poolableObjects.Clear();
		halfHeartCount = 0;
	}

	protected override void ApplyNumericFieldViewValuesToModel(float fieldModel)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyNumericFieldViewValuesToModel", fieldModel);
		}
		base.OnValueChanged?.Invoke((int)fieldModel);
	}

	private void SpawnHeartsUpTo(int model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SpawnHeartsUpTo", model);
		}
		int num = model - halfHeartCount;
		int maximumAmountOfVisibleHalfHearts = GetMaximumAmountOfVisibleHalfHearts();
		if (halfHeartCount + num > maximumAmountOfVisibleHalfHearts)
		{
			num = maximumAmountOfVisibleHalfHearts - halfHeartCount;
		}
		for (int i = 0; i < num; i++)
		{
			bool num2 = halfHeartCount == 0;
			bool flag = (halfHeartCount - 1) % 2 == 0;
			Transform parent;
			if (!num2 && !flag)
			{
				PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
				UIHeartHalfSpaceView prefab = heartHalfSpaceSource;
				parent = heartHalfContainer;
				UIHeartHalfSpaceView item = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
				poolableObjects.Add(item);
			}
			PoolManagerT instance2 = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIHeartHalfView prefab2 = heartHalfSource;
			parent = heartHalfContainer;
			UIHeartHalfView uIHeartHalfView = instance2.Spawn(prefab2, default(Vector3), default(Quaternion), parent);
			poolableObjects.Add(uIHeartHalfView);
			uIHeartHalfView.View(halfHeartCount, i);
			halfHeartCount++;
		}
	}

	private void DespawnHeartsDownTo(int model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DespawnHeartsDownTo", model);
		}
		int value = halfHeartCount - model;
		int maximumAmountOfVisibleHalfHearts = GetMaximumAmountOfVisibleHalfHearts();
		value = Mathf.Clamp(value, 0, maximumAmountOfVisibleHalfHearts);
		while (value > 0 && poolableObjects.Count > 0)
		{
			DespawnLastItemIfSpace();
			List<UIPoolableGameObject> list = poolableObjects;
			if (list[list.Count - 1] is UIHeartHalfView uIHeartHalfView)
			{
				uIHeartHalfView.HideAndDespawnOnComplete();
			}
			else
			{
				DebugUtility.LogException(new InvalidCastException("Could not cast UIPoolableGameObject to UIHeartHalfView"), this);
			}
			poolableObjects.RemoveAt(poolableObjects.Count - 1);
			halfHeartCount--;
			value--;
			DespawnLastItemIfSpace();
		}
		void DespawnLastItemIfSpace()
		{
			if (poolableObjects.Count != 0)
			{
				List<UIPoolableGameObject> list2 = poolableObjects;
				if (list2[list2.Count - 1] is UIHeartHalfSpaceView uIHeartHalfSpaceView)
				{
					uIHeartHalfSpaceView.HideAndDespawnOnComplete();
					poolableObjects.RemoveAt(poolableObjects.Count - 1);
				}
			}
		}
	}

	private void DisableSurplus()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "DisableSurplus");
		}
		if (poolableObjects.Count > 0)
		{
			List<UIPoolableGameObject> list = poolableObjects;
			if (list[list.Count - 1] is UIHeartHalfView uIHeartHalfView)
			{
				uIHeartHalfView.DisableSurplus();
			}
			else
			{
				DebugUtility.LogException(new InvalidCastException("Could not cast UIPoolableGameObject to UIHeartHalfView!"), this);
			}
		}
	}

	private void ViewSurplus(int model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewSurplus", model);
		}
		int num = GetCountOfSurplusOfHalfHearts();
		if (num > 0)
		{
			List<UIPoolableGameObject> list = poolableObjects;
			if (list[list.Count - 1] is UIHeartHalfView uIHeartHalfView)
			{
				uIHeartHalfView.ViewSurplus(num);
			}
			else
			{
				DebugUtility.LogException(new InvalidCastException("Could not cast UIPoolableGameObject to UIHeartHalfView"), this);
			}
		}
		int GetCountOfSurplusOfHalfHearts()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("GetCountOfSurplusOfHalfHearts", this);
			}
			int maximumAmountOfVisibleHalfHearts = GetMaximumAmountOfVisibleHalfHearts();
			int result = 0;
			if (model > maximumAmountOfVisibleHalfHearts)
			{
				result = model - maximumAmountOfVisibleHalfHearts;
			}
			return result;
		}
	}

	private int GetMaximumAmountOfVisibleHalfHearts()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetMaximumAmountOfVisibleHalfHearts");
		}
		int num = 0;
		Canvas.ForceUpdateCanvases();
		float num2 = heartHalfContainer.rect.width;
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "area", num2), heartHalfContainer);
		}
		while (num2 > 0f)
		{
			num2 -= heartHalfSource.RectTransform.rect.width;
			bool num3 = num == 0;
			bool flag = num % 2 == 0;
			if (!num3 && !flag)
			{
				num2 -= heartHalfSpaceSource.RectTransform.rect.width;
			}
			num++;
		}
		if (num <= 0)
		{
			num = 1;
		}
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0}: {1}", "maximumAmountOfVisibleHalfHearts", num), this);
		}
		return num;
	}
}
