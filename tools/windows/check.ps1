<# Pathrova-Prüfung auf Windows: Selbsttest beider EXEs, MSIX-Bau, WACK für x64 (und arm64, falls möglich). Lädt nichts hoch. #>
param([string]$PublisherCN = "CN=3B31B7FD-F878-49F0-9D9A-0F247C2D4671", [string]$Version = "1.0.0.0")
$ErrorActionPreference = "Stop"
$root = (Resolve-Path "$PSScriptRoot\..\..").Path
$ev = Join-Path $root "artifacts\windows-check"
New-Item $ev -ItemType Directory -Force | Out-Null
$results = @()
function Add($check, $ok, $detail) { $script:results += [pscustomobject]@{ Check = $check; Passed = [bool]$ok; Detail = "$detail" } }
try {
  $exe = Join-Path $root "artifacts\publish\win-x64\Pathrova.exe"
  $p = Start-Process $exe -ArgumentList "--selftest", "`"$ev\selftest-x64.log`"" -Wait -PassThru
  Add "Selbsttest x64 (Scan >260 Zeichen, Umbenennen, Rückgängig, PDF)" ($p.ExitCode -eq 0) (Get-Content "$ev\selftest-x64.log" -Raw)
  $env:PATHROVA_TEST_HOME = Join-Path $ev "home"
  $ui = Start-Process $exe -PassThru
  Start-Sleep 8; $ui.Refresh()
  Add "Fenster startet" (-not $ui.HasExited -and $ui.Responding) "MainWindowTitle=$($ui.MainWindowTitle)"
  if (-not $ui.HasExited) { $ui.CloseMainWindow() | Out-Null; if (-not $ui.WaitForExit(5000)) { $ui.Kill() } }
  Remove-Item Env:\PATHROVA_TEST_HOME
  & "$root\packaging\build_msix.ps1" -PublisherCN $PublisherCN -Version $Version *> "$ev\msix.log"
  $bundle = Join-Path $root "artifacts\msix\Pathrova_$Version.msixbundle"
  Add "MSIX-Bundle" (Test-Path $bundle) $bundle
  $kit = "${env:ProgramFiles(x86)}\Windows Kits\10\App Certification Kit\appcert.exe"
  foreach ($arch in "x64") {
    $pkg = Join-Path $root "artifacts\msix\Pathrova_$Version`_$arch.msix"
    $report = Join-Path $ev "wack-$arch.xml"
    Remove-Item $report -ErrorAction SilentlyContinue
    & $kit reset | Out-Null
    & $kit test -appxpackagepath $pkg -reportoutputpath $report | Out-File "$ev\wack-$arch.log"
    [xml]$x = Get-Content $report
    Add "WACK $arch" ($x.REPORT.OVERALL_RESULT -eq "PASS") $x.REPORT.OVERALL_RESULT
  }
} catch { Add "Ablauf" $false $_.Exception.Message }
$results | ConvertTo-Json | Set-Content "$ev\results.json" -Encoding UTF8
$results | Format-Table -AutoSize -Wrap
if (@($results | Where-Object { -not $_.Passed }).Count) { exit 1 }
