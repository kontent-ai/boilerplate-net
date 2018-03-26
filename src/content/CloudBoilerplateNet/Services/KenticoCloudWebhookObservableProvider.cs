using System;
using System.Linq;
using System.Reactive.Linq;

using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public class KenticoCloudWebhookObservableProvider : IWebhookObservableProvider
    {
        public event EventHandler<WebhookNotificationEventArgs> WebhookNotification;

        public void RaiseWebhookNotification(IdentifierSet identifierSet)
        {
            if (WebhookNotification != null && identifierSet != null)
            {
                WebhookNotification.Invoke(this, new WebhookNotificationEventArgs(identifierSet));
            }
        }

        public IObservable<WebhookNotificationEventArgs> GetObservable()
        {
            return Observable.FromEventPattern<WebhookNotificationEventArgs>(this, nameof(WebhookNotification)).Select(e => e.EventArgs);
        }
    }
}
