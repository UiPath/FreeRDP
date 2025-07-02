pushd .
call "%~dp0\getvars.bat"

cd "%freeRdpDir%\.."

rem checkout zlib
mkdir zlib
cd zlib

git clone --no-checkout --filter=tree:0 --depth=1 --single-branch --branch=v1.3 https://github.com/madler/zlib.git .
git branch buildZlib v1.3
git checkout buildZlib

@REM rem build zlib
@REM cmake -GNinja -B "./Build" ^
@REM     -DCMAKE_BUILD_TYPE=Release ^
@REM     -DCMAKE_SKIP_INSTALL_ALL_DEPENDENCY=ON ^
@REM     -DCMAKE_INSTALL_PREFIX="./Install" ^
@REM     -DLIBRESSL_APPS=OFF ^
@REM     -DLIBRESSL_TESTS=OFF

@REM cmake --build "./Build"
@REM cmake --install "./Build"

popd