pushd .
set freeRdpScriptsDir=%~dp0
cd %freeRdpScriptsDir%\..

cd OpenSSL
git clean -xdff
cmd /c %freeRdpScriptsDir%\BuildOpenSSLx86.bat
git clean -xdff
cmd /c %freeRdpScriptsDir%\BuildOpenSSLx64.bat
popd