call %~dp0\getvars.bat
cd %freeRdpDir%\..\OpenSSL
git clean -xdff

set "START_DIR=%CD%"
call "%vsDir%\VC\Auxiliary\Build\vcvars32.bat"
cd /D "%START_DIR%"


perl Configure VC-WIN32 no-asm --prefix=..\OpenSSL-VC-32
call ms\do_ms
nmake -f ms\nt.mak
nmake -f ms\nt.mak install
