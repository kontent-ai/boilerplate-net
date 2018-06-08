using System;

using CloudBoilerplateNet.Models;

namespace CloudBoilerplateNet.Helpers
{
    /// <summary>
    /// Invokes an event, based on a webhook notification.
    /// </summary>
    public interface IWebhookListener
    {
        /// <summary>
        /// Method to deal with the notification.
        /// </summary>
        event EventHandler<WebhookNotificationEventArgs> WebhookNotification;

        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="sender">Initiator of the invocation.</param>
        /// <param name="operation">Codename of the remote operation being advertised through the webhook call.</param>
        /// <param name="identifierSet">Identifiers of the data being processed by the <paramref name="operation"/>.</param>
        void RaiseWebhookNotification(object sender, string operation, IdentifierSet identifierSet);
    }
}