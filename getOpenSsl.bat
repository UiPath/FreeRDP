pushd .
set freeRdpDir=%~dp0
cd %freeRdpDir%\..

rem checkout and build openssl libs
mkdir OpenSSL
cd OpenSSL
git clone https://github.com/openssl/openssl .
git checkout OpenSSL_1_0_2u
popd