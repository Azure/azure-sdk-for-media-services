<h1>Windows Azure Media Services .NET SDK Sample</h1>
    
<h2>Introduction</h2>
<p>The code examples in this project demonstrate how to accomplish common tasks with Windows Azure Media Services. The code examples are written in C#, and uses the Media Services SDK for .NET.</p>
<p>Some of the tasks that are shown in the example include:</p>
<ul>
<li>Uploading your media content to the Windows Azure Media Services account</li>
<li>Encoding and Packaging assets</li>
<li>Protecting assets</li>
<li>Delivering content using the following options:<ul>
<li>Downloading Media Assets to a local computer</li>
<li>Creating a SAS locator</li>
<li>Creating an streaming origin locator for Smooth Streaming or HLS content</li>
</ul>

<p>For more information, see the following topics 
<p>
<a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj129571.aspx">Connecting to Media Services
</a><br>
<a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj129584.aspx">Ingesting Assets</a>
<br>
<a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj129580.aspx">Encoding, Packaging, and Protecting Assets</a>.<br>
<a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj129589.aspx">Managing Assets</a>
<br>
<a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj129575.aspx">Delivering Assets</a></p>

<h2>Sample Prerequisites</h2>
To build and run this sample you must have a Windows Azure Media Services account. If you do not already have an account, see <a href="http://go.microsoft.com/fwlink/?LinkId=256662"> How to Create a Media Services Account</a>. Also ensure that you have .NET Frawork 4.0 or newer installed. If not, you can download the lastest version of the .NET Framework from <a href="http://www.microsoft.com/en-us/download/details.aspx?id=30653">Download .NET Framework 4.5</a>. This sample was built with Visual Studio 2012. If you don't have this installed you can download it from <a href="http://www.microsoft.com/visualstudio/eng/downloads">Visual Studio 2012 Downloads</a>. The Windows Azure Media Services SDK is installed using a nuget package. For more information about Nuget and to install the Nuget Package Manager into Visual Studio, see <a href="http://nuget.org/">Nuget.org</a>.

<p><h2>Building the sample</h2></p>
Load the project into Visual Studio 2012 and add the windowsazure.mediaservices Nuget package to the solution. This will install the Windows Azure Media Services SDK for .NET and any related libraries. Open the app.config file and find the <appSettings> tag. This section configures your Windows Azure Media Services account as well as the Windows Azure Storage account associated with you Media Services account. You must enter your account name and key for each account in <appSettings> section. To find these values go to the <a href=https://manage.windowsazure.com>Windows Azure Portal</a>. On the left side of the screen, click the Media Services link and then click the Manage Keys link on the bottom of the page. This will allow you to copy your account name and primary key onto the clipboard. Paste these values into the appropriate item in the app.config file. Do the same for the Windows Azure Storage Account.

<h2>Running the Sample</h2>
<p>To see how each method works, call it from the Main method.</p>
<h2>Source Code Files</h2>
<p>For the Visual Studio project:</p>
<ul>
<li>Program.cs: Contains all the source code for the Windows Console application.
</li><li>App.config: Contains configuration data with your account name and account key for connecting to Media Services.
</li></ul>

