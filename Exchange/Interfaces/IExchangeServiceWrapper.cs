using Microsoft.Exchange.WebServices.Data;
using System.Collections.Generic;

namespace Exchange.Interfaces
{
    public interface IExchangeServiceWrapper
    {
        IList<EmailMessage> FindItems(ExchangeConfigurationCredentials credentials, FolderId folderId, SearchFilter.SearchFilterCollection unreadEmailsFilter, ItemView itemView);

        EmailMessage GetEmailMessage(ExchangeConfigurationCredentials credentials);

        void SendAndSaveCopy(EmailMessage emailMessage, FolderId folderId);

        void UpdateEmail(EmailMessage email);

        ExchangeService GetClient(ExchangeConfigurationCredentials credentials);

        EmailMessage GetEmailByItemId(string itemId, PropertySet isReadPropertySet, ExchangeService client);

    }
}
