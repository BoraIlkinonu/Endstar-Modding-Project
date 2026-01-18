using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Endless.Data
{
	// Token: 0x02000006 RID: 6
	public class PlayerInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000C RID: 12 RVA: 0x000023CC File Offset: 0x000005CC
		public InputActionAsset asset { get; }

		// Token: 0x0600000D RID: 13 RVA: 0x000023D4 File Offset: 0x000005D4
		public PlayerInputActions()
		{
			this.asset = InputActionAsset.FromJson("{\n    \"name\": \"PlayerInputActions\",\n    \"maps\": [\n        {\n            \"name\": \"Player\",\n            \"id\": \"d2ba8fbf-edd0-41ec-8e88-0a219637a43e\",\n            \"actions\": [\n                {\n                    \"name\": \"Move\",\n                    \"type\": \"Value\",\n                    \"id\": \"530c6077-7be7-42c1-a1b0-3616f501acc4\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Look\",\n                    \"type\": \"Value\",\n                    \"id\": \"5dc1327c-c9c8-4be2-a2a3-e93b4efbe9c9\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"ScaleVector2(x=0.05,y=0.05)\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Zoom\",\n                    \"type\": \"Value\",\n                    \"id\": \"16cc75a5-ba06-461f-82e0-41606f143952\",\n                    \"expectedControlType\": \"Axis\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"CameraPanWindows\",\n                    \"type\": \"Button\",\n                    \"id\": \"bb903760-2883-435c-a35f-72b172d819fc\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"CameraPanMac\",\n                    \"type\": \"Button\",\n                    \"id\": \"83f8dc21-2762-4e3f-ba1f-47e87be32edc\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MajorEquipmentPrimaryAction\",\n                    \"type\": \"Button\",\n                    \"id\": \"84b89e63-0776-4999-b3e2-adcff022596c\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MajorEquipmentAlternateAction\",\n                    \"type\": \"Button\",\n                    \"id\": \"84b40f95-ad04-4faf-9823-5e2c13976f38\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MinorEquipment\",\n                    \"type\": \"Button\",\n                    \"id\": \"b1178e82-226e-4424-a26b-caca8c27f8ac\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Jump\",\n                    \"type\": \"Button\",\n                    \"id\": \"dc71fa27-9a53-4b2f-ac15-7b99f4528cb7\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"EnableCursor\",\n                    \"type\": \"Button\",\n                    \"id\": \"ecb50b8a-8fc7-4858-a772-3fe373f0bfe8\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Down\",\n                    \"type\": \"Button\",\n                    \"id\": \"b1eb43f7-3443-4ba6-8a16-1758ba331dc9\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MainToolAction\",\n                    \"type\": \"Button\",\n                    \"id\": \"c87655b4-ff78-49ba-a7c1-7ffa3fa0810e\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"AlternativeToolAction\",\n                    \"type\": \"Button\",\n                    \"id\": \"403c904a-401d-4e55-97cc-77eab66817c8\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Ghost\",\n                    \"type\": \"Button\",\n                    \"id\": \"6cd140ea-6f7e-48be-bd1e-b663583d744d\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"CycleVisuals\",\n                    \"type\": \"Button\",\n                    \"id\": \"01300fe4-622a-4cbf-9a1a-5d45144e44f1\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"FlipGameState\",\n                    \"type\": \"Button\",\n                    \"id\": \"d2a84b88-2ca3-43d1-b0b1-2521ec3bb100\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"EmptyTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"852141d1-5e95-473d-adba-42d116df0277\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PaintingTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"02d8670f-2d0e-4a80-b308-678e261105ab\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PropBasedTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"8d245fee-a680-4e8a-b373-e4a383a94d68\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"EraseTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"919b6b98-7498-406e-9a02-0fa75bd32340\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"WiringTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"d474a5e5-f368-4273-a9b5-f2e64b8a59e7\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"InspectorTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"618ef007-e42d-43d4-ad2a-a4da1e8fcd7c\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"CopyTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"a85158be-6918-4991-b61b-27b55ce73869\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MoveTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"d2c4cb5f-486b-4af8-8e5f-6fb51c9a76ce\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Interact\",\n                    \"type\": \"Button\",\n                    \"id\": \"ceec459a-6daf-4ce8-9c4d-1577f75b55d2\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PopLastRerouteNode\",\n                    \"type\": \"Button\",\n                    \"id\": \"a9e27920-3317-497a-862a-3dfd65b4703a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"OpenUserReportModal\",\n                    \"type\": \"Button\",\n                    \"id\": \"660802f8-ea66-42eb-a608-3b06c3a173b3\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ToggleDevLog\",\n                    \"type\": \"Button\",\n                    \"id\": \"5e570a38-c1f5-4a28-90ec-7fe409de55fc\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"OpenLogsModal\",\n                    \"type\": \"Button\",\n                    \"id\": \"caed0c71-9e73-4a72-9f35-f581df0a2b00\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrimaryPointerPosition\",\n                    \"type\": \"PassThrough\",\n                    \"id\": \"d2e04d6b-37f3-44be-9011-6291d208189b\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ScriptAutoComplete\",\n                    \"type\": \"Button\",\n                    \"id\": \"14c0c115-73af-47a1-af26-d9646049952f\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Pinch\",\n                    \"type\": \"Value\",\n                    \"id\": \"69660cd6-2549-40ae-9c5a-28d942216ecd\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"LeftControl\",\n                    \"type\": \"Button\",\n                    \"id\": \"4a10fe13-68ff-4b2b-8371-8979c94b64e7\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory1\",\n                    \"type\": \"Button\",\n                    \"id\": \"4e963e2f-b16e-4aab-81e3-32ff48607f86\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory2\",\n                    \"type\": \"Button\",\n                    \"id\": \"61ce9130-917b-4cf7-9a81-f1406cdb2b78\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory3\",\n                    \"type\": \"Button\",\n                    \"id\": \"3041587f-1d92-4053-833d-69df079430aa\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory4\",\n                    \"type\": \"Button\",\n                    \"id\": \"523e66d0-f473-4790-89ba-2c2177b01204\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory5\",\n                    \"type\": \"Button\",\n                    \"id\": \"5fa1c08f-18ae-46e6-976c-14a0b6c742a5\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory6\",\n                    \"type\": \"Button\",\n                    \"id\": \"d2642d2a-7006-475c-9187-5f4310e4a0e5\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory7\",\n                    \"type\": \"Button\",\n                    \"id\": \"b01000f8-7604-4b83-b8ad-fefe0b0c1617\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory8\",\n                    \"type\": \"Button\",\n                    \"id\": \"88e31722-d9a2-4fd3-a8ef-55e727383aef\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory9\",\n                    \"type\": \"Button\",\n                    \"id\": \"6da88458-e7ac-41d4-8bc7-fd4d6d0a12e1\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory10\",\n                    \"type\": \"Button\",\n                    \"id\": \"32a0a6e9-204b-48ff-bf51-99448b2f2b32\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Reload\",\n                    \"type\": \"Button\",\n                    \"id\": \"1c26c07c-dd8d-45b5-8ae0-2040032113ea\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"DisplayGameLibraryModerationWindow\",\n                    \"type\": \"Button\",\n                    \"id\": \"7e4abc4e-f30e-4155-be46-efd9602a6960\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"\",\n                    \"id\": \"978bfe49-cc26-4a3d-ab7b-7d7a29327403\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Gamepad\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"WASD\",\n                    \"id\": \"00ca640b-d935-4593-8157-c05846ea39b3\",\n                    \"path\": \"Dpad\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"e2062cb9-1b15-46a2-838c-2f8d72a0bdd9\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"8180e8bd-4097-4f4e-ab88-4523101a6ce9\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"320bffee-a40b-4347-ac70-c210eb8bc73a\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"1c5327b5-f71c-4f60-99c7-4e737386f1d1\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"d2581a9b-1d11-4566-b27d-b92aff5fabbc\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"2e46982e-44cc-431b-9f0b-c11910bf467a\",\n                    \"path\": \"<Keyboard>/leftArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \";Keyboard&Mouse\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\":[...string is too long...]");
			this.m_Player = this.asset.FindActionMap("Player", true);
			this.m_Player_Move = this.m_Player.FindAction("Move", true);
			this.m_Player_Look = this.m_Player.FindAction("Look", true);
			this.m_Player_Zoom = this.m_Player.FindAction("Zoom", true);
			this.m_Player_CameraPanWindows = this.m_Player.FindAction("CameraPanWindows", true);
			this.m_Player_CameraPanMac = this.m_Player.FindAction("CameraPanMac", true);
			this.m_Player_MajorEquipmentPrimaryAction = this.m_Player.FindAction("MajorEquipmentPrimaryAction", true);
			this.m_Player_MajorEquipmentAlternateAction = this.m_Player.FindAction("MajorEquipmentAlternateAction", true);
			this.m_Player_MinorEquipment = this.m_Player.FindAction("MinorEquipment", true);
			this.m_Player_Jump = this.m_Player.FindAction("Jump", true);
			this.m_Player_EnableCursor = this.m_Player.FindAction("EnableCursor", true);
			this.m_Player_Down = this.m_Player.FindAction("Down", true);
			this.m_Player_MainToolAction = this.m_Player.FindAction("MainToolAction", true);
			this.m_Player_AlternativeToolAction = this.m_Player.FindAction("AlternativeToolAction", true);
			this.m_Player_Ghost = this.m_Player.FindAction("Ghost", true);
			this.m_Player_CycleVisuals = this.m_Player.FindAction("CycleVisuals", true);
			this.m_Player_FlipGameState = this.m_Player.FindAction("FlipGameState", true);
			this.m_Player_EmptyTool = this.m_Player.FindAction("EmptyTool", true);
			this.m_Player_PaintingTool = this.m_Player.FindAction("PaintingTool", true);
			this.m_Player_PropBasedTool = this.m_Player.FindAction("PropBasedTool", true);
			this.m_Player_EraseTool = this.m_Player.FindAction("EraseTool", true);
			this.m_Player_WiringTool = this.m_Player.FindAction("WiringTool", true);
			this.m_Player_InspectorTool = this.m_Player.FindAction("InspectorTool", true);
			this.m_Player_CopyTool = this.m_Player.FindAction("CopyTool", true);
			this.m_Player_MoveTool = this.m_Player.FindAction("MoveTool", true);
			this.m_Player_Interact = this.m_Player.FindAction("Interact", true);
			this.m_Player_PopLastRerouteNode = this.m_Player.FindAction("PopLastRerouteNode", true);
			this.m_Player_OpenUserReportModal = this.m_Player.FindAction("OpenUserReportModal", true);
			this.m_Player_ToggleDevLog = this.m_Player.FindAction("ToggleDevLog", true);
			this.m_Player_OpenLogsModal = this.m_Player.FindAction("OpenLogsModal", true);
			this.m_Player_PrimaryPointerPosition = this.m_Player.FindAction("PrimaryPointerPosition", true);
			this.m_Player_ScriptAutoComplete = this.m_Player.FindAction("ScriptAutoComplete", true);
			this.m_Player_Pinch = this.m_Player.FindAction("Pinch", true);
			this.m_Player_LeftControl = this.m_Player.FindAction("LeftControl", true);
			this.m_Player_Inventory1 = this.m_Player.FindAction("Inventory1", true);
			this.m_Player_Inventory2 = this.m_Player.FindAction("Inventory2", true);
			this.m_Player_Inventory3 = this.m_Player.FindAction("Inventory3", true);
			this.m_Player_Inventory4 = this.m_Player.FindAction("Inventory4", true);
			this.m_Player_Inventory5 = this.m_Player.FindAction("Inventory5", true);
			this.m_Player_Inventory6 = this.m_Player.FindAction("Inventory6", true);
			this.m_Player_Inventory7 = this.m_Player.FindAction("Inventory7", true);
			this.m_Player_Inventory8 = this.m_Player.FindAction("Inventory8", true);
			this.m_Player_Inventory9 = this.m_Player.FindAction("Inventory9", true);
			this.m_Player_Inventory10 = this.m_Player.FindAction("Inventory10", true);
			this.m_Player_Reload = this.m_Player.FindAction("Reload", true);
			this.m_Player_DisplayGameLibraryModerationWindow = this.m_Player.FindAction("DisplayGameLibraryModerationWindow", true);
			this.m_UI = this.asset.FindActionMap("UI", true);
			this.m_UI_Navigate = this.m_UI.FindAction("Navigate", true);
			this.m_UI_Submit = this.m_UI.FindAction("Submit", true);
			this.m_UI_Cancel = this.m_UI.FindAction("Cancel", true);
			this.m_UI_Point = this.m_UI.FindAction("Point", true);
			this.m_UI_Click = this.m_UI.FindAction("Click", true);
			this.m_UI_ScrollWheel = this.m_UI.FindAction("ScrollWheel", true);
			this.m_UI_MiddleClick = this.m_UI.FindAction("MiddleClick", true);
			this.m_UI_RightClick = this.m_UI.FindAction("RightClick", true);
			this.m_UI_TrackedDevicePosition = this.m_UI.FindAction("TrackedDevicePosition", true);
			this.m_UI_TrackedDeviceOrientation = this.m_UI.FindAction("TrackedDeviceOrientation", true);
			this.m_UI_SelectNext = this.m_UI.FindAction("SelectNext", true);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002968 File Offset: 0x00000B68
		~PlayerInputActions()
		{
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002990 File Offset: 0x00000B90
		public void Dispose()
		{
			global::UnityEngine.Object.Destroy(this.asset);
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000010 RID: 16 RVA: 0x0000299D File Offset: 0x00000B9D
		// (set) Token: 0x06000011 RID: 17 RVA: 0x000029AA File Offset: 0x00000BAA
		public InputBinding? bindingMask
		{
			get
			{
				return this.asset.bindingMask;
			}
			set
			{
				this.asset.bindingMask = value;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000029B8 File Offset: 0x00000BB8
		// (set) Token: 0x06000013 RID: 19 RVA: 0x000029C5 File Offset: 0x00000BC5
		public ReadOnlyArray<InputDevice>? devices
		{
			get
			{
				return this.asset.devices;
			}
			set
			{
				this.asset.devices = value;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000029D3 File Offset: 0x00000BD3
		public ReadOnlyArray<InputControlScheme> controlSchemes
		{
			get
			{
				return this.asset.controlSchemes;
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000029E0 File Offset: 0x00000BE0
		public bool Contains(InputAction action)
		{
			return this.asset.Contains(action);
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000029EE File Offset: 0x00000BEE
		public IEnumerator<InputAction> GetEnumerator()
		{
			return this.asset.GetEnumerator();
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000029FB File Offset: 0x00000BFB
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002A03 File Offset: 0x00000C03
		public void Enable()
		{
			this.asset.Enable();
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002A10 File Offset: 0x00000C10
		public void Disable()
		{
			this.asset.Disable();
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002A1D File Offset: 0x00000C1D
		public IEnumerable<InputBinding> bindings
		{
			get
			{
				return this.asset.bindings;
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002A2A File Offset: 0x00000C2A
		public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
		{
			return this.asset.FindAction(actionNameOrId, throwIfNotFound);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002A39 File Offset: 0x00000C39
		public int FindBinding(InputBinding bindingMask, out InputAction action)
		{
			return this.asset.FindBinding(bindingMask, out action);
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002A48 File Offset: 0x00000C48
		public PlayerInputActions.PlayerActions Player
		{
			get
			{
				return new PlayerInputActions.PlayerActions(this);
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600001E RID: 30 RVA: 0x00002A50 File Offset: 0x00000C50
		public PlayerInputActions.UIActions UI
		{
			get
			{
				return new PlayerInputActions.UIActions(this);
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002A58 File Offset: 0x00000C58
		public InputControlScheme KeyboardMouseScheme
		{
			get
			{
				if (this.m_KeyboardMouseSchemeIndex == -1)
				{
					this.m_KeyboardMouseSchemeIndex = this.asset.FindControlSchemeIndex("Keyboard&Mouse");
				}
				return this.asset.controlSchemes[this.m_KeyboardMouseSchemeIndex];
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00002AA0 File Offset: 0x00000CA0
		public InputControlScheme GamepadScheme
		{
			get
			{
				if (this.m_GamepadSchemeIndex == -1)
				{
					this.m_GamepadSchemeIndex = this.asset.FindControlSchemeIndex("Gamepad");
				}
				return this.asset.controlSchemes[this.m_GamepadSchemeIndex];
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000021 RID: 33 RVA: 0x00002AE8 File Offset: 0x00000CE8
		public InputControlScheme TouchScheme
		{
			get
			{
				if (this.m_TouchSchemeIndex == -1)
				{
					this.m_TouchSchemeIndex = this.asset.FindControlSchemeIndex("Touch");
				}
				return this.asset.controlSchemes[this.m_TouchSchemeIndex];
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000022 RID: 34 RVA: 0x00002B30 File Offset: 0x00000D30
		public InputControlScheme JoystickScheme
		{
			get
			{
				if (this.m_JoystickSchemeIndex == -1)
				{
					this.m_JoystickSchemeIndex = this.asset.FindControlSchemeIndex("Joystick");
				}
				return this.asset.controlSchemes[this.m_JoystickSchemeIndex];
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002B78 File Offset: 0x00000D78
		public InputControlScheme XRScheme
		{
			get
			{
				if (this.m_XRSchemeIndex == -1)
				{
					this.m_XRSchemeIndex = this.asset.FindControlSchemeIndex("XR");
				}
				return this.asset.controlSchemes[this.m_XRSchemeIndex];
			}
		}

		// Token: 0x040000C6 RID: 198
		private readonly InputActionMap m_Player;

		// Token: 0x040000C7 RID: 199
		private List<PlayerInputActions.IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<PlayerInputActions.IPlayerActions>();

		// Token: 0x040000C8 RID: 200
		private readonly InputAction m_Player_Move;

		// Token: 0x040000C9 RID: 201
		private readonly InputAction m_Player_Look;

		// Token: 0x040000CA RID: 202
		private readonly InputAction m_Player_Zoom;

		// Token: 0x040000CB RID: 203
		private readonly InputAction m_Player_CameraPanWindows;

		// Token: 0x040000CC RID: 204
		private readonly InputAction m_Player_CameraPanMac;

		// Token: 0x040000CD RID: 205
		private readonly InputAction m_Player_MajorEquipmentPrimaryAction;

		// Token: 0x040000CE RID: 206
		private readonly InputAction m_Player_MajorEquipmentAlternateAction;

		// Token: 0x040000CF RID: 207
		private readonly InputAction m_Player_MinorEquipment;

		// Token: 0x040000D0 RID: 208
		private readonly InputAction m_Player_Jump;

		// Token: 0x040000D1 RID: 209
		private readonly InputAction m_Player_EnableCursor;

		// Token: 0x040000D2 RID: 210
		private readonly InputAction m_Player_Down;

		// Token: 0x040000D3 RID: 211
		private readonly InputAction m_Player_MainToolAction;

		// Token: 0x040000D4 RID: 212
		private readonly InputAction m_Player_AlternativeToolAction;

		// Token: 0x040000D5 RID: 213
		private readonly InputAction m_Player_Ghost;

		// Token: 0x040000D6 RID: 214
		private readonly InputAction m_Player_CycleVisuals;

		// Token: 0x040000D7 RID: 215
		private readonly InputAction m_Player_FlipGameState;

		// Token: 0x040000D8 RID: 216
		private readonly InputAction m_Player_EmptyTool;

		// Token: 0x040000D9 RID: 217
		private readonly InputAction m_Player_PaintingTool;

		// Token: 0x040000DA RID: 218
		private readonly InputAction m_Player_PropBasedTool;

		// Token: 0x040000DB RID: 219
		private readonly InputAction m_Player_EraseTool;

		// Token: 0x040000DC RID: 220
		private readonly InputAction m_Player_WiringTool;

		// Token: 0x040000DD RID: 221
		private readonly InputAction m_Player_InspectorTool;

		// Token: 0x040000DE RID: 222
		private readonly InputAction m_Player_CopyTool;

		// Token: 0x040000DF RID: 223
		private readonly InputAction m_Player_MoveTool;

		// Token: 0x040000E0 RID: 224
		private readonly InputAction m_Player_Interact;

		// Token: 0x040000E1 RID: 225
		private readonly InputAction m_Player_PopLastRerouteNode;

		// Token: 0x040000E2 RID: 226
		private readonly InputAction m_Player_OpenUserReportModal;

		// Token: 0x040000E3 RID: 227
		private readonly InputAction m_Player_ToggleDevLog;

		// Token: 0x040000E4 RID: 228
		private readonly InputAction m_Player_OpenLogsModal;

		// Token: 0x040000E5 RID: 229
		private readonly InputAction m_Player_PrimaryPointerPosition;

		// Token: 0x040000E6 RID: 230
		private readonly InputAction m_Player_ScriptAutoComplete;

		// Token: 0x040000E7 RID: 231
		private readonly InputAction m_Player_Pinch;

		// Token: 0x040000E8 RID: 232
		private readonly InputAction m_Player_LeftControl;

		// Token: 0x040000E9 RID: 233
		private readonly InputAction m_Player_Inventory1;

		// Token: 0x040000EA RID: 234
		private readonly InputAction m_Player_Inventory2;

		// Token: 0x040000EB RID: 235
		private readonly InputAction m_Player_Inventory3;

		// Token: 0x040000EC RID: 236
		private readonly InputAction m_Player_Inventory4;

		// Token: 0x040000ED RID: 237
		private readonly InputAction m_Player_Inventory5;

		// Token: 0x040000EE RID: 238
		private readonly InputAction m_Player_Inventory6;

		// Token: 0x040000EF RID: 239
		private readonly InputAction m_Player_Inventory7;

		// Token: 0x040000F0 RID: 240
		private readonly InputAction m_Player_Inventory8;

		// Token: 0x040000F1 RID: 241
		private readonly InputAction m_Player_Inventory9;

		// Token: 0x040000F2 RID: 242
		private readonly InputAction m_Player_Inventory10;

		// Token: 0x040000F3 RID: 243
		private readonly InputAction m_Player_Reload;

		// Token: 0x040000F4 RID: 244
		private readonly InputAction m_Player_DisplayGameLibraryModerationWindow;

		// Token: 0x040000F5 RID: 245
		private readonly InputActionMap m_UI;

		// Token: 0x040000F6 RID: 246
		private List<PlayerInputActions.IUIActions> m_UIActionsCallbackInterfaces = new List<PlayerInputActions.IUIActions>();

		// Token: 0x040000F7 RID: 247
		private readonly InputAction m_UI_Navigate;

		// Token: 0x040000F8 RID: 248
		private readonly InputAction m_UI_Submit;

		// Token: 0x040000F9 RID: 249
		private readonly InputAction m_UI_Cancel;

		// Token: 0x040000FA RID: 250
		private readonly InputAction m_UI_Point;

		// Token: 0x040000FB RID: 251
		private readonly InputAction m_UI_Click;

		// Token: 0x040000FC RID: 252
		private readonly InputAction m_UI_ScrollWheel;

		// Token: 0x040000FD RID: 253
		private readonly InputAction m_UI_MiddleClick;

		// Token: 0x040000FE RID: 254
		private readonly InputAction m_UI_RightClick;

		// Token: 0x040000FF RID: 255
		private readonly InputAction m_UI_TrackedDevicePosition;

		// Token: 0x04000100 RID: 256
		private readonly InputAction m_UI_TrackedDeviceOrientation;

		// Token: 0x04000101 RID: 257
		private readonly InputAction m_UI_SelectNext;

		// Token: 0x04000102 RID: 258
		private int m_KeyboardMouseSchemeIndex = -1;

		// Token: 0x04000103 RID: 259
		private int m_GamepadSchemeIndex = -1;

		// Token: 0x04000104 RID: 260
		private int m_TouchSchemeIndex = -1;

		// Token: 0x04000105 RID: 261
		private int m_JoystickSchemeIndex = -1;

		// Token: 0x04000106 RID: 262
		private int m_XRSchemeIndex = -1;

		// Token: 0x0200000D RID: 13
		public struct PlayerActions
		{
			// Token: 0x06000041 RID: 65 RVA: 0x00002F05 File Offset: 0x00001105
			public PlayerActions(PlayerInputActions wrapper)
			{
				this.m_Wrapper = wrapper;
			}

			// Token: 0x17000018 RID: 24
			// (get) Token: 0x06000042 RID: 66 RVA: 0x00002F0E File Offset: 0x0000110E
			public InputAction Move
			{
				get
				{
					return this.m_Wrapper.m_Player_Move;
				}
			}

			// Token: 0x17000019 RID: 25
			// (get) Token: 0x06000043 RID: 67 RVA: 0x00002F1B File Offset: 0x0000111B
			public InputAction Look
			{
				get
				{
					return this.m_Wrapper.m_Player_Look;
				}
			}

			// Token: 0x1700001A RID: 26
			// (get) Token: 0x06000044 RID: 68 RVA: 0x00002F28 File Offset: 0x00001128
			public InputAction Zoom
			{
				get
				{
					return this.m_Wrapper.m_Player_Zoom;
				}
			}

			// Token: 0x1700001B RID: 27
			// (get) Token: 0x06000045 RID: 69 RVA: 0x00002F35 File Offset: 0x00001135
			public InputAction CameraPanWindows
			{
				get
				{
					return this.m_Wrapper.m_Player_CameraPanWindows;
				}
			}

			// Token: 0x1700001C RID: 28
			// (get) Token: 0x06000046 RID: 70 RVA: 0x00002F42 File Offset: 0x00001142
			public InputAction CameraPanMac
			{
				get
				{
					return this.m_Wrapper.m_Player_CameraPanMac;
				}
			}

			// Token: 0x1700001D RID: 29
			// (get) Token: 0x06000047 RID: 71 RVA: 0x00002F4F File Offset: 0x0000114F
			public InputAction MajorEquipmentPrimaryAction
			{
				get
				{
					return this.m_Wrapper.m_Player_MajorEquipmentPrimaryAction;
				}
			}

			// Token: 0x1700001E RID: 30
			// (get) Token: 0x06000048 RID: 72 RVA: 0x00002F5C File Offset: 0x0000115C
			public InputAction MajorEquipmentAlternateAction
			{
				get
				{
					return this.m_Wrapper.m_Player_MajorEquipmentAlternateAction;
				}
			}

			// Token: 0x1700001F RID: 31
			// (get) Token: 0x06000049 RID: 73 RVA: 0x00002F69 File Offset: 0x00001169
			public InputAction MinorEquipment
			{
				get
				{
					return this.m_Wrapper.m_Player_MinorEquipment;
				}
			}

			// Token: 0x17000020 RID: 32
			// (get) Token: 0x0600004A RID: 74 RVA: 0x00002F76 File Offset: 0x00001176
			public InputAction Jump
			{
				get
				{
					return this.m_Wrapper.m_Player_Jump;
				}
			}

			// Token: 0x17000021 RID: 33
			// (get) Token: 0x0600004B RID: 75 RVA: 0x00002F83 File Offset: 0x00001183
			public InputAction EnableCursor
			{
				get
				{
					return this.m_Wrapper.m_Player_EnableCursor;
				}
			}

			// Token: 0x17000022 RID: 34
			// (get) Token: 0x0600004C RID: 76 RVA: 0x00002F90 File Offset: 0x00001190
			public InputAction Down
			{
				get
				{
					return this.m_Wrapper.m_Player_Down;
				}
			}

			// Token: 0x17000023 RID: 35
			// (get) Token: 0x0600004D RID: 77 RVA: 0x00002F9D File Offset: 0x0000119D
			public InputAction MainToolAction
			{
				get
				{
					return this.m_Wrapper.m_Player_MainToolAction;
				}
			}

			// Token: 0x17000024 RID: 36
			// (get) Token: 0x0600004E RID: 78 RVA: 0x00002FAA File Offset: 0x000011AA
			public InputAction AlternativeToolAction
			{
				get
				{
					return this.m_Wrapper.m_Player_AlternativeToolAction;
				}
			}

			// Token: 0x17000025 RID: 37
			// (get) Token: 0x0600004F RID: 79 RVA: 0x00002FB7 File Offset: 0x000011B7
			public InputAction Ghost
			{
				get
				{
					return this.m_Wrapper.m_Player_Ghost;
				}
			}

			// Token: 0x17000026 RID: 38
			// (get) Token: 0x06000050 RID: 80 RVA: 0x00002FC4 File Offset: 0x000011C4
			public InputAction CycleVisuals
			{
				get
				{
					return this.m_Wrapper.m_Player_CycleVisuals;
				}
			}

			// Token: 0x17000027 RID: 39
			// (get) Token: 0x06000051 RID: 81 RVA: 0x00002FD1 File Offset: 0x000011D1
			public InputAction FlipGameState
			{
				get
				{
					return this.m_Wrapper.m_Player_FlipGameState;
				}
			}

			// Token: 0x17000028 RID: 40
			// (get) Token: 0x06000052 RID: 82 RVA: 0x00002FDE File Offset: 0x000011DE
			public InputAction EmptyTool
			{
				get
				{
					return this.m_Wrapper.m_Player_EmptyTool;
				}
			}

			// Token: 0x17000029 RID: 41
			// (get) Token: 0x06000053 RID: 83 RVA: 0x00002FEB File Offset: 0x000011EB
			public InputAction PaintingTool
			{
				get
				{
					return this.m_Wrapper.m_Player_PaintingTool;
				}
			}

			// Token: 0x1700002A RID: 42
			// (get) Token: 0x06000054 RID: 84 RVA: 0x00002FF8 File Offset: 0x000011F8
			public InputAction PropBasedTool
			{
				get
				{
					return this.m_Wrapper.m_Player_PropBasedTool;
				}
			}

			// Token: 0x1700002B RID: 43
			// (get) Token: 0x06000055 RID: 85 RVA: 0x00003005 File Offset: 0x00001205
			public InputAction EraseTool
			{
				get
				{
					return this.m_Wrapper.m_Player_EraseTool;
				}
			}

			// Token: 0x1700002C RID: 44
			// (get) Token: 0x06000056 RID: 86 RVA: 0x00003012 File Offset: 0x00001212
			public InputAction WiringTool
			{
				get
				{
					return this.m_Wrapper.m_Player_WiringTool;
				}
			}

			// Token: 0x1700002D RID: 45
			// (get) Token: 0x06000057 RID: 87 RVA: 0x0000301F File Offset: 0x0000121F
			public InputAction InspectorTool
			{
				get
				{
					return this.m_Wrapper.m_Player_InspectorTool;
				}
			}

			// Token: 0x1700002E RID: 46
			// (get) Token: 0x06000058 RID: 88 RVA: 0x0000302C File Offset: 0x0000122C
			public InputAction CopyTool
			{
				get
				{
					return this.m_Wrapper.m_Player_CopyTool;
				}
			}

			// Token: 0x1700002F RID: 47
			// (get) Token: 0x06000059 RID: 89 RVA: 0x00003039 File Offset: 0x00001239
			public InputAction MoveTool
			{
				get
				{
					return this.m_Wrapper.m_Player_MoveTool;
				}
			}

			// Token: 0x17000030 RID: 48
			// (get) Token: 0x0600005A RID: 90 RVA: 0x00003046 File Offset: 0x00001246
			public InputAction Interact
			{
				get
				{
					return this.m_Wrapper.m_Player_Interact;
				}
			}

			// Token: 0x17000031 RID: 49
			// (get) Token: 0x0600005B RID: 91 RVA: 0x00003053 File Offset: 0x00001253
			public InputAction PopLastRerouteNode
			{
				get
				{
					return this.m_Wrapper.m_Player_PopLastRerouteNode;
				}
			}

			// Token: 0x17000032 RID: 50
			// (get) Token: 0x0600005C RID: 92 RVA: 0x00003060 File Offset: 0x00001260
			public InputAction OpenUserReportModal
			{
				get
				{
					return this.m_Wrapper.m_Player_OpenUserReportModal;
				}
			}

			// Token: 0x17000033 RID: 51
			// (get) Token: 0x0600005D RID: 93 RVA: 0x0000306D File Offset: 0x0000126D
			public InputAction ToggleDevLog
			{
				get
				{
					return this.m_Wrapper.m_Player_ToggleDevLog;
				}
			}

			// Token: 0x17000034 RID: 52
			// (get) Token: 0x0600005E RID: 94 RVA: 0x0000307A File Offset: 0x0000127A
			public InputAction OpenLogsModal
			{
				get
				{
					return this.m_Wrapper.m_Player_OpenLogsModal;
				}
			}

			// Token: 0x17000035 RID: 53
			// (get) Token: 0x0600005F RID: 95 RVA: 0x00003087 File Offset: 0x00001287
			public InputAction PrimaryPointerPosition
			{
				get
				{
					return this.m_Wrapper.m_Player_PrimaryPointerPosition;
				}
			}

			// Token: 0x17000036 RID: 54
			// (get) Token: 0x06000060 RID: 96 RVA: 0x00003094 File Offset: 0x00001294
			public InputAction ScriptAutoComplete
			{
				get
				{
					return this.m_Wrapper.m_Player_ScriptAutoComplete;
				}
			}

			// Token: 0x17000037 RID: 55
			// (get) Token: 0x06000061 RID: 97 RVA: 0x000030A1 File Offset: 0x000012A1
			public InputAction Pinch
			{
				get
				{
					return this.m_Wrapper.m_Player_Pinch;
				}
			}

			// Token: 0x17000038 RID: 56
			// (get) Token: 0x06000062 RID: 98 RVA: 0x000030AE File Offset: 0x000012AE
			public InputAction LeftControl
			{
				get
				{
					return this.m_Wrapper.m_Player_LeftControl;
				}
			}

			// Token: 0x17000039 RID: 57
			// (get) Token: 0x06000063 RID: 99 RVA: 0x000030BB File Offset: 0x000012BB
			public InputAction Inventory1
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory1;
				}
			}

			// Token: 0x1700003A RID: 58
			// (get) Token: 0x06000064 RID: 100 RVA: 0x000030C8 File Offset: 0x000012C8
			public InputAction Inventory2
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory2;
				}
			}

			// Token: 0x1700003B RID: 59
			// (get) Token: 0x06000065 RID: 101 RVA: 0x000030D5 File Offset: 0x000012D5
			public InputAction Inventory3
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory3;
				}
			}

			// Token: 0x1700003C RID: 60
			// (get) Token: 0x06000066 RID: 102 RVA: 0x000030E2 File Offset: 0x000012E2
			public InputAction Inventory4
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory4;
				}
			}

			// Token: 0x1700003D RID: 61
			// (get) Token: 0x06000067 RID: 103 RVA: 0x000030EF File Offset: 0x000012EF
			public InputAction Inventory5
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory5;
				}
			}

			// Token: 0x1700003E RID: 62
			// (get) Token: 0x06000068 RID: 104 RVA: 0x000030FC File Offset: 0x000012FC
			public InputAction Inventory6
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory6;
				}
			}

			// Token: 0x1700003F RID: 63
			// (get) Token: 0x06000069 RID: 105 RVA: 0x00003109 File Offset: 0x00001309
			public InputAction Inventory7
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory7;
				}
			}

			// Token: 0x17000040 RID: 64
			// (get) Token: 0x0600006A RID: 106 RVA: 0x00003116 File Offset: 0x00001316
			public InputAction Inventory8
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory8;
				}
			}

			// Token: 0x17000041 RID: 65
			// (get) Token: 0x0600006B RID: 107 RVA: 0x00003123 File Offset: 0x00001323
			public InputAction Inventory9
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory9;
				}
			}

			// Token: 0x17000042 RID: 66
			// (get) Token: 0x0600006C RID: 108 RVA: 0x00003130 File Offset: 0x00001330
			public InputAction Inventory10
			{
				get
				{
					return this.m_Wrapper.m_Player_Inventory10;
				}
			}

			// Token: 0x17000043 RID: 67
			// (get) Token: 0x0600006D RID: 109 RVA: 0x0000313D File Offset: 0x0000133D
			public InputAction Reload
			{
				get
				{
					return this.m_Wrapper.m_Player_Reload;
				}
			}

			// Token: 0x17000044 RID: 68
			// (get) Token: 0x0600006E RID: 110 RVA: 0x0000314A File Offset: 0x0000134A
			public InputAction DisplayGameLibraryModerationWindow
			{
				get
				{
					return this.m_Wrapper.m_Player_DisplayGameLibraryModerationWindow;
				}
			}

			// Token: 0x0600006F RID: 111 RVA: 0x00003157 File Offset: 0x00001357
			public InputActionMap Get()
			{
				return this.m_Wrapper.m_Player;
			}

			// Token: 0x06000070 RID: 112 RVA: 0x00003164 File Offset: 0x00001364
			public void Enable()
			{
				this.Get().Enable();
			}

			// Token: 0x06000071 RID: 113 RVA: 0x00003171 File Offset: 0x00001371
			public void Disable()
			{
				this.Get().Disable();
			}

			// Token: 0x17000045 RID: 69
			// (get) Token: 0x06000072 RID: 114 RVA: 0x0000317E File Offset: 0x0000137E
			public bool enabled
			{
				get
				{
					return this.Get().enabled;
				}
			}

			// Token: 0x06000073 RID: 115 RVA: 0x0000318B File Offset: 0x0000138B
			public static implicit operator InputActionMap(PlayerInputActions.PlayerActions set)
			{
				return set.Get();
			}

			// Token: 0x06000074 RID: 116 RVA: 0x00003194 File Offset: 0x00001394
			public void AddCallbacks(PlayerInputActions.IPlayerActions instance)
			{
				if (instance == null || this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance))
				{
					return;
				}
				this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
				this.Move.started += instance.OnMove;
				this.Move.performed += instance.OnMove;
				this.Move.canceled += instance.OnMove;
				this.Look.started += instance.OnLook;
				this.Look.performed += instance.OnLook;
				this.Look.canceled += instance.OnLook;
				this.Zoom.started += instance.OnZoom;
				this.Zoom.performed += instance.OnZoom;
				this.Zoom.canceled += instance.OnZoom;
				this.CameraPanWindows.started += instance.OnCameraPanWindows;
				this.CameraPanWindows.performed += instance.OnCameraPanWindows;
				this.CameraPanWindows.canceled += instance.OnCameraPanWindows;
				this.CameraPanMac.started += instance.OnCameraPanMac;
				this.CameraPanMac.performed += instance.OnCameraPanMac;
				this.CameraPanMac.canceled += instance.OnCameraPanMac;
				this.MajorEquipmentPrimaryAction.started += instance.OnMajorEquipmentPrimaryAction;
				this.MajorEquipmentPrimaryAction.performed += instance.OnMajorEquipmentPrimaryAction;
				this.MajorEquipmentPrimaryAction.canceled += instance.OnMajorEquipmentPrimaryAction;
				this.MajorEquipmentAlternateAction.started += instance.OnMajorEquipmentAlternateAction;
				this.MajorEquipmentAlternateAction.performed += instance.OnMajorEquipmentAlternateAction;
				this.MajorEquipmentAlternateAction.canceled += instance.OnMajorEquipmentAlternateAction;
				this.MinorEquipment.started += instance.OnMinorEquipment;
				this.MinorEquipment.performed += instance.OnMinorEquipment;
				this.MinorEquipment.canceled += instance.OnMinorEquipment;
				this.Jump.started += instance.OnJump;
				this.Jump.performed += instance.OnJump;
				this.Jump.canceled += instance.OnJump;
				this.EnableCursor.started += instance.OnEnableCursor;
				this.EnableCursor.performed += instance.OnEnableCursor;
				this.EnableCursor.canceled += instance.OnEnableCursor;
				this.Down.started += instance.OnDown;
				this.Down.performed += instance.OnDown;
				this.Down.canceled += instance.OnDown;
				this.MainToolAction.started += instance.OnMainToolAction;
				this.MainToolAction.performed += instance.OnMainToolAction;
				this.MainToolAction.canceled += instance.OnMainToolAction;
				this.AlternativeToolAction.started += instance.OnAlternativeToolAction;
				this.AlternativeToolAction.performed += instance.OnAlternativeToolAction;
				this.AlternativeToolAction.canceled += instance.OnAlternativeToolAction;
				this.Ghost.started += instance.OnGhost;
				this.Ghost.performed += instance.OnGhost;
				this.Ghost.canceled += instance.OnGhost;
				this.CycleVisuals.started += instance.OnCycleVisuals;
				this.CycleVisuals.performed += instance.OnCycleVisuals;
				this.CycleVisuals.canceled += instance.OnCycleVisuals;
				this.FlipGameState.started += instance.OnFlipGameState;
				this.FlipGameState.performed += instance.OnFlipGameState;
				this.FlipGameState.canceled += instance.OnFlipGameState;
				this.EmptyTool.started += instance.OnEmptyTool;
				this.EmptyTool.performed += instance.OnEmptyTool;
				this.EmptyTool.canceled += instance.OnEmptyTool;
				this.PaintingTool.started += instance.OnPaintingTool;
				this.PaintingTool.performed += instance.OnPaintingTool;
				this.PaintingTool.canceled += instance.OnPaintingTool;
				this.PropBasedTool.started += instance.OnPropBasedTool;
				this.PropBasedTool.performed += instance.OnPropBasedTool;
				this.PropBasedTool.canceled += instance.OnPropBasedTool;
				this.EraseTool.started += instance.OnEraseTool;
				this.EraseTool.performed += instance.OnEraseTool;
				this.EraseTool.canceled += instance.OnEraseTool;
				this.WiringTool.started += instance.OnWiringTool;
				this.WiringTool.performed += instance.OnWiringTool;
				this.WiringTool.canceled += instance.OnWiringTool;
				this.InspectorTool.started += instance.OnInspectorTool;
				this.InspectorTool.performed += instance.OnInspectorTool;
				this.InspectorTool.canceled += instance.OnInspectorTool;
				this.CopyTool.started += instance.OnCopyTool;
				this.CopyTool.performed += instance.OnCopyTool;
				this.CopyTool.canceled += instance.OnCopyTool;
				this.MoveTool.started += instance.OnMoveTool;
				this.MoveTool.performed += instance.OnMoveTool;
				this.MoveTool.canceled += instance.OnMoveTool;
				this.Interact.started += instance.OnInteract;
				this.Interact.performed += instance.OnInteract;
				this.Interact.canceled += instance.OnInteract;
				this.PopLastRerouteNode.started += instance.OnPopLastRerouteNode;
				this.PopLastRerouteNode.performed += instance.OnPopLastRerouteNode;
				this.PopLastRerouteNode.canceled += instance.OnPopLastRerouteNode;
				this.OpenUserReportModal.started += instance.OnOpenUserReportModal;
				this.OpenUserReportModal.performed += instance.OnOpenUserReportModal;
				this.OpenUserReportModal.canceled += instance.OnOpenUserReportModal;
				this.ToggleDevLog.started += instance.OnToggleDevLog;
				this.ToggleDevLog.performed += instance.OnToggleDevLog;
				this.ToggleDevLog.canceled += instance.OnToggleDevLog;
				this.OpenLogsModal.started += instance.OnOpenLogsModal;
				this.OpenLogsModal.performed += instance.OnOpenLogsModal;
				this.OpenLogsModal.canceled += instance.OnOpenLogsModal;
				this.PrimaryPointerPosition.started += instance.OnPrimaryPointerPosition;
				this.PrimaryPointerPosition.performed += instance.OnPrimaryPointerPosition;
				this.PrimaryPointerPosition.canceled += instance.OnPrimaryPointerPosition;
				this.ScriptAutoComplete.started += instance.OnScriptAutoComplete;
				this.ScriptAutoComplete.performed += instance.OnScriptAutoComplete;
				this.ScriptAutoComplete.canceled += instance.OnScriptAutoComplete;
				this.Pinch.started += instance.OnPinch;
				this.Pinch.performed += instance.OnPinch;
				this.Pinch.canceled += instance.OnPinch;
				this.LeftControl.started += instance.OnLeftControl;
				this.LeftControl.performed += instance.OnLeftControl;
				this.LeftControl.canceled += instance.OnLeftControl;
				this.Inventory1.started += instance.OnInventory1;
				this.Inventory1.performed += instance.OnInventory1;
				this.Inventory1.canceled += instance.OnInventory1;
				this.Inventory2.started += instance.OnInventory2;
				this.Inventory2.performed += instance.OnInventory2;
				this.Inventory2.canceled += instance.OnInventory2;
				this.Inventory3.started += instance.OnInventory3;
				this.Inventory3.performed += instance.OnInventory3;
				this.Inventory3.canceled += instance.OnInventory3;
				this.Inventory4.started += instance.OnInventory4;
				this.Inventory4.performed += instance.OnInventory4;
				this.Inventory4.canceled += instance.OnInventory4;
				this.Inventory5.started += instance.OnInventory5;
				this.Inventory5.performed += instance.OnInventory5;
				this.Inventory5.canceled += instance.OnInventory5;
				this.Inventory6.started += instance.OnInventory6;
				this.Inventory6.performed += instance.OnInventory6;
				this.Inventory6.canceled += instance.OnInventory6;
				this.Inventory7.started += instance.OnInventory7;
				this.Inventory7.performed += instance.OnInventory7;
				this.Inventory7.canceled += instance.OnInventory7;
				this.Inventory8.started += instance.OnInventory8;
				this.Inventory8.performed += instance.OnInventory8;
				this.Inventory8.canceled += instance.OnInventory8;
				this.Inventory9.started += instance.OnInventory9;
				this.Inventory9.performed += instance.OnInventory9;
				this.Inventory9.canceled += instance.OnInventory9;
				this.Inventory10.started += instance.OnInventory10;
				this.Inventory10.performed += instance.OnInventory10;
				this.Inventory10.canceled += instance.OnInventory10;
				this.Reload.started += instance.OnReload;
				this.Reload.performed += instance.OnReload;
				this.Reload.canceled += instance.OnReload;
				this.DisplayGameLibraryModerationWindow.started += instance.OnDisplayGameLibraryModerationWindow;
				this.DisplayGameLibraryModerationWindow.performed += instance.OnDisplayGameLibraryModerationWindow;
				this.DisplayGameLibraryModerationWindow.canceled += instance.OnDisplayGameLibraryModerationWindow;
			}

			// Token: 0x06000075 RID: 117 RVA: 0x00003E74 File Offset: 0x00002074
			private void UnregisterCallbacks(PlayerInputActions.IPlayerActions instance)
			{
				this.Move.started -= instance.OnMove;
				this.Move.performed -= instance.OnMove;
				this.Move.canceled -= instance.OnMove;
				this.Look.started -= instance.OnLook;
				this.Look.performed -= instance.OnLook;
				this.Look.canceled -= instance.OnLook;
				this.Zoom.started -= instance.OnZoom;
				this.Zoom.performed -= instance.OnZoom;
				this.Zoom.canceled -= instance.OnZoom;
				this.CameraPanWindows.started -= instance.OnCameraPanWindows;
				this.CameraPanWindows.performed -= instance.OnCameraPanWindows;
				this.CameraPanWindows.canceled -= instance.OnCameraPanWindows;
				this.CameraPanMac.started -= instance.OnCameraPanMac;
				this.CameraPanMac.performed -= instance.OnCameraPanMac;
				this.CameraPanMac.canceled -= instance.OnCameraPanMac;
				this.MajorEquipmentPrimaryAction.started -= instance.OnMajorEquipmentPrimaryAction;
				this.MajorEquipmentPrimaryAction.performed -= instance.OnMajorEquipmentPrimaryAction;
				this.MajorEquipmentPrimaryAction.canceled -= instance.OnMajorEquipmentPrimaryAction;
				this.MajorEquipmentAlternateAction.started -= instance.OnMajorEquipmentAlternateAction;
				this.MajorEquipmentAlternateAction.performed -= instance.OnMajorEquipmentAlternateAction;
				this.MajorEquipmentAlternateAction.canceled -= instance.OnMajorEquipmentAlternateAction;
				this.MinorEquipment.started -= instance.OnMinorEquipment;
				this.MinorEquipment.performed -= instance.OnMinorEquipment;
				this.MinorEquipment.canceled -= instance.OnMinorEquipment;
				this.Jump.started -= instance.OnJump;
				this.Jump.performed -= instance.OnJump;
				this.Jump.canceled -= instance.OnJump;
				this.EnableCursor.started -= instance.OnEnableCursor;
				this.EnableCursor.performed -= instance.OnEnableCursor;
				this.EnableCursor.canceled -= instance.OnEnableCursor;
				this.Down.started -= instance.OnDown;
				this.Down.performed -= instance.OnDown;
				this.Down.canceled -= instance.OnDown;
				this.MainToolAction.started -= instance.OnMainToolAction;
				this.MainToolAction.performed -= instance.OnMainToolAction;
				this.MainToolAction.canceled -= instance.OnMainToolAction;
				this.AlternativeToolAction.started -= instance.OnAlternativeToolAction;
				this.AlternativeToolAction.performed -= instance.OnAlternativeToolAction;
				this.AlternativeToolAction.canceled -= instance.OnAlternativeToolAction;
				this.Ghost.started -= instance.OnGhost;
				this.Ghost.performed -= instance.OnGhost;
				this.Ghost.canceled -= instance.OnGhost;
				this.CycleVisuals.started -= instance.OnCycleVisuals;
				this.CycleVisuals.performed -= instance.OnCycleVisuals;
				this.CycleVisuals.canceled -= instance.OnCycleVisuals;
				this.FlipGameState.started -= instance.OnFlipGameState;
				this.FlipGameState.performed -= instance.OnFlipGameState;
				this.FlipGameState.canceled -= instance.OnFlipGameState;
				this.EmptyTool.started -= instance.OnEmptyTool;
				this.EmptyTool.performed -= instance.OnEmptyTool;
				this.EmptyTool.canceled -= instance.OnEmptyTool;
				this.PaintingTool.started -= instance.OnPaintingTool;
				this.PaintingTool.performed -= instance.OnPaintingTool;
				this.PaintingTool.canceled -= instance.OnPaintingTool;
				this.PropBasedTool.started -= instance.OnPropBasedTool;
				this.PropBasedTool.performed -= instance.OnPropBasedTool;
				this.PropBasedTool.canceled -= instance.OnPropBasedTool;
				this.EraseTool.started -= instance.OnEraseTool;
				this.EraseTool.performed -= instance.OnEraseTool;
				this.EraseTool.canceled -= instance.OnEraseTool;
				this.WiringTool.started -= instance.OnWiringTool;
				this.WiringTool.performed -= instance.OnWiringTool;
				this.WiringTool.canceled -= instance.OnWiringTool;
				this.InspectorTool.started -= instance.OnInspectorTool;
				this.InspectorTool.performed -= instance.OnInspectorTool;
				this.InspectorTool.canceled -= instance.OnInspectorTool;
				this.CopyTool.started -= instance.OnCopyTool;
				this.CopyTool.performed -= instance.OnCopyTool;
				this.CopyTool.canceled -= instance.OnCopyTool;
				this.MoveTool.started -= instance.OnMoveTool;
				this.MoveTool.performed -= instance.OnMoveTool;
				this.MoveTool.canceled -= instance.OnMoveTool;
				this.Interact.started -= instance.OnInteract;
				this.Interact.performed -= instance.OnInteract;
				this.Interact.canceled -= instance.OnInteract;
				this.PopLastRerouteNode.started -= instance.OnPopLastRerouteNode;
				this.PopLastRerouteNode.performed -= instance.OnPopLastRerouteNode;
				this.PopLastRerouteNode.canceled -= instance.OnPopLastRerouteNode;
				this.OpenUserReportModal.started -= instance.OnOpenUserReportModal;
				this.OpenUserReportModal.performed -= instance.OnOpenUserReportModal;
				this.OpenUserReportModal.canceled -= instance.OnOpenUserReportModal;
				this.ToggleDevLog.started -= instance.OnToggleDevLog;
				this.ToggleDevLog.performed -= instance.OnToggleDevLog;
				this.ToggleDevLog.canceled -= instance.OnToggleDevLog;
				this.OpenLogsModal.started -= instance.OnOpenLogsModal;
				this.OpenLogsModal.performed -= instance.OnOpenLogsModal;
				this.OpenLogsModal.canceled -= instance.OnOpenLogsModal;
				this.PrimaryPointerPosition.started -= instance.OnPrimaryPointerPosition;
				this.PrimaryPointerPosition.performed -= instance.OnPrimaryPointerPosition;
				this.PrimaryPointerPosition.canceled -= instance.OnPrimaryPointerPosition;
				this.ScriptAutoComplete.started -= instance.OnScriptAutoComplete;
				this.ScriptAutoComplete.performed -= instance.OnScriptAutoComplete;
				this.ScriptAutoComplete.canceled -= instance.OnScriptAutoComplete;
				this.Pinch.started -= instance.OnPinch;
				this.Pinch.performed -= instance.OnPinch;
				this.Pinch.canceled -= instance.OnPinch;
				this.LeftControl.started -= instance.OnLeftControl;
				this.LeftControl.performed -= instance.OnLeftControl;
				this.LeftControl.canceled -= instance.OnLeftControl;
				this.Inventory1.started -= instance.OnInventory1;
				this.Inventory1.performed -= instance.OnInventory1;
				this.Inventory1.canceled -= instance.OnInventory1;
				this.Inventory2.started -= instance.OnInventory2;
				this.Inventory2.performed -= instance.OnInventory2;
				this.Inventory2.canceled -= instance.OnInventory2;
				this.Inventory3.started -= instance.OnInventory3;
				this.Inventory3.performed -= instance.OnInventory3;
				this.Inventory3.canceled -= instance.OnInventory3;
				this.Inventory4.started -= instance.OnInventory4;
				this.Inventory4.performed -= instance.OnInventory4;
				this.Inventory4.canceled -= instance.OnInventory4;
				this.Inventory5.started -= instance.OnInventory5;
				this.Inventory5.performed -= instance.OnInventory5;
				this.Inventory5.canceled -= instance.OnInventory5;
				this.Inventory6.started -= instance.OnInventory6;
				this.Inventory6.performed -= instance.OnInventory6;
				this.Inventory6.canceled -= instance.OnInventory6;
				this.Inventory7.started -= instance.OnInventory7;
				this.Inventory7.performed -= instance.OnInventory7;
				this.Inventory7.canceled -= instance.OnInventory7;
				this.Inventory8.started -= instance.OnInventory8;
				this.Inventory8.performed -= instance.OnInventory8;
				this.Inventory8.canceled -= instance.OnInventory8;
				this.Inventory9.started -= instance.OnInventory9;
				this.Inventory9.performed -= instance.OnInventory9;
				this.Inventory9.canceled -= instance.OnInventory9;
				this.Inventory10.started -= instance.OnInventory10;
				this.Inventory10.performed -= instance.OnInventory10;
				this.Inventory10.canceled -= instance.OnInventory10;
				this.Reload.started -= instance.OnReload;
				this.Reload.performed -= instance.OnReload;
				this.Reload.canceled -= instance.OnReload;
				this.DisplayGameLibraryModerationWindow.started -= instance.OnDisplayGameLibraryModerationWindow;
				this.DisplayGameLibraryModerationWindow.performed -= instance.OnDisplayGameLibraryModerationWindow;
				this.DisplayGameLibraryModerationWindow.canceled -= instance.OnDisplayGameLibraryModerationWindow;
			}

			// Token: 0x06000076 RID: 118 RVA: 0x00004B29 File Offset: 0x00002D29
			public void RemoveCallbacks(PlayerInputActions.IPlayerActions instance)
			{
				if (this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
				{
					this.UnregisterCallbacks(instance);
				}
			}

			// Token: 0x06000077 RID: 119 RVA: 0x00004B48 File Offset: 0x00002D48
			public void SetCallbacks(PlayerInputActions.IPlayerActions instance)
			{
				foreach (PlayerInputActions.IPlayerActions playerActions in this.m_Wrapper.m_PlayerActionsCallbackInterfaces)
				{
					this.UnregisterCallbacks(playerActions);
				}
				this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
				this.AddCallbacks(instance);
			}

			// Token: 0x04000133 RID: 307
			private PlayerInputActions m_Wrapper;
		}

		// Token: 0x0200000E RID: 14
		public struct UIActions
		{
			// Token: 0x06000078 RID: 120 RVA: 0x00004BB8 File Offset: 0x00002DB8
			public UIActions(PlayerInputActions wrapper)
			{
				this.m_Wrapper = wrapper;
			}

			// Token: 0x17000046 RID: 70
			// (get) Token: 0x06000079 RID: 121 RVA: 0x00004BC1 File Offset: 0x00002DC1
			public InputAction Navigate
			{
				get
				{
					return this.m_Wrapper.m_UI_Navigate;
				}
			}

			// Token: 0x17000047 RID: 71
			// (get) Token: 0x0600007A RID: 122 RVA: 0x00004BCE File Offset: 0x00002DCE
			public InputAction Submit
			{
				get
				{
					return this.m_Wrapper.m_UI_Submit;
				}
			}

			// Token: 0x17000048 RID: 72
			// (get) Token: 0x0600007B RID: 123 RVA: 0x00004BDB File Offset: 0x00002DDB
			public InputAction Cancel
			{
				get
				{
					return this.m_Wrapper.m_UI_Cancel;
				}
			}

			// Token: 0x17000049 RID: 73
			// (get) Token: 0x0600007C RID: 124 RVA: 0x00004BE8 File Offset: 0x00002DE8
			public InputAction Point
			{
				get
				{
					return this.m_Wrapper.m_UI_Point;
				}
			}

			// Token: 0x1700004A RID: 74
			// (get) Token: 0x0600007D RID: 125 RVA: 0x00004BF5 File Offset: 0x00002DF5
			public InputAction Click
			{
				get
				{
					return this.m_Wrapper.m_UI_Click;
				}
			}

			// Token: 0x1700004B RID: 75
			// (get) Token: 0x0600007E RID: 126 RVA: 0x00004C02 File Offset: 0x00002E02
			public InputAction ScrollWheel
			{
				get
				{
					return this.m_Wrapper.m_UI_ScrollWheel;
				}
			}

			// Token: 0x1700004C RID: 76
			// (get) Token: 0x0600007F RID: 127 RVA: 0x00004C0F File Offset: 0x00002E0F
			public InputAction MiddleClick
			{
				get
				{
					return this.m_Wrapper.m_UI_MiddleClick;
				}
			}

			// Token: 0x1700004D RID: 77
			// (get) Token: 0x06000080 RID: 128 RVA: 0x00004C1C File Offset: 0x00002E1C
			public InputAction RightClick
			{
				get
				{
					return this.m_Wrapper.m_UI_RightClick;
				}
			}

			// Token: 0x1700004E RID: 78
			// (get) Token: 0x06000081 RID: 129 RVA: 0x00004C29 File Offset: 0x00002E29
			public InputAction TrackedDevicePosition
			{
				get
				{
					return this.m_Wrapper.m_UI_TrackedDevicePosition;
				}
			}

			// Token: 0x1700004F RID: 79
			// (get) Token: 0x06000082 RID: 130 RVA: 0x00004C36 File Offset: 0x00002E36
			public InputAction TrackedDeviceOrientation
			{
				get
				{
					return this.m_Wrapper.m_UI_TrackedDeviceOrientation;
				}
			}

			// Token: 0x17000050 RID: 80
			// (get) Token: 0x06000083 RID: 131 RVA: 0x00004C43 File Offset: 0x00002E43
			public InputAction SelectNext
			{
				get
				{
					return this.m_Wrapper.m_UI_SelectNext;
				}
			}

			// Token: 0x06000084 RID: 132 RVA: 0x00004C50 File Offset: 0x00002E50
			public InputActionMap Get()
			{
				return this.m_Wrapper.m_UI;
			}

			// Token: 0x06000085 RID: 133 RVA: 0x00004C5D File Offset: 0x00002E5D
			public void Enable()
			{
				this.Get().Enable();
			}

			// Token: 0x06000086 RID: 134 RVA: 0x00004C6A File Offset: 0x00002E6A
			public void Disable()
			{
				this.Get().Disable();
			}

			// Token: 0x17000051 RID: 81
			// (get) Token: 0x06000087 RID: 135 RVA: 0x00004C77 File Offset: 0x00002E77
			public bool enabled
			{
				get
				{
					return this.Get().enabled;
				}
			}

			// Token: 0x06000088 RID: 136 RVA: 0x00004C84 File Offset: 0x00002E84
			public static implicit operator InputActionMap(PlayerInputActions.UIActions set)
			{
				return set.Get();
			}

			// Token: 0x06000089 RID: 137 RVA: 0x00004C90 File Offset: 0x00002E90
			public void AddCallbacks(PlayerInputActions.IUIActions instance)
			{
				if (instance == null || this.m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
				{
					return;
				}
				this.m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				this.Navigate.started += instance.OnNavigate;
				this.Navigate.performed += instance.OnNavigate;
				this.Navigate.canceled += instance.OnNavigate;
				this.Submit.started += instance.OnSubmit;
				this.Submit.performed += instance.OnSubmit;
				this.Submit.canceled += instance.OnSubmit;
				this.Cancel.started += instance.OnCancel;
				this.Cancel.performed += instance.OnCancel;
				this.Cancel.canceled += instance.OnCancel;
				this.Point.started += instance.OnPoint;
				this.Point.performed += instance.OnPoint;
				this.Point.canceled += instance.OnPoint;
				this.Click.started += instance.OnClick;
				this.Click.performed += instance.OnClick;
				this.Click.canceled += instance.OnClick;
				this.ScrollWheel.started += instance.OnScrollWheel;
				this.ScrollWheel.performed += instance.OnScrollWheel;
				this.ScrollWheel.canceled += instance.OnScrollWheel;
				this.MiddleClick.started += instance.OnMiddleClick;
				this.MiddleClick.performed += instance.OnMiddleClick;
				this.MiddleClick.canceled += instance.OnMiddleClick;
				this.RightClick.started += instance.OnRightClick;
				this.RightClick.performed += instance.OnRightClick;
				this.RightClick.canceled += instance.OnRightClick;
				this.TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
				this.TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
				this.TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
				this.TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
				this.TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
				this.TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
				this.SelectNext.started += instance.OnSelectNext;
				this.SelectNext.performed += instance.OnSelectNext;
				this.SelectNext.canceled += instance.OnSelectNext;
			}

			// Token: 0x0600008A RID: 138 RVA: 0x00004FE0 File Offset: 0x000031E0
			private void UnregisterCallbacks(PlayerInputActions.IUIActions instance)
			{
				this.Navigate.started -= instance.OnNavigate;
				this.Navigate.performed -= instance.OnNavigate;
				this.Navigate.canceled -= instance.OnNavigate;
				this.Submit.started -= instance.OnSubmit;
				this.Submit.performed -= instance.OnSubmit;
				this.Submit.canceled -= instance.OnSubmit;
				this.Cancel.started -= instance.OnCancel;
				this.Cancel.performed -= instance.OnCancel;
				this.Cancel.canceled -= instance.OnCancel;
				this.Point.started -= instance.OnPoint;
				this.Point.performed -= instance.OnPoint;
				this.Point.canceled -= instance.OnPoint;
				this.Click.started -= instance.OnClick;
				this.Click.performed -= instance.OnClick;
				this.Click.canceled -= instance.OnClick;
				this.ScrollWheel.started -= instance.OnScrollWheel;
				this.ScrollWheel.performed -= instance.OnScrollWheel;
				this.ScrollWheel.canceled -= instance.OnScrollWheel;
				this.MiddleClick.started -= instance.OnMiddleClick;
				this.MiddleClick.performed -= instance.OnMiddleClick;
				this.MiddleClick.canceled -= instance.OnMiddleClick;
				this.RightClick.started -= instance.OnRightClick;
				this.RightClick.performed -= instance.OnRightClick;
				this.RightClick.canceled -= instance.OnRightClick;
				this.TrackedDevicePosition.started -= instance.OnTrackedDevicePosition;
				this.TrackedDevicePosition.performed -= instance.OnTrackedDevicePosition;
				this.TrackedDevicePosition.canceled -= instance.OnTrackedDevicePosition;
				this.TrackedDeviceOrientation.started -= instance.OnTrackedDeviceOrientation;
				this.TrackedDeviceOrientation.performed -= instance.OnTrackedDeviceOrientation;
				this.TrackedDeviceOrientation.canceled -= instance.OnTrackedDeviceOrientation;
				this.SelectNext.started -= instance.OnSelectNext;
				this.SelectNext.performed -= instance.OnSelectNext;
				this.SelectNext.canceled -= instance.OnSelectNext;
			}

			// Token: 0x0600008B RID: 139 RVA: 0x00005305 File Offset: 0x00003505
			public void RemoveCallbacks(PlayerInputActions.IUIActions instance)
			{
				if (this.m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
				{
					this.UnregisterCallbacks(instance);
				}
			}

			// Token: 0x0600008C RID: 140 RVA: 0x00005324 File Offset: 0x00003524
			public void SetCallbacks(PlayerInputActions.IUIActions instance)
			{
				foreach (PlayerInputActions.IUIActions iuiactions in this.m_Wrapper.m_UIActionsCallbackInterfaces)
				{
					this.UnregisterCallbacks(iuiactions);
				}
				this.m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
				this.AddCallbacks(instance);
			}

			// Token: 0x04000134 RID: 308
			private PlayerInputActions m_Wrapper;
		}

		// Token: 0x0200000F RID: 15
		public interface IPlayerActions
		{
			// Token: 0x0600008D RID: 141
			void OnMove(InputAction.CallbackContext context);

			// Token: 0x0600008E RID: 142
			void OnLook(InputAction.CallbackContext context);

			// Token: 0x0600008F RID: 143
			void OnZoom(InputAction.CallbackContext context);

			// Token: 0x06000090 RID: 144
			void OnCameraPanWindows(InputAction.CallbackContext context);

			// Token: 0x06000091 RID: 145
			void OnCameraPanMac(InputAction.CallbackContext context);

			// Token: 0x06000092 RID: 146
			void OnMajorEquipmentPrimaryAction(InputAction.CallbackContext context);

			// Token: 0x06000093 RID: 147
			void OnMajorEquipmentAlternateAction(InputAction.CallbackContext context);

			// Token: 0x06000094 RID: 148
			void OnMinorEquipment(InputAction.CallbackContext context);

			// Token: 0x06000095 RID: 149
			void OnJump(InputAction.CallbackContext context);

			// Token: 0x06000096 RID: 150
			void OnEnableCursor(InputAction.CallbackContext context);

			// Token: 0x06000097 RID: 151
			void OnDown(InputAction.CallbackContext context);

			// Token: 0x06000098 RID: 152
			void OnMainToolAction(InputAction.CallbackContext context);

			// Token: 0x06000099 RID: 153
			void OnAlternativeToolAction(InputAction.CallbackContext context);

			// Token: 0x0600009A RID: 154
			void OnGhost(InputAction.CallbackContext context);

			// Token: 0x0600009B RID: 155
			void OnCycleVisuals(InputAction.CallbackContext context);

			// Token: 0x0600009C RID: 156
			void OnFlipGameState(InputAction.CallbackContext context);

			// Token: 0x0600009D RID: 157
			void OnEmptyTool(InputAction.CallbackContext context);

			// Token: 0x0600009E RID: 158
			void OnPaintingTool(InputAction.CallbackContext context);

			// Token: 0x0600009F RID: 159
			void OnPropBasedTool(InputAction.CallbackContext context);

			// Token: 0x060000A0 RID: 160
			void OnEraseTool(InputAction.CallbackContext context);

			// Token: 0x060000A1 RID: 161
			void OnWiringTool(InputAction.CallbackContext context);

			// Token: 0x060000A2 RID: 162
			void OnInspectorTool(InputAction.CallbackContext context);

			// Token: 0x060000A3 RID: 163
			void OnCopyTool(InputAction.CallbackContext context);

			// Token: 0x060000A4 RID: 164
			void OnMoveTool(InputAction.CallbackContext context);

			// Token: 0x060000A5 RID: 165
			void OnInteract(InputAction.CallbackContext context);

			// Token: 0x060000A6 RID: 166
			void OnPopLastRerouteNode(InputAction.CallbackContext context);

			// Token: 0x060000A7 RID: 167
			void OnOpenUserReportModal(InputAction.CallbackContext context);

			// Token: 0x060000A8 RID: 168
			void OnToggleDevLog(InputAction.CallbackContext context);

			// Token: 0x060000A9 RID: 169
			void OnOpenLogsModal(InputAction.CallbackContext context);

			// Token: 0x060000AA RID: 170
			void OnPrimaryPointerPosition(InputAction.CallbackContext context);

			// Token: 0x060000AB RID: 171
			void OnScriptAutoComplete(InputAction.CallbackContext context);

			// Token: 0x060000AC RID: 172
			void OnPinch(InputAction.CallbackContext context);

			// Token: 0x060000AD RID: 173
			void OnLeftControl(InputAction.CallbackContext context);

			// Token: 0x060000AE RID: 174
			void OnInventory1(InputAction.CallbackContext context);

			// Token: 0x060000AF RID: 175
			void OnInventory2(InputAction.CallbackContext context);

			// Token: 0x060000B0 RID: 176
			void OnInventory3(InputAction.CallbackContext context);

			// Token: 0x060000B1 RID: 177
			void OnInventory4(InputAction.CallbackContext context);

			// Token: 0x060000B2 RID: 178
			void OnInventory5(InputAction.CallbackContext context);

			// Token: 0x060000B3 RID: 179
			void OnInventory6(InputAction.CallbackContext context);

			// Token: 0x060000B4 RID: 180
			void OnInventory7(InputAction.CallbackContext context);

			// Token: 0x060000B5 RID: 181
			void OnInventory8(InputAction.CallbackContext context);

			// Token: 0x060000B6 RID: 182
			void OnInventory9(InputAction.CallbackContext context);

			// Token: 0x060000B7 RID: 183
			void OnInventory10(InputAction.CallbackContext context);

			// Token: 0x060000B8 RID: 184
			void OnReload(InputAction.CallbackContext context);

			// Token: 0x060000B9 RID: 185
			void OnDisplayGameLibraryModerationWindow(InputAction.CallbackContext context);
		}

		// Token: 0x02000010 RID: 16
		public interface IUIActions
		{
			// Token: 0x060000BA RID: 186
			void OnNavigate(InputAction.CallbackContext context);

			// Token: 0x060000BB RID: 187
			void OnSubmit(InputAction.CallbackContext context);

			// Token: 0x060000BC RID: 188
			void OnCancel(InputAction.CallbackContext context);

			// Token: 0x060000BD RID: 189
			void OnPoint(InputAction.CallbackContext context);

			// Token: 0x060000BE RID: 190
			void OnClick(InputAction.CallbackContext context);

			// Token: 0x060000BF RID: 191
			void OnScrollWheel(InputAction.CallbackContext context);

			// Token: 0x060000C0 RID: 192
			void OnMiddleClick(InputAction.CallbackContext context);

			// Token: 0x060000C1 RID: 193
			void OnRightClick(InputAction.CallbackContext context);

			// Token: 0x060000C2 RID: 194
			void OnTrackedDevicePosition(InputAction.CallbackContext context);

			// Token: 0x060000C3 RID: 195
			void OnTrackedDeviceOrientation(InputAction.CallbackContext context);

			// Token: 0x060000C4 RID: 196
			void OnSelectNext(InputAction.CallbackContext context);
		}
	}
}
