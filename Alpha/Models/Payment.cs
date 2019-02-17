using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using Alpha.Models;
using System.ComponentModel.DataAnnotations;

namespace Alpha.Models
{
    [Table("Payments")]
    public class Payment
    {
        public int PaymentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public virtual Order Order { get; set; }

        public string RequestId { get; set; }
        public string PayerIPAddress { get; set; }
        public string SaferPayToken { get; set; }

        public string PaymentGatewayNotifyURL { get; set; }
    }

    public enum PaymentStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Approved = 2,
        Failed = 3
    }

    [Table("PaymentsSavedCards")]
    public class SavedPaymentMethod
    {
        [Key]
        public int SavedPaymentId { get; set; }
        public string PaymentToken { get; set; }
        public ApplicationUser User { get; set; }
        public string MaskedCardIdNbr { get; set; }
        public string EndDate { get; set; }
    }


    public class SaferPayInitializeRequest
    {
        public SaferPayRequestHeader RequestHeader { get; set; }
        public SaferPayPayment Payment { get; set; }
        public SaferPayPayer Payer { get; set; }
        public SaferPayReturnUrls ReturnUrls { get; set; }

        public string TerminalId { get; set; }
    }

    public class SaferPayErrorMessage
    {
        public SaferPayResponseHeader ResponseHeader { get; set; }
        public string Behavior { get; set; }
        public string ErrorName { get; set; }
        public string ErrorMessage { get; set; }
        public string[] ErrorDetail { get; set; }
    }

    public class SaferPayInitializeResponse
    {
        public SaferPayResponseHeader ResponseHeader { get; set; }
        public string Token { get; set; }
        public string Expiration { get; set; }
        public string RedirectUrl { get; set; }
    }

    public class SaferPayPaymentPageAssertRequest
    {
        public SaferPayRequestHeader RequestHeader { get; set; }
        public string Token { get; set; }
    }

    public class SaferPayPaymentPageAssertResponse
    {
        public SaferPayResponseHeader ResponseHeader { get; set; }
        public SaferPayTransaction Transaction { get; set; }
        public SaferPayPaymentMeans PaymentMeans { get; set; }
        public SaferPayLiability Liability { get; set; }
    }


    public class SaferPayRequestHeader
    {
        public string SpecVersion { get; set; }
        public string CustomerId { get; set; }
        public string RequestId { get; set; }
        public int RetryIndicator { get; set; }
    }

    public class SaferPayPayment
    {
        public SaferPayAmount Amount { get; set; }
        public string Description { get; set; }
        public string OrderId { get; set; }
    }
    public class SaferPayAmount
    {
        public string Value { get; set; }
        public string CurrencyCode { get; set; }
    }
    public class SaferPayPayer
    {
        public string LanguageCode { get; set; }
        public string IpAddress { get; set; }
    }
    public class SaferPayReturnUrls
    {
        public string Success { get; set; }
        public string Fail { get; set; }
        public string Abort { get; set; }
    }

    public class SaferPayResponseHeader
    {
        public string SpecVersion { get; set; }
        public string RequestId { get; set; }
    }

    public class SaferPayTransaction
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public string Id { get; set; }
        public string Date { get; set; }
        public SaferPayAmount Amount { get; set; }
        public string AcquirerName { get; set; }
        public string AcquirerReference { get; set; }
        public string SixTransactionReference { get; set; }
        public string ApprovalCode { get; set; }
    }

    public class SaferPayPaymentMeans
    {
        public SaferPayPayBrand Brand { get; set; }
        public string DisplayText { get; set; }
        public SaferPayPayCard Card { get; set; }

    }

    public class SaferPayPayBrand
    {
        public string PaymentMethod  { get; set;}
        public string Name { get; set; }
    }

    public class SaferPayPayCard
    {
        public string MaskedNumber { get; set; }
        public string ExpYear { get; set; }
        public string ExpMonth { get; set; }
        public string HolderName { get; set; }
        public string CountryCode { get; set; }
    }

    public class SaferPayLiability
    {
        public bool LiabilityShift { get; set; }
        public string LiableEntity { get; set; }
        public SaferPayThreeDs ThreeDs { get; set; }
        public SaferPayFraudFree FraudFree { get; set; }
    }

    public class SaferPayThreeDs
    {
        public bool Authenticated { get; set; }
        public bool LiabilityShift { get; set; }
        public string Xid { get; set; }
        public string VerificationValue { get; set; }
    }

    public class SaferPayFraudFree
    {
        public bool Authenticated { get; set; }
        public bool LiabilityShift { get; set; }
        public decimal Score { get; set; }
    }











}
