using UnityEngine;

namespace Endless.Gameplay;

public class NpcSource : MonoBehaviour, INpcSource
{
	[SerializeField]
	private NpcEntity npcEntity;

	public NpcEntity Npc => npcEntity;
}
