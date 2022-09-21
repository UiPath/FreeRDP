call %~dp0\getvars.bat
pushd .
set testProj="UiPath.FreeRdpClient\UiPath.FreeRdpClient.Tests\UiPath.FreeRdp.Tests.csproj"
cd %freeRdpDir%

dotnet test %testProj% --no-build --logger trx --results-directory "%freeRdpDir%\..\_temp" /p:Configuration=Release %*
popd