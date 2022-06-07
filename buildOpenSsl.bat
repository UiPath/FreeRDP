pushd .
cd %~dp0\..

cd OpenSSL
git clean -xdff
cmd /c ..\FreeRDP\BuildOpenSSLx86.bat
git clean -xdff
cmd /c ..\FreeRDP\BuildOpenSSLx64.bat
popd