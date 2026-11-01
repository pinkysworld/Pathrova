<#
  Pathrova: baut aus artifacts\publish\win-x64 und win-arm64 je ein MSIX und ein .msixbundle (unsigniert; Microsoft signiert beim Einreichen).
  Voraussetzung: Windows SDK (makeappx.exe, makepri.exe).
  .\packaging\build_msix.ps1 -PublisherCN "CN=..." [-IdentityName MinhStudios.Pathrova] [-Version 1.0.0.0] [-Test]
  -Test signiert zusätzlich eine lokale Testkopie mit einem selbst erstellten Zertifikat (nicht hochladen).
#>
param(
  [string]$IdentityName = "MinhStudios.Pathrova",
  [Parameter(Mandatory = $true)][string]$PublisherCN,
  [string]$PublisherDisplayName = "Minh Studios",
  [string]$Version = "1.0.0.0",
  [switch]$Test
)
$ErrorActionPreference = "Stop"
if ($Version -notmatch '^\d+\.\d+\.\d+\.0$') { throw "Version muss a.b.c.0 sein." }
$repo = (Resolve-Path "$PSScriptRoot\..").Path
$work = Join-Path $env:TEMP "pathrova-msix"
$out = Join-Path $repo "artifacts\msix"
Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
New-Item $work, $out -ItemType Directory -Force | Out-Null
$tool = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\makeappx.exe" | Sort-Object FullName | Select-Object -Last 1
if (-not $tool) { throw "makeappx.exe nicht gefunden – Windows SDK installieren." }
$bin = $tool.DirectoryName
$bundleDir = Join-Path $work "bundle"
New-Item $bundleDir -ItemType Directory -Force | Out-Null
foreach ($arch in "x64", "arm64") {
  $src = Join-Path $repo "artifacts\publish\win-$arch"
  if (-not (Test-Path "$src\Pathrova.exe")) { throw "Fehlt: $src\Pathrova.exe (auf dem Mac packaging/publish.sh ausführen)" }
  $stage = Join-Path $work "stage-$arch"
  Copy-Item $src $stage -Recurse
  Copy-Item "$PSScriptRoot\Assets" "$stage\Assets" -Recurse
  $m = Get-Content "$PSScriptRoot\AppxManifest.xml" -Raw -Encoding UTF8
  $map = @{ "{{IDENTITY_NAME}}" = $IdentityName; "{{PUBLISHER_CN}}" = $PublisherCN; "{{PUBLISHER_DISPLAY_NAME}}" = $PublisherDisplayName; "{{VERSION}}" = $Version; "{{ARCH}}" = $arch }
  foreach ($k in $map.Keys) { $m = $m.Replace($k, [System.Security.SecurityElement]::Escape([string]$map[$k])) }
  if ($m.Contains("{{")) { throw "Platzhalter im Manifest übrig." }
  [xml]$null = $m
  [IO.File]::WriteAllText("$stage\AppxManifest.xml", $m, (New-Object Text.UTF8Encoding $false))
  & "$bin\makepri.exe" createconfig /cf "$stage\priconfig.xml" /dq de-DE_en-US /o | Out-Null
  if ($LASTEXITCODE) { throw "makepri createconfig ($arch)" }
  & "$bin\makepri.exe" new /pr $stage /cf "$stage\priconfig.xml" /of "$stage\resources.pri" /o | Out-Null
  if ($LASTEXITCODE) { throw "makepri new ($arch)" }
  Remove-Item "$stage\priconfig.xml"
  & "$bin\makeappx.exe" pack /d $stage /p (Join-Path $bundleDir "Pathrova_$Version`_$arch.msix") /o | Out-Null
  if ($LASTEXITCODE) { throw "makeappx pack ($arch)" }
}
$bundle = Join-Path $out "Pathrova_$Version.msixbundle"
& "$bin\makeappx.exe" bundle /d $bundleDir /p $bundle /bv $Version /o | Out-Null
if ($LASTEXITCODE) { throw "makeappx bundle" }
Copy-Item "$bundleDir\*.msix" $out -Force
Write-Host "Fertig: $bundle (unsigniert, für das Partner Center)"
if ($Test) {
  $cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object Subject -eq $PublisherCN | Select-Object -First 1
  if (-not $cert) { $cert = New-SelfSignedCertificate -Type Custom -Subject $PublisherCN -KeyUsage DigitalSignature -FriendlyName "Pathrova Test" -CertStoreLocation Cert:\CurrentUser\My -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") }
  $testBundle = Join-Path $out "Pathrova_$Version`_TEST.msixbundle"
  Copy-Item $bundle $testBundle -Force
  & (Join-Path $bin "signtool.exe") sign /fd SHA256 /sha1 $cert.Thumbprint $testBundle
  if ($LASTEXITCODE) { throw "Testsignatur fehlgeschlagen" }
  Write-Host "Testpaket: $testBundle (nur lokal, nicht hochladen)"
}
