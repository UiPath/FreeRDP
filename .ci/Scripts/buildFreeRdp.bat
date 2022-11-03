call "%~dp0\getvars.bat"
pushd .
cd %freeRdpDir%
git clean -xdff

cmake . -B"./Build/x64" -G"Visual Studio 17 2022"^
	-A x64^
	-DOPENSSL_ROOT_DIR="../OpenSSL-VC-64"^
	-DCMAKE_INSTALL_PREFIX="./Install/x64"^
	-DMSVC_RUNTIME="static"^
	-DBUILD_SHARED_LIBS=OFF^
	-DWITH_CLIENT_INTERFACE=ON^
	-DBUILTIN_CHANNELS=OFF^
	-DWITH_CHANNELS=OFF^
	-DWITH_MEDIA_FOUNDATION=OFF^

rem build freerdp libs

%msbuild% "%buildDir%\x64\FreeRDP.sln" /p:Configuration=Debug /p:Platform=x64
%msbuild% "%buildDir%\x64\FreeRDP.sln" /p:Configuration=Release /p:Platform=x64
popd