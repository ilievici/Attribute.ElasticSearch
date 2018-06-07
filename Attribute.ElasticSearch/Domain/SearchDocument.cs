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
        [ElasticMeta("TYPE", false)]
        public string Type { get; set; }

        [Number(NumberType.Long, Name = nameof(EntityId))]
        [ElasticMeta("CONFIRMATION", false)]
        public long EntityId { get; set; }

        [Keyword(Name = nameof(CollectorId))]
        public string CollectorId { get; set; }

        [Keyword(Name = nameof(SiteId))]
        public string SiteId { get; set; }

        [Keyword(Name = nameof(ChangedBy))]
        public string ChangedBy { get; set; }

        [Keyword(Name = nameof(Channel))]
        public string Channel { get; set; }

        [Keyword(Name = nameof(PaymentType))]
        public string PaymentType { get; set; }

        [Keyword(Name = nameof(PaymentTypeWeight))]
        public string PaymentTypeWeight { get; set; }

        [Number(NumberType.Double, Name = nameof(Fee))]
        public double Fee { get; set; }

        [Keyword(Name = nameof(FirstName))]
        [ElasticMeta("FIRST_NAME", true)]
        public string FirstName { get; set; }

        [Keyword(Name = nameof(LastName))]
        [ElasticMeta("LAST_NAME", true)]
        public string LastName { get; set; }

        [Keyword(Name = nameof(CreditAccount))]
        [ElasticMeta("CREDIT_ACCOUNT", false, true)]
        public string CreditAccount { get; set; }

        [Keyword(Name = nameof(DebitAccount))]
        public string DebitAccount { get; set; }

        [Keyword(Name = nameof(DebitAccountMask))]
        public string DebitAccountMask { get; set; }

        [Keyword(Name = nameof(Emails))]
        public List<string> Emails { get; set; }

        [Keyword(Name = nameof(Phones))]
        public List<string> Phones { get; set; }

        [Date(Name = nameof(EnteredDate))]
        [ElasticMeta("ENTERED_DATE", false)]
        public DateTime? EnteredDate { get; set; }

        [Keyword(Name = nameof(Status))]
        public string Status { get; set; }

        [Boolean(Name = nameof(HasAuditDetails))]
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
        public double Amount { get; set; }

        [Nested(Name = nameof(Mis))]
        public List<MisFieldNestedTypes> Mis { get; set; }
    }

    public class PaymentSearchDocument : SearchDocument
    {
        public PaymentSearchDocument()
        {
            Mis = new List<MisFieldNestedTypes>();
        }

        [Number(NumberType.Double, Name = nameof(Amount))]
        [ElasticMeta("AMOUNT", false)]
        public double Amount { get; set; }

        [Nested(Name = nameof(Mis))]
        public List<MisFieldNestedTypes> Mis { get; set; }

        [Date(Name = nameof(ActuatedDate))]
        public DateTime? ActuatedDate { get; set; }

        [Date(Name = nameof(ScheduledDate))]
        public DateTime? ScheduledDate { get; set; }
    }

    public class PbtSearchDocument : SearchDocument
    {
        [Number(Name = nameof(EnabledCode))]
        public int? EnabledCode { get; set; }

        [Keyword(Name = nameof(CellNumber))]
        [ElasticMeta("CELL_NUMBER", false)]
        public string CellNumber { get; set; }
    }
}