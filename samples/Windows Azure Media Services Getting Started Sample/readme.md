<h1>Windows Azure Media Services Getting Started Sample</h1>

The code examples in this project show a first-time developer on Windows Azure Media Services how to complete a simple end-to-end workflow of creating an asset, uploading it, encoding it, and then downloading the output asset.  The code examples are written in C# using the Media Services SDK for .NET.  The project is a Windows Console application and is designed to accompany the following Getting Started tutorial on MSDN. 

<h2>Sample Prerequisites</h2>
To build and run this sample you must have a Windows Azure Media Services account. If you do not already have an account, see <a href="http://go.microsoft.com/fwlink/?LinkId=256662"> How to Create a Media Services Account</a>. Also ensure that you have .NET Frawork 4.0 or newer installed. If not, you can download the lastest version of the .NET Framework from <a href="http://www.microsoft.com/en-us/download/details.aspx?id=30653">Download .NET Framework 4.5</a>. This sample was built with Visual Studio 2012. If you don't have this installed you can download it from <a href="http://www.microsoft.com/visualstudio/eng/downloads">Visual Studio 2012 Downloads</a>. The Windows Azure Media Services SDK is installed using a nuget package. For more information about Nuget and to install the Nuget Package Manager into Visual Studio, see <a href="http://nuget.org/">Nuget.org</a>.

<h2>Building the sample</h2></p>
Load the project into Visual Studio 2012 and add the windowsazure.mediaservices Nuget package to the solution. This will install the Windows Azure Media Services SDK for .NET and any related libraries. Open the app.config file and find the <appSettings> tag. This section configures your Windows Azure Media Services account as well as the Windows Azure Storage account associated with you Media Services account. You must enter your account name and key for each account in <appSettings> section. To find these values go to the <a href=https://manage.windowsazure.com>Windows Azure Portal</a>. On the left side of the screen, click the Media Services link and then click the Manage Keys link on the bottom of the page. This will allow you to copy your account name and primary key onto the clipboard. Paste these values into the appropriate item in the app.config file. Do the same for the Windows Azure Storage Account.

<h2>Running the Sample</h2>

To run the sample, follow the steps described in the accompanying tutorial:

<a href=http://msdn.microsoft.com/en-us/library/hh973620.aspx>Getting Started with the Media Services SDK for .NET</a>

<h2>Source Code Files</h2>

For the Visual Studio project:
<ul>
<li>Program.cs - Contains all the source code for the Windows Console application</li>
<li>App.config - Contains configuration data with your account name and account key for connecting to Media Services</li></ul>
