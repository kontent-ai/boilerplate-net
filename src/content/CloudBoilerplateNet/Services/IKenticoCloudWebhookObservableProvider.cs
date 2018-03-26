using System;
using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public interface IWebhookObservableProvider
    {
        event EventHandler<WebhookNotificationEventArgs> WebhookNotification;

        void RaiseWebhookNotification(IdentifierSet identifierSet);

        IObservable<WebhookNotificationEventArgs> GetObservable();
    }
}