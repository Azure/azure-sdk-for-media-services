
# This Repository is Deprecated now. Customers should migrate to use the v3 API only

Please use the latest v3 API for .NET. 
- [Configure the v3 API for .NET](https://docs.microsoft.com/en-us/azure/media-services/latest/configure-connect-dotnet-howto)
- See the v3 Tutorials [Tutorial: Encode a remote file based on URL and stream the video - .NET](https://docs.microsoft.com/en-us/azure/media-services/latest/stream-files-dotnet-quickstart)
- Check out the [v3 .NET Samples repo](https://github.com/Azure-Samples/media-services-v3-dotnet)

The new .NET SDK is located here in Nuget
https://www.nuget.org/packages/Microsoft.Azure.Management.Media/

To install the newest SDK using the .NET CLI
```
dotnet add package Microsoft.Azure.Management.Media
```

## IMPORTANT! Update your Azure Media Services REST API and SDKs to v3 by 29 February 2024

Because version 3 of Azure Media Services REST API and client SDKs for .NET and Java offers more capabilities than version 2, we’re retiring version 2 of the Azure Media Services REST API and client SDKs for .NET and Java. We encourage you to make the switch sooner to gain the richer benefits of version 3 of Azure Media Services REST API and client SDKs for .NET and Java. Version 3 provides: 

### Action Required:
To minimize disruption to your workloads, review the migration guide to transition your code from the version 2 to version 3 API and SDK before 29 February 2024. 

After 29 February 2024, Azure Media Services will no longer accept traffic on the version 2 REST API, the ARM account management API version 2015-10-01, or from the version 2 .NET client SDKs. This includes any 3rd party open-source client SDKS that may call the version 2 API.  

See [Update your Azure Media Services REST API and SDKs to v3 by 29 February 2024](https://azure.microsoft.com/en-us/updates/update-your-azure-media-services-rest-api-and-sdks-to-v3-by-29-february-2024)

# (Deprecated) Windows Azure Media Services SDK .NET 4.5 (REST v2)
## This library will be retired after 29 February 2024. Please migrate to the v3 API

Windows Azure Media Services allows you to build a media distribution solution that can stream audio and video to Windows, iOS, Android, and other devices and platforms.To learn more, visit our [Developer Center](http://www.windowsazure.com/en-us/develop/media-services/).

## v2 Release Notes

Please read the latest note here: https://github.com/Azure/azure-sdk-for-media-services/releases.

## v2 Getting Started

If you are new to Media Services, you can get started by following our [tutorials](http://www.windowsazure.com/en-us/develop/media-services/tutorials/get-started/). You could be able to bring media into Azure, encoded it, package it into stream-able format and make it available for streaming. 

## Download Source Code 

To get the source code of our v2 SDKs and samples via **git** just type:

    git clone https://github.com/WindowsAzure/azure-sdk-for-media-services.git
    cd ./azure-sdk-for-media-services/

## .NET SDK

### Prerequisites

1. A Media Services account in a new or existing Windows Azure subscription. See the topic [How to Create a Media Services Account](http://www.windowsazure.com/en-us/manage/services/media-services/how-to-create-a-media-services-account/).
2. Operating Systems: Windows 7, Windows 2008 R2, or Windows 8.
3. .NET Framework 4.5.
4. Visual Studio 2012 or Visual Studio 2010 SP1 (Professional, Premium, Ultimate, or Express).

### Building and Referencing the SDK

To build sdk sources and tests type following commands:

	cd ./azure-sdk-for-media-services/
	msbuild ./SDK.Client.sln


### Running the Tests

This set of test allows you to verify Windows Azure Media Services .Net SDK functionality using your existing Windows Azure Media Services account. Please check out [Test instruction](https://github.com/WindowsAzure/azure-sdk-for-media-services/tree/master/test) on how to use it.


## Need Help?

Be sure to check out the Mobile Services [Developer Forum](http://social.msdn.microsoft.com/Forums/en-US/MediaServices/threads) if you are having trouble. The Media Services product team actively monitors the forum and will be more than happy to assist you.

## Contribute Code or Provide Feedback

If you would like to become an active contributor to this project please follow the instructions provided in [Windows Azure Projects Contribution Guidelines](http://windowsazure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/WindowsAzure/azure-media-services/issues) section of the project.

## Learn More
[Windows Azure Media Services Developer Center](http://www.windowsazure.com/en-us/develop/media-services/)
