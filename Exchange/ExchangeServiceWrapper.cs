using Exchange.Interfaces;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Exchange
{
    public class ExchangeServiceWrapper : IExchangeServiceWrapper
    {

        private const ExchangeVersion ExchangeServerVersion = ExchangeVersion.Exchange2007_SP1;
        private readonly ITraceListener _traceListener;
        private readonly bool _traceEnabled;

        //public ExchangeServiceWrapper()
        //{
        //    _traceEnabled = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnableExchangeTracing"]);

        //    if (_traceEnabled)
        //    {
        //        _traceListener = new TextTraceListener(System.Configuration.ConfigurationManager.AppSettings["ExchangeTracePath"]);
        //    }
        //}

        public IList<EmailMessage> FindItems(ExchangeConfigurationCredentials credentials, FolderId folderId, SearchFilter.SearchFilterCollection filter, ItemView itemView)
        {
            var client = GetClient(credentials);
            
            client.Timeout = 900000; // 15 minutes in milliseconds

            return client.FindItems(folderId, filter, itemView)
            .Cast<EmailMessage>()
            .ToList();
        }

        public EmailMessage GetEmailByItemId(string itemId, PropertySet isReadPropertySet, ExchangeService client)
        {
            return EmailMessage.Bind(client, new ItemId(itemId), isReadPropertySet);
        }

        public void UpdateEmail(EmailMessage email)
        {
            email.Update(ConflictResolutionMode.AutoResolve);
        }

        public void SendAndSaveCopy(EmailMessage emailMessage, FolderId folderId)
        {
            emailMessage.SendAndSaveCopy(folderId);
        }

        public void Send(EmailMessage emailMessage)
        {
            emailMessage.Send();
        }

        public void Save(EmailMessage emailMessage, FolderId folderId)
        {
            emailMessage.Save(folderId);
        }

        public EmailMessage GetEmailMessage(ExchangeConfigurationCredentials credentials)
        {
            return new EmailMessage(GetClient(credentials));
        }

        public ExchangeService GetClient(ExchangeConfigurationCredentials credentials)
        {
            if (credentials == null) { throw new ArgumentNullException("credentials"); }

            //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;


            var client = new ExchangeService(ExchangeServerVersion)
            {
                Url = new Uri(credentials.ConnectionUrl, UriKind.Absolute)
            };
    

            if (true)
            {
                client.TraceEnabled = true;
                client.TraceListener = new TextTraceListener("C:\\Temp\\Globomantics\\exchangeTrace.txt"); ;
            }

            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol =
            //        SecurityProtocolType.Tls12;

            var proxy = new WebProxy();
            proxy.UseDefaultCredentials = false;
            proxy.BypassProxyOnLocal = true;
            proxy.Address = new Uri("http://de.coia.siemens.net:9400");
            proxy.BypassList = new[] { "webmail-emea.siemens.net", "mail-de.siemens.net" };
            client.WebProxy = proxy;

            client.UseDefaultCredentials = true;
            client.PreAuthenticate = false;

            client.Credentials = new WebCredentials(credentials.Username
                                                        , credentials.Password
                                                        , credentials.Domain);

            return client;
        }
    }

    public class TextTraceListener : ITraceListener
    {
        private readonly string _tracePath;
        private static readonly object Locker = new object();
        public int CountLockedThreads { get; private set; }

        public TextTraceListener(string tracePath)
        {
            _tracePath = tracePath;
            if (!Directory.Exists(tracePath))
            {
                var directoryPath = Path.GetDirectoryName(tracePath);
                Directory.CreateDirectory(directoryPath);
            }
            using (File.CreateText(tracePath))
            {

            }
        }

        public void Trace(string traceType, string traceMessage)
        {
            try
            {
                lock (Locker)
                {
                    using (var writer = File.AppendText(_tracePath))
                    {
                        writer.WriteLine(traceMessage);
                    }
                }
            }
            catch (IOException ex)
            {
                CountLockedThreads++;
                System.Diagnostics.Debug.WriteLine(new IOException(ex.Message));
            }
        }
    }
}
