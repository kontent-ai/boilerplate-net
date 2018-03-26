using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudBoilerplateNet
{
    public class WebhookNotificationEventArgs : EventArgs
    {
        public IdentifierSet IdentifierSet { get; }

        public WebhookNotificationEventArgs(IdentifierSet identifierSet)
        {
            IdentifierSet = identifierSet ?? throw new ArgumentNullException(nameof(identifierSet));
        }
    }
}
