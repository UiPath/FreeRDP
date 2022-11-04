## UiPath fork of FreeRDP

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

#### 
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

* Steps
** Clone [OpenSSL](https://github.com/openssl/openssl) to `..\openssl` && Checkout tag `OpenSSL_1_0_2u` (getOpenSsl)
** Generate OpenSSL build to `..\OpenSSL-VC-64`.  (buildOpenSsl)
** Use CMake to generate and then build Visual Studio 2022 solutions.  (BuildFreeRDP)
** The freerdp solution is generated in `.\Build\x64\` directories.

* Scripts
** Simple
```
cd .ci/Scripts
.\PrepareFreeRdpDev
```
** or detailed
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
>>>>>>> 159ff5b60 (freerdp client for Feature/mr session  (#1))
