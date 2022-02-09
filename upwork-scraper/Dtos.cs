using System.Collections.Generic;

namespace upwork_scraper
{
    class Response
    {
        public List<Job> Results { get; set; }
    }

    class Job
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
        public AmountDto Amount { get; set; }
        public string Uid { get; set; }
        public string ProposalsTier { get; set; }
        public string ShortEngagement { get; set; }
        public List<Attrs> Attrs { get; set; }
        public string CipherText { get; set; }
    }

    class AmountDto
    {
        public string CurrencyCode { get; set; }
        public int Amount { get; set; }
    }

    class Attrs
    {
        public string PrettyName { get; set; }
    }
}