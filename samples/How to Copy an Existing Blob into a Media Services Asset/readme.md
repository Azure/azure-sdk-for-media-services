<h1>How to Copy an Existing Blob into a Media Services Asset</h1>
   
<h2>Introduction</h2>
<p>This code example shows how to copy blobs from a storage account into a new Windows Azure Media Services asset. The code example is written in C#. The project uses the <strong>Media Services SDK for .NET </strong>and <strong>Windows Azure SDK for .NET</strong> version <strong>1.7.1</strong>. The project is a Windows Console application and is designed to accompany the <a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj933290.aspx#CopyBlobsToWAMSDiffAccount"> Copying Blobs from a Storage Account NOT Associated with Media Services Account into a Media Services Asset </a>section of the <a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj933290.aspx">
How-to: Copy an Existing Blob into a Media Services Asset</a> topic&nbsp;on MSDN.</p>

<h2>Sample Prerequisites</h2>
To build and run this sample you must have a Windows Azure Media Services account. If you do not already have an account, see <a href="http://go.microsoft.com/fwlink/?LinkId=256662"> How to Create a Media Services Account</a>. Also ensure that you have .NET Frawork 4.0 or newer installed. If not, you can download the lastest version of the .NET Framework from <a href="http://www.microsoft.com/en-us/download/details.aspx?id=30653">Download .NET Framework 4.5</a>. This sample was built with Visual Studio 2012. If you don't have this installed you can download it from <a href="http://www.microsoft.com/visualstudio/eng/downloads">Visual Studio 2012 Downloads</a>. The Windows Azure Media Services SDK is installed using a nuget package. For more information about Nuget and to install the Nuget Package Manager into Visual Studio, see <a href="http://nuget.org/">Nuget.org</a>.

<h2>Building the sample</h2></p>
Load the project into Visual Studio 2012 and add the windowsazure.mediaservices Nuget package to the solution. This will install the Windows Azure Media Services SDK for .NET and any related libraries. Open the app.config file and find the <appSettings> tag. This section configures your Windows Azure Media Services account as well as the Windows Azure Storage account associated with you Media Services account. You must enter your account name and key for each account in <appSettings> section. To find these values go to the <a href=https://manage.windowsazure.com>Windows Azure Portal</a>. On the left side of the screen, click the Media Services link and then click the Manage Keys link on the bottom of the page. This will allow you to copy your account name and primary key onto the clipboard. Paste these values into the appropriate item in the app.config file. Do the same for the Windows Azure Storage Account.

<p>To build the project, follow the steps described in the <a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj933290.aspx">
How-to: Copy an Existing Blob into a Media Services Asset</a> topic.&nbsp;</p>

<h2>Building StorageClient 1.7.1</h2>
<p>The <strong>StartCopyFromBlob </strong>method used to copy blobs between different storage accounts was introduced in the <strong>Windows Azure SDK for .NET</strong> version <strong>1.7.1</strong>. This version is only available through <a href="https://github.com/WindowsAzure/azure-sdk-for-net/tree/sdk_1.7.1">GITHUB</a>.</p>
<p>After you clone the https://github.com/WindowsAzure/azure-sdk-for-net/tree/sdk_1.7.1 repo, use the following git commands to get the 1.7.1 branch of the SDK.</p>

<pre>
$ cd azure-sdk-for-net
$ git remote add upstream git@github.com:WindowsAzure/azure-sdk-for-net.git
$ git fetch upstream 
$ git fetch upstream sdk_1.7.1:my_sdk_1.7.1
$ git checkout my_sdk_1.7.1
</pre>

<p>After you build the 1.7.1 version of the SDK, replace the <code>Microsoft.WindowsAzure.StorageClient.dll</code> that was added by the <strong>windowsazure.mediaservices</strong> NuGet package with the <code>Microsoft.WindowsAzure.StorageClient.dll</code> that you just built.</p>
<h1>Source Code Files</h1>
<p>For the Visual Studio project:</p>
<ul>
<li>Program.cs:  Contains all the source code that shows how to copy blobs.
</li><li>App.config: Contains configuration data with your account name and key as well as the storage account infromation&nbsp;from which you want to copy the blob.&nbsp;
<em></em></li></ul>

