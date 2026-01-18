using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public sealed class UIInventorySlotView : UIGameObject, IPoolableT
{
	private enum States
	{
		EquipmentSlotOccupied,
		InventorySlotOccupied,
		InventorySlotUnoccupied
	}

	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[SerializeField]
	private UIDisplayAndHideHandler occupiedBorderDisplayAndHideHandler;

	[SerializeField]
	private UIDisplayAndHideHandler lockedDisplayAndHideHandler;

	[Header("Equipment Type Coloring")]
	[SerializeField]
	private Graphic[] occupiedBorderGraphics = Array.Empty<Graphic>();

	[SerializeField]
	private ColorVariable majorEquipmentColor;

	[SerializeField]
	private ColorVariable minorEquipmentColor;

	[Header("Drop Feedback")]
	[SerializeField]
	private TweenCollection validDropTweenCollection;

	[SerializeField]
	private TweenCollection invalidDropTweenCollection;

	[Header("Borders")]
	[SerializeField]
	private Image inventorySlotOccupiedBorderImage;

	[SerializeField]
	private SpriteVariable inventorySlotOccupiedBorderSpriteVariable;

	[SerializeField]
	private SpriteVariable equipmentSlotOccupiedBorderSpriteVariable;

	[SerializeField]
	private Image inventorySlotUnoccupiedBorderImage;

	[SerializeField]
	private SpriteVariable inventorySlotUnoccupiedBorderSpriteVariable;

	[SerializeField]
	private SpriteVariable equipmentSlotUnoccupiedBorderSpriteVariable;

	[Header("Hot Key")]
	[SerializeField]
	private TextMeshProUGUI hotKeyText;

	[SerializeField]
	private UIDisplayAndHideHandler hotKeyDisplayAndHideHandler;

	[SerializeField]
	private UIItemView item;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private bool isFirstDisplay = true;

	private bool didTweenValidDropTweenCollection;

	private bool waitFrameAndViewBorderOnEnable;

	public InventorySlot Model { get; private set; }

	public int Index { get; private set; }

	public MonoBehaviour Prefab { get; set; }

	public bool IsUi => true;

	public Vector3 ItemPosition => item.transform.position;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		if (waitFrameAndViewBorderOnEnable)
		{
			StartCoroutine(WaitFrameAndViewBorder());
		}
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		displayAndHideHandler.OnHideComplete.AddListener(Despawn);
	}

	public void OnSpawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSpawn");
		}
	}

	public void OnDespawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDespawn");
		}
		displayAndHideHandler.SetToHideEnd(triggerUnityEvent: false);
		waitFrameAndViewBorderOnEnable = false;
		hotKeyDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: false);
	}

	public void Initialize(InventorySlot model, float delay, int index)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", model, delay, index);
		}
		View(model, index);
		if (isFirstDisplay)
		{
			displayAndHideHandler.SetToHideEnd(triggerUnityEvent: false);
			item.Initialize();
			hotKeyDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: false);
			isFirstDisplay = false;
		}
		SetSetDisplayDelay(isFirstDisplay ? delay : 0f);
		displayAndHideHandler.Display();
		if (index <= 9)
		{
			hotKeyDisplayAndHideHandler.Display();
		}
	}

	public void View(InventorySlot model, int index)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", JsonUtility.ToJson(model.Item), index);
		}
		Model = model;
		bool flag = model.Item == null;
		item.View(model.Item);
		item.gameObject.SetActive(!flag);
		Index = index;
		if (model.Locked)
		{
			lockedDisplayAndHideHandler.Display();
		}
		else
		{
			lockedDisplayAndHideHandler.Hide();
		}
		if (base.gameObject.activeInHierarchy)
		{
			StartCoroutine(WaitFrameAndViewBorder());
		}
		else
		{
			waitFrameAndViewBorderOnEnable = true;
		}
		hotKeyText.text = (index + 1).ToString();
	}

	public void Close()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Close");
		}
		displayAndHideHandler.Hide();
	}

	public void SetSetDisplayDelay(float delay)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetSetDisplayDelay", delay);
		}
		displayAndHideHandler.SetDisplayDelay(delay);
		item.SetDisplayDelay(delay + (float)(isFirstDisplay ? 1 : 0));
	}

	public void ViewDropFeedback(bool dropIsValid)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewDropFeedback", dropIsValid);
		}
		if (dropIsValid)
		{
			if (!didTweenValidDropTweenCollection)
			{
				validDropTweenCollection.Tween();
				didTweenValidDropTweenCollection = true;
			}
		}
		else if (didTweenValidDropTweenCollection)
		{
			invalidDropTweenCollection.Tween();
			didTweenValidDropTweenCollection = false;
		}
	}

	private IEnumerator WaitFrameAndViewBorder()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "WaitFrameAndViewBorder");
		}
		yield return new WaitForEndOfFrame();
		bool num = Model.Item == null;
		States states = States.InventorySlotUnoccupied;
		if (num)
		{
			inventorySlotUnoccupiedBorderImage.color = new Color(1f, 1f, 1f, inventorySlotUnoccupiedBorderImage.color.a);
		}
		else
		{
			states = ((MonoBehaviourSingleton<UIInventoryView>.Instance.Inventory.GetEquippedSlotIndex(Index) <= -1) ? States.InventorySlotOccupied : States.EquipmentSlotOccupied);
			Color color = ((Model.Item.InventorySlot == Item.InventorySlotType.Major) ? majorEquipmentColor.Value : minorEquipmentColor.Value);
			Graphic[] array = occupiedBorderGraphics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = color;
			}
			inventorySlotUnoccupiedBorderImage.color = new Color(color.r, color.g, color.b, inventorySlotUnoccupiedBorderImage.color.a);
		}
		switch (states)
		{
		case States.EquipmentSlotOccupied:
			inventorySlotOccupiedBorderImage.sprite = equipmentSlotOccupiedBorderSpriteVariable.Value;
			inventorySlotUnoccupiedBorderImage.sprite = equipmentSlotUnoccupiedBorderSpriteVariable.Value;
			occupiedBorderDisplayAndHideHandler.Display();
			break;
		case States.InventorySlotOccupied:
			inventorySlotOccupiedBorderImage.sprite = inventorySlotOccupiedBorderSpriteVariable.Value;
			inventorySlotUnoccupiedBorderImage.sprite = inventorySlotUnoccupiedBorderSpriteVariable.Value;
			occupiedBorderDisplayAndHideHandler.Display();
			break;
		case States.InventorySlotUnoccupied:
			inventorySlotOccupiedBorderImage.sprite = inventorySlotUnoccupiedBorderSpriteVariable.Value;
			inventorySlotUnoccupiedBorderImage.sprite = inventorySlotUnoccupiedBorderSpriteVariable.Value;
			occupiedBorderDisplayAndHideHandler.Hide();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void Despawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Despawn");
		}
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
	}
}
