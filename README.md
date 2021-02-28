# Kentico Kontent Boilerplate for ASP.NET Core MVC
[![Build & Test](https://github.com/Kentico/kontent-boilerplate-net/actions/workflows/integrate.yml/badge.svg)](https://github.com/Kentico/kontent-boilerplate-net/actions/workflows/integrate.yml)
[![codecov](https://codecov.io/gh/Kentico/kontent-boilerplate-net/branch/master/graph/badge.svg?token=DThPm8Jowt)](https://codecov.io/gh/Kentico/kontent-boilerplate-net)
[![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kentico-kontent)


 | Package | Downloads | Compatibility | 
 |:-------------:| :-------------:|  :-------------:|  
| [![NuGet](https://img.shields.io/nuget/v/Kentico.Kontent.Boilerplate.svg)](https://www.nuget.org/packages/Kentico.Kontent.Boilerplate/) | [![NuGet](https://img.shields.io/nuget/dt/Kentico.Kontent.Boilerplate.svg)](https://www.nuget.org/packages/Kentico.Kontent.Boilerplate) | [`net5.0`](https://dotnet.microsoft.com/download/dotnet/5.0) | 

This boilerplate lets you easily scaffold a web project for development with Kentico Kontent and give you a head start in a successful web project. It includes a set of pre-configured features and demonstrates best practices in order to kick off your website development with Kontent smoothly.

## What's included
[<img align="right" src="/img/template_thumbnail.png" alt="Boilerplate screenshot" />](/img/template.png)
- [Kentico Delivery SDK](https://github.com/Kentico/delivery-sdk-net)
  - [Sample link resolver](#how-to-resolve-links)
- [Pre-build event for model generating](#how-to-generate-strongly-typed-models-for-content-types)  
- [Webhook-enabed caching](#how-to-set-up-webhook-enabled-caching)
- [HTTP Status codes handling (404, 500, ...)](#how-to-handle-404-errors-or-any-other-error)
- [Adjustable images](#how-to-resize-images-based-on-window-width)
- [Sitemap.xml](#how-to-adjust-the-sitemapxml) generator
- [URL Rewriting examples](#how-to-adjust-url-rewriting)
  - 301 URL Rewriting
  - www -> non-www redirection
- Configs for Dev and Production environment
- robots.txt
- Logging
- Unit tests ([xUnit](https://xunit.net/))

## Getting started


### Installation from NuGet

1. Run `dotnet new --install "Kentico.Kontent.Boilerplate::*"` to install the boilerplate to your machine
2. Run `dotnet new kentico-kontent-mvc --name "MyWebsite" [-pid|project-id "<projectid>"] [-d|domain "<domain_name>"] [--output "<path>"]` to init a website from the template
   - You can change the project ID later at any time in `appsettings.json`
3. Open in the IDE of your choice and Run

_Note: You can [install the tempalte from the sourcecode](../../wiki/Installation-from-source) too._

## How Tos

### How to generate Strongly Typed Models for Content Types
By convention, all [strongly-typed Content Type models](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Working-with-strongly-typed-models) are generated and stored within the `Models/ContentTypes` folder. All generated classes are marked as [`partial`](https://msdn.microsoft.com/en-us/library/wa80x488.aspx) to enable further customization without losing the generated code.

The generating is facilitated by a [.NET generator tool](https://github.com/Kentico/kontent-generators-net) as pre-build event. If you wish to customize the process, adjust the [`Tools/GenerateModels.ps1`](https://github.com/Kentico/kontent-boilerplate-net/blob/master/src/content/Kentico.Kontent.Boilerplate/Tools/GenerateModels.ps1) script.

For instance, to set a different namespace, set the `-n` command line parameter to `[project namespace].Models`. Or, to enable usage of [Display Templates (MVC)](http://www.growingwiththeweb.com/2012/12/aspnet-mvc-display-and-editor-templates.html) for rich-text elements, set `--structuredmodel true`.

You can regenerate the models using the included PowerShell script that utilizes the model generator utility. The script is located at .

### How to resolve links
Rich text elements in Kentico Kontent can contain links to other content items. It's up to a developer to decide how the links should be represented on a live site. Resolution logic can be adjusted in the [`CustomContentLinkUrlResolver`](https://github.com/Kentico/kontent-boilerplate-net/blob/master/src/content/Kentico.Kontent.Boilerplate/Resolvers/CustomContentLinkUrlResolver.cs). See the [documentation](https://github.com/Kentico/delivery-sdk-net/wiki/Resolving-Links-to-Content-Items) for detailed info.

### How to set up webhook-enabled caching

All content retrieved from Kentico Kontent is by default [cached](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Caching-responses) for 24 minutes. When content is stale (a newer version exists) it is cached for only 2 seconds. You can change the expiration times via the `DeliveryCacheOptions` in [Startup](https://github.com/Kentico/kontent-boilerplate-net/blob/master/src/content/Kentico.Kontent.Boilerplate/Startup.cs#L40-L44).

If you want to invalidate cache items as soon as they're updated in Kontent, you need to [create a webhook](https://docs.kontent.ai/tutorials/develop-apps/integrate/using-webhooks-for-automatic-updates#a-creating-a-webhook) and point it to the `/Webhooks/Webhooks` relative path of your application. The URL of the app needs to be publicly accessible, e.g. `https://myboilerplate.azurewebsites.net/Webhooks/Webhooks`. Finally, copy the API secret and store it as `KenticoKontentWebhookSecret` in your `Configuration` object, typically in the [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) or [Azure Key Vault](https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)

![New webhook configuration](https://i.imgur.com/TjJ7n5H.png)

**Note**: During local development, you can use the [ngrok](https://ngrok.com/) service to route to your workstation. Simply start your application locally and run command `.\ngrok.exe http [port] -host-header="localhost:[port]"` (e.g. `.\ngrok.exe http 59652 -host-header="localhost:59652"`) and set the webhook URL to the displayed HTTPS address.

**Note**: Speed of the Delivery/Preview API service is already tuned up because the service uses a geo-distributed CDN network for most of the types of requests. Therefore, the main advantage of caching in Kentico Kontent applications is not speed but lowering the amount of requests needed (See [pricing](https://kontent.ai/pricing) for details).

### How to render responsive images
The boilerplate contains a sample implementation of the `img-asset` tag helper from the [Kentico.Kontent.AspNetCore](https://www.nuget.org/packages/Kentico.Kontent.AspNetCore) NuGet package. Using the `img-asset` tag helper, you can easily create an `img` tag with `srcset` and `sizes` attributes.

### How to adjust the sitemap.xml
The boilerplate contains a sample implementation of the [`SiteMapController`](https://github.com/Kentico/kontent-boilerplate-net/blob/master/src/content/Kentico.Kontent.Boilerplate/Controllers/SiteMapController.cs). Make sure you specify desired content types in the `Index()` action method. Also, you can adjust the URL resolution logic in the `GetPageUrl()` method.

### How to handle 404 errors or any other error

Error handling is setup by default. Any server exception or error response within 400-600 status code range is handled by ErrorController. By default, it's configured to display Not Found error page for 404 error and General Error for anything else. 

### How to adjust URL rewriting

The Boilerplate is configured to load all [URL Rewriting](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting) rules from [IISUrlRewrite.xml](/src/content/Kentico.Kontent.Boilerplate/IISUrlRewrite.xml) file. Add or modify existing rules to match your expected behavior.
This is a good way to set up 301 Permanent redirects or www<->non-www redirects.

You can adjust the domain name in the default rewriting rules during the template instantiation by applying the `-d|domain` parameter.

## Get involved

Check out the [contributing](CONTRIBUTING.md) page to see the best places to file issues, start discussions, and begin contributing.

