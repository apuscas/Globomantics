using System;

namespace Exchange
{
    public class EmailMessageWrapper
    {
        public string ItemId { get; set; }

        public Guid ExchangeConfigurationId { get; set; }

        public byte[] EmlContent { get; set; }

        public DateTime ReceivedDate { get; set; }

        public string Address { get; set; }

        public string DisplayName { get; set; }
    }
}
