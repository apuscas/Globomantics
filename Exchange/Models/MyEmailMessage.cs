using System;

namespace Exchange
{
    public class MyEmailMessage
    {
        public Guid Id { get; set; }

        public string ItemId { get; set; }

        public Guid ExchangeConfigurationId { get; set; }

        public byte[] EmlContent { get; set; }

        public DateTime ReceivedDate { get; set; }
    }
}
