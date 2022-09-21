call %~dp0\getvars.bat
pushd .
set sln=UiPath.FreeRdpClient\UiPath.FreeRdpClient.sln
cd %freeRdpDir%

%msbuild% %sln% /t:clean /p:Configuration=Release
%msbuild% %sln% /t:clean /p:Configuration=Debug
%msbuild% %sln% /p:Configuration=Release %*
popd