pushd .
call "%~dp0\getvars.bat"

cd "%freeRdpDir%\.."

rem checkout zlib
mkdir zlib
cd zlib

git clone --no-checkout --filter=tree:0 --depth=1 --single-branch --branch=v1.3 https://github.com/madler/zlib.git .
git branch buildZlib v1.3
git checkout buildZlib

popd
