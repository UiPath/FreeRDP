call %~dp0\getvars.bat

pushd .
cd %freeRdpDir%\..

rem checkout openssl
mkdir OpenSSL
cd OpenSSL
git clone https://github.com/openssl/openssl .
git checkout OpenSSL_1_0_2u
popd