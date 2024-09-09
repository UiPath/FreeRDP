## UiPath fork of FreeRDP

This repo forks the FreeRDP repo, thus allowing us to make changes that fit our needs and augment the codebase with other components.
From time to time there is a need to merge the changes from the original repo into this one.

### ❗When updating the FreeRDP library from the official repo, please update the following in this file (README.md):

|**Original repository tag/branch used:**| `2.5.0` |
| --- | --- |
|**Original corresponding commit hash:**| `d50aef95520df4216c638495a6049125c00742cb` |


<<<<<<< HEAD
[UiPath/Driver](https://github.com/UiPath/Driver) has this fork as a static library dependency.

<<<<<<< HEAD
## Code Quality Status

[![abi-checker](https://github.com/FreeRDP/FreeRDP/actions/workflows/abi-checker.yml/badge.svg)](https://github.com/FreeRDP/FreeRDP/actions/workflows/abi-checker.yml)
[![clang-tidy-review](https://github.com/FreeRDP/FreeRDP/actions/workflows/clang-tidy.yml/badge.svg?event=pull_request_target)](https://github.com/FreeRDP/FreeRDP/actions/workflows/clang-tidy.yml)
[![CodeQL](https://github.com/FreeRDP/FreeRDP/actions/workflows/codeql-analysis.yml/badge.svg?branch=master)](https://github.com/FreeRDP/FreeRDP/actions/workflows/codeql-analysis.yml)
[![mingw-builder](https://github.com/FreeRDP/FreeRDP/actions/workflows/mingw.yml/badge.svg)](https://github.com/FreeRDP/FreeRDP/actions/workflows/mingw.yml)
[![macos-builder](https://github.com/FreeRDP/FreeRDP/actions/workflows/macos.yml/badge.svg)](https://github.com/FreeRDP/FreeRDP/actions/workflows/macos.yml)
[![[arm,ppc,ricsv] architecture builds](https://github.com/FreeRDP/FreeRDP/actions/workflows/alt-architectures.yml/badge.svg)](https://github.com/FreeRDP/FreeRDP/actions/workflows/alt-architectures.yml)
[![[freebsd] architecture builds](https://github.com/FreeRDP/FreeRDP/actions/workflows/freebsd.yml/badge.svg)](https://github.com/FreeRDP/FreeRDP/actions/workflows/freebsd.yml)
[![coverity](https://scan.coverity.com/projects/616/badge.svg)](https://scan.coverity.com/projects/freerdp)

## Resources

Project website: https://www.freerdp.com/

Issue tracker: https://github.com/FreeRDP/FreeRDP/issues

Sources: https://github.com/FreeRDP/FreeRDP/

Downloads: https://pub.freerdp.com/releases/

Wiki: https://github.com/FreeRDP/FreeRDP/wiki

API documentation: https://pub.freerdp.com/api/

Security policy: https://github.com/FreeRDP/FreeRDP/security/policy

FAQ: https://github.com/FreeRDP/FreeRDP/wiki/FAQ

### Contact

* Matrix room : `#FreeRDP:matrix.org` (main)
  * ~~XMPP channel: `#FreeRDP#matrix.org@matrix.org` (bridged)~~ no longer available
  * IRC channel : `#freerdp @ irc.oftc.net` (bridged)
* Mailing list: https://lists.sourceforge.net/lists/listinfo/freerdp-devel

## Microsoft Open Specifications

Information regarding the Microsoft Open Specifications can be found at:
https://www.microsoft.com/openspecifications/

A list of reference documentation is maintained here:
https://github.com/FreeRDP/FreeRDP/wiki/Reference-Documentation

## Compilation

Instructions on how to get started compiling FreeRDP can be found on the wiki:
https://github.com/FreeRDP/FreeRDP/wiki/Compilation
=======
=======
>>>>>>> 159ff5b60 (freerdp client for Feature/mr session  (#1))
### Build instructions
* Visual Studio 2022 installed in `C:\Program Files` required.

* Install [StrawberryPerl](http://strawberryperl.com).  Make sure the `perl` command is in PATH.
  You may try on newer Windows 10:
```
    winget install -e --id StrawberryPerl.StrawberryPerl
```
<<<<<<< HEAD
* The solutions are generated in `.\Build\x86\` and `.\Build\x64\` directories.  Build these for target Release.
>>>>>>> 3b537ad78 (Add uipath changes from previous version (2.0.0-rc3))
=======

#### Build FreeRDP and Build OpenSSL (dependency for FreeRDP)

> Use a developer console for VS 2022 instead of normal PowerShell or CMD, the commands require `nmake`

* Steps
  * Clone [OpenSSL](https://github.com/openssl/openssl) to `..\openssl` && Checkout tag `OpenSSL_1_0_2u` (getOpenSsl)
  * Generate OpenSSL build to `..\OpenSSL-VC-64`.  (buildOpenSsl)
  * Use CMake to generate and then build Visual Studio 2022 solutions.  (BuildFreeRDP)
  * The freerdp solution is generated in `.\Build\x64\` directories.

* Scripts
  * Simple
    ```
    cd .ci/Scripts
    .\PrepareFreeRdpDev
    ```
  * or detailed
    ```
    cd .ci/Scripts
    .\getOpenSsl
    .\buildOpenSsl
    .\buildFreeRDP Debug
    ```

### Work on the FreeRdpClient
* Open [UiPath.FreeRdpClient/UiPath.FreeRdpClient.sln](file://UiPath.FreeRdpClient/UiPath.FreeRdpClient.sln)
* To test with a nugetRef instead of projectRef edit the [UiPath.FreeRdp.Tests.csproj](file://UiPath.FreeRdpClient/UiPath.FreeRdpClient.Tests/UiPath.FreeRdp.Tests.csproj)
search for: `<When Condition="'$(UseNugetRef)'!=''">`
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
>>>>>>> 159ff5b60 (freerdp client for Feature/mr session  (#1))
=======

### Our current baseline from the original FreeRDP repo

Check this [wiki page](./UiPath.FreeRdpClient/README.md)
>>>>>>> 805227adf (- author changes in the root README.md so that contributors are aware about the need for manually recording the current TAG and HASH from the offical FreeRDP repo)
=======
>>>>>>> e27f7dd78 (SessionTools: Fix flaky tests [ROBO-3137] (#21))
=======

#### Running unit tests

* Run Visual Studio as Admin
* Make sure RDP is enabled on local machine - `View advanced system settings > Remote tab > Allow remote connections to this computer`
>>>>>>> 0d6a9bac7 (Add optional callback that's called when FreeRDP disconnects ROBO-4003 (#39))
