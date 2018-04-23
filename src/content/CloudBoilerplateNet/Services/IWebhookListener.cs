using System;
using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public interface IWebhookListener
    {
        event EventHandler<WebhookNotificationEventArgs> WebhookNotification;

        void RaiseWebhookNotification(object sender, IdentifierSet identifierSet);
    }
}