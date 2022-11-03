call "%~dp0\getvars.bat"
pushd .
cd %freeRdpDir%
git clean -xdff

cmd /c "%scriptsDir%\BuildFreeRDPx64.bat"

rem build freerdp libs

%msbuild% "%buildDir%\x64\FreeRDP.sln" /p:Configuration=Debug /p:Platform=x64
%msbuild% "%buildDir%\x64\FreeRDP.sln" /p:Configuration=Release /p:Platform=x64
popd