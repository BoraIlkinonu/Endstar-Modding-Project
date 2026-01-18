using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

// Token: 0x02000003 RID: 3
public class DanWHelperScript : MonoBehaviour
{
	// Token: 0x06000006 RID: 6 RVA: 0x000020CB File Offset: 0x000002CB
	private void Awake()
	{
		this.playerInputActions = new PlayerInputActions();
	}

	// Token: 0x06000007 RID: 7 RVA: 0x000020D8 File Offset: 0x000002D8
	private void OnEnable()
	{
		this.playerInputActions.Player.MainToolAction.started += this.MainToolAction;
		this.playerInputActions.Player.MainToolAction.Enable();
	}

	// Token: 0x06000008 RID: 8 RVA: 0x00002121 File Offset: 0x00000321
	private void Start()
	{
		this.animator = base.GetComponent<PlayerNetworkController>().AppearanceController.AppearanceAnimator.Animator;
		this.cooldown = this.slashes[0].newCooldown;
	}

	// Token: 0x06000009 RID: 9 RVA: 0x00002158 File Offset: 0x00000358
	private void OnDisable()
	{
		this.playerInputActions.Player.MainToolAction.started -= this.MainToolAction;
		this.playerInputActions.Player.MainToolAction.Disable();
	}

	// Token: 0x0600000A RID: 10 RVA: 0x000021A1 File Offset: 0x000003A1
	private void Update()
	{
		if (this.cooldown > 0f)
		{
			this.cooldown -= Time.deltaTime;
		}
	}

	// Token: 0x0600000B RID: 11 RVA: 0x000021C4 File Offset: 0x000003C4
	private void MainToolAction(InputAction.CallbackContext callbackContext)
	{
		if (this.cooldown > 0f)
		{
			return;
		}
		this.animator.SetTrigger("Attack");
		this.slashes[this.comboBookmark].slashFX.Play();
		if (this.comboBookmark < this.comboAmount)
		{
			this.comboBookmark++;
		}
		else
		{
			this.comboBookmark = 0;
		}
		this.cooldown = this.slashes[this.comboBookmark].newCooldown;
		this.animator.SetInteger("ComboBookmark", this.comboBookmark);
	}

	// Token: 0x04000004 RID: 4
	public Animator animator;

	// Token: 0x04000005 RID: 5
	public int comboBookmark;

	// Token: 0x04000006 RID: 6
	public int comboAmount = 2;

	// Token: 0x04000007 RID: 7
	public float cooldown = 1f;

	// Token: 0x04000008 RID: 8
	public List<Slash> slashes;

	// Token: 0x04000009 RID: 9
	private PlayerInputActions playerInputActions;
}
