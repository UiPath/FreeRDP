call "%~dp0\getvars.bat"
pushd .
call "%~dp0\install_msvc"

cd "%freeRdpDir%\..\OpenSSL"
git clean -xdff

perl Configure VC-WIN64A no-asm no-shared no-module --prefix="%~dp0\..\..\..\OpenSSL-VC-64"
nmake
nmake install_dev
popd