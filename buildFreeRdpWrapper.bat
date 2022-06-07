pushd .
set sln=UiPath.FreeRdpClient\UiPath.FreeRdpClient.sln
cd %~dp0
RMDIR /S /Q "UiPath.FreeRdpClient\UiPath.FreeRdpWrapper\Win32"
msbuild %sln% /t:clean /p:Configuration=Debug /p:Platform=Win32
msbuild %sln% /p:Configuration=Debug /p:Platform=Win32
RMDIR /S /Q "UiPath.FreeRdpClient\UiPath.FreeRdpWrapper\Win32"
msbuild %sln% /t:clean /p:Configuration=Release /p:Platform=Win32
msbuild %sln% /p:Configuration=Release /p:Platform=Win32

RMDIR /S /Q "UiPath.FreeRdpClient\UiPath.FreeRdpWrapper\x64"
msbuild %sln% /t:clean /p:Configuration=Debug /p:Platform=x64
msbuild %sln% /p:Configuration=Debug /p:Platform=x64

RMDIR /S /Q "UiPath.FreeRdpClient\UiPath.FreeRdpWrapper\x64"
msbuild %sln% /t:clean /p:Configuration=Release /p:Platform=x64
msbuild %sln% /p:Configuration=Release /p:Platform=x64

popd