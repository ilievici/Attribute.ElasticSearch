using System;
using System.Collections.Generic;
using Attribute.ElasticSearch.Attributes;
using Nest;

namespace Attribute.ElasticSearch.Domain
{
    public abstract class SearchDocument
    {
        protected SearchDocument()
        {
            Phones = new List<string>();
            Emails = new List<string>();
        }

        [Keyword(Name = nameof(Type))]
        [ElasticMeta("TYPE")]
        public string Type { get; set; }

        [Number(NumberType.Long, Name = nameof(EntityId))]
        [ElasticMeta("CONFIRMATION")]
        public long EntityId { get; set; }

        [Keyword(Name = nameof(CollectorId))]
        [ElasticMeta("COLLECTOR_ID")]
        public string CollectorId { get; set; }

        [Keyword(Name = nameof(SiteId))]
        [ElasticMeta("SITE_ID")]
        public string SiteId { get; set; }

        [Keyword(Name = nameof(ChangedBy))]
        public string ChangedBy { get; set; }

        [Keyword(Name = nameof(Channel))]
        [ElasticMeta("CHANNEL")]
        public string Channel { get; set; }

        [Keyword(Name = nameof(PaymentType))]
        [ElasticMeta("PAYMENT_TYPE")]
        public string PaymentType { get; set; }

        [Keyword(Name = nameof(PaymentTypeWeight))]
        public string PaymentTypeWeight { get; set; }

        [Number(NumberType.Double, Name = nameof(Fee))]
        [ElasticMeta("FEE")]
        public double Fee { get; set; }

        [Keyword(Name = nameof(FirstName))]
        [ElasticMeta("FIRST_NAME", WildcardSearch = true)]
        public string FirstName { get; set; }

        [Keyword(Name = nameof(LastName))]
        [ElasticMeta("LAST_NAME", WildcardSearch = true)]
        public string LastName { get; set; }

        [Keyword(Name = nameof(CreditAccount))]
        [ElasticMeta("CREDIT_ACCOUNT", IsHased = true)]
        public string CreditAccount { get; set; }

        [Keyword(Name = nameof(DebitAccount))]
        [ElasticMeta("DEBIT_ACCOUNT", IsHased = true)]
        public string DebitAccount { get; set; }

        [Keyword(Name = nameof(DebitAccountMask))]
        public string DebitAccountMask { get; set; }

        [Keyword(Name = nameof(Emails))]
        [ElasticMeta("EMAIL")]
        public List<string> Emails { get; set; }

        [Keyword(Name = nameof(Phones))]
        [ElasticMeta("PHONE", WildcardSearch = true)]
        public List<string> Phones { get; set; }

        [Date(Name = nameof(EnteredDate))]
        [ElasticMeta("ENTERED_DATE")]
        public DateTime? EnteredDate { get; set; }

        [Keyword(Name = nameof(Status))]
        public string Status { get; set; }

        [Boolean(Name = nameof(HasAuditDetails))]
        [ElasticMeta("AUDIT")]
        public bool HasAuditDetails { get; set; }

        [Keyword(Name = nameof(CreditAccountWeight))]
        public string CreditAccountWeight { get; set; }

        public class MisFieldNestedTypes
        {
            [Keyword(Name = nameof(Name))]
            public string Name { get; set; }

            [Keyword(Name = nameof(Value))]
            public string Value { get; set; }
        }
    }

    public class AutopaySearchDocument : SearchDocument
    {
        public AutopaySearchDocument()
        {
            Mis = new List<MisFieldNestedTypes>();
        }

        [Number(NumberType.Double, Name = nameof(Amount))]
        [ElasticMeta("AMOUNT")]
        public double Amount { get; set; }

        [Nested(Name = nameof(Mis))]
        [ElasticMeta("MIS", IsNested = true)]
        public List<MisFieldNestedTypes> Mis { get; set; }
    }
    public class PaymentSearchDocument : SearchDocument
    {
        public PaymentSearchDocument()
        {
            Mis = new List<MisFieldNestedTypes>();
        }

        [Number(NumberType.Double, Name = nameof(Amount))]
        [ElasticMeta("AMOUNT")]
        public double Amount { get; set; }

        [Nested(Name = nameof(Mis))]
        [ElasticMeta("MIS", IsNested = true)]
        public List<MisFieldNestedTypes> Mis { get; set; }

        [Date(Name = nameof(ActuatedDate))]
        [ElasticMeta("ACTUATED_DATE")]
        public DateTime? ActuatedDate { get; set; }

        [Date(Name = nameof(ScheduledDate))]
        [ElasticMeta("SCHEDULED_DATE")]
        public DateTime? ScheduledDate { get; set; }
    }

    public class PbtSearchDocument : SearchDocument
    {
        [Number(Name = nameof(EnabledCode))]
        public int? EnabledCode { get; set; }

        [Keyword(Name = nameof(CellNumber))]
        [ElasticMeta("CELL_NUMBER")]
        public string CellNumber { get; set; }
    }
}