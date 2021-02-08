using Exchange.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace Exchange
{
    public class ExchangeServiceClient : IExchangeServiceClient
    {
        private readonly IExchangeServiceWrapper _exchangeService;
        private const int EmailCleanupBatchSize = 100;
        private const int MaxItemCount = 20;
        private const int MinimumCleanupDaysThreshold = 7;

        public ExchangeServiceClient(IExchangeServiceWrapper exchangeService)
        {
            if (exchangeService == null) throw new ArgumentNullException("exchangeService");
            _exchangeService = exchangeService;
        }

        public IList<EmailMessageWrapper> FindUnreadEmails(ExchangeConfigurationCredentials credentials)
        {
            if (credentials == null) throw new ArgumentNullException("credentials");

            var itemView = new ItemView(MaxItemCount);
            itemView.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Ascending);

            var outOffOfficeEmailFilter = new SearchFilter.IsNotEqualTo(ItemSchema.ItemClass, "IPM.Note.Rules.OofTemplate.Microsoft");
            var filters = new SearchFilter.SearchFilterCollection(LogicalOperator.And)
            {
                UnreadEmailFilter,
                outOffOfficeEmailFilter
            };
            var folderId = GetInboxFolder(credentials);

            var result = _exchangeService.FindItems(credentials, folderId, filters, itemView)
                                         .Select(message => GetEmailMessageWrapper(message, credentials.Id))
                                         .ToList();
            if (result.Any(c => c.Address == null || c.DisplayName == null || c.ExchangeConfigurationId == null || c.EmlContent == null || c.ItemId == null))
            {
                System.Diagnostics.Debug.WriteLine($"{result.Count} unread email(s) found in mailbox {credentials.ExchangeName}");
            }
            return result;
        }

        public void ReadEmails(ExchangeConfigurationCredentials credentials,
           IList<MyEmailMessage> emailMessages,
           Guid exchangeConfigurationId)
        {
            if (credentials == null) throw new ArgumentNullException("credentials");
            if (emailMessages == null) throw new ArgumentNullException("emailMessages");
            //if (emailMessageWriteRepository == null) throw new ArgumentNullException("emailMessageWriteRepository");

            if (emailMessages.Any())
            {
                var exchangeService = _exchangeService.GetClient(credentials);
                foreach (var email in emailMessages)
                {
                    var exchangeEmail = GetUnreadEmail(email.ItemId, exchangeService);
                    try
                    {
                        //emailMessageWriteRepository.Save(email);
                        MarkEmail(exchangeEmail, true);
                        if (exchangeEmail.Id != null && exchangeEmail.Id.UniqueId != null)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"Saved email in DB. Id: {{0}}",
                                exchangeEmail.Id.UniqueId);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"An error happened while setting email {email.Id} to read. {ex}");

                        MarkEmail(exchangeEmail, false);

                        throw;
                    }
                }
            }
        }

        #region Helpers

        private void MarkEmail(EmailMessage exchangeEmail, bool isRead)
        {
            exchangeEmail.IsRead = isRead;
            _exchangeService.UpdateEmail(exchangeEmail);
        }

        private static EmailMessageWrapper GetEmailMessageWrapper(EmailMessage email, Guid exchangeConfigurationId)
        {
            var receivedDate = DateTime.UtcNow;
            email.Load(new PropertySet(ItemSchema.DateTimeReceived, EmailMessageSchema.ReceivedBy));
            try
            {
                receivedDate = email.DateTimeReceived;
            }
            catch
            {
                try
                {
                    receivedDate = email.DateTimeSent;
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine(new Exception("Cannot retrieve date from email"));
                }
            }
            return new EmailMessageWrapper
            {
                ExchangeConfigurationId = exchangeConfigurationId,
                ItemId = email.Id.UniqueId,
                EmlContent = GetEmlContent(email),
                ReceivedDate = receivedDate
            };
        }

        private EmailMessage GetUnreadEmail(string itemId, ExchangeService client)
        {
            //TODO: check why we need to get the email body from the EWS to set the email to read!
            var isReadPropertySet = new PropertySet(BasePropertySet.FirstClassProperties, EmailMessageSchema.IsRead,
                ItemSchema.Body)
            {
                RequestedBodyType = BodyType.Text
            };
            return _exchangeService.GetEmailByItemId(itemId, isReadPropertySet, client);
        }

        private static byte[] GetEmlContent(Item emailMessage)
        {
            byte[] emlContent = null;
            if (emailMessage != null)
            {
                emailMessage.Load(new PropertySet(ItemSchema.MimeContent));
                var mimeContent = emailMessage.MimeContent;
                emlContent = new byte[mimeContent.Content.Length];
                using (var memoryStream = new MemoryStream(emlContent))
                {
                    memoryStream.Write(mimeContent.Content, 0, mimeContent.Content.Length);
                }
            }
            return emlContent;
        }

        private static readonly SearchFilter.IsEqualTo UnreadEmailFilter =
            new SearchFilter.IsEqualTo(EmailMessageSchema.IsRead, false);


        private static FolderId GetInboxFolder(ExchangeConfigurationCredentials credentials)
        {
            return GetFolder(credentials, WellKnownFolderName.Inbox);
        }

        private static FolderId GetFolder(ExchangeConfigurationCredentials credentials, WellKnownFolderName folder)
        {
            FolderId result;
            var emailAddressValidator = new EmailAddressAttribute();
            if (emailAddressValidator.IsValid(credentials.ExchangeName))
            {
                var mailBox = new Mailbox(credentials.ExchangeName);
                result = new FolderId(folder, mailBox);
            }
            else
            {
                result = new FolderId(folder);
            }
            return result;
        }
        #endregion

    }
}
