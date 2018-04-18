using System;
using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public interface IKenticoCloudWebhookListener
    {
        event EventHandler<WebhookNotificationEventArgs> WebhookNotification;

        void RaiseWebhookNotification(object sender, IdentifierSet identifierSet);
    }
}