# Static Analysis Results
Generated: 2026-01-09 16:23:55

## Summary

This document contains the reflection-based analysis of Endstar's prop system.
For full method implementations, use ILSpy/dnSpy to decompile the DLLs.

## Key Findings

### Critical Data Flow Path
`
PropLibrary.loadedPropMap
    |
    v
PropLibrary._referenceFilterMap (populated by PopulateReferenceFilterMap)
    |
    v
PropLibrary.GetReferenceFilteredDefinitionList(filter)
    |
    v
UIRuntimePropInfoListModel.Synchronize(filter, ignore)
    |
    v
UIRuntimePropInfoListModel internal List
    |
    v
UI Display
`

### Next Steps

1. **Decompile these methods in ILSpy/dnSpy:**
   - UIRuntimePropInfoListModel.Synchronize()
   - PropLibrary.GetReferenceFilteredDefinitionList()
   - PropLibrary.PopulateReferenceFilterMap()
   - UIPropToolPanelView.OnLibraryRepopulated()

2. **Document the exact logic of each method**

3. **Identify where our injected prop gets filtered out**

