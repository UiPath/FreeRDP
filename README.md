## UiPath fork of FreeRDP

[UiPath/Driver](https://github.com/UiPath/Driver) has this fork as a static library dependency.

### Build instructions

#### Build OpenSSL (dependency for FreeRDP)
* Visual Studio 2022 installed in `C:\Program Files` required.  
* For older VS versions you may need to install [CMake](https://cmake.org/download) and make sure the `cmake` command is in PATH.
* Clone [OpenSSL](https://github.com/openssl/openssl) to `..\openssl` && Checkout tag `OpenSSL_1_0_2u` && Generate OpenSSL build to `..\OpenSSL-VC-64`.
```
cd .ci/Scripts
.\getOpenSsl
.\buildOpenSsl
```

#### Build FreeRDP.
* Install [StrawberryPerl](http://strawberryperl.com).  Make sure the `perl` command is in PATH.
  You may try on newer Windows 10:
```
winget install -e --id StrawberryPerl.StrawberryPerl
```
* Use CMake to generate and then build Visual Studio 2022 solutions.  The cmake invocation is in the `BuildFreeRDPx*.bat` files. 
```
.\buildFreeRDP
```
* The freerdp solution is generated in `.\Build\x64\` directories.

### Work on the FreeRdpClient
* Open [UiPath.FreeRdpClient/UiPath.FreeRdpClient.sln](file://UiPath.FreeRdpClient/UiPath.FreeRdpClient.sln)
* To test with a nugetRef instead of projectRef edit the [UiPath.FreeRdp.Tests.csproj](file://UiPath.FreeRdpClient/UiPath.FreeRdpClient.Tests/UiPath.FreeRdp.Tests.csproj)
search for: `<When Condition="'$(UseNugetRef)'!=''">`
