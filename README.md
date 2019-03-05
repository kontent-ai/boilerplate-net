# Kentico Cloud Boilerplate for ASP.NET Core MVC
[<img align="right" src="/img/template_thumbnail.png" alt="Boilerplate screenshot" />](/img/template.png)
[![Build status](https://ci.appveyor.com/api/projects/status/1s02tbk1tml2wdmj/branch/master?svg=true)](https://ci.appveyor.com/project/kentico/cloud-boilerplate-net/branch/master)
[![NuGet](https://img.shields.io/nuget/v/KenticoCloud.CloudBoilerplateNet.svg)](https://www.nuget.org/packages/KenticoCloud.CloudBoilerplateNet/)
[![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kentico-cloud)

This boilerplate includes a set of features and best practices to kick off your website development with Kentico Cloud smoothly.

## What's included

- [Kentico Delivery SDK](https://github.com/Kentico/delivery-sdk-net)
  - [Sample generated strongly-typed models](#how-to-generate-strongly-typed-models-for-content-types)  
  - [Sample link resolver](#how-to-resolve-links)
- [HTTP Status codes handling (404, 500, ...)](#how-to-handle-404-errors-or-any-other-error)
- [Adjustable images](#how-to-resize-images-based-on-window-width)
- [Sitemap.xml](#how-to-adjust-the-sitemapxml) generator
- URL [Rewriting examples](#how-to-adjust-url-rewriting)
  - 301 URL Rewriting
  - www -> non-www redirection
- Configs for Dev and Production environment
- robots.txt
- Logging
- Unit tests ([xUnit](https://xunit.github.io))

## Quick start

### Prerequisites

The most seamless way to get all prerequisities is to install [Visual Studio 2017](https://www.visualstudio.com/vs/) with the ".NET Core cross-platform development" workload.

**Required:**
You must have the latest version ([.NET Core 2](https://www.microsoft.com/net/download/core) is minimal) of the dotnet tooling installed. It comes with Visual Studio 2017 (15.3 or higher), or you can download it with the [.NET Core SDK](https://www.microsoft.com/net/download/core). You can check your .NET Core version via `dotnet --version`.

Optional:
* [Visual Studio 2017](https://www.visualstudio.com/vs/) for full experience
* or [Visual Studio Code](https://code.visualstudio.com/)

### Installation from NuGet

1. Open Developer Command Prompt for VS 2017
2. Run `dotnet new --install "KenticoCloud.CloudBoilerplateNet::*"` to install the boilerplate to your machine
3. Wait for the command to finish (it may take a minute or two)
4. Run `dotnet new kentico-cloud-mvc --name "MyWebsite"  [--output "<path>"]`. (**Note:** the `--output` parameter is optional.)
5. Open in Visual Studio 2017/Code and Run

### Installation from source

1. `git clone https://github.com/Kentico/cloud-boilerplate-net.git`
2. `dotnet build`
3. `dotnet new -i artifacts/*.nupkg`

## How Tos

### How to debug the app

In Windows, there are basically two ways to run the app:

* via IIS
* as a standalone command-line app

You can read more about the differences on [Rick Strahl's blog](https://weblog.west-wind.com/posts/2016/Jun/06/Publishing-and-Running-ASPNET-Core-Applications-with-IIS).

We recommend choosing the **second option - as a standalone app**. In Visual Studio, you can switch to it in the toolbar:

![Debugging mode](https://i.imgur.com/PoxRBus.png)

### How to change Kentico Cloud Project ID and Delivery Preview API key

Kentico Cloud Project ID is stored inside `appsettings.json` file. This setting is automatically loaded [using Options and configuration objects](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration). You can also provide additional environment-specific configuration in `appsettings.production.json` and `appsettings.development.json` files.

For security reasons, Delivery Preview API key should be stored outside of the project tree. It's recommended to use [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) to store sensitive data.

### How to generate Strongly Typed Models for Content Types

With the new Delivery SDK, you can [take advantage](https://github.com/Kentico/delivery-sdk-net/wiki/Working-with-Strongly-Typed-Models-(aka-Code-First-Approach)) of code-first approach. To do that you have to instruct the SDK to use strongly-typed models. These models can be generated automatically by [model generator utility](https://github.com/Kentico/cloud-generators-net). By convention, all Content Type Models are stored within the `Models/ContentTypes` folder. All generated classes are marked as [`partial`](https://msdn.microsoft.com/en-us/library/wa80x488.aspx) which means that they can be extended in separate files. This should prevent losing custom code in case the models get regenerated. When generating models, be sure to set the `-n` command line parameter to `[project namespace].Models`.

If you want to use [Display Templates (MVC)](http://www.growingwiththeweb.com/2012/12/aspnet-mvc-display-and-editor-templates.html), make sure you generate also a custom type provider (add the `--withtypeprovider` parameter when running the generator utility).

### How to resolve links
Rich text elements in Kentico Cloud can contain links to other content items. It's up to a developer to decide how the links should be represented on a live site. Resolution logic can be adjusted in the [`CustomContentLinkUrlResolver`](https://github.com/Kentico/cloud-boilerplate-net/blob/master/src/content/CloudBoilerplateNet/Resolvers/CustomContentLinkUrlResolver.cs). See the [documentation](https://github.com/Kentico/delivery-sdk-net/wiki/Resolving-Links-to-Content-Items) for detailed info.

### How to set up webhook-enabled caching

All content retrieved from Kentico Cloud is by default [cached](https://github.com/Kentico/cloud-boilerplate-net/blob/master/src/content/CloudBoilerplateNet/Services/CachedDeliveryClient.cs) for 24 hours in a [MemoryCache](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.caching.memorycache) singleton object. You can either change the time by overriding the value of the `CacheTimeoutSeconds` environment variable (e.g. in appsettings.json). Or, if you want your app to immediately clear cache entries of changed content, you can create a webhook in Kentico Cloud for that.

To create the web hook, go to Project settings --> Webhooks --> Create new Webhook. Give it a name (like "Cache invalidation webhook") and the publicly routable URL of your app with `/WebHooks/KenticoCloud` appended (like "https://myapp.azurewebsites.net/WebHooks/KenticoCloud"). Then, copy the API secret and paste it as the `KenticoCloudWebhookSecret` environment variable (secret) into your app's settings.

![New webhook configuration](https://i.imgur.com/ootVcPZ.png)

**Note**: During local development, you can use the [ngrok](https://ngrok.com/) service to route to your workstation. 

**Note**: Speed of the Delivery/Preview API service is already tuned up because the service uses a geo-distributed CDN network for most of the types of requests. Therefore, the main advantage of caching in Kentico Cloud applications is not speed but lowering the amount of requests needed (See [pricing](https://kenticocloud.com/pricing) for details).

### How to resize images based on window width
The boilerplate contains a sample implementation of the [`HtmlHelperExtensions`](https://github.com/Kentico/cloud-boilerplate-net/blob/responsive-images/src/content/CloudBoilerplateNet/Helpers/Extensions/HtmlHelperExtensions.cs). Using the `AssetImage()` extension method, you can easily create an `img` tag with `srcset` and `sizes` attributes.

### How to adjust the sitemap.xml
The boilerplate contains a sample implementation of the [`SiteMapController`](https://github.com/Kentico/cloud-boilerplate-net/blob/master/src/content/CloudBoilerplateNet/Controllers/SiteMapController.cs). Make sure you specify desired content types in the `Index()` action method. Also, you can adjust the URL resolution logic in the `GetPageUrl()` method.

### How to handle 404 errors or any other error

Error handling is setup by default. Any server exception or error response within 400-600 status code range is handled by ErrorController. By default, it's configured to display Not Found error page for 404 error and General Error for anything else. 

### How to adjust URL rewriting

The Boilerplate is configured to load all [URL Rewriting](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting) rules from [IISUrlRewrite.xml](/src/content/CloudBoilerplateNet/IISUrlRewrite.xml) file. Add or modify existing rules to match your expected behavior.
This is a good way to set up 301 Permanent redirects or www<->non-www redirects.

## Feedback & Contributing
Any feedback is much appreciated. Check out the [contributing](https://github.com/Kentico/Home/blob/master/CONTRIBUTING.md) to see the best places to file issues, start discussions and begin contributing.

### Wall of Fame
We would like to express our thanks to the following people who contributed and made the project possible:

- [Emmanuel Tissera](https://github.com/emmanueltissera) - [GetStarted](https://github.com/getstarted) 
- [Andy Thompson](https://github.com/andythompy)- [GetStarted](https://github.com/getstarted)
- [Sayed Ibrahim Hashimi](https://github.com/sayedihashimi) - [Microsoft](https://github.com/Microsoft)
- [Charith Sooriyaarachchi](https://github.com/charithsoori) - [99X Technology](http://www.99xtechnology.com/)
- [Lex Li](https://github.com/lextm)

Would you like to become a hero too? Pick an [issue](https://github.com/Kentico/cloud-boilerplate-net/issues) and send us a pull request!


![Analytics](https://kentico-ga-beacon.azurewebsites.net/api/UA-69014260-4/Kentico/cloud-boilerplate-net?pixel)
