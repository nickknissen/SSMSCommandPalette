# Releasing

No GitHub Actions release pipeline is set up yet for this repo. Once one
is added it will mirror the workflows used by
[TablePlusCommandPalette](https://github.com/nickknissen/TablePlusCommandPalette/blob/main/.github/workflows/release.yml)
and
[TailscaleCommandPalette](https://github.com/nickknissen/TailscaleCommandPalette/blob/main/.github/workflows/release.yml):
build signed x64 + ARM64 MSIX, bundle into `.msixbundle`, attach to a
GitHub Release, and dispatch a `wingetcreate` PR to `microsoft/winget-pkgs`.
