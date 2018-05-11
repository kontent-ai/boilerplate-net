using System;
using System.Collections.Generic;
using System.Text;

using CloudBoilerplateNet.Models;
using CloudBoilerplateNet.Services;
using Xunit;

namespace CloudBoilerplateNet.Tests.Services
{
    public class KenticoCloudWebhookListenerTests
    {
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void RaiseWebhookNotificationThrows(bool createSender, bool createIdentifierSet)
        {
            object sender = null;
            IdentifierSet identifierSet = null;

            if (createSender)
            {
                sender = new object();
            }

            if (createIdentifierSet)
            {
                identifierSet = new IdentifierSet
                {
                    Codename = "Test",
                    Type = "Test"
                };
            }

            var listener = new WebhookListener();
            Assert.Throws<ArgumentNullException>(() => listener.RaiseWebhookNotification(sender, "upsert", identifierSet));
        }
    }
}
