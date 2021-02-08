using System;
using System.Collections.Generic;

namespace Exchange.Interfaces
{
    public interface IExchangeServiceClient
    {
        IList<EmailMessageWrapper> FindUnreadEmails(ExchangeConfigurationCredentials credentials);

        void ReadEmails(ExchangeConfigurationCredentials credentials, IList<MyEmailMessage> emailMessages, Guid exchangeConfigurationId);
    }
}
