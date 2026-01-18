# ENDSTAR PROP TOOL UI DEEP ANALYSIS
Generated: 01/06/2026 14:37:31

## LOADED ASSEMBLIES

| Assembly | Status |
|----------|--------|
| Creator.dll | Loaded |
| Gameplay.dll | Loaded |
| Props.dll | Loaded |
| Assets.dll | Loaded |

## SECTION 1: CREATOR.DLL ALL TYPES

Total types loaded from Creator.dll: 793

### Namespaces:
```
: 60 types
__GEN: 1 types
Endless.Creator: 131 types
Endless.Creator.DynamicPropCreation: 13 types
Endless.Creator.LevelEditing: 4 types
Endless.Creator.LevelEditing.Runtime: 103 types
Endless.Creator.Notifications: 7 types
Endless.Creator.Test.LuaParsing: 14 types
Endless.Creator.Test.LuaParsing.SyntaxTree.Expressions: 12 types
Endless.Creator.Test.LuaParsing.SyntaxTree.Statements: 12 types
Endless.Creator.Test.LuaParsing.SyntaxTree.Visitors: 2 types
Endless.Creator.UI: 425 types
Runtime.Creator.UI: 9 types
```

### Prop-related types (61):
```
Endless.Creator.GameEditor+<AddPropsToGameLibrary>d__28
Endless.Creator.GameEditor+<AddPropsToGameLibrary>d__31
Endless.Creator.GameEditor+<AddPropToGameLibrary>d__27
Endless.Creator.GameEditor+<AddPropToGameLibrary>d__29
Endless.Creator.GameEditor+<AddPropToGameLibrary>d__30
Endless.Creator.LevelEditing.Runtime.CopyTool+<AttemptCopyProp_Execute>d__39
Endless.Creator.LevelEditing.Runtime.MoveTool+<AttemptMoveProp>d__19
Endless.Creator.LevelEditing.Runtime.PropTool+<AttemptPlaceProp>d__13
Endless.Creator.UI.UIScriptWindowModel+<GetPropVersions>d__52
Endless.Creator.ProceduralTemplateSource+<GetRequiredPropAssets>d__18
Endless.Creator.StaticLevelStateTemplateSource+<GetRequiredPropAssets>d__20
Endless.Creator.LevelEditing.Runtime.PropTool+<LoadPropPrefab>d__17
Endless.Creator.LevelEditing.Runtime.PropTool+<PlaceProp>d__15
Endless.Creator.GameEditor+<RemovePropFromGameLibrary>d__34
Endless.Creator.UI.UISaveScriptAndPropAndApplyToGameHandler+<SaveProp>d__10
Endless.Creator.UI.UISaveScriptAndPropAndApplyToGameHandler+<SaveScriptAndPropAndApplyToGame>d__8
Endless.Creator.UI.UIScriptWindowController+<SaveScriptAndPropAndApplyToGameAsync>d__34
Endless.Creator.GameEditor+<SetPropVersionInGameLibrary>d__25
Endless.Creator.DynamicPropCreation.PropCreationScreenData+<UploadProp>d__10
Endless.Creator.DynamicPropCreation.AbstractPropCreationScreenData+<UploadProp>d__3
Endless.Creator.DynamicPropCreation.GenericPropCreationScreenData+<UploadProp>d__3
Endless.Creator.DynamicPropCreation.AbstractPropCreationScreenData
Endless.Creator.DynamicPropCreation.AbstractPropIconUtility
Endless.Creator.DynamicPropCreation.GenericPropCreationScreenData
Endless.Creator.UI.IUIBasePropertyViewable
Endless.Creator.LevelEditing.Runtime.PropBasedTool
Endless.Creator.DynamicPropCreation.PropCreationData
Endless.Creator.DynamicPropCreation.PropCreationMenuData
Endless.Creator.DynamicPropCreation.PropCreationPromptData
Endless.Creator.DynamicPropCreation.PropCreationScreenData
Endless.Creator.LevelEditing.Runtime.PropEraseData
Endless.Creator.LevelEditing.Runtime.PropEraseDataNetworkExtensions
Endless.Creator.LevelEditing.Runtime.PropLocationMarker
Endless.Creator.LevelEditing.Runtime.PropLocationMarkRecord
Endless.Creator.LevelEditing.Runtime.PropLocationType
Endless.Creator.LevelEditing.Runtime.PropTool
Endless.Creator.UI.UIAbstractPropCreationModalController
Endless.Creator.UI.UIAddPropGameAssetToGameLibraryModalController
Endless.Creator.UI.UIBasePropCreationModalController
Endless.Creator.UI.UIGenericPropCreationModalController
Endless.Creator.UI.UIInspectorPropertyModel
Endless.Creator.UI.UIPropCreationDataListCellController
Endless.Creator.UI.UIPropCreationDataListModel
Endless.Creator.UI.UIPropCreationDataListView
Endless.Creator.UIPropCreationDataView
Endless.Creator.UI.UIPropCreationPromptDataModalController
Endless.Creator.UI.UIPropEntryListCellController
Endless.Creator.UI.UIPropEntryListModel
Endless.Creator.UI.UIPropEntryListView
Endless.Creator.UI.UIPropToolPanelController
Endless.Creator.UI.UIPropToolPanelView
Endless.Creator.UI.UIRuntimePropDetailController
Endless.Creator.UI.UIRuntimePropInfoDetailView
Endless.Creator.UI.UIRuntimePropInfoListCellController
Endless.Creator.UI.UIRuntimePropInfoListController
Endless.Creator.UI.UIRuntimePropInfoListModel
Endless.Creator.UI.UIRuntimePropInfoListView
Endless.Creator.UI.UIRuntimePropInfoSelectionWindowController
Endless.Creator.UI.UISaveScriptAndPropAndApplyToGameHandler
Endless.Creator.UI.UIWirePropertyModifierView
Endless.Creator.UI.UIWireShaderPropertiesDictionary
```

### Tool-related types (42):
```
Endless.Creator.LevelEditing.Runtime.CopyTool
Endless.Creator.LevelEditing.Runtime.EmptyTool
Endless.Creator.LevelEditing.Runtime.EndlessTool
Endless.Creator.LevelEditing.Runtime.EraseTool
Endless.Creator.LevelEditing.Runtime.EraseToolFunction
Endless.Creator.LevelEditing.Runtime.GameEditorTool
Endless.Creator.UI.IDockableToolPanelView
Endless.Creator.LevelEditing.Runtime.InspectorTool
Endless.Creator.LevelEditing.Runtime.LevelEditorTool
Endless.Creator.LevelEditing.Runtime.MoveTool
Endless.Creator.LevelEditing.Runtime.PaintingTool
Endless.Creator.LevelEditing.Runtime.PropBasedTool
Endless.Creator.LevelEditing.Runtime.PropTool
Endless.Creator.LevelEditing.Runtime.ScreenshotTool
Endless.Creator.LevelEditing.Runtime.ToolManager
Endless.Creator.LevelEditing.Runtime.ToolState
Endless.Creator.LevelEditing.Runtime.ToolType
Endless.Creator.UI.UIBaseToolPanelController`1
Endless.Creator.UI.UIBaseToolPanelView`1
Endless.Creator.UI.UICopyToolPanelController
Endless.Creator.UI.UICopyToolPanelView
Endless.Creator.UI.UIDockableToolPanelController
Endless.Creator.UI.UIDockableToolPanelView`1
Endless.Creator.UI.UIEraseToolPanelController
Endless.Creator.UI.UIEraseToolPanelView
Endless.Creator.UI.UIInspectorToolPanelController
Endless.Creator.UI.UIInspectorToolPanelView
Endless.Creator.UI.UIItemSelectionToolPanelController`2
Endless.Creator.UI.UIItemSelectionToolPanelView`2
Endless.Creator.UI.UIPaintingToolPanelController
Endless.Creator.UI.UIPaintingToolPanelView
Endless.Creator.UI.UIPropToolPanelController
Endless.Creator.UI.UIPropToolPanelView
Endless.Creator.UI.UIScreenshotToolController
Endless.Creator.UI.UIScreenshotToolView
Endless.Creator.UI.UIScreenshotToolVisibilityHandler
Endless.Creator.UI.UIToolController
Endless.Creator.UI.UIToolPrompterManager
Endless.Creator.UI.UIToolTypeColorDictionary
Endless.Creator.UI.UIToolView
Endless.Creator.LevelEditing.Runtime.WiringTool
Endless.Creator.LevelEditing.Runtime.WiringTool+WiringToolState
```

### UI types (275):
```
Endless.Creator.UI.UIAbstractPropCreationModalController
Endless.Creator.UI.UIActiveLevelRoleVisibilityHandler
Endless.Creator.UI.UIAddPropGameAssetToGameLibraryModalController
Endless.Creator.UI.UIAddScreenshotsToGameModalController
Endless.Creator.UI.UIAddScreenshotsToGameModalModel
Endless.Creator.UI.UIAddScreenshotsToLevelModalController
Endless.Creator.UI.UIAddScreenshotsToLevelModalModel
Endless.Creator.UI.UIAdminWindowDisplayHandler
Runtime.Creator.UI.UIAssetList`2
Endless.Creator.UI.UIAssetModelHandler`1
Endless.Creator.UI.UIAssetModerationModalController
Endless.Creator.UI.UIAssetReadAndWriteController
Endless.Creator.UI.UIAssetReadAndWriteView`1
Endless.Creator.UI.UIAssetReadOnlyView`1
Endless.Creator.UI.UIAssetView`1
Endless.Creator.UI.UIAssetWithRolesModelHandler`1
Endless.Creator.UI.UIAssetWithScreenshotsController
Endless.Creator.UI.UIAssetWithScreenshotsView`1
Endless.Creator.UI.UIAudioCategoryAudioMixerGroupDictionary
Endless.Creator.UI.UIBaseGameAssetCloudPaginatedListModel
Endless.Creator.UI.UIBaseGameAssetView
Endless.Creator.UI.UIBasePropCreationModalController
Endless.Creator.UI.UIBaseToolPanelController`1
Endless.Creator.UI.UIBaseToolPanelView`1
Endless.Creator.UI.UIBaseWireController
Endless.Creator.UI.UIClientCopyHistoryEntryListCellController
Endless.Creator.UI.UIClientCopyHistoryEntryListModel
Endless.Creator.UI.UIClientCopyHistoryEntryListView
Endless.Creator.UI.UICloudGameAssetListController
Endless.Creator.UI.UIConsoleMessageListController
Endless.Creator.UI.UIConsoleMessageListModel
Endless.Creator.UI.UIConsoleMessageListView
Endless.Creator.UI.UIConsoleMessageModel
Endless.Creator.UI.UICopyToolPanelController
Endless.Creator.UI.UICopyToolPanelView
Endless.Creator.UI.UICreatorPlayerListModel
Endless.Creator.UI.UICreatorPlayerListView
Endless.Creator.UI.UICreatorReferenceManager
Endless.Creator.UI.UICreatorVisibilityHandler
Runtime.Creator.UI.UICuratedGamesList
Endless.Creator.UI.UIDisplayGameEditorWindowHandler
Endless.Creator.UI.UIDisplayLevelEditorWindowHandler
Endless.Creator.UI.UIDockableToolPanelController
Endless.Creator.UI.UIDockableToolPanelView`1
Endless.Creator.UI.UIDropdownVersion
Endless.Creator.UI.UIDynamicTypeFactory
Endless.Creator.UI.UIEndlessEventInfoListCellController
Endless.Creator.UI.UIEndlessEventInfoListModel
Endless.Creator.UI.UIEndlessEventInfoListView
Endless.Creator.LevelEditing.Runtime.UIEndlessEventList
Endless.Creator.UI.UIEndlessParameterInfoListCellController
Endless.Creator.UI.UIEndlessParameterInfoListModel
Endless.Creator.UI.UIEndlessParameterInfoListView
Endless.Creator.UI.UIEnumEntryListModel
Endless.Creator.UI.UIEnumEntryListView
Endless.Creator.UI.UIEraseToolPanelController
Endless.Creator.UI.UIEraseToolPanelView
Endless.Creator.UI.UIFileInstanceTexture2DView
Endless.Creator.UI.UIGameAsset
Endless.Creator.UI.UIGameAssetCreatorTypes
Endless.Creator.UI.UIGameAssetCreatorTypesColorDictionary
Endless.Creator.UI.UIGameAssetDeserializer
Endless.Creator.UI.UIGameAssetDetailController
Endless.Creator.UI.UIGameAssetDetailView
Endless.Creator.UI.UIGameAssetListCellController
Endless.Creator.UI.UIGameAssetListCellSelectableController
Endless.Creator.UI.UIGameAssetListCellViewDragInstanceHandler
Endless.Creator.UI.UIGameAssetListModel
Endless.Creator.UI.UIGameAssetListView
Endless.Creator.UI.UIGameAssetPublishModalController
Endless.Creator.UI.UIGameAssetPublishModalModel
Endless.Creator.UI.UIGameAssetSummaryView
Endless.Creator.UI.UIGameAssetTypeFilterDictionary
Endless.Creator.UI.UIGameAssetTypes
Endless.Creator.UI.UIGameAssetTypesExtensions
Endless.Creator.UI.UIGameAssetTypeStyle
Endless.Creator.UI.UIGameAssetTypeStyleDictionary
Endless.Creator.UI.UIGameAssetVersionNotificationCountView
Endless.Creator.UI.UIGameAssetVersionNotificationView
Endless.Creator.UI.UIGameController
Endless.Creator.UI.UIGameEditorWindowController
Endless.Creator.UI.UIGameLibraryAssetAdditionModalController
Endless.Creator.UI.UIGameLibraryAssetDetailModalController
Endless.Creator.UI.UIGameLibraryAssetReplacementConfirmationModalController
Endless.Creator.UI.UIGameLibraryAssetReplacementModalController
Endless.Creator.UI.UIGameLibraryFilterListCellController
Endless.Creator.UI.UIGameLibraryFilterListModel
Endless.Creator.UI.UIGameLibraryFilterListView
Endless.Creator.UI.UIGameLibraryListController
Endless.Creator.UI.UIGameLibraryListModel
Endless.Creator.UI.UIGameLibraryRemoveDropHandler
Endless.Creator.UI.UIGameLibrarySelectionWindowController
Endless.Creator.UI.UIGameModelHandler
Endless.Creator.UI.UIGameView
Endless.Creator.UI.UIGenericPropCreationModalController
Endless.Creator.UI.UIGhostModeController
Endless.Creator.UI.UIGhostModeView
Endless.Creator.UI.UIInMemoryScreenshotListCellController
Endless.Creator.UI.UIInMemoryScreenshotListModel
Endless.Creator.UI.UIInMemoryScreenshotListView
Endless.Creator.UI.UIInspectorGroupName
Endless.Creator.UI.UIInspectorPropertyModel
Endless.Creator.UI.UIInspectorScriptValueInputModalController
Endless.Creator.UI.UIInspectorScriptValueListCellController
Endless.Creator.UI.UIInspectorScriptValueListModel
Endless.Creator.UI.UIInspectorScriptValueListView
Endless.Creator.UI.UIInspectorScriptValueTypeRadio
Endless.Creator.UI.UIInspectorScriptValueTypeSelectionModalController
Endless.Creator.UI.UIInspectorToolPanelController
Endless.Creator.UI.UIInspectorToolPanelView
Endless.Creator.UI.UIInventorySpawnOptionsListCellController
Endless.Creator.UI.UIInventorySpawnOptionsListModel
Endless.Creator.UI.UIInventorySpawnOptionsListView
Endless.Creator.UI.UIItemSelectionToolPanelController`2
Endless.Creator.UI.UIItemSelectionToolPanelView`2
Endless.Creator.UI.UIJumpAndDownSlider
Endless.Creator.UI.UILevelAssetAndScreenshotsListModel
Endless.Creator.UI.UILevelAssetAndScreenshotsListModelEntry
Endless.Creator.UI.UILevelAssetAndScreenshotsListView
Endless.Creator.UI.UILevelAssetListCellController
Endless.Creator.UI.UILevelAssetListCellViewDragInstanceHandler
Endless.Creator.UI.UILevelAssetListController
Endless.Creator.UI.UILevelAssetListModel
Endless.Creator.UI.UILevelAssetListView
Endless.Creator.UI.UILevelAssetView
Endless.Creator.UI.UILevelController
Endless.Creator.UI.UILevelDestinationSelectionListCellController
Endless.Creator.UI.UILevelDestinationSelectionListModel
Endless.Creator.UI.UILevelDestinationSelectionListView
Endless.Creator.UI.UILevelEditorWindowController
Endless.Creator.UI.UILevelLoaderModalController
Endless.Creator.UI.UILevelModelHandler
Endless.Creator.UI.UILevelScreenshotsView
Endless.Creator.UI.UILevelStateSelectionModalController
Endless.Creator.UI.UILevelStateTemplateListCellController
Endless.Creator.UI.UILevelStateTemplateListModel
Endless.Creator.UI.UILevelStateTemplateListView
Endless.Creator.UI.UILevelUtility
Endless.Creator.UI.UILevelView
Runtime.Creator.UI.UILibraryContentPack
Endless.Creator.UI.UIMarkAllGameAssetVersionNotificationsAsSeenButton
Endless.Creator.UI.UIModerationFlagDropdown
Endless.Creator.UI.UINewLevelStateModalController
Endless.Creator.UI.UINoneOrContextRadio
Endless.Creator.UI.UIOwnedGameAssetCloudPaginatedListModel
Endless.Creator.UI.UIPaintingToolPanelController
Endless.Creator.UI.UIPaintingToolPanelView
Endless.Creator.UI.UIPlayBar
Endless.Creator.UI.UIPlayerReferenceListCellController
Endless.Creator.UI.UIPlayerReferenceListModel
Endless.Creator.UI.UIPlayerReferenceListView
Endless.Creator.UI.UIPropCreationDataListCellController
Endless.Creator.UI.UIPropCreationDataListModel
Endless.Creator.UI.UIPropCreationDataListView
Endless.Creator.UIPropCreationDataView
Endless.Creator.UI.UIPropCreationPromptDataModalController
Endless.Creator.UI.UIPropEntryListCellController
Endless.Creator.UI.UIPropEntryListModel
Endless.Creator.UI.UIPropEntryListView
Endless.Creator.UI.UIPropToolPanelController
Endless.Creator.UI.UIPropToolPanelView
Endless.Creator.UI.UIPublishController
Endless.Creator.UI.UIPublishedGameAssetCloudPaginatedListModel
Endless.Creator.UI.UIPublishModel
Endless.Creator.UI.UIPublishStates
Endless.Creator.UI.UIPublishStatesExtensions
Endless.Creator.UI.UIPublishView
Endless.Creator.UI.UIRevisionListModel
Endless.Creator.UI.UIRevisionListView
Endless.Creator.UI.UIRevisionsView
Endless.Creator.UI.UIRoleInteractabilityHandler
Endless.Creator.UI.UIRolesDescription
Endless.Creator.UI.UIRolesDescriptionsDictionary
Endless.Creator.UI.UIRolesListCellController
Endless.Creator.UI.UIRolesListModel
Endless.Creator.UI.UIRolesListView
Endless.Creator.UI.UIRoleSubscriptionHandler
Endless.Creator.UI.UIRoleVisibilityHandler
Endless.Creator.UI.UIRuntimePropDetailController
Endless.Creator.UI.UIRuntimePropInfoDetailView
Endless.Creator.UI.UIRuntimePropInfoListCellController
Endless.Creator.UI.UIRuntimePropInfoListController
Endless.Creator.UI.UIRuntimePropInfoListModel
Endless.Creator.UI.UIRuntimePropInfoListView
Endless.Creator.UI.UIRuntimePropInfoSelectionWindowController
Endless.Creator.UI.UISaveScriptAndPropAndApplyToGameHandler
Endless.Creator.UI.UISaveStatusManager
Endless.Creator.UI.UIScreenshotCanvasVisibilityHandler
Endless.Creator.UI.UIScreenshotController
Endless.Creator.UI.UIScreenshotFileInstancesGameAdditionListCellController
Endless.Creator.UI.UIScreenshotFileInstancesListCellController
Endless.Creator.UI.UIScreenshotFileInstancesListCellViewDragInstanceHandler
Endless.Creator.UI.UIScreenshotFileInstancesListController
Endless.Creator.UI.UIScreenshotFileInstancesListModel
Endless.Creator.UI.UIScreenshotFileInstancesListView
Endless.Creator.UI.UIScreenshotToolController
Endless.Creator.UI.UIScreenshotToolView
Endless.Creator.UI.UIScreenshotToolVisibilityHandler
Endless.Creator.UI.UIScreenshotView
Endless.Creator.UI.UIScriptDataTypeRadio
Endless.Creator.UI.UIScriptDataTypeSelectionModalController
Endless.Creator.UI.UIScriptDataWizard
Endless.Creator.UI.UIScriptEventInputModalController
Endless.Creator.UI.UIScriptInputField
Endless.Creator.UI.UIScriptReferenceInputModalController
Endless.Creator.UI.UIScriptReferenceListCellController
Endless.Creator.UI.UIScriptReferenceListModel
Endless.Creator.UI.UIScriptReferenceListView
Endless.Creator.UI.UIScriptWindowController
Endless.Creator.UI.UIScriptWindowModel
Endless.Creator.UI.UISpawnPoint
Endless.Creator.UI.UISpawnPointListCellController
Endless.Creator.UI.UISpawnPointListModel
Endless.Creator.UI.UISpawnPointListView
Endless.Creator.UI.UISpawnPointSelectionModalController
Endless.Creator.UI.UISpawnPointUtility
Endless.Creator.UI.UIStoredParameterPackage
Endless.Creator.UI.UITilesetDetailController
Endless.Creator.UI.UITilesetDetailView
Endless.Creator.UI.UITilesetListCellController
Endless.Creator.UI.UITilesetListController
Endless.Creator.UI.UITilesetListModel
Endless.Creator.UI.UITilesetListView
Endless.Creator.UI.UITokenGroupTypeColorDictionary
Endless.Creator.UI.UITokenGroupTypeSpriteDictionary
Endless.Creator.UI.UIToolController
Endless.Creator.UI.UIToolPrompterManager
Endless.Creator.UI.UIToolTypeColorDictionary
Endless.Creator.UI.UIToolView
Endless.Creator.UI.UITransformIdentifier
Endless.Creator.UI.UITransformIdentifierListCellController
Endless.Creator.UI.UITransformIdentifierListModel
Endless.Creator.UI.UITransformIdentifierListView
Endless.Creator.UI.UITransformIdentifierSelectionWindowController
Endless.Creator.UI.UIUserReportedGameAssetCloudPaginatedListModel
Endless.Creator.UI.UIUserRoleListCellController
Endless.Creator.UI.UIUserRoleListModel
Endless.Creator.UI.UIUserRoleListView
Endless.Creator.UI.UIUserRolesController
Endless.Creator.UI.UIUserRolesModalController
Endless.Creator.UI.UIUserRolesModalModel
Endless.Creator.UI.UIUserRolesModel
Endless.Creator.UI.UIUserRolesView
Endless.Creator.UI.UIUserRoleWizard
Endless.Creator.UI.UIUserScriptAutocompleteListCellController
Endless.Creator.UI.UIUserScriptAutocompleteListModel
Endless.Creator.UI.UIUserScriptAutocompleteListModelItem
Endless.Creator.UI.UIUserScriptAutocompleteListView
Endless.Creator.UI.UIVersion
Endless.Creator.UI.UIVersionListCellController
Endless.Creator.UI.UIVersionListModel
Endless.Creator.UI.UIVersionListView
Endless.Creator.UI.UIWireColorDropdown
Endless.Creator.UI.UIWireColorDropdownValue
Endless.Creator.UI.UIWireConfirmationModalController
Endless.Creator.UI.UIWireConfirmationModalView
Endless.Creator.UI.UIWireController
Endless.Creator.UI.UIWireCreatorController
Endless.Creator.UI.UIWireEditorController
Endless.Creator.UI.UIWireEditorModalController
Endless.Creator.UI.UIWireEditorModalView
Endless.Creator.UI.UIWireNodeController
Endless.Creator.UI.UIWirePropertyModifierView
Endless.Creator.UI.UIWireShaderPropertiesDictionary
Endless.Creator.UI.UIWiresView
Endless.Creator.UI.UIWireUtility
Endless.Creator.UI.UIWiringInspectorPositioner
Endless.Creator.UI.UIWiringManager
Endless.Creator.UI.UIWiringObjectInspectorController
Endless.Creator.UI.UIWiringObjectInspectorView
Endless.Creator.UI.UIWiringPrompterManager
Endless.Creator.UI.UIWiringRerouteController
Endless.Creator.UI.UIWiringRerouteView
Endless.Creator.UI.UIWiringStates
Endless.Creator.UI.UIWriteableAssetModelHandler`1
```

### Panel/View types (89):
```
Endless.Creator.UI.UIAssetModerationModalView+<RequestAndViewExistingAssetModerationFlagsAsync>d__24
Endless.Creator.UI.UIInspectorToolPanelView+<View>d__33
Endless.Creator.UI.UIRuntimePropInfoDetailView+<View>d__35
Endless.Creator.UI.UIUserRoleListCellView+<ViewAsync>d__17
Endless.Creator.UI.UIBaseGameAssetView+<ViewAsync>d__32
Endless.Creator.UI.UIGameAssetDetailController+<ViewVersionAsync>d__35
Endless.Creator.UI.UIGameAssetDetailController+<WaitForAudioClipAndViewPlayButton>d__37
Endless.Creator.UI.IDockableToolPanelView
Endless.Creator.UI.IInspectorReferenceViewable
Endless.Creator.UI.IInstanceReferenceViewable
Endless.Creator.UI.IUIBasePropertyViewable
Endless.Creator.UI.UIAssetReadAndWriteView`1
Endless.Creator.UI.UIAssetReadOnlyView`1
Endless.Creator.UI.UIAssetView`1
Endless.Creator.UI.UIAssetWithScreenshotsView`1
Endless.Creator.UI.UIBaseGameAssetView
Endless.Creator.UI.UIBaseToolPanelController`1
Endless.Creator.UI.UIBaseToolPanelView`1
Endless.Creator.UI.UIClientCopyHistoryEntryListView
Endless.Creator.UI.UIConsoleMessageListView
Endless.Creator.UI.UICopyToolPanelController
Endless.Creator.UI.UICopyToolPanelView
Endless.Creator.UI.UICreatorPlayerListView
Endless.Creator.UI.UIDockableToolPanelController
Endless.Creator.UI.UIDockableToolPanelView`1
Endless.Creator.UI.UIEndlessEventInfoListView
Endless.Creator.UI.UIEndlessParameterInfoListView
Endless.Creator.UI.UIEnumEntryListView
Endless.Creator.UI.UIEraseToolPanelController
Endless.Creator.UI.UIEraseToolPanelView
Endless.Creator.UI.UIFileInstanceTexture2DView
Endless.Creator.UI.UIGameAssetDetailView
Endless.Creator.UI.UIGameAssetListCellViewDragInstanceHandler
Endless.Creator.UI.UIGameAssetListView
Endless.Creator.UI.UIGameAssetSummaryView
Endless.Creator.UI.UIGameAssetVersionNotificationCountView
Endless.Creator.UI.UIGameAssetVersionNotificationView
Endless.Creator.UI.UIGameLibraryFilterListView
Endless.Creator.UI.UIGameView
Endless.Creator.UI.UIGhostModeView
Endless.Creator.UI.UIInMemoryScreenshotListView
Endless.Creator.UI.UIInspectorScriptValueListView
Endless.Creator.UI.UIInspectorToolPanelController
Endless.Creator.UI.UIInspectorToolPanelView
Endless.Creator.UI.UIInventorySpawnOptionsListView
Endless.Creator.UI.UIItemSelectionToolPanelController`2
Endless.Creator.UI.UIItemSelectionToolPanelView`2
Endless.Creator.UI.UILevelAssetAndScreenshotsListView
Endless.Creator.UI.UILevelAssetListCellViewDragInstanceHandler
Endless.Creator.UI.UILevelAssetListView
Endless.Creator.UI.UILevelAssetView
Endless.Creator.UI.UILevelDestinationSelectionListView
Endless.Creator.UI.UILevelScreenshotsView
Endless.Creator.UI.UILevelStateTemplateListView
Endless.Creator.UI.UILevelView
Endless.Creator.UI.UIPaintingToolPanelController
Endless.Creator.UI.UIPaintingToolPanelView
Endless.Creator.UI.UIPlayerReferenceListView
Endless.Creator.UI.UIPropCreationDataListView
Endless.Creator.UIPropCreationDataView
Endless.Creator.UI.UIPropEntryListView
Endless.Creator.UI.UIPropToolPanelController
Endless.Creator.UI.UIPropToolPanelView
Endless.Creator.UI.UIPublishView
Endless.Creator.UI.UIRevisionListView
Endless.Creator.UI.UIRevisionsView
Endless.Creator.UI.UIRolesListView
Endless.Creator.UI.UIRuntimePropInfoDetailView
Endless.Creator.UI.UIRuntimePropInfoListView
Endless.Creator.UI.UIScreenshotFileInstancesListCellViewDragInstanceHandler
Endless.Creator.UI.UIScreenshotFileInstancesListView
Endless.Creator.UI.UIScreenshotToolView
Endless.Creator.UI.UIScreenshotView
Endless.Creator.UI.UIScriptReferenceListView
Endless.Creator.UI.UISpawnPointListView
Endless.Creator.UI.UITilesetDetailView
Endless.Creator.UI.UITilesetListView
Endless.Creator.UI.UIToolView
Endless.Creator.UI.UITransformIdentifierListView
Endless.Creator.UI.UIUserRoleListView
Endless.Creator.UI.UIUserRolesView
Endless.Creator.UI.UIUserScriptAutocompleteListView
Endless.Creator.UI.UIVersionListView
Endless.Creator.UI.UIWireConfirmationModalView
Endless.Creator.UI.UIWireEditorModalView
Endless.Creator.UI.UIWirePropertyModifierView
Endless.Creator.UI.UIWiresView
Endless.Creator.UI.UIWiringObjectInspectorView
Endless.Creator.UI.UIWiringRerouteView
```

### Controller types (93):
```
Endless.Creator.CreatorGunController
Endless.Creator.UI.UIAbstractPropCreationModalController
Endless.Creator.UI.UIAddPropGameAssetToGameLibraryModalController
Endless.Creator.UI.UIAddScreenshotsToGameModalController
Endless.Creator.UI.UIAddScreenshotsToLevelModalController
Endless.Creator.UI.UIAssetModerationModalController
Endless.Creator.UI.UIAssetReadAndWriteController
Endless.Creator.UI.UIAssetWithScreenshotsController
Endless.Creator.UI.UIBasePropCreationModalController
Endless.Creator.UI.UIBaseToolPanelController`1
Endless.Creator.UI.UIBaseWireController
Endless.Creator.UI.UIClientCopyHistoryEntryListCellController
Endless.Creator.UI.UICloudGameAssetListController
Endless.Creator.UI.UIConsoleMessageListController
Endless.Creator.UI.UICopyToolPanelController
Endless.Creator.UI.UIDockableToolPanelController
Endless.Creator.UI.UIEndlessEventInfoListCellController
Endless.Creator.UI.UIEndlessParameterInfoListCellController
Endless.Creator.UI.UIEraseToolPanelController
Endless.Creator.UI.UIGameAssetDetailController
Endless.Creator.UI.UIGameAssetListCellController
Endless.Creator.UI.UIGameAssetListCellSelectableController
Endless.Creator.UI.UIGameAssetPublishModalController
Endless.Creator.UI.UIGameController
Endless.Creator.UI.UIGameEditorWindowController
Endless.Creator.UI.UIGameLibraryAssetAdditionModalController
Endless.Creator.UI.UIGameLibraryAssetDetailModalController
Endless.Creator.UI.UIGameLibraryAssetReplacementConfirmationModalController
Endless.Creator.UI.UIGameLibraryAssetReplacementModalController
Endless.Creator.UI.UIGameLibraryFilterListCellController
Endless.Creator.UI.UIGameLibraryListController
Endless.Creator.UI.UIGameLibrarySelectionWindowController
Endless.Creator.UI.UIGenericPropCreationModalController
Endless.Creator.UI.UIGhostModeController
Endless.Creator.UI.UIInMemoryScreenshotListCellController
Endless.Creator.UI.UIInspectorScriptValueInputModalController
Endless.Creator.UI.UIInspectorScriptValueListCellController
Endless.Creator.UI.UIInspectorScriptValueTypeSelectionModalController
Endless.Creator.UI.UIInspectorToolPanelController
Endless.Creator.UI.UIInventorySpawnOptionsListCellController
Endless.Creator.UI.UIItemSelectionToolPanelController`2
Endless.Creator.UI.UILevelAssetListCellController
Endless.Creator.UI.UILevelAssetListController
Endless.Creator.UI.UILevelController
Endless.Creator.UI.UILevelDestinationSelectionListCellController
Endless.Creator.UI.UILevelEditorWindowController
Endless.Creator.UI.UILevelLoaderModalController
Endless.Creator.UI.UILevelStateSelectionModalController
Endless.Creator.UI.UILevelStateTemplateListCellController
Endless.Creator.UI.UINewLevelStateModalController
Endless.Creator.UI.UIPaintingToolPanelController
Endless.Creator.UI.UIPlayerReferenceListCellController
Endless.Creator.UI.UIPropCreationDataListCellController
Endless.Creator.UI.UIPropCreationPromptDataModalController
Endless.Creator.UI.UIPropEntryListCellController
Endless.Creator.UI.UIPropToolPanelController
Endless.Creator.UI.UIPublishController
Endless.Creator.UI.UIRolesListCellController
Endless.Creator.UI.UIRuntimePropDetailController
Endless.Creator.UI.UIRuntimePropInfoListCellController
Endless.Creator.UI.UIRuntimePropInfoListController
Endless.Creator.UI.UIRuntimePropInfoSelectionWindowController
Endless.Creator.UI.UIScreenshotController
Endless.Creator.UI.UIScreenshotFileInstancesGameAdditionListCellController
Endless.Creator.UI.UIScreenshotFileInstancesListCellController
Endless.Creator.UI.UIScreenshotFileInstancesListController
Endless.Creator.UI.UIScreenshotToolController
Endless.Creator.UI.UIScriptDataTypeSelectionModalController
Endless.Creator.UI.UIScriptEventInputModalController
Endless.Creator.UI.UIScriptReferenceInputModalController
Endless.Creator.UI.UIScriptReferenceListCellController
Endless.Creator.UI.UIScriptWindowController
Endless.Creator.UI.UISpawnPointListCellController
Endless.Creator.UI.UISpawnPointSelectionModalController
Endless.Creator.UI.UITilesetDetailController
Endless.Creator.UI.UITilesetListCellController
Endless.Creator.UI.UITilesetListController
Endless.Creator.UI.UIToolController
Endless.Creator.UI.UITransformIdentifierListCellController
Endless.Creator.UI.UITransformIdentifierSelectionWindowController
Endless.Creator.UI.UIUserRoleListCellController
Endless.Creator.UI.UIUserRolesController
Endless.Creator.UI.UIUserRolesModalController
Endless.Creator.UI.UIUserScriptAutocompleteListCellController
Endless.Creator.UI.UIVersionListCellController
Endless.Creator.UI.UIWireConfirmationModalController
Endless.Creator.UI.UIWireController
Endless.Creator.UI.UIWireCreatorController
Endless.Creator.UI.UIWireEditorController
Endless.Creator.UI.UIWireEditorModalController
Endless.Creator.UI.UIWireNodeController
Endless.Creator.UI.UIWiringObjectInspectorController
Endless.Creator.UI.UIWiringRerouteController
```

### Manager types (13):
```
Endless.Creator.CreatorManager
Endless.Creator.CreatorPlayerReferenceManager
Endless.Creator.GameEditorAssetVersionManager
Endless.Creator.LevelEditing.Runtime.MarkerManager
Endless.Creator.Notifications.NotificationManager`3
Endless.Creator.PlayBarManager
Endless.Creator.LevelEditing.Runtime.SaveLoadManager
Endless.Creator.LevelEditing.Runtime.ToolManager
Endless.Creator.UI.UICreatorReferenceManager
Endless.Creator.UI.UISaveStatusManager
Endless.Creator.UI.UIToolPrompterManager
Endless.Creator.UI.UIWiringManager
Endless.Creator.UI.UIWiringPrompterManager
```

## SECTION 2: GAMEPLAY.DLL ALL TYPES

Total types loaded from Gameplay.dll: 1236

### Namespaces:
```
: 119 types
__GEN: 1 types
ELM.Endless.EndlessFramework.Serialization: 6 types
Endless: 2 types
Endless.Assets: 9 types
Endless.Core: 1 types
Endless.Creator.LevelEditing.Runtime: 5 types
Endless.FileManagement: 14 types
Endless.Gameplay: 560 types
Endless.Gameplay.Fsm: 19 types
Endless.Gameplay.LevelEditing: 50 types
Endless.Gameplay.LevelEditing.Level: 97 types
Endless.Gameplay.LevelEditing.Level.UpgradeVersions: 4 types
Endless.Gameplay.LevelEditing.Tilesets: 45 types
Endless.Gameplay.LevelEditing.Wires: 8 types
Endless.Gameplay.LuaEnums: 39 types
Endless.Gameplay.LuaInterfaces: 42 types
Endless.Gameplay.Mobile: 1 types
Endless.Gameplay.PlayerInventory: 6 types
Endless.Gameplay.RightsManagement: 19 types
Endless.Gameplay.Screenshotting: 4 types
Endless.Gameplay.Scripting: 33 types
Endless.Gameplay.Serialization: 17 types
Endless.Gameplay.SoVariables: 1 types
Endless.Gameplay.Stats: 7 types
Endless.Gameplay.Test: 6 types
Endless.Gameplay.UI: 89 types
Endless.Gameplay.VisualManagement: 4 types
Endless.Props.Scripting: 1 types
Endless.Shared.UI: 1 types
HackAnythingAnywhere.Core: 8 types
Microsoft.CodeAnalysis: 1 types
Runtime.Gameplay.LevelEditing: 13 types
Runtime.Gameplay.LevelEditing.Levels: 2 types
Runtime.Gameplay.LuaClasses: 1 types
System.Runtime.CompilerServices: 1 types
```

### PropLibrary FOUND
Full Name: Endless.Gameplay.LevelEditing.PropLibrary
Base Type: System.Object

#### Fields (9):
```
  ReferenceFilter[] dynamicFilters
  Dictionary`2 loadedPropMap
  List`1 injectedPropIds
  HashSet`1 inflightLoadRequests
  Dictionary`2 _referenceFilterMap
  EndlessProp loadingObjectProp
  Transform prefabSpawnRoot
  EndlessProp basePropPrefab
  EndlessProp missingObjectPrefab
```

#### Methods (25):
```
  IReadOnlyList`1 GetReferenceFilteredDefinitionList(ReferenceFilter filter)
  Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
  Task`1 LoadPropPrefabs(LevelState levelState, Transform prefabSpawnTransform, EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action`2 progressCallback)
  Task LoadPropPrefab(EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action`1 progressCallback, Prop prop, List`1 modifiedIds)
  Void UnloadProp(RuntimePropInfo info, Boolean dataUnload)
  Void PopulateReferenceFilterMap()
  AssetReference[] GetAssetReferences()
  RuntimePropInfo GetRuntimePropInfo(AssetReference assetReference)
  RuntimePropInfo GetRuntimePropInfo(SerializableGuid assetId)
  RuntimePropInfo[] GetAllRuntimeProps()
  Boolean TryGetRuntimePropInfo(AssetReference assetReference, RuntimePropInfo& metadata)
  Boolean TryGetRuntimePropInfo(SerializableGuid assetId, RuntimePropInfo& metadata)
  Void AddMissingObjectRuntimePropInfo(RuntimePropInfo missingObjectInfo)
  Task PreloadData(CancellationToken cancelToken, List`1 previouslyMissingProps, Action`2 propLoadingUpdate)
  List`1 UnloadPropsNotInGameLibrary()
  Task FetchAndSpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List`1 modifiedIds)
  Task SpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List`1 modifiedIds, Prop prop)
  Void UnloadAll()
  Boolean IsRepopulateRequired(Game newGame, Game oldGame)
  Task`1 Repopulate(Stage activeStage, CancellationToken cancellationToken)
  Boolean SanitizeMemberChanges(PropEntry propEntry)
  Boolean SanitizeWireBundles(LevelState levelState, PropEntry propEntry)
  Boolean IsInjectedProp(String propDataAssetID)
  List`1 GetBaseTypeList(String propDataBaseTypeId)
  List`1 GetBaseTypeList(SerializableGuid[] propDataBaseTypeId)
```

#### Nested Types (21):

##### RuntimePropInfo
```
  Prop PropData
  Sprite Icon
  EndlessProp EndlessProp
  Boolean <IsLoading>k__BackingField
  Boolean <IsMissingObject>k__BackingField
```

##### <>c
```
```

##### <>c__DisplayClass15_0
```
  PropLibrary <>4__this
  EndlessProp propPrefab
  EndlessProp missingObjectPrefab
  CancellationToken cancelToken
  BulkAssetCacheResult`1 bulkResult
  List`1 modifiedIds
  Action`2 progressCallback
  List`1 propReferences
```

##### <>c__DisplayClass25_0
```
  SerializableGuid assetId
```

##### <>c__DisplayClass28_0
```
  SerializableGuid assetId
```

##### <>c__DisplayClass30_0
```
  BulkAssetCacheResult`1 bulkResult
  Texture2D[] iconTextures
```

##### <>c__DisplayClass30_1
```
  AssetReference propToPreload
```

##### <>c__DisplayClass31_0
```
  KeyValuePair`2 entry
```

##### <>c__DisplayClass33_0
```
  Prop prop
```

##### <>c__DisplayClass35_0
```
  AssetReference currentReference
```

##### <>c__DisplayClass36_0
```
  AssetIdVersionKey asset
  Func`2 <>9__0
```

##### <>c__DisplayClass37_0
```
  MemberChange memberChange
```

##### <>c__DisplayClass38_0
```
  WireEntry wireEntry
```

##### <>c__DisplayClass41_0
```
  SerializableGuid[] propDataBaseTypeId
```

##### <FetchAndSpawnPropPrefab>d__32
```
  Int32 <>1__state
  AsyncTaskMethodBuilder <>t__builder
  PropLibrary <>4__this
  AssetReference assetReference
  CancellationToken cancelToken
  List`1 modifiedIds
  EndlessProp missingObjectPrefab
  EndlessProp propPrefab
  YieldAwaiter <>u__1
  TaskAwaiter`1 <>u__2
  TaskAwaiter <>u__3
```

##### <InjectProp>d__14
```
  Int32 <>1__state
  AsyncVoidMethodBuilder <>t__builder
  PropLibrary <>4__this
  Prop prop
  EndlessProp propPrefab
  Transform prefabSpawnTransform
  GameObject testPrefab
  Script testScript
  Sprite icon
  EndlessProp <newProp>5__2
  TaskAwaiter <>u__1
```

##### <LoadPropPrefab>d__16
```
  Int32 <>1__state
  AsyncTaskMethodBuilder <>t__builder
  Prop prop
  PropLibrary <>4__this
  List`1 modifiedIds
  CancellationToken cancelToken
  EndlessProp propPrefab
  EndlessProp missingObjectPrefab
  Action`1 progressCallback
  AssetReference <propReference>5__2
  TaskAwaiter <>u__1
```

##### <LoadPropPrefabs>d__15
```
  Int32 <>1__state
  AsyncTaskMethodBuilder`1 <>t__builder
  PropLibrary <>4__this
  EndlessProp propPrefab
  EndlessProp missingObjectPrefab
  CancellationToken cancelToken
  Action`2 progressCallback
  LevelState levelState
  <>c__DisplayClass15_0 <>8__1
  TaskAwaiter`1 <>u__1
  TaskAwaiter <>u__2
```

##### <PreloadData>d__30
```
  Int32 <>1__state
  AsyncTaskMethodBuilder <>t__builder
  <>c__DisplayClass30_0 <>8__1
  CancellationToken cancelToken
  PropLibrary <>4__this
  Action`2 propLoadingUpdate
  List`1 previouslyMissingProps
  IReadOnlyList`1 <propsToPreload>5__2
  TaskAwaiter`1 <>u__1
  TaskAwaiter <>u__2
```

##### <Repopulate>d__36
```
  Int32 <>1__state
  AsyncTaskMethodBuilder`1 <>t__builder
  PropLibrary <>4__this
  Stage activeStage
  CancellationToken cancellationToken
  List`1 <previouslyMissingProps>5__2
  PropPopulateResult <propPopulateResult>5__3
  TaskAwaiter <>u__1
  TaskAwaiter`1 <>u__2
  TaskAwaiter`1 <>u__3
```

##### <SpawnPropPrefab>d__33
```
  Int32 <>1__state
  AsyncTaskMethodBuilder <>t__builder
  Prop prop
  EndlessProp propPrefab
  PropLibrary <>4__this
  CancellationToken cancelToken
  AssetReference assetReference
  <>c__DisplayClass33_0 <>8__1
  List`1 modifiedIds
  EndlessProp missingObjectPrefab
  EndlessProp <newProp>5__2
  RuntimePropInfo <currentInfo>5__3
  TaskAwaiter <>u__1
  TaskAwaiter`1 <>u__2
```

### StageManager FOUND
Full Name: Endless.Gameplay.LevelEditing.Level.StageManager
Base Type: MonoBehaviourSingleton`1

#### Fields (30):
```
  Stage stageTemplate
  OfflineStage offlineStagePrefab
  List`1 propRequirements
  List`1 baseTypeRequirements
  BaseTypeList baseTypeList
  ComponentList componentList
  Transform prefabSpawnTransform
  EndlessProp basePropPrefab
  CollisionLibrary collisionLibrary
  GameObject fallbackTerrain
  Sprite fallbackDisplayIcon
  GameObject loadingTerrain
  EndlessProp loadingPropObject
  EndlessProp missingObjectPrefab
  Dictionary`2 spawnedLevels
  SerializableGuid lastGameGuid
  Dictionary`2 spawnedOfflineStages
  Dictionary`2 cachedLevelStates
  List`1 injectedProps
  PropLibrary activePropLibrary
  RuntimePalette activeTerrainPalette
  AudioLibrary activeAudioLibrary
  Dictionary`2 persistantPropStateMap
  Dictionary`2 destroyedObjectMapByStage
  SerializableGuid activeLevelGuid
  Dictionary`2 propRequirementLookup
  Dictionary`2 baseTypeRequirementLookup
  UnityEvent`1 OnActiveStageChanged
  UnityEvent`1 TerrainAndPropsLoaded
  UnityEvent`1 OnLevelLoaded
```

#### Public Methods (32):
```
  Void Start()
  Boolean TryGetOfflineStage(AssetReference reference, OfflineStage& offlineStage)
  Void RemoveOfflineStage(AssetReference reference)
  OfflineStage GetNewOfflineStage(AssetReference reference, String stageName)
  Boolean TryGetCachedLevel(SerializableGuid levelId, String versionNumber, LevelState& levelState)
  Void UpdateStageVersion(AssetReference oldReference, AssetReference newReference)
  Void SetJoinLevelId(SerializableGuid levelId)
  Object GetPropState(SerializableGuid instanceId, String componentName)
  Void SavePropState(SerializableGuid instanceId, String componentName, Object newState)
  Void ClearPersistantPropStates()
  Void ClearDestroyedObjectMap()
  Void PropDestroyed(SerializableGuid instanceId)
  Boolean IsPropDestroyed(SerializableGuid instanceId)
  Void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
  Task RepopulateAudioLibrary(CancellationToken cancellationToken)
  Void PrepareForLevelChange(SerializableGuid levelToChangeTo)
  Task LoadLevel(LevelState levelState, Boolean loadLibraryPrefabs, CancellationToken cancelToken, Action`1 progressCallback)
  Task LoadLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action`1 progressCallback)
  Boolean LevelIsLoaded(SerializableGuid levelId)
  Void RegisterStage(Stage stage)
  Void FlushLoadedAndSpawnedStages(Boolean destroyStageObjects)
  Void LeavingSession()
  Void PropFailedToLoad(PropEntry propEntry)
  BaseTypeDefinition GetBaseTypeDefinition(Type type)
  ComponentDefinition GetComponentDefinition(Type type)
  Task FetchAndSpawnPropPrefab(AssetReference assetReference)
  Void LoadTilesetByIndex(Int32 tilesetIndex)
  RuntimePropInfo GetMissingObjectInfo(Prop propData)
  Boolean TryGetComponentDefinition(Type type, ComponentDefinition& definition)
  Boolean TryGetDataTypeSignature(List`1 eventInfos, String memberName, Int32[]& signature)
  Boolean SignaturesMatch(Int32[] signatureOne, Int32[] signatureTwo)
  Void UnloadAll()
```

### Prop-related types in Gameplay.dll (31):
```
Endless.Gameplay.LevelEditing.DefaultContentManager+<AddDefaultProps>d__18
Endless.Gameplay.LevelEditing.PropLibrary+<FetchAndSpawnPropPrefab>d__32
Endless.Gameplay.LevelEditing.Level.StageManager+<FetchAndSpawnPropPrefab>d__84
Endless.Gameplay.LevelEditing.PropLibrary+<InjectProp>d__14
Endless.Gameplay.LevelEditing.Level.StageManager+<LoadPropLibraryReferences>d__74
Endless.Gameplay.LevelEditing.PropLibrary+<LoadPropPrefab>d__16
Endless.Gameplay.LevelEditing.PropLibrary+<LoadPropPrefabs>d__15
Endless.Gameplay.LevelEditing.Level.Stage+<LoadProps>d__109
Endless.Gameplay.LevelEditing.Level.Stage+<LoadPropsInStage>d__110
Endless.Gameplay.LevelEditing.PropLibrary+<SpawnPropPrefab>d__33
Endless.Gameplay.Scripting.EndlessProp
Endless.Gameplay.LevelEditing.Level.InjectedProps
Endless.Gameplay.InspectorPropReference
Endless.Gameplay.IPropPlacedSubscriber
Endless.Gameplay.LevelEditing.Level.PropCell
Endless.Gameplay.LuaEnums.PropCombatMode
Endless.Gameplay.LuaEnums.PropDamageMode
Endless.Gameplay.PropDamageReaction
Endless.Gameplay.LevelEditing.Level.PropEntry
Endless.Gameplay.NpcEnum+PropFallMode
Endless.Gameplay.LevelEditing.PropLibrary
Endless.Gameplay.PropLibraryReference
Endless.Gameplay.LuaEnums.PropMovementMode
Endless.Gameplay.LuaEnums.PropPhysicsMode
Endless.Gameplay.LevelEditing.PropPopulateResult
Endless.Gameplay.LevelEditing.Level.Stage+PropRegisteredRequest
Endless.Creator.LevelEditing.Runtime.PropRequirement
Endless.Creator.LevelEditing.Runtime.PropRequirement+PropRequirementEntry
Runtime.Gameplay.LevelEditing.PropUsage
Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo
Endless.Gameplay.LevelEditing.Level.SerializedProp
```

### Stage-related types (5):
```
Endless.Gameplay.LevelEditing.Level.Stage+<HandleEndOfStageLoading>d__106
Endless.Gameplay.LevelEditing.Level.Stage+<LoadPropsInStage>d__110
Endless.Gameplay.LevelEditing.Level.OfflineStage
Endless.Gameplay.LevelEditing.Level.Stage
Endless.Gameplay.LevelEditing.Level.StageManager
```

## SECTION 3: PROPS.DLL ALL TYPES

Total types loaded from Props.dll: 80

### All types in Props.dll:
```
<PrivateImplementationDetails>+__StaticArrayInitTypeSize=3372
<PrivateImplementationDetails>+__StaticArrayInitTypeSize=4132
Endless.Props.Assets.Script+<>c
Endless.Props.ReferenceComponents.CollectionLengthValidator+<>c
Endless.Props.Assets.StringPairDictionary+<>c__DisplayClass1_0
Endless.Props.Assets.StringPairDictionary+<>c__DisplayClass2_0
Endless.Props.Assets.Script+<>c__DisplayClass52_0
Endless.Props.Assets.Script+<>c__DisplayClass53_0
Endless.Props.Assets.Script+<>c__DisplayClass54_0
<PrivateImplementationDetails>
Endless.Props.Assets.AudioAsset
Endless.Props.Assets.AudioCategory
Endless.Props.ReferenceComponents.BaseTypeReferences
Endless.Props.ReferenceComponents.BasicLevelGateReferences
Endless.Props.ReferenceComponents.BouncePadReferences
Endless.Props.ReferenceComponents.BoxColliderValidator
Endless.Props.Scripting.ClampValue+ClampUsage
Endless.Props.Scripting.ClampValue
Endless.Props.ReferenceComponents.CollectionLengthValidator
Endless.Props.ColliderInfo
Endless.Props.ColliderInfoValidator
Endless.Props.ColliderInfo+ColliderType
Endless.Props.ColliderInfo+ColliderTypeInfoAttribute
Endless.Props.Scripting.ComponentListEntry
Endless.Props.ReferenceComponents.ComponentReferences
Endless.Props.ReferenceComponents.DashPackVisualReferences
Endless.Props.ReferenceComponents.DestroyableReferences
Endless.Props.ReferenceComponents.DoorReferences
Endless.Props.ReferenceComponents.DraggablePhysicsCubeReferences
Endless.Props.Scripting.EndlessEventInfo
Endless.Props.Scripting.EndlessLuaEvent
Endless.Props.Scripting.EndlessParameterInfo
Endless.Props.Assets.EndlessPrefabAsset
Endless.Props.Scripting.EnumEntry
Endless.Props.ReferenceComponents.HittableReferences
Endless.Props.Assets.InspectorOrganizationData
Endless.Props.Scripting.InspectorScriptValue
Endless.Props.ReferenceComponents.InstantPickupReferences
Endless.Props.ReferenceComponents.JetpackVisualReferences
Endless.Props.ReferenceComponents.KeyReferences
Endless.Props.Assets.LocationOffsetState
Endless.Props.ReferenceComponents.LockableComponentReferences
Endless.Props.ReferenceComponents.MeleeWeaponItemReferences
UnitySourceGeneratedAssemblyMonoScriptTypes_v1+MonoScriptData
Endless.Props.ReferenceComponents.NotNullObjectValidator
Endless.Props.ReferenceComponents.OneHandedRangedWeaponVisualReferences
Endless.Props.ProjectileShooterEjectionSettings
Endless.Props.ReferenceComponents.ProjectileShooterReferences
Endless.Props.Assets.Prop
Endless.Props.Loaders.PropLoader
Endless.Props.Assets.PropLocationOffset
Runtime.Props.Validations.ReadWriteSetValidator
Endless.Props.ReferenceComponents.ReferenceBase
Endless.Props.ReferenceComponents.ReferenceBaseValidator
Endless.Props.Assets.ScriptReference+ReferenceType
Endless.Props.ReferenceComponents.ResizableVolumeReferences
Endless.Props.ReferenceComponents.ResourcePickupReferences
Endless.Props.Assets.Script
Endless.Props.Assets.ScriptReference
Endless.Props.Scripting.SDKComponentList
Endless.Props.ReferenceComponents.SensorReferences
Endless.Props.ReferenceComponents.SentryReferences
Endless.Props.ReferenceComponents.ShieldComponentReferences
Endless.Props.ReferenceComponents.SimpleInteractableReferences
Endless.Props.ReferenceComponents.SpawnPointReferences
Endless.Props.ReferenceComponents.SpikeTrapReferences
Endless.Props.ReferenceComponents.StaticPropReferences
Endless.Props.Assets.StringPair
Endless.Props.Assets.StringPairDictionary
Endless.Props.ReferenceComponents.SwordVisualReferences
Endless.Props.ReferenceComponents.TargeterReferences
Endless.Props.ReferenceComponents.TextBubbleReferences
Endless.Props.ReferenceComponents.TextReferences
Endless.Props.TransformIdentifier
Endless.Props.ReferenceComponents.TreasureVisualReferences
Endless.Props.ReferenceComponents.TriggerComponentReferences
Endless.Props.ReferenceComponents.TwoHandedRangedWeaponVisualReferences
UnitySourceGeneratedAssemblyMonoScriptTypes_v1
Endless.Props.Assets.WireOrganizationData
Endless.Props.Scripting.WiringReceiver
```

### Prop Class DETAILED ANALYSIS
Full Name: Endless.Props.Assets.Prop
Base Type: Endless.Assets.Asset

#### Fields (21):
```
  String baseTypeId
  List`1 componentIds
  AssetReference scriptAsset
  AssetReference prefabBundle
  List`1 visualAssets
  Int32 iconFileInstanceId
  PropLocationOffset[] propLocationOffsets
  Boolean applyXOffset
  Boolean applyZOffset
  StringPairDictionary propMetaData
  Boolean openSource
  Vector3Int bounds
  List`1 transformRemaps
  Dictionary`2 remapDictionary
  String Description
  String InternalVersion
  RevisionMetaData RevisionMetaData
  String Name
  String AssetID
  String AssetVersion
  String AssetType
```

#### Methods (12):
```
  String GetPropMetaData(String key)
  Void SetPropMetaData(String key, String value)
  List`1 RemapTransforms(List`1 transforms)
  Void SetComponentIds(SerializableGuid baseType, List`1 components)
  Prop Clone()
  Void AddLocationOffset(PropLocationOffset offset)
  Void CleanupProp()
  Object GetAnonymousObjectForUpload()
  Vector3Int GetBoundingSize()
  Vector3[] GetLocalCellPositions()
  Vector3[] GetAllOffsets()
  Void SetPropLocationOffsets(PropLocationOffset[] newPropLocationOffsets)
```
## SECTION 4: ASSETS.DLL KEY TYPES

Total types loaded from Assets.dll: 31

### AssetReference FOUND
Full Name: Endless.Assets.AssetReference

#### Fields:
```
  String AssetID
  String AssetVersion
  String AssetType
  Boolean UpdateParentVersion
```

#### Constructors (1):
```
  AssetReference()
```
### Asset Base Types:
```
Endless.Assets.AssetCore : Object
Endless.Assets.Asset : AssetCore
```
## SECTION 5: SEARCHING FOR RuntimePropInfo ACROSS ALL ASSEMBLIES

Found: Endless.Creator.UI.UIRuntimePropInfoListCellController in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoListController in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoListModel in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoListView in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropDetailController in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoDetailView in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoSelectionWindowController in Creator.dll
Found: Endless.Gameplay.LevelEditing.PropLibrary+RuntimePropInfo in Gameplay.dll
Found: UnityEngine.AddressableAssets.Initialization.AddressablesRuntimeProperties in Unity.Addressables.dll

## SECTION 6: SEARCHING FOR UI LIST TYPES

Found: Endless.Creator.UI.UIClientCopyHistoryEntryListCellController in Creator.dll
Found: Endless.Creator.UI.UIClientCopyHistoryEntryListModel in Creator.dll
Found: Endless.Creator.UI.UIClientCopyHistoryEntryListView in Creator.dll
Found: Endless.Creator.UI.UIConsoleMessageListController in Creator.dll
Found: Endless.Creator.UI.UIConsoleMessageListModel in Creator.dll
Found: Endless.Creator.UI.UIConsoleMessageListView in Creator.dll
Found: Endless.Creator.UI.UICreatorPlayerListModel in Creator.dll
Found: Endless.Creator.UI.UICreatorPlayerListView in Creator.dll
Found: Endless.Creator.UI.UIEndlessEventInfoListCellController in Creator.dll
Found: Endless.Creator.UI.UIEndlessEventInfoListModel in Creator.dll
Found: Endless.Creator.UI.UIEndlessEventInfoListView in Creator.dll
Found: Endless.Creator.UI.UIEndlessParameterInfoListCellController in Creator.dll
Found: Endless.Creator.UI.UIEndlessParameterInfoListModel in Creator.dll
Found: Endless.Creator.UI.UIEndlessParameterInfoListView in Creator.dll
Found: Endless.Creator.UI.UIEnumEntryListModel in Creator.dll
Found: Endless.Creator.UI.UIEnumEntryListView in Creator.dll
Found: Endless.Creator.UI.UIGameAssetListCellController in Creator.dll
Found: Endless.Creator.UI.UIGameAssetListCellSelectableController in Creator.dll
Found: Endless.Creator.UI.UIGameAssetListCellViewDragInstanceHandler in Creator.dll
Found: Endless.Creator.UI.UICloudGameAssetListController in Creator.dll
Found: Endless.Creator.UI.UIGameLibraryListController in Creator.dll
Found: Endless.Creator.UI.IGameAssetListModel in Creator.dll
Found: Endless.Creator.UI.UIBaseGameAssetCloudPaginatedListModel in Creator.dll
Found: Endless.Creator.UI.UIGameAssetListModel in Creator.dll
Found: Endless.Creator.UI.UIGameLibraryListModel in Creator.dll
Found: Endless.Creator.UI.UIOwnedGameAssetCloudPaginatedListModel in Creator.dll
Found: Endless.Creator.UI.UIPublishedGameAssetCloudPaginatedListModel in Creator.dll
Found: Endless.Creator.UI.UIUserReportedGameAssetCloudPaginatedListModel in Creator.dll
Found: Endless.Creator.UI.UIGameAssetListView in Creator.dll
Found: Endless.Creator.UI.UIGameLibraryFilterListCellController in Creator.dll
Found: Endless.Creator.UI.UIGameLibraryFilterListModel in Creator.dll
Found: Endless.Creator.UI.UIGameLibraryFilterListView in Creator.dll
Found: Endless.Creator.UI.UIInMemoryScreenshotListCellController in Creator.dll
Found: Endless.Creator.UI.UIInMemoryScreenshotListModel in Creator.dll
Found: Endless.Creator.UI.UIInMemoryScreenshotListView in Creator.dll
Found: Endless.Creator.UI.UIInspectorScriptValueListCellController in Creator.dll
Found: Endless.Creator.UI.UIInspectorScriptValueListModel in Creator.dll
Found: Endless.Creator.UI.UIInspectorScriptValueListView in Creator.dll
Found: Endless.Creator.UI.UIInventorySpawnOptionsListCellController in Creator.dll
Found: Endless.Creator.UI.UIInventorySpawnOptionsListModel in Creator.dll
Found: Endless.Creator.UI.UIInventorySpawnOptionsListView in Creator.dll
Found: Endless.Creator.UI.UILevelAssetListCellController in Creator.dll
Found: Endless.Creator.UI.UILevelAssetListCellViewDragInstanceHandler in Creator.dll
Found: Endless.Creator.UI.UILevelAssetListController in Creator.dll
Found: Endless.Creator.UI.UILevelAssetListModel in Creator.dll
Found: Endless.Creator.UI.UILevelAssetListView in Creator.dll
Found: Endless.Creator.UI.UILevelAssetAndScreenshotsListModelEntry in Creator.dll
Found: Endless.Creator.UI.UILevelAssetAndScreenshotsListModel in Creator.dll
Found: Endless.Creator.UI.UILevelAssetAndScreenshotsListView in Creator.dll
Found: Endless.Creator.UI.UILevelDestinationSelectionListCellController in Creator.dll
Found: Endless.Creator.UI.UILevelDestinationSelectionListModel in Creator.dll
Found: Endless.Creator.UI.UILevelDestinationSelectionListView in Creator.dll
Found: Endless.Creator.UI.UILevelStateTemplateListCellController in Creator.dll
Found: Endless.Creator.UI.UILevelStateTemplateListModel in Creator.dll
Found: Endless.Creator.UI.UILevelStateTemplateListView in Creator.dll
Found: Endless.Creator.UI.UIPlayerReferenceListCellController in Creator.dll
Found: Endless.Creator.UI.UIPlayerReferenceListModel in Creator.dll
Found: Endless.Creator.UI.UIPlayerReferenceListView in Creator.dll
Found: Endless.Creator.UI.UIPropCreationDataListCellController in Creator.dll
Found: Endless.Creator.UI.UIPropCreationDataListModel in Creator.dll
Found: Endless.Creator.UI.UIPropCreationDataListView in Creator.dll
Found: Endless.Creator.UI.UIPropEntryListCellController in Creator.dll
Found: Endless.Creator.UI.UIPropEntryListModel in Creator.dll
Found: Endless.Creator.UI.UIPropEntryListView in Creator.dll
Found: Endless.Creator.UI.UIRevisionListModel in Creator.dll
Found: Endless.Creator.UI.UIRevisionListView in Creator.dll
Found: Endless.Creator.UI.UIRolesListCellController in Creator.dll
Found: Endless.Creator.UI.UIRolesListModel in Creator.dll
Found: Endless.Creator.UI.UIRolesListView in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoListCellController in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoListController in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoListModel in Creator.dll
Found: Endless.Creator.UI.UIRuntimePropInfoListView in Creator.dll
Found: Endless.Creator.UI.UIScreenshotFileInstancesListCellController in Creator.dll
Found: Endless.Creator.UI.UIScreenshotFileInstancesListCellViewDragInstanceHandler in Creator.dll
Found: Endless.Creator.UI.UIScreenshotFileInstancesListController in Creator.dll
Found: Endless.Creator.UI.UIScreenshotFileInstancesListModel in Creator.dll
Found: Endless.Creator.UI.UIScreenshotFileInstancesListView in Creator.dll
Found: Endless.Creator.UI.UIScreenshotFileInstancesGameAdditionListCellController in Creator.dll
Found: Endless.Creator.UI.UIUserScriptAutocompleteListCellController in Creator.dll
Found: Endless.Creator.UI.UIUserScriptAutocompleteListModel in Creator.dll
Found: Endless.Creator.UI.UIUserScriptAutocompleteListModelItem in Creator.dll
Found: Endless.Creator.UI.UIUserScriptAutocompleteListView in Creator.dll
Found: Endless.Creator.UI.UIScriptReferenceListCellController in Creator.dll
Found: Endless.Creator.UI.UIScriptReferenceListModel in Creator.dll
Found: Endless.Creator.UI.UIScriptReferenceListView in Creator.dll
Found: Endless.Creator.UI.UISpawnPointListCellController in Creator.dll
Found: Endless.Creator.UI.UISpawnPointListModel in Creator.dll
Found: Endless.Creator.UI.UISpawnPointListView in Creator.dll
Found: Endless.Creator.UI.UITilesetListCellController in Creator.dll
Found: Endless.Creator.UI.UITilesetListController in Creator.dll
Found: Endless.Creator.UI.UITilesetListModel in Creator.dll
Found: Endless.Creator.UI.UITilesetListView in Creator.dll
Found: Endless.Creator.UI.UITransformIdentifierListCellController in Creator.dll
Found: Endless.Creator.UI.UITransformIdentifierListModel in Creator.dll
Found: Endless.Creator.UI.UITransformIdentifierListView in Creator.dll
Found: Endless.Creator.UI.UIUserRoleListCellController in Creator.dll
Found: Endless.Creator.UI.UIUserRoleListModel in Creator.dll
Found: Endless.Creator.UI.UIUserRoleListView in Creator.dll
Found: Endless.Creator.UI.UIVersionListCellController in Creator.dll
Found: Endless.Creator.UI.UIVersionListModel in Creator.dll
Found: Endless.Creator.UI.UIVersionListView in Creator.dll
Found: Endless.Shared.UI.IListCellViewable in Shared.dll
Found: Endless.Shared.UI.UIBaseListCellController`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseListCellViewDragInstanceHandler`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseCloudListController`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseCloudPaginatedListController`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseFilterableListController`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseListController`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseLocalFilterableListController`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseRearrangeableListController`1 in Shared.dll
Found: Endless.Shared.UI.IListModel in Shared.dll
Found: Endless.Shared.UI.IListView in Shared.dll
Found: Endless.Shared.UI.UIBaseAssetCloudPaginatedListModel`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseCloudPaginatedListModel`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseCloudListModel`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseListModel`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseLocalFilterableListModel`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseRearrangeableListModel`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseListView`1 in Shared.dll
Found: Endless.Shared.UI.UIBaseRoleInteractableListView`1 in Shared.dll
Found: Endless.Shared.UI.UIInputFieldListCellController in Shared.dll
Found: Endless.Shared.UI.UIInputFieldListController in Shared.dll
Found: Endless.Shared.UI.UIInputFieldListModel in Shared.dll
Found: Endless.Shared.UI.UIInputFieldListView in Shared.dll
Found: Endless.Shared.UI.UILogsListCellController in Shared.dll
Found: Endless.Shared.UI.UILogsListController in Shared.dll
Found: Endless.Shared.UI.UILogsListModel in Shared.dll
Found: Endless.Shared.UI.UILogsListModelHandler in Shared.dll
Found: Endless.Shared.UI.UILogsListView in Shared.dll
Found: Endless.Shared.UI.UISpriteListModel in Shared.dll
Found: Endless.Shared.UI.UISpriteListView in Shared.dll
Found: Endless.Shared.UI.UIStringListCellController in Shared.dll
Found: Endless.Shared.UI.UIStringListModel in Shared.dll
Found: Endless.Shared.UI.UIStringListView in Shared.dll
Found: Endless.Shared.UI.UITexture2DListModel in Shared.dll
Found: Endless.Shared.UI.UITexture2DListView in Shared.dll
Found: Endless.Shared.UI.Test.UITestListCellController in Shared.dll
Found: Endless.Shared.UI.Test.UITestListCellViewDragInstanceHandler in Shared.dll
Found: Endless.Shared.UI.Test.UITestLocalFilterableListController in Shared.dll
Found: Endless.Shared.UI.Test.UITestRearrangeableListController in Shared.dll
Found: Endless.Shared.UI.Test.UITestListModelHandler in Shared.dll
Found: Endless.Shared.UI.Test.UITestLocalFilterableListModel in Shared.dll
Found: Endless.Shared.UI.Test.UITestRearrangeableListModel in Shared.dll
Found: Endless.Shared.UI.Test.UITestListView in Shared.dll
Found: Endless.Gameplay.UI.UICharacterCosmeticsDefinitionListCellController in Gameplay.dll
Found: Endless.Gameplay.UI.UICharacterCosmeticsDefinitionListController in Gameplay.dll
Found: Endless.Gameplay.UI.UICharacterCosmeticsDefinitionListModel in Gameplay.dll
Found: Endless.Gameplay.UI.UICharacterCosmeticsDefinitionListView in Gameplay.dll
Found: Endless.Gameplay.UI.UIGameplayPlayerListModel in Gameplay.dll
Found: Endless.Gameplay.UI.UIGameplayPlayerListView in Gameplay.dll
Found: Endless.Gameplay.UI.UIPlayerReferenceManagerListModel in Gameplay.dll
Found: Endless.Gameplay.UI.UIIconDefinitionListCellController in Gameplay.dll
Found: Endless.Gameplay.UI.UIIconDefinitionListModel in Gameplay.dll
Found: Endless.Gameplay.UI.UIIconDefinitionListView in Gameplay.dll
Found: Endless.Gameplay.UI.UINpcClassCustomizationDataListCellController in Gameplay.dll
Found: Endless.Gameplay.UI.UINpcClassCustomizationDataListModel in Gameplay.dll
Found: Endless.Gameplay.UI.UINpcClassCustomizationDataListView in Gameplay.dll
Found: Endless.Gameplay.UI.UIBasePaginatedSocialListModel`1 in Gameplay.dll
Found: Endless.Gameplay.UI.UIBaseSocialListModel`1 in Gameplay.dll
Found: Endless.Gameplay.UI.UIBlockedUserListModel in Gameplay.dll
Found: Endless.Gameplay.UI.UIFriendRequestListModel in Gameplay.dll
Found: Endless.Gameplay.UI.UISentFriendRequestListModel in Gameplay.dll
Found: Endless.Gameplay.UI.UIFriendshipListModel in Gameplay.dll
Found: System.DirectoryServices.DirectoryVirtualListView in System.DirectoryServices.dll
Found: System.DirectoryServices.DirectoryVirtualListViewContext in System.DirectoryServices.dll
Found: System.ComponentModel.IBindingListView in System.dll
Found: UnityEngine.UIElements.BaseListViewController in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.ListViewController in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.MultiColumnListViewController in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.ReusableListViewItem in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.ReusableMultiColumnListViewItem in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.ListViewReorderMode in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.BaseListView in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.ListView in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.MultiColumnListView in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.ListViewDragger in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.ListViewDraggerExtension in UnityEngine.UIElementsModule.dll
Found: UnityEngine.UIElements.ListViewDraggerAnimated in UnityEngine.UIElementsModule.dll
Found: Endless.Core.UI.UIClientDataListCellController in Core.dll
Found: Endless.Core.UI.UIClientDataListController in Core.dll
Found: Endless.Core.UI.UIClientDataListModel in Core.dll
Found: Endless.Core.UI.UIClientDataListView in Core.dll
Found: Endless.Core.UI.UIClientDataUserGroupListCellController in Core.dll
Found: Endless.Core.UI.UIListModelAddButtonHandler in Core.dll
Found: Endless.Core.UI.UIMainMenuGameModelCloudPaginatedListController in Core.dll
Found: Endless.Core.UI.UIMainMenuGameModelListController in Core.dll
Found: Endless.Core.UI.UIMainMenuGameModelListModel in Core.dll
Found: Endless.Core.UI.UIMainMenuGameModelListView in Core.dll
Found: Endless.Core.UI.UIMainMenuGameModelPaginatedListModel in Core.dll
Found: Unity.Collections.FixedList32BytesDebugView`1 in Unity.Collections.dll
Found: Unity.Collections.FixedList64BytesDebugView`1 in Unity.Collections.dll
Found: Unity.Collections.FixedList128BytesDebugView`1 in Unity.Collections.dll
Found: Unity.Collections.FixedList512BytesDebugView`1 in Unity.Collections.dll
Found: Unity.Collections.FixedList4096BytesDebugView`1 in Unity.Collections.dll
Found: Unity.Collections.NativeListDebugView`1 in Unity.Collections.dll
Found: System.Windows.Forms.DrawListViewColumnHeaderEventArgs in System.Windows.Forms.dll
Found: System.Windows.Forms.DrawListViewColumnHeaderEventHandler in System.Windows.Forms.dll
Found: System.Windows.Forms.DrawListViewItemEventArgs in System.Windows.Forms.dll
Found: System.Windows.Forms.DrawListViewItemEventHandler in System.Windows.Forms.dll
Found: System.Windows.Forms.DrawListViewSubItemEventArgs in System.Windows.Forms.dll
Found: System.Windows.Forms.DrawListViewSubItemEventHandler in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItemConverter in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewSubItemConverter in System.Windows.Forms.dll
Found: System.Windows.Forms.ListView in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewAlignment in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewGroup in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewGroupCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewGroupConverter in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewGroupItemCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewHitTestInfo in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewHitTestLocations in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewInsertionMark in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItem in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItemMouseHoverEventArgs in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItemMouseHoverEventHandler in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItemSelectionChangedEventArgs in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItemSelectionChangedEventHandler in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItemStates in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewVirtualItemsSelectionRangeChangedEventArgs in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewVirtualItemsSelectionRangeChangedEventHandler in System.Windows.Forms.dll
Found: System.Windows.Forms.ListView+CheckedListViewItemCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ListView+SelectedListViewItemCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ListView+ListViewItemCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ListView+ListViewNativeItemCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ListView+ListViewAccessibleObject in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItem+ListViewItemImageIndexer in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItem+ListViewSubItem in System.Windows.Forms.dll
Found: System.Windows.Forms.ListViewItem+ListViewSubItemCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.NativeMethods+ListViewCompareCallback in System.Windows.Forms.dll
Found: System.Windows.Forms.VisualStyles.VisualStyleElement+ListView in System.Windows.Forms.dll
Found: System.Windows.Forms.Design.ListViewDesigner in System.Design.dll
Found: System.Windows.Forms.Design.ListViewActionList in System.Design.dll
Found: System.Windows.Forms.Design.ListViewGroupCollectionEditor in System.Design.dll
Found: System.Windows.Forms.Design.ListViewItemCollectionEditor in System.Design.dll
Found: System.Windows.Forms.Design.ListViewSubItemCollectionEditor in System.Design.dll
Found: System.Web.UI.Design.WebControls.CreateDataSourceDialog+DataSourceListViewItem in System.Design.dll
Found: System.Web.UI.Design.WebControls.DataControlFieldsEditor+ListViewWithEnter in System.Design.dll
Found: System.Web.UI.Design.WebControls.ParameterEditorUserControl+ParameterListViewItem in System.Design.dll
Found: System.Design.NativeMethods+ListViewCompareCallback in System.Design.dll

## SECTION 7: SEARCHING FOR TOOL PANEL TYPES

Found: Endless.Creator.UI.UIBaseToolPanelController`1 in Creator.dll
Found: Endless.Creator.UI.UIBaseToolPanelView`1 in Creator.dll
Found: Endless.Creator.UI.UICopyToolPanelController in Creator.dll
Found: Endless.Creator.UI.UICopyToolPanelView in Creator.dll
Found: Endless.Creator.UI.UIDockableToolPanelController in Creator.dll
Found: Endless.Creator.UI.IDockableToolPanelView in Creator.dll
Found: Endless.Creator.UI.UIDockableToolPanelView`1 in Creator.dll
Found: Endless.Creator.UI.UIEraseToolPanelController in Creator.dll
Found: Endless.Creator.UI.UIEraseToolPanelView in Creator.dll
Found: Endless.Creator.UI.UIInspectorToolPanelController in Creator.dll
Found: Endless.Creator.UI.UIInspectorToolPanelView in Creator.dll
Found: Endless.Creator.UI.UIItemSelectionToolPanelController`2 in Creator.dll
Found: Endless.Creator.UI.UIItemSelectionToolPanelView`2 in Creator.dll
Found: Endless.Creator.UI.UIPaintingToolPanelController in Creator.dll
Found: Endless.Creator.UI.UIPaintingToolPanelView in Creator.dll
Found: Endless.Creator.UI.UIPropToolPanelController in Creator.dll
Found: Endless.Creator.UI.UIPropToolPanelView in Creator.dll
Found: System.Windows.Forms.ISupportToolStripPanel in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripContentPanel in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanel in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanelCell in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanelRenderEventArgs in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanelRenderEventHandler in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripContentPanelRenderEventArgs in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripContentPanelRenderEventHandler in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanelRow in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanel+ToolStripPanelRowCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanel+ToolStripPanelControlCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanelRow+ToolStripPanelRowManager in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanelRow+ToolStripPanelRowControlCollection in System.Windows.Forms.dll
Found: System.Windows.Forms.ToolStripPanelRow+ToolStripPanelRowControlCollection+ToolStripPanelCellToControlEnumerator in System.Windows.Forms.dll
Found: System.Windows.Forms.Design.ToolStripContentPanelDesigner in System.Design.dll
Found: System.Windows.Forms.Design.ToolStripPanelDesigner in System.Design.dll
Found: System.Windows.Forms.Design.Behavior.ToolStripPanelSelectionBehavior in System.Design.dll
Found: System.Windows.Forms.Design.Behavior.ToolStripPanelSelectionGlyph in System.Design.dll
