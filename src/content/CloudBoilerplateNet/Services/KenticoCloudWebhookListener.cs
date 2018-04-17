using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Services
{
    public class KenticoCloudWebhookListener : IKenticoCloudWebhookListener
    {
        public event EventHandler<WebhookNotificationEventArgs> WebhookNotification = delegate { };

        public void RaiseWebhookNotification(object sender, IdentifierSet identifierSet)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (identifierSet == null)
            {
                throw new ArgumentNullException(nameof(identifierSet));
            }

            WebhookNotification(sender, new WebhookNotificationEventArgs(identifierSet));
        }
    }
}
