## UiPath fork of FreeRDP

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
### Build instructions

#### Build OpenSSL (dependency for FreeRDP)
* Visual Studio 2022 installed in `C:\Program Files` required.  For different VS versions change the path in the build scripts.
* Install [CMake](https://cmake.org/download).  Make sure the `cmake` command is in PATH.
* Clone [OpenSSL](https://github.com/openssl/openssl) to `..\openssl`.
```
cd ..
git clone https://github.com/openssl/openssl
```
* Checkout tag `OpenSSL_1_0_2u`.
```
git checkout OpenSSL_1_0_2u
```
* Run x86 build script.  It should generate the build to `..\OpenSSL-VC-32`.
```
..\FreeRDP\BuildOpenSSLx86.bat
```
* The build system is messy and leaves intermediary x86 build files in the repo directory.  Clean these before building x64.
```
git clean -xdff
```
* Run x64 build script.  It should generate the build to `..\OpenSSL-VC-64`.
```
..\FreeRDP\BuildOpenSSLx64.bat
```

#### Build FreeRDP.
* Install [StrawberryPerl](http://strawberryperl.com).  Make sure the `perl` command is in PATH.
* Use CMake to generate Visual Studio 2022 solutions.  The cmake invocation is in the `BuildFreeRDP*.bat` files.
```
.\BuildFreeRDPx86.bat
.\BuildFreeRDPx64.bat
```
* The solutions are generated in `.\Build\x86\` and `.\Build\x64\` directories.  Build these for target Release.
>>>>>>> 3b537ad78 (Add uipath changes from previous version (2.0.0-rc3))
