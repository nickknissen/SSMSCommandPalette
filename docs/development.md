# Development

## Local build for testing

```powershell
.\SSMSCommandPalette\build-msix.ps1 -Version 1.0.0 -Platforms @('x64','arm64') -Bundle
```

Produces `SSMSCommandPalette\bin\Release\msix\SSMSCommandPalette_1.0.0.0.msixbundle`.
With no `-CertPath` / `-CertBase64`, the package is left unsigned.

For a single-architecture build during development:

```powershell
.\SSMSCommandPalette\build-msix.ps1 -Version 1.0.0 -Platforms x64
```

## Sign locally

The shared CmdPal signing cert lives in 1Password under
`Private/CmdPal Signing Cert`. The same cert is used by
[TablePlusCommandPalette](https://github.com/nickknissen/TablePlusCommandPalette),
[TailscaleCommandPalette](https://github.com/nickknissen/TailscaleCommandPalette),
and
[SSMSCommandPalette](https://github.com/nickknissen/SSMSCommandPalette).

```powershell
.\scripts\sign-local.ps1 -Path .\SSMSCommandPalette\bin\Release\msix\*.msix*
```

## Install / uninstall

```powershell
Add-AppxPackage .\SSMSCommandPalette\bin\Release\msix\SSMSCommandPalette_1.0.0.0_x64.msix

# Remove every installed copy (sideloaded, dev-registered):
.\scripts\uninstall.ps1
```

## Demo mode

Pass `-Demo` to `build-msix.ps1` to compile with the `DEMO_MODE` define so
the extension surfaces hard-coded demo connections instead of reading the
local SSMS settings file (used for Microsoft Store screenshots so real
server names aren't leaked).

```powershell
.\SSMSCommandPalette\build-msix.ps1 -Version 1.0.0 -Platforms x64 -Demo
```
