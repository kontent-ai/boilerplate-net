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
        public IRelatedTypesResolver RelatedTypesResolver { get; }

        public IdentifierSet IdentifierSet { get; }

        public WebhookNotificationEventArgs(IRelatedTypesResolver relatedTypesResolver, IdentifierSet identifierSet)
        {
            RelatedTypesResolver = relatedTypesResolver ?? throw new ArgumentNullException(nameof(relatedTypesResolver));
            IdentifierSet = identifierSet ?? throw new ArgumentNullException(nameof(identifierSet));
        }
    }
}
