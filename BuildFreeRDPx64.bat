cmake . -B"./Build/x64" -G"Visual Studio 15 2017 Win64"^
	-DOPENSSL_ROOT_DIR="../OpenSSL-VC-64"^
	-DCMAKE_INSTALL_PREFIX="./Install/x64"^
	-DMSVC_RUNTIME="static"^
	-DBUILD_SHARED_LIBS=OFF^
	-DWITH_CLIENT_INTERFACE=ON^
	-DBUILTIN_CHANNELS=OFF^
	-DWITH_CHANNELS=OFF^
	-DWITH_MEDIA_FOUNDATION=OFF
pause
