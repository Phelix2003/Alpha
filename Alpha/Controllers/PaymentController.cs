using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Alpha.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Alpha.Controllers
{
    public class PaymentController : Controller
    {
        private ApplicationDbContext _dbManager;
        public ApplicationDbContext DbManager
        {
            get
            {
                return _dbManager ?? HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _dbManager = value;
            }
        }


        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private static readonly HttpClient client = new HttpClient();


        // GET: Payment
        public ActionResult Index()
        {
            return View();
        }



        // Initialize the payment through the SaferPay Gateway
        public async Task<ActionResult> ValidateAndPayOrderBySaferPay(int OrderId)
        {
            Order order = await DbManager.Orders.FirstOrDefaultAsync(r => r.Id == OrderId);

            if (order != null)
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(SaferPayGetGateWayURL(GateWayURL.Initialize));
                httpWebRequest.ContentType = "application/Json";
                httpWebRequest.Method = "POST";

                // For Debug. To remove
                if(order.Payment != null)
                {
                    Payment payment = order.Payment;
                    DbManager.Payments.Remove(payment);
                    await DbManager.SaveChangesAsync();
                }

                // Configure basic Authentication
                string username = ConfigurationManager.AppSettings["SaferPayUserName"];
                string password = ConfigurationManager.AppSettings["SaferPayPassword"];

                string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));

                httpWebRequest.Headers.Add("Authorization", "Basic " + svcCredentials);

                //Generate a Unique token for this transaction
                string requestToken = ((DateTime.Now).Ticks).ToString();

                // Creates SaferPay Initial JSonCommand
                string json = SaferPayTransactionInitializeJSon(order.OrderedItems.Sum(r => r.ConfiguredPrice()),
                    requestToken,
                    order.Id.ToString(),
                    "Payer votre commande chez Easio",
                    GetUSerIPAddress());
                if(ConfigurationManager.AppSettings["ConsolDebug"] == "on")
                {
                    System.Diagnostics.Debug.WriteLine("Payment request sent to SaferPay (JSon):");
                    System.Diagnostics.Debug.WriteLine(json);
                }
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    await streamWriter.WriteAsync(json);
                    await streamWriter.FlushAsync();
                    streamWriter.Close();
                }

                // Send request to SaferPay server
                WebResponse httpWebResponse;
                try
                {
                    httpWebResponse = await httpWebRequest.GetResponseAsync();
                    SaferPayInitializeResponse response;
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = await streamReader.ReadToEndAsync();
                        response = JsonConvert.DeserializeObject<SaferPayInitializeResponse>(result);
                        if (ConfigurationManager.AppSettings["ConsolDebug"] == "on")
                        {
                            System.Diagnostics.Debug.WriteLine("Payment request response from SaferPay (JSon) - ERROR Message:");
                            System.Diagnostics.Debug.WriteLine(result);
                        }
                    }
                    // Creating the payment in DDB
                    Payment payment = new Payment
                    {
                        Order = order,
                        PayerIPAddress = GetUSerIPAddress(),
                        PaymentGatewayNotifyURL = response.RedirectUrl,
                        RequestId = requestToken,
                        PaymentStatus = PaymentStatus.InProgress,
                        SaferPayToken = response.Token
                    };
                    DbManager.Payments.Add(payment);
                    await DbManager.SaveChangesAsync();                

                    // Continue the process through SaferPay payment pages
                    return Redirect(response.RedirectUrl);
                }
                catch (WebException we)
                {
                    // The rpayment request has been refused by SaferPay for technical reasons. 
                    httpWebResponse = we.Response as HttpWebResponse;
                    SaferPayErrorMessage errorMessage;
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = await streamReader.ReadToEndAsync();
                        errorMessage = JsonConvert.DeserializeObject<SaferPayErrorMessage>(result);
                        if (ConfigurationManager.AppSettings["ConsolDebug"] == "on")
                        {
                            System.Diagnostics.Debug.WriteLine("Payment request response from SaferPay (JSon) - ERROR Message:");
                            System.Diagnostics.Debug.WriteLine(result);
                        }
                    }
                    return View("Error");
                }            
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // Verify the payment has been executed. 
        public async Task<ActionResult> SaferPaySuccess(int OrderId)
        {
            Order order = await DbManager.Orders.FirstOrDefaultAsync(r => r.Id == OrderId);
            if(order != null)
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(SaferPayGetGateWayURL(GateWayURL.PaymentPageAssert));
                httpWebRequest.ContentType = "application/Json";
                httpWebRequest.Method = "POST";

                // Configure basic Authentication
                string username = ConfigurationManager.AppSettings["SaferPayUserName"];
                string password = ConfigurationManager.AppSettings["SaferPayPassword"];

                string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));

                httpWebRequest.Headers.Add("Authorization", "Basic " + svcCredentials);

                // Creates SaferPay Initial JSonCommand
                string json = SaferPayPaymentPageAssertJSon(order.Payment.RequestId, order.Payment.SaferPayToken);

                if (ConfigurationManager.AppSettings["ConsolDebug"] == "on")
                {
                    System.Diagnostics.Debug.WriteLine("Payment request sent to SaferPay (JSon):");
                    System.Diagnostics.Debug.WriteLine(json);
                }
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    await streamWriter.WriteAsync(json);
                    await streamWriter.FlushAsync();
                    streamWriter.Close();
                }

                // Send request to SaferPay server
                WebResponse httpWebResponse;
                try
                {
                    httpWebResponse = await httpWebRequest.GetResponseAsync();
                    SaferPayPaymentPageAssertResponse response;
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = await streamReader.ReadToEndAsync();
                        response = JsonConvert.DeserializeObject<SaferPayPaymentPageAssertResponse>(result);
                        if (ConfigurationManager.AppSettings["ConsolDebug"] == "on")
                        {
                            System.Diagnostics.Debug.WriteLine("Payment request response from SaferPay (JSon) - ERROR Message:");
                            System.Diagnostics.Debug.WriteLine(result);
                        }
                    }
                    // Update the payment in DDB
                    if(response.Transaction.Status == "AUTHORIZED" || response.Transaction.Status == "CAPTURED")
                    {
                        order.IsOrderCompleted = true;
                        order.Payment.PaymentStatus = PaymentStatus.Approved;
                        ViewBag.OrderId = OrderId;
                        await DbManager.SaveChangesAsync();
                        return View();
                    }

                    if(response.Transaction.Status == "PENDING")
                    {
                        //TODO to anlyze when this situatin can occures and take correct actions
                        return View("Error");
                    }
                    return View("Error");
                }
                catch (WebException we)
                {
                    // The rpayment request has been refused by SaferPay for technical reasons. 
                    httpWebResponse = we.Response as HttpWebResponse;
                    SaferPayErrorMessage errorMessage;
                    using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        var result = await streamReader.ReadToEndAsync();
                        errorMessage = JsonConvert.DeserializeObject<SaferPayErrorMessage>(result);
                        if (ConfigurationManager.AppSettings["ConsolDebug"] == "on")
                        {
                            System.Diagnostics.Debug.WriteLine("Payment request response from SaferPay (JSon) - ERROR Message:");
                            System.Diagnostics.Debug.WriteLine(errorMessage);
                        }
                    }
                    return View("Error");
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        public ActionResult SaferPayAbort(int OrderId)
        {
            return RedirectToAction("FinalizeOrder", "order", new { OrderId = OrderId }); 
        }

        public ActionResult SaferPayFail(int OrderId)
        {
            return RedirectToAction("FinalizeOrder", "order", new { OrderId = OrderId });
        }

        private string SaferPayGetGateWayURL(GateWayURL Context)
        {

            switch (Context)
            {
                case GateWayURL.Initialize:
                    return ConfigurationManager.AppSettings["SaferPayGatewayBaseURL"] + "/Payment/v1/PaymentPage/Initialize";
                case GateWayURL.PaymentPageAssert:
                    return ConfigurationManager.AppSettings["SaferPayGatewayBaseURL"] + "/Payment/v1/PaymentPage/Assert";
                default:
                    return null;
            }
        }

        private enum GateWayURL
        {
            Initialize = 1,
            PaymentPageAssert = 2

        }

        private string SaferPayTransactionInitializeJSon(decimal? OrderAmount, string RequestID, string OrderId, string Description, string PayerIP)
        {
            if (OrderAmount == null) return null;
            SaferPayInitializeRequest saferPayInitializeJSon = new SaferPayInitializeRequest
            {
                TerminalId = ConfigurationManager.AppSettings["SaferPayTerminalId"],
                RequestHeader = new SaferPayRequestHeader
                {
                    SpecVersion = "1.10",
                    CustomerId = ConfigurationManager.AppSettings["SaferPayCustomerId"],
                    RequestId = RequestID,
                    RetryIndicator = 0
                },
                Payment = new SaferPayPayment
                {
                    Amount = new SaferPayAmount
                    {
                        Value = Convert.ToInt16(OrderAmount*100).ToString(),
                        CurrencyCode = "EUR"            
                    },
                    Description = Description,
                    OrderId = OrderId
                },
                Payer = new SaferPayPayer
                {
                    IpAddress = PayerIP,
                    LanguageCode = "fr"
                },
                ReturnUrls = new SaferPayReturnUrls
                {
                    Success = Url.Action("SaferPaySuccess","Payment", new { OrderId = OrderId } , Request.Url.Scheme),
                    Abort = Url.Action("SaferPayAbort", "Payment", new { OrderId = OrderId }, Request.Url.Scheme),
                    Fail = Url.Action("SaferPayFail", "Payment", new { OrderId = OrderId }, Request.Url.Scheme)
                }
            };          

            return JsonConvert.SerializeObject(saferPayInitializeJSon, Formatting.Indented);
        }

        private string SaferPayPaymentPageAssertJSon(string RequestID, string PaymentToken)
        {
            SaferPayPaymentPageAssertRequest paymentPageAssertRequest = new SaferPayPaymentPageAssertRequest
            {
                RequestHeader = new SaferPayRequestHeader
                {
                    SpecVersion = "1.10",
                    CustomerId = ConfigurationManager.AppSettings["SaferPayCustomerId"],
                    RequestId = RequestID,
                    RetryIndicator = 0
                },
                Token = PaymentToken
            };

            return JsonConvert.SerializeObject(paymentPageAssertRequest, Formatting.Indented);
        }

        protected string GetUSerIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            return "0.0.0.0";
        }
    }
}