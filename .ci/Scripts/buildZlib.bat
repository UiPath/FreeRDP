pushd .
call "%~dp0\getvars.bat"

cd %zlibDir%

cmake -GNinja -B "./Build" ^
    -DCMAKE_BUILD_TYPE=Release ^
    -DCMAKE_SKIP_INSTALL_ALL_DEPENDENCY=ON ^
    -DCMAKE_INSTALL_PREFIX="./Install" ^
    -DLIBRESSL_APPS=OFF ^
    -DLIBRESSL_TESTS=OFF

cmake --build "./Build"
cmake --install "./Build"

popd