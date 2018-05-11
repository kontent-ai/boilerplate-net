using System;
using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public interface IWebhookListener
    {
        event EventHandler<WebhookNotificationEventArgs> WebhookNotification;

        void RaiseWebhookNotification(object sender, string operation, IdentifierSet identifierSet);
    }
}