using System;
using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public interface IWebhookObservableProvider
    {
        event EventHandler<WebhookNotificationEventArgs> WebhookNotification;

        void RaiseWebhookNotification(IRelatedTypesResolver relatedTypesResolver, IdentifierSet identifierSet);

        IObservable<WebhookNotificationEventArgs> GetObservable();
    }
}