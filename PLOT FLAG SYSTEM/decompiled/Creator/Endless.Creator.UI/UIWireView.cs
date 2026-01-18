using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UILineRenderer))]
public class UIWireView : UIGameObject, IPoolableT
{
	[SerializeField]
	private int tilingIfNotConnected = 5;

	[SerializeField]
	private int tilingIfConnected = 25;

	[FormerlySerializedAs("darkenTweems")]
	[SerializeField]
	private TweenCollection darkenTweens;

	[SerializeField]
	private TweenCollection lightenTweens;

	[SerializeField]
	private WireColorDictionary wireColorDictionary;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[SerializeField]
	private bool superVerboseLogging;

	private RectTransform container;

	private bool flowLeftToRight = true;

	private UILineRenderer lineRenderer;

	public SerializableGuid WireId { get; private set; } = SerializableGuid.Empty;

	public UIWireNodeView EmitterNode { get; private set; }

	public UIWireNodeView ReceiverNode { get; private set; }

	public WireColor CurrentWireColor { get; private set; }

	public UILineRenderer LineRenderer
	{
		get
		{
			if (!lineRenderer)
			{
				TryGetComponent<UILineRenderer>(out lineRenderer);
			}
			return lineRenderer;
		}
	}

	public MonoBehaviour Prefab { get; set; }

	public bool IsUi => true;

	private void Update()
	{
		UpdateLineRendererPoints();
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
		SetColor(WireColor.NoColor);
		lightenTweens.SetToEnd();
		base.enabled = false;
	}

	public void Initialize(SerializableGuid wireId, UIWireNodeView emitterNode, UIWireNodeView receiverNode, bool flowLeftToRight)
	{
		if (verboseLogging)
		{
			if ((bool)emitterNode)
			{
				_ = emitterNode.MemberName;
			}
			if ((bool)receiverNode)
			{
				_ = receiverNode.MemberName;
			}
			DebugUtility.LogMethod(this, "Initialize", wireId, emitterNode.DebugSafeName(), receiverNode.DebugSafeName(), flowLeftToRight);
		}
		WireId = wireId;
		EmitterNode = emitterNode;
		ReceiverNode = receiverNode;
		this.flowLeftToRight = flowLeftToRight;
		WireEntry wireEntry = WiringUtilities.GetWireEntry(emitterNode.InspectedObjectId, emitterNode.MemberName, receiverNode.InspectedObjectId, receiverNode.MemberName);
		if (wireEntry != null)
		{
			SetColor((WireColor)wireEntry.WireColor);
		}
		if (!container)
		{
			base.RectTransform.parent.TryGetComponent<RectTransform>(out container);
		}
		UpdateLineRendererPoints();
	}

	public void Darken()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Darken");
		}
		darkenTweens.Tween();
	}

	public void Lighten()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Lighten");
		}
		lightenTweens.Tween();
	}

	public void SetColor(WireColor color)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetColor", color);
		}
		LineRenderer.material = wireColorDictionary[color].UIMaterial;
		CurrentWireColor = color;
	}

	public void UpdateLineRendererPoints()
	{
		if (superVerboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateLineRendererPoints");
		}
		Vector2 vector = new Vector2(-100f, 0f);
		Vector2 vector2 = new Vector2(100f, 0f);
		if ((bool)EmitterNode)
		{
			vector = container.InverseTransformPoint(EmitterNode.WirePoint);
		}
		vector2 = ((!ReceiverNode) ? (vector + (flowLeftToRight ? new Vector2(100f, 0f) : new Vector2(-100f, 0f))) : ((Vector2)container.InverseTransformPoint(ReceiverNode.WirePoint)));
		if (!EmitterNode)
		{
			vector = vector2 + (flowLeftToRight ? new Vector2(100f, 0f) : new Vector2(-100f, 0f));
		}
		Vector2[] points = new Vector2[2] { vector, vector2 };
		LineRenderer.SetPoints(points);
		LineRenderer.SetTiling(((bool)EmitterNode && (bool)ReceiverNode) ? tilingIfConnected : tilingIfNotConnected);
	}
}
