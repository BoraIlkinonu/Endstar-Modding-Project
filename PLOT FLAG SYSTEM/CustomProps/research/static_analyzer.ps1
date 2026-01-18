# Endstar Prop System Static Analyzer
# Purpose: Extract method signatures, fields, and relationships from game DLLs
# Usage: Run in PowerShell with admin rights if needed

$ErrorActionPreference = "Continue"
$managedPath = "C:\Endless Studios\Endless Launcher\Endstar\Endstar_Data\Managed"
$outputPath = "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\CustomProps\research"

Write-Host "=== ENDSTAR PROP SYSTEM STATIC ANALYZER ===" -ForegroundColor Cyan
Write-Host "Managed DLL Path: $managedPath"
Write-Host "Output Path: $outputPath"
Write-Host ""

# Helper function to get binding flags for all members
function Get-AllBindingFlags {
    return [System.Reflection.BindingFlags]::Public -bor
           [System.Reflection.BindingFlags]::NonPublic -bor
           [System.Reflection.BindingFlags]::Instance -bor
           [System.Reflection.BindingFlags]::Static
}

# Helper function to safely get types from assembly (handles partial load failures)
function Get-SafeTypes($assembly) {
    try {
        return $assembly.GetTypes()
    } catch [System.Reflection.ReflectionTypeLoadException] {
        Write-Host "  (Some types failed to load, using partial list)" -ForegroundColor Yellow
        return $_.Exception.Types | Where-Object { $_ -ne $null }
    }
}

# Helper function to format method parameters
function Format-Parameters($method) {
    $params = $method.GetParameters()
    $paramStrings = @()
    foreach ($p in $params) {
        $paramStrings += "$($p.ParameterType.Name) $($p.Name)"
    }
    return $paramStrings -join ", "
}

# Helper function to format method signature
function Format-MethodSignature($method) {
    $returnType = $method.ReturnType.Name
    $name = $method.Name
    $params = Format-Parameters $method
    $modifiers = @()
    if ($method.IsStatic) { $modifiers += "static" }
    if ($method.IsVirtual) { $modifiers += "virtual" }
    if ($method.IsAbstract) { $modifiers += "abstract" }
    $modStr = if ($modifiers.Count -gt 0) { ($modifiers -join " ") + " " } else { "" }
    return "$modStr$returnType $name($params)"
}

# Load assemblies
Write-Host "Loading assemblies..." -ForegroundColor Yellow
$assemblies = @{}

$dllsToLoad = @(
    "Gameplay.dll",
    "Creator.dll",
    "Props.dll",
    "Shared.dll",
    "Data.dll",
    "Assets.dll"
)

foreach ($dll in $dllsToLoad) {
    $path = Join-Path $managedPath $dll
    if (Test-Path $path) {
        try {
            $assemblies[$dll] = [System.Reflection.Assembly]::LoadFrom($path)
            Write-Host "  Loaded: $dll" -ForegroundColor Green
        } catch {
            Write-Host "  Failed to load: $dll - $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "  Not found: $dll" -ForegroundColor Red
    }
}

Write-Host ""

# ============================================================================
# ANALYSIS 1: UIRuntimePropInfoListModel.Synchronize
# ============================================================================
Write-Host "=== ANALYSIS 1: UIRuntimePropInfoListModel ===" -ForegroundColor Cyan

$creatorAsm = $assemblies["Creator.dll"]
if ($creatorAsm) {
    $listModelType = (Get-SafeTypes $creatorAsm) | Where-Object { $_.Name -eq "UIRuntimePropInfoListModel" }

    if ($listModelType) {
        Write-Host "Found: $($listModelType.FullName)" -ForegroundColor Green

        # Get Synchronize method(s)
        $syncMethods = $listModelType.GetMethods((Get-AllBindingFlags)) | Where-Object { $_.Name -eq "Synchronize" }

        Write-Host "`nSynchronize Methods:" -ForegroundColor Yellow
        foreach ($m in $syncMethods) {
            Write-Host "  $(Format-MethodSignature $m)"

            # Get parameter details
            foreach ($p in $m.GetParameters()) {
                Write-Host "    Param: $($p.ParameterType.FullName) $($p.Name)"
            }
        }

        # Get all fields
        Write-Host "`nFields:" -ForegroundColor Yellow
        $fields = $listModelType.GetFields((Get-AllBindingFlags))
        foreach ($f in $fields) {
            Write-Host "  $($f.FieldType.Name) $($f.Name)"
        }

        # Get base type chain
        Write-Host "`nInheritance Chain:" -ForegroundColor Yellow
        $current = $listModelType
        while ($current -ne $null) {
            Write-Host "  $($current.FullName)"
            $current = $current.BaseType
        }

        # Look for Add method
        $addMethods = $listModelType.GetMethods((Get-AllBindingFlags)) | Where-Object { $_.Name -eq "Add" }
        Write-Host "`nAdd Methods:" -ForegroundColor Yellow
        foreach ($m in $addMethods) {
            Write-Host "  $(Format-MethodSignature $m)"
        }
    } else {
        Write-Host "UIRuntimePropInfoListModel not found" -ForegroundColor Red
    }
}

# ============================================================================
# ANALYSIS 2: UIPropToolPanelView
# ============================================================================
Write-Host "`n=== ANALYSIS 2: UIPropToolPanelView ===" -ForegroundColor Cyan

if ($creatorAsm) {
    $panelType = (Get-SafeTypes $creatorAsm) | Where-Object { $_.Name -eq "UIPropToolPanelView" }

    if ($panelType) {
        Write-Host "Found: $($panelType.FullName)" -ForegroundColor Green

        # Find OnLibraryRepopulated and related methods
        $relevantMethods = $panelType.GetMethods((Get-AllBindingFlags)) |
            Where-Object { $_.Name -match "Library|Repopulate|Synchronize|Refresh|Update|Init" }

        Write-Host "`nRelevant Methods:" -ForegroundColor Yellow
        foreach ($m in $relevantMethods) {
            Write-Host "  $(Format-MethodSignature $m)"
        }

        # Find runtimePropInfoListModel field
        $listModelField = $panelType.GetFields((Get-AllBindingFlags)) |
            Where-Object { $_.Name -match "listModel|List|prop" }

        Write-Host "`nList-Related Fields:" -ForegroundColor Yellow
        foreach ($f in $listModelField) {
            Write-Host "  $($f.FieldType.Name) $($f.Name)"
        }
    }
}

# ============================================================================
# ANALYSIS 3: PropLibrary
# ============================================================================
Write-Host "`n=== ANALYSIS 3: PropLibrary ===" -ForegroundColor Cyan

$gameplayAsm = $assemblies["Gameplay.dll"]
if ($gameplayAsm) {
    $propLibType = (Get-SafeTypes $gameplayAsm) | Where-Object { $_.Name -eq "PropLibrary" }

    if ($propLibType) {
        Write-Host "Found: $($propLibType.FullName)" -ForegroundColor Green

        # Find critical methods
        $criticalMethods = @(
            "GetReferenceFilteredDefinitionList",
            "PopulateReferenceFilterMap",
            "GetAllRuntimeProps",
            "InjectProp",
            "TryGetRuntimePropInfo"
        )

        Write-Host "`nCritical Methods:" -ForegroundColor Yellow
        foreach ($methodName in $criticalMethods) {
            $methods = $propLibType.GetMethods((Get-AllBindingFlags)) | Where-Object { $_.Name -eq $methodName }
            foreach ($m in $methods) {
                Write-Host "  $(Format-MethodSignature $m)"
            }
        }

        # Find filter-related fields
        $filterFields = $propLibType.GetFields((Get-AllBindingFlags)) |
            Where-Object { $_.Name -match "filter|Filter|map|Map|loaded|Loaded" }

        Write-Host "`nStorage Fields:" -ForegroundColor Yellow
        foreach ($f in $filterFields) {
            Write-Host "  $($f.FieldType.FullName) $($f.Name)"
        }

        # Find nested types (RuntimePropInfo)
        $nestedTypes = $propLibType.GetNestedTypes((Get-AllBindingFlags))
        Write-Host "`nNested Types:" -ForegroundColor Yellow
        foreach ($t in $nestedTypes) {
            Write-Host "  $($t.Name)"
            $fields = $t.GetFields((Get-AllBindingFlags))
            foreach ($f in $fields) {
                Write-Host "    $($f.FieldType.Name) $($f.Name)"
            }
        }
    }
}

# ============================================================================
# ANALYSIS 4: CreatorManager Events
# ============================================================================
Write-Host "`n=== ANALYSIS 4: CreatorManager Events ===" -ForegroundColor Cyan

if ($creatorAsm) {
    $creatorMgrType = (Get-SafeTypes $creatorAsm) | Where-Object { $_.Name -eq "CreatorManager" }

    if ($creatorMgrType) {
        Write-Host "Found: $($creatorMgrType.FullName)" -ForegroundColor Green

        # Find event fields
        $eventFields = $creatorMgrType.GetFields((Get-AllBindingFlags)) |
            Where-Object { $_.Name -match "On|Event|Action|Repopulate|Library" }

        Write-Host "`nEvent Fields:" -ForegroundColor Yellow
        foreach ($f in $eventFields) {
            Write-Host "  $($f.FieldType.Name) $($f.Name)"
        }

        # Find actual events
        $events = $creatorMgrType.GetEvents((Get-AllBindingFlags))
        Write-Host "`nDeclared Events:" -ForegroundColor Yellow
        foreach ($e in $events) {
            Write-Host "  $($e.EventHandlerType.Name) $($e.Name)"
        }
    }
}

# ============================================================================
# ANALYSIS 5: ReferenceFilter Enum
# ============================================================================
Write-Host "`n=== ANALYSIS 5: ReferenceFilter Enum ===" -ForegroundColor Cyan

$sharedAsm = $assemblies["Shared.dll"]
if ($sharedAsm) {
    $filterType = (Get-SafeTypes $sharedAsm) | Where-Object { $_.Name -eq "ReferenceFilter" }

    if ($filterType) {
        Write-Host "Found: $($filterType.FullName)" -ForegroundColor Green
        Write-Host "Values:" -ForegroundColor Yellow

        $values = [System.Enum]::GetValues($filterType)
        foreach ($v in $values) {
            Write-Host "  $v = $([int]$v)"
        }
    }
}

# ============================================================================
# ANALYSIS 6: StageManager
# ============================================================================
Write-Host "`n=== ANALYSIS 6: StageManager ===" -ForegroundColor Cyan

if ($gameplayAsm) {
    $stageMgrType = (Get-SafeTypes $gameplayAsm) | Where-Object { $_.Name -eq "StageManager" }

    if ($stageMgrType) {
        Write-Host "Found: $($stageMgrType.FullName)" -ForegroundColor Green

        # Find Instance property
        $instanceProp = $stageMgrType.GetProperty("Instance", (Get-AllBindingFlags))
        if ($instanceProp) {
            Write-Host "`nSingleton: $($instanceProp.PropertyType.Name) Instance" -ForegroundColor Yellow
        }

        # Find activePropLibrary
        $propLibField = $stageMgrType.GetFields((Get-AllBindingFlags)) |
            Where-Object { $_.Name -match "propLibrary|PropLibrary|activeProp" }

        Write-Host "`nPropLibrary Fields:" -ForegroundColor Yellow
        foreach ($f in $propLibField) {
            Write-Host "  $($f.FieldType.Name) $($f.Name)"
        }

        # Find InjectProp methods
        $injectMethods = $stageMgrType.GetMethods((Get-AllBindingFlags)) | Where-Object { $_.Name -eq "InjectProp" }
        Write-Host "`nInjectProp Methods:" -ForegroundColor Yellow
        foreach ($m in $injectMethods) {
            Write-Host "  $(Format-MethodSignature $m)"
        }
    }
}

# ============================================================================
# OUTPUT SUMMARY TO FILE
# ============================================================================
Write-Host "`n=== SAVING RESULTS ===" -ForegroundColor Cyan

$outputFile = Join-Path $outputPath "STATIC_ANALYSIS_RESULTS.md"
$output = @"
# Static Analysis Results
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Summary

This document contains the reflection-based analysis of Endstar's prop system.
For full method implementations, use ILSpy/dnSpy to decompile the DLLs.

## Key Findings

### Critical Data Flow Path
```
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
```

### Next Steps

1. **Decompile these methods in ILSpy/dnSpy:**
   - UIRuntimePropInfoListModel.Synchronize()
   - PropLibrary.GetReferenceFilteredDefinitionList()
   - PropLibrary.PopulateReferenceFilterMap()
   - UIPropToolPanelView.OnLibraryRepopulated()

2. **Document the exact logic of each method**

3. **Identify where our injected prop gets filtered out**

"@

$output | Out-File -FilePath $outputFile -Encoding UTF8
Write-Host "Results saved to: $outputFile" -ForegroundColor Green

Write-Host "`n=== ANALYSIS COMPLETE ===" -ForegroundColor Cyan
Write-Host "Run this script again after reviewing to capture any additional types."
