call "%~dp0\getvars.bat"
pushd .
cd %freeRdpDir%
git clean -xdff

rem cmd /c "%scriptsDir%\BuildFreeRDPx86.bat"
cmd /c "%scriptsDir%\BuildFreeRDPx64.bat"

rem build freerdp libs

rem %msbuild% "%buildDir%\x86\FreeRDP.sln" /p:Configuration=Debug /p:Platform=Win32
rem %msbuild% "%buildDir%\x86\FreeRDP.sln" /p:Configuration=Release /p:Platform=Win32

%msbuild% "%buildDir%\x64\FreeRDP.sln" /p:Configuration=Debug /p:Platform=x64
%msbuild% "%buildDir%\x64\FreeRDP.sln" /p:Configuration=Release /p:Platform=x64
popd