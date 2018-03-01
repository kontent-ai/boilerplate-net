using System;
using System.Linq;
using System.Reactive.Linq;

using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public class WebhookObservableProvider : IWebhookObservableProvider
    {
        public event EventHandler<WebhookNotificationEventArgs> WebhookNotification;

        public void RaiseWebhookNotification(IRelatedTypesResolver relatedTypesResolver, IdentifierSet identifierSet)
        {
            if (WebhookNotification != null && relatedTypesResolver != null && identifierSet != null)
            {
                WebhookNotification.Invoke(this, new WebhookNotificationEventArgs(relatedTypesResolver, identifierSet));
            }
        }

        public IObservable<WebhookNotificationEventArgs> GetObservable()
        {
            return Observable.FromEventPattern<WebhookNotificationEventArgs>(this, nameof(WebhookNotification)).Select(e => e.EventArgs);
        }
    }
}
