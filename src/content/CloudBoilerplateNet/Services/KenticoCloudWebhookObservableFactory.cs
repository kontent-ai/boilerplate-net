using System;
using System.Linq;
using System.Reactive.Linq;

namespace CloudBoilerplateNet.Services
{
    public static class KenticoCloudWebhookObservableFactory
    {
        public static IObservable<WebhookNotificationEventArgs> GetObservable(object sender, string eventName)
        {
            return Observable.FromEventPattern<WebhookNotificationEventArgs>(sender, eventName).Select(e => e.EventArgs);
        }
    }
}
