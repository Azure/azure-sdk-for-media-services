@echo off

echo Cleaning signed and packages directories
del /q c:\signing\signed\*.*
del /q c:\packages\*.*

echo Copying managed desktop library DLLs to signing source directory
copy /y .\Publish\Build\Release\Microsoft.WindowsAzure.MediaServices.Client.dll c:\signing\tosign\
if %ERRORLEVEL% neq 0 goto copyfailed
echo OK

echo Signing managed desktop library DLLs...
%CI_SIGNING%\CodeSignUtility\csu.exe /c1=72 /c2=10006 "/d=.NET SDK" "/kw=MediaServices"
if %ERRORLEVEL% neq 0 goto signfailed
echo OK

echo Removing all unsigned files from \\adksdksign\unsigned...
del /q c:\signing\tosign\*.*
echo OK

echo Copying Media SDK signed managed DLLs and the pdbs to the final drop location...
echo Creating \drop\WAMSSDK\lib\net45
md .\drop\WAMSSDK\lib\net45
echo Copy MediaServices.Client.dll
copy /y c:\signing\signed\Microsoft.WindowsAzure.MediaServices.Client.dll .\drop\WAMSSDK\lib\net45\
echo Copy MediaServices.Client.pdb
copy /y .\Publish\Build\Release\Microsoft.WindowsAzure.MediaServices.Client.pdb .\drop\WAMSSDK\lib\net45\
echo Copy Nuget spec
copy /y .\.nuget\windowsazure.mediaservices.nuspec .\drop\WAMSSDK\
if %ERRORLEVEL% neq 0 goto copyfailed
echo OK

echo Creating Media SDK NuGet Packages....
nuget.exe pack .\drop\WAMSSDK\windowsazure.mediaservices.nuspec -o .\drop -Symbols
if %ERRORLEVEL% neq 0 goto packagingfailed
echo OK

echo Copying files to the packages share...
copy .\drop\*.* c:\packages
if %ERRORLEVEL% neq 0 goto copyfailed
echo OK

echo Removing all signed files from \\adksdksign\unsigned...
del /q c:\signing\signed\*.*
echo OK

echo Removing all unsigned files from \\adksdksign\unsigned...
del /q c:\signing\tosign\*.*
echo OK

echo SUCCESS. Signed files are available at \\adxsdksign\packages

exit /b 0

:packagingfailed

echo FAILED. Unable to create NuGet packages
exit /b -1

:copyfailed

echo FAILED. Unable to copy native DLLs
exit /b -1

:publishfailed

echo FAILED. Unable to publish to myget
exit /b -1

:signfailed

echo FAILED. Signing tool failed.
exit /b -1