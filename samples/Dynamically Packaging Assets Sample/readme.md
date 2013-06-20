<h1>Dynamically Packaging Assets Sample</h1>
             
<h2>Introduction</h2>
<p>Windows Azure Media Services can be used to deliver multitude of media source file formats, media streaming formats, and content protection formats to a variety of client technologies (for example, iOS, XBOX, Silverlight, Windows 8). These clients understand different protocols, for example iOS requires an <a href="http://tools.ietf.org/html/draft-pantos-http-live-streaming-08"> HTTP Live Streaming (HLS) V4</a> format and Silverlight and XBox require Smooth Streaming. If you have a set of multi-bitrate MP4 (ISO Base Media 14496-12) files or Smooth Streaming files that you want to serve to clients that understand HLS or Smooth Streaming,
you should take advantage of Media Services dynamic packaging.

With dynamic packaging all you need is to create an asset that contains a set of multi-bitrate MP4 files or multi-bitrate Smooth Streaming source files. Then, based on the specified format in the manifest or fragment request, the On-Demand Streaming server will ensure that you receive the stream in the protocol you have chosen. As a result, you only need to store and pay for the files in single storage format and Media Services service will build and serve the appropriate response based on requests from a client.

This sample demonstrates how to:
<ul>
<li>Prepare an asset for dynamic packaging </li>
<li>Request a locator and compose the streaming URLs for Smooth Streaming and HLS</li>
</ul>
<br>
For more infomrmation, see <a href="http://msdn.microsoft.com/en-us/library/windowsazure/jj889436.aspx">
Walkthrough: Dynamically Packaging Assets</a>.

<h2>Sample Prerequisites</h2>
To build and run this sample you must have a Windows Azure Media Services account. If you do not already have an account, see <a href="http://go.microsoft.com/fwlink/?LinkId=256662"> How to Create a Media Services Account</a>. Also ensure that you have .NET Frawork 4.0 or newer installed. If not, you can download the lastest version of the .NET Framework from <a href="http://www.microsoft.com/en-us/download/details.aspx?id=30653">Download .NET Framework 4.5</a>. This sample was built with Visual Studio 2012. If you don't have this installed you can download it from <a href="http://www.microsoft.com/visualstudio/eng/downloads">Visual Studio 2012 Downloads</a>. The Windows Azure Media Services SDK is installed using a nuget package. For more information about Nuget and to install the Nuget Package Manager into Visual Studio, see <a href="http://nuget.org/">Nuget.org</a>.

<h2>Building the sample</h2></p>
Load the project into Visual Studio 2012 and add the windowsazure.mediaservices Nuget package to the solution. This will install the Windows Azure Media Services SDK for .NET and any related libraries. Open the app.config file and find the <appSettings> tag. This section configures your Windows Azure Media Services account as well as the Windows Azure Storage account associated with you Media Services account. You must enter your account name and key for each account in <appSettings> section. To find these values go to the <a href=https://manage.windowsazure.com>Windows Azure Portal</a>. On the left side of the screen, click the Media Services link and then click the Manage Keys link on the bottom of the page. This will allow you to copy your account name and primary key onto the clipboard. Paste these values into the appropriate item in the app.config file. Do the same for the Windows Azure Storage Account.
<h2>Building the Sample</h2>

<h2>Source Code Files</h2>
<p>For the Visual Studio project:</p>
<ul>
<li>Program.cs:  Contains all the source code for the Windows Console application.
</li><li>App.config: Contains configuration data with your account name and account key for connecting to Media Services.
</li></ul>
<p>Support files that are used in this project are located in the <strong>Dynamic packaging\supportFiles
</strong>folder.</p>

</div>


    
