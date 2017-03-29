# Kentico Cloud Boilerplate for ASP.NET Core MVC
[<img align="right" src="/img/template_thumbnail.png" alt="Boilerplate screenshot" />](/img/template.png)
[![Build status](https://ci.appveyor.com/api/projects/status/1s02tbk1tml2wdmj/branch/master?svg=true)](https://ci.appveyor.com/project/kentico/cloud-boilerplate-net/branch/master)
[![NuGet](https://img.shields.io/nuget/v/KenticoCloud.CloudBoilerplateNet.svg)](https://www.nuget.org/packages/KenticoCloud.CloudBoilerplateNet/)
[![Forums](https://img.shields.io/badge/chat-on%20forums-orange.svg)](https://forums.kenticocloud.com)

This boilerplate includes a set of features and best practices to kick off your website development with Kentico Cloud smoothly.

## What's included

- [Kentico Delivery SDK](https://github.com/Kentico/delivery-sdk-net)
  - [Sample generated strongly-typed models](#how-to-generate-strongly-typed-models-for-content-types)  
  - [Sample link resolver](#how-to-resolve-links)
- [Simple fixed-time caching](#how-to-set-up-fixed-time-caching)
- [HTTP Status codes handling (404, 500, ...)](#how-to-handle-404-errors-or-any-other-error)
- [301 URL Rewriting](#how-to-handle-301-permanent-redirect)
- [Sitemap.xml](#how-to-adjust-the-sitemapxml) generator
- www -> non-www redirection
- Configs for Dev and Production environment
- robots.txt
- Logging
- Unit tests ([xUnit](https://xunit.github.io))

## Quick start

### Installation from NuGet

1. You must have the latest version of the dotnet tooling installed. It comes with Visual Studio 2017 or you can download it with the [.NET Core SDK](https://www.microsoft.com/net/download/core).
2. Open Developer Command Prompt for VS 2017
3. Run `dotnet new --install "KenticoCloud.CloudBoilerplateNet::*"` to install the boilerplate to your machine
3. Wait for the command to finish (it may take a minute or two)
4. Run `dotnet new kentico-cloud-mvc --name "MyWebsite"  [--output "<path>"]`
5. Open in Visual Studio 2017/Code and Run

### Installation from source

1. `git clone https://github.com/Kentico/cloud-boilerplate-net.git`
2. `dotnet build`
3. `dotnet new -i artifacts/*.nupkg`

## How Tos


### How to change Kentico Cloud Project ID and Delivery Preview API key

Kentico Cloud Project ID is stored inside `appsettings.json` file. This setting is automatically loaded [using Options and configuration objects](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration). You can also provide additional environment-specific configuration in `appsettings.production.json` and `appsettings.development.json` files.

For security reasons, Delivery Preview API key should be stored outside of the project tree. It's recommended to use [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) to store sensitive data.

### How to generate Strongly Typed Models for Content Types

With the new Delivery SDK, you can [take advantage](https://github.com/Kentico/delivery-sdk-net/wiki/Working-with-Strongly-Typed-Models-(aka-Code-First-Approach)) of code-first approach. To do that you have to instruct the SDK to use strongly-typed models. These models can be generated automatically by [model generator utility](https://github.com/Kentico/cloud-generators-net). By convention, all Content Type Models are stored within the `Models/ContentTypes` folder. All generated classes are marked as [`partial`](https://msdn.microsoft.com/en-us/library/wa80x488.aspx) which means that they can be extended in separate files. This should prevent losing custom code in case the models get regenerated.

If you want to use [Display Templates (MVC)](http://www.growingwiththeweb.com/2012/12/aspnet-mvc-display-and-editor-templates.html), make sure you generate also a custom type provider (add the `--withtypeprovider` parameter when running the generator utility).

### How to resolve links
Rich text elements in Kentico Cloud can contain links to other content items. It's up to a developer to decide how the links should be represented on a live site. Resolution logic can be adjusted in the [`CustomContentLinkUrlResolver`](https://github.com/Kentico/cloud-boilerplate-net/blob/master/src/content/CloudBoilerplateNet/Resolvers/CustomContentLinkUrlResolver.cs). See the [documentation](https://github.com/Kentico/delivery-sdk-net/wiki/Resolving-Links-to-Content-Items) for detailed info.

### How to set up fixed-time caching

All content retrieved from Kentico Cloud is by default [cached](https://github.com/Kentico/cloud-boilerplate-net/blob/master/src/content/CloudBoilerplateNet/Services/CachedDeliveryClient.cs) for five minutes. You can change the time by overriding the value of `CacheTimeoutSeconds` (e.g. in appsettings.json).

### How to adjust the sitemap.xml
The boilerplate contains a sample implementation of the [`SiteMapController`](https://github.com/Kentico/cloud-boilerplate-net/blob/master/src/content/CloudBoilerplateNet/Controllers/SiteMapController.cs). Make sure you specify desired content types in the `Index()` action method. Also, you can adjust the URL resolution logic in the `GetPageUrl()` method.

### How to handle 404 errors or any other error

Error handling is setup by default. Any server exception or error response within 400-600 status code range is handled by ErrorController. By default, it's configured to display Not Found error page for 404 error and General Error for anything else. 


### How to handle 301 Permanent redirect

The Boilerplate is configured to load all [URL Rewriting](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting) rules from [IISUrlRewrite.xml](/src/CloudBoilerplateNet/IISUrlRewrite.xml) file. Add or modify existing rules to match your expected behavior.

:warning: ASP.NET Core 1.1 currently [doesn't support Rewrite Maps](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting#unsupported-features), but there is a simple workaround for `/oldUrl -> /newUrl` mapping (see [IISUrlRewrite.xml](/src/CloudBoilerplateNet/IISUrlRewrite.xml) for example).


## Feedback & Contributing
Any feedback is much appreciated. Check out the [contributing](https://github.com/Kentico/Home/blob/master/CONTRIBUTING.md) to see the best places to file issues, start discussions and begin contributing.

## Contributions and Thanks
- [Emmanuel Tissera](https://github.com/emmanueltissera) - [GetStarted](https://github.com/getstarted) 
- [Andy Thompson](https://github.com/andythompy)- [GetStarted](https://github.com/getstarted)
- [Sayed Ibrahim Hashimi](https://github.com/sayedihashimi) - [Microsoft](https://github.com/Microsoft)
