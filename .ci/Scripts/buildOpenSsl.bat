call %~dp0\getvars.bat
pushd .
cd %freeRdpDir%\..

cd OpenSSL
git clean -xdff
cmd /c %scriptsDir%\BuildOpenSSLx86.bat
git clean -xdff
cmd /c %scriptsDir%\BuildOpenSSLx64.bat
popd