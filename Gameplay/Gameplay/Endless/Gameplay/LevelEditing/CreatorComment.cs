using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x020004D8 RID: 1240
	public class CreatorComment : MonoBehaviour
	{
		// Token: 0x04001795 RID: 6037
		[SerializeField]
		[TextArea]
		private string[] comments = new string[1];
	}
}
