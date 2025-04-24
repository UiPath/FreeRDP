call "%~dp0\getvars.bat"
set vs_installer="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vs_installer.exe"
%vs_installer% modify --installPath "%vsdir%" --add Microsoft.VisualStudio.Component.VC.%vc_ver%.x86.x64 Microsoft.VisualStudio.Component.VC.%vc_ver%.ATL --includeRecommended --quiet --installWhileDownloading
 
