pushd .
set freeRdpDir=%~dp0

cd %freeRdpDir%
git clean -xdff
cmd /c .\BuildFreeRDPx86.bat
cmd /c .\BuildFreeRDPx64.bat

rem build freerdp libs

msbuild Build\x86\FreeRDP.sln /p:Configuration=Debug /p:Platform=Win32
msbuild Build\x86\FreeRDP.sln /p:Configuration=Release /p:Platform=Win32

msbuild Build\x64\FreeRDP.sln /p:Configuration=Debug /p:Platform=x64
msbuild Build\x64\FreeRDP.sln /p:Configuration=Release /p:Platform=x64
popd