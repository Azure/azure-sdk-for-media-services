
echo Copying Media SDK signed managed DLLs and the pdbs to the final drop location...
echo Creating \drop\WAMSSDK\lib\net45
md .\drop\WAMSSDK\lib\net45

echo Copy MediaServices.Client.dll
copy /y "\\iisdist\release\Media\nimbus\Client SDK\dlls\latest signed\Microsoft.WindowsAzure.MediaServices.Client.dll" .\drop\WAMSSDK\lib\net45\
echo Copy Microsoft.WindowsAzure.MediaServices.Client.Common.Authentication.dll
copy /y "\\iisdist\release\Media\nimbus\Client SDK\dlls\latest signed\Microsoft.WindowsAzure.MediaServices.Client.Common.Authentication.dll" .\drop\WAMSSDK\lib\net45\
echo Copy Microsoft.WindowsAzure.MediaServices.Client.Common.FileEncryption.dll
copy /y "\\iisdist\release\Media\nimbus\Client SDK\dlls\latest signed\Microsoft.WindowsAzure.MediaServices.Client.Common.FileEncryption.dll" .\drop\WAMSSDK\lib\net45\
echo Copy Microsoft.WindowsAzure.MediaServices.Client.Common.BlobTransfer.dll
copy /y "\\iisdist\release\Media\nimbus\Client SDK\dlls\latest signed\Microsoft.WindowsAzure.MediaServices.Client.Common.BlobTransfer.dll" .\drop\WAMSSDK\lib\net45\
echo Copy MediaServices.Client.pdb
copy /y .\Publish\Build\Release\Microsoft.WindowsAzure.MediaServices.Client.pdb .\drop\WAMSSDK\lib\net45\

echo Copy Nuget spec
copy /y .\.nuget\windowsazure.mediaservices.nuspec .\drop\WAMSSDK\
if %ERRORLEVEL% neq 0 goto copyfailed
echo OK

echo Creating Media SDK NuGet Packages....
.\.nuget\nuget.exe pack .\drop\WAMSSDK\windowsazure.mediaservices.nuspec -o .\drop -Symbols
if %ERRORLEVEL% neq 0 goto packagingfailed
echo OK

echo Copying files to the packages share...
copy .\drop\*.* c:\packages
if %ERRORLEVEL% neq 0 goto copyfailed
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