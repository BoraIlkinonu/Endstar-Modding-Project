# Counter-research: Verify Delegate.Combine execution order
# Delegate.Combine(A, B) should execute A first, then B

$first = { Write-Host "FIRST" }
$second = { Write-Host "SECOND" }

Write-Host "Test 1: Combine(first, second)"
$combined1 = [System.Delegate]::Combine($first, $second)
$combined1.Invoke()

Write-Host ""
Write-Host "Test 2: Combine(second, first)"
$combined2 = [System.Delegate]::Combine($second, $first)
$combined2.Invoke()
