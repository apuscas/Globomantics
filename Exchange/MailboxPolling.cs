using Exchange.Interfaces;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Exchange
{
    //https://stacksecrets.com/dot-net-core/scheduled-repeating-task-with-net-core#google_vignette
    public class MailboxPolling : IHostedService
    {
        private readonly IExchangeServiceClient _exchangeServiceClient;
        private readonly Guid _exchangeConfigurationId;
        private Timer _timer;

        public ExchangeConfigurationCredentials Credentials { get; private set; }

        public MailboxPolling(IExchangeServiceClient exchangeServiceClient)
        {
            _exchangeServiceClient = exchangeServiceClient;
            this.Credentials = GetCredential();
            _exchangeConfigurationId = new Guid();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // timer repeates call to RemoveScheduledAccounts every 24 hours.
            _timer = new Timer(
                TriggerEmailPooling,
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1)
            );

            return Task.CompletedTask;
            
        }

        // Call the Stop async method if required from within the app.
        public Task StopAsync(CancellationToken cancellationToken)
        {
            //the timer can be removed if required from StopAsync
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void TriggerEmailPooling(object state)
        {
            var emailMessageWrappers = _exchangeServiceClient.FindUnreadEmails(Credentials);
            var emailMessages = 
                emailMessageWrappers.Select(
                    e =>
                        new MyEmailMessage
                        {
                            Id = Guid.NewGuid(),
                            ItemId = e.ItemId,
                            EmlContent = e.EmlContent,
                            ExchangeConfigurationId = e.ExchangeConfigurationId,
                            ReceivedDate = e.ReceivedDate,
                        }).ToList();
            
            if (emailMessages != null && emailMessages.Any())
            {
                _exchangeServiceClient.ReadEmails(Credentials,
                    emailMessages,
                    _exchangeConfigurationId);
            }           
        }

        private static ExchangeConfigurationCredentials GetCredential()
        {
            var credential = new ExchangeConfigurationCredentials
            {
                Id = Guid.NewGuid(), //e04dfecb-4da0-4b39-a238-f7bd81e32681
                ConnectionUrl = "https://outlook.office365.com/EWS/Exchange.asmx",
                Domain = "ad001.siemens.net",
                ExchangeName = "TestExchangeConection",
                Password = "saltedCaramel8", // "Lqy0bB05wni612a0XBMyiQ==",//EncryptString("saltedCarame8"),
                Username = "andreea.puscas@siemens.com",
            };
            return credential;
        }

        private static string EncryptString(string clearText)
        {
            if (!string.IsNullOrWhiteSpace(clearText))
            {
                var clearTextBytes = Encoding.UTF8.GetBytes(clearText);

                var rijn = SymmetricAlgorithm.Create();
                var ms = new MemoryStream();
                var rgbIv = Encoding.ASCII.GetBytes("lkjhfuefvbhjoiwq");
                var key = Encoding.ASCII.GetBytes("oiturekjhbkjvhguzgweriuzguzrgkac");
                var cs = new CryptoStream(ms, rijn.CreateEncryptor(key, rgbIv), CryptoStreamMode.Write);

                cs.Write(clearTextBytes, 0, clearTextBytes.Length);

                cs.Close();

                return Convert.ToBase64String(ms.ToArray());
            }
            return null;
        }
    }
}
