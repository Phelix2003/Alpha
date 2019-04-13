using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Alpha.Models;
using Newtonsoft.Json;

namespace Alpha.Helpers.Common
{
    public class BlueSnapPayment
    {
        public string BlueSnapAddVendorJson(Resto resto)
        {
            if (resto == null) return null;

            BlueSnapCreateVendor vendor = new BlueSnapCreateVendor
            {
                email = resto.Owner_email,
                firstName = resto.Owner_FirstName,
                lastName = resto.Owner_LastName,
                phone = resto.PhoneNumber,
                address = resto.Address,
                city = resto.Address_City,
                country = "BE",
                zip = resto.Address_ZIPCode,
                defaultPayoutCurrency = "EUR",
                vendorPrincipal = new BlueSnapVendorPrincipal
                {
                    firstName = resto.Owner_FirstName,
                    lastName = resto.Owner_LastName,
                    address = resto.Owner_Address,
                    city = resto.Owner_City,
                    country = "BE",
                    zip = resto.Owner_ZipCode,
                    dob = resto.Owner_DateOfBirth.Value.Day.ToString("D2") + "-" + resto.Owner_DateOfBirth.Value.Month.ToString("D2") + "-"+resto.Owner_DateOfBirth.Value.Year.ToString(),
                    personalIdentificationNumber = resto.Owner_PersonalIDNumber,
                    driverLicenseNumber = resto.Owner_PassportNumber,
                    email = resto.Owner_email
                },
                vendorAgreement = new BlueSnapVendorAgreement
                {
                    commissionPercent = resto.Payment_commissionPercent
                },
                payoutInfo = new List<BlueSnapPayoutInfo>(){new BlueSnapPayoutInfo
                {
                    payoutType = "SEPA",
                    baseCurrency = "EUR",
                    nameOnAccount = resto.Payment_NameOnAccount,
                    bankAccountType = "CHECKING",
                    bankAccountClass = "PERSONAL",
                    bankName = resto.Payment_BankName,                    
                    swiftBic = resto.Payment_SwiftBIC,
                    country = "BE",
                    city = resto.Payment_BankCity,
                    zip = resto.Payment_BankZip,
                    //bankAccountId = resto.Payment_BankAccountId, //BankAccountId or Iban but not both
                    iban = resto.Payment_BankAccountId,
                    minimalPayoutAmount = resto.Payment_minimalPayoutAmount
                } }
            };

            return JsonConvert.SerializeObject(vendor, Formatting.Indented);
        }
    }
}