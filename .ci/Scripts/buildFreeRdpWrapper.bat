call %~dp0\getvars.bat
set sln=UiPath.FreeRdpClient\UiPath.FreeRdpClient.sln
set nugetProj=UiPath.FreeRdpClient\UiPath.FreeRdpClient\UiPath.FreeRdpClient.csproj
set testsProj=UiPath.FreeRdpClient\UiPath.FreeRdpClient.Tests\UiPath.FreeRdp.Tests.csproj

pushd .
cd %freeRdpDir%

%msbuild% %sln% /t:clean /p:Configuration=Release
%msbuild% %sln% /t:clean /p:Configuration=Debug

echo ============= building nuget ===============

%msbuild% %nugetProj% /t:restore /p:Configuration=Release %*
%msbuild% %nugetProj% /t:build /p:Configuration=Release %*
echo ============= done build nuget ===============

echo ============= building tests ===============
%msbuild% %nugetProj% /t:restore /p:Configuration=Release %*
%msbuild% %testsProj% /t:build /p:Configuration=Release %*
echo ============= done build tests ===============
popd