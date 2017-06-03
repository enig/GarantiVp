﻿namespace GarantiVp.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using GarantiVP;
    using System.Diagnostics;

    [TestClass]
    public class VPTests
    {
        //GARANTI VPos definations
        private const string MerchandID = "7000679";
        private const string SubMerchandID = "";

        //GARANTI Terminal definations
        private const string TerminalID_For_XML = "30691244";
        private const string TerminalID_For_3D = "30691297";
        private const string TerminalID_For_3D_PAY = "30691298";
        private const string TerminalID_For_3D_OOS_PAY = "30691299";
        private const string TerminalID_For_OOS_PAY = "30691300";
        private const string TerminalID_For_3D_FULL = "30691301";

        private const string ProvUserID_For_XML = "PROVAUT";
        private const string ProvUserID_For_3D = "GARANTI";
        private const string ProvUserPassword = "123qweASD";

        private const string Securekey = "12345678";

        //GARANTI VPos test configuration
        private string TerminalID = TerminalID_For_3D;
        private string UserID = ProvUserID_For_XML;
        private string UserPassword = ProvUserPassword;

        //Credit card details
        private const string credit_card_number = "4824894728063019";
        private const string credit_card_cvv2 = "959";
        private const int credit_card_month = 6;
        private const int credit_card_year = 17;

        //Credit card details for 3D
        private const string credit_card_number_for_3D = "4282209004348015";
        private const string credit_card_cvv2_for_3D = "123";
        private const int credit_card_month_for_3D = 2;
        private const int credit_card_year_for_3D = 15;

        //Order details
        static string OrderId = "cc5f63d899e64f39b996b1e9156a270e";
        static string OrderIdForCancel; //Getting SalesTest
        static string OrderRefNumberForCancel; //Getting SalesTest
        static string OrderIdForRefund; //Getting SalesWithDetailsTest
        static string OrderIdForRefundCancel; //Getting RefundTest
        static string OrderRefNumberForRefundCancel; //Getting RefundTest
        static string OrderIdForPreAuthSales; //Getting PreAuthSales
        static string OrderRefNumberForPreaAuthSales; //Getting PreAuthSales
        static bool PreAuthCancelTestSuccess;

        //Order address details
        private const string order_address_name = "TÜBİTAK";
        private const string order_address_city = "Ankara";
        private const string order_address_district = "Çankaya";
        private const string order_address_text = "Remzi Oğuz Arık Mah., Tunus Cd. No:80";
        private const string order_address_postalCode = "06540";
        private const string order_address_phone = "+903124685300";

        //Customer details
        private const string customer_email = "eticaret@garanti.com.tr";
        private const string customer_ipAddress = "192.168.0.1";

        private void ValidateResult(GVPSResponse _pay)
        {
            Debug.WriteLine("Request: " + _pay.RawRequest);
            Debug.WriteLine("Response: " + _pay.RawResponse);

            Debug.WriteLine("Message: " + _pay.Transaction.Response.Message);
            Debug.WriteLine("ErrorMsg: " + _pay.Transaction.Response.ErrorMsg);
            Debug.WriteLine("SysErrMsg: " + _pay.Transaction.Response.SysErrMsg);

            Debug.WriteLine("OrderID: " + ((_pay.Order == null) ? "" : _pay.Order.OrderID));
            Debug.WriteLine("RetrefNum: " + _pay.Transaction.RetrefNum);
            Debug.WriteLine("AuthCode: " + _pay.Transaction.AuthCode);
            Debug.WriteLine("SequenceNum: " + _pay.Transaction.SequenceNum);
            Debug.WriteLine("BatchNum: " + _pay.Transaction.BatchNum);
            if ((_pay.Transaction.HostMsgList != null) && (_pay.Transaction.HostMsgList.HostMsg != null))
            {
                foreach (string item in _pay.Transaction.HostMsgList.HostMsg)
                {
                    Debug.WriteLine("Bank message: " + item);
                }
            }
            var ProvDate = _pay.Transaction.ProvDate;
            object ProvisionDate = null;
            if (!string.IsNullOrWhiteSpace(ProvDate))
            { 
                ProvisionDate = new DateTime(int.Parse(ProvDate.Substring(0, 4)), int.Parse(ProvDate.Substring(4, 2)), int.Parse(ProvDate.Substring(6, 2)), int.Parse(ProvDate.Substring(9, 2)), int.Parse(ProvDate.Substring(12, 2)), int.Parse(ProvDate.Substring(15, 2)));
            }
            Debug.WriteLine("Provision date: " + ProvisionDate);
            if (_pay.Transaction.RewardInqResult != null)
            {
                if (_pay.Transaction.RewardInqResult.ChequeList != null)
                {
                    //TODO _pay.Transaction.RewardInqResult.ChequeList childs
                }
                if ((_pay.Transaction.RewardInqResult.RewardList != null) && (_pay.Transaction.RewardInqResult.RewardList.Reward != null))
                {
                    foreach (GVPSResponseReward item in _pay.Transaction.RewardInqResult.RewardList.Reward)
                    {
                        Debug.WriteLine("Reward\n Type: {0}\n Used: {1}\n Earned:{2}", item.Type, (item.TotalAmount / 100), (item.LastTxnGainAmount / 100));
                    }
                }

            }

            Assert.AreEqual("00", _pay.Transaction.Response.Code);
            Assert.AreEqual("Approved", _pay.Transaction.Response.Message);
        }


        private string GenerateMD5(string val)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var md5Bytes = System.Text.Encoding.UTF8.GetBytes(val);
            var md5Hash = md5.ComputeHash(md5Bytes, 0, md5Bytes.Length);
            var ret = BitConverter.ToString(md5Hash).Replace("-", "");
            return ret;
        }

        [TestInitialize()]
        private void Initial()
        {
            OrderId = Guid.NewGuid().ToString("N"); //GenerateMD5(DateTime.Now.ToString());
            OrderIdForCancel = OrderId;
        }
        
        [TestMethod]
        public void SalesTest()
        {
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, UserID, UserPassword, SubMerchandID)
                                    .Customer(customer_email, customer_ipAddress)
                                    .CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                    .Order(Guid.NewGuid().ToString("N"))
                                    .Amount(1234.567, GVPSCurrencyCodeEnum.TRL)
                                    .Sales();
            ValidateResult(_pay);
            OrderIdForCancel = _pay.Order.OrderID;
            OrderRefNumberForCancel = _pay.Transaction.RetrefNum;
        }

        [TestMethod]
        public void SalesWithDetailsTest()
        {
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, UserID, UserPassword, SubMerchandID)
                                    .Customer(customer_email, customer_ipAddress)
                                    .CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                    .Order(Guid.NewGuid().ToString("N"))
                                    .AddOrderAddress(GVPSAddressTypeEnum.Billing, order_address_city, order_address_district, order_address_text , order_address_phone, null, null, order_address_name, order_address_postalCode)
                                    .AddOrderItem(1, "0001", "ProductA ğüşiöçĞÜŞİÖÇ", 1.456, 3.456, "product A ğüşiöçĞÜŞİÖÇ description")
                                    .AddOrderItem(2, "0002", "ProductB", 1.4, 1.1, "product B description")
                                    .AddOrderComment(1, "COM1 ğüşiöçĞÜŞİÖÇ")
                                    .Amount(95, GVPSCurrencyCodeEnum.TRL)
                                    .Sales();
            ValidateResult(_pay);
            OrderIdForRefund = _pay.Order.OrderID;
        }

        [TestMethod]
        public void Sales_USD_Test()
        {
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, UserID, UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                    .Order(Guid.NewGuid().ToString("N"))
                                    .Amount(95, GVPSCurrencyCodeEnum.USD)
                                    .Sales();

            ValidateResult(_pay);
        }

        [TestMethod]
        public void SalesWithInstallmentTest()
        {
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                    .Order(Guid.NewGuid().ToString("N"))
                                    .Amount(95)
                                    .Installment(2)
                                    .Sales();

            ValidateResult(_pay);
        }

        [TestMethod]
        public void Delay_SalesTest()
        {
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                    .Order(Guid.NewGuid().ToString("N"))
                                    .Delay(10)
                                    .Amount(95)
                                    .Sales();

            ValidateResult(_pay);
        }

        [TestMethod]
        public void DownPaymentRate_SalesTest()
        {
            var _pay = new GarantiVPClient().Test(true)
                                        .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                            .Customer(customer_email, customer_ipAddress)
                                            .CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                            .Order(Guid.NewGuid().ToString("N"))
                                            .DownPaymentRate(5)
                                            .Amount(95)
                                            .Sales();

            Debug.WriteLine("Request: " + _pay.RawRequest);
            Debug.WriteLine("Response: " + _pay.RawResponse);

            Debug.WriteLine(_pay.Transaction.Response.Message);
            Debug.WriteLine(_pay.Transaction.Response.ErrorMsg);
            Debug.WriteLine(_pay.Transaction.Response.SysErrMsg);

            Assert.AreEqual("00", _pay.Transaction.Response.Code);
            Assert.AreEqual("Approved", _pay.Transaction.Response.Message);
        }

        [TestMethod]
        public void CancelTest()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (string.IsNullOrWhiteSpace(OrderIdForCancel) && (sw.ElapsedMilliseconds < 10000))
            {
                System.Threading.Thread.SpinWait(1000);
            }
            sw.Stop();
            sw = null;
            if (string.IsNullOrWhiteSpace(OrderIdForCancel))
                throw new ArgumentNullException("orderIdForCancel");
            if (string.IsNullOrWhiteSpace(OrderIdForCancel))
                throw new ArgumentNullException("OrderRefNumberForCancel");
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVRFN", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .Order(OrderIdForCancel)
                                    .Amount(95)
                                    .Cancel(OrderRefNumberForCancel);

            ValidateResult(_pay);
        }

        [TestMethod]
        public void RefundTest()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (string.IsNullOrWhiteSpace(OrderIdForRefund) && (sw.ElapsedMilliseconds < 10000))
            {
                System.Threading.Thread.SpinWait(1000);
            }
            sw.Stop();
            sw = null;
            if (string.IsNullOrWhiteSpace(OrderIdForRefund))
                throw new ArgumentNullException("orderIdForRefund");
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .Order(OrderIdForRefund)
                                    .Amount(95)
                                    .Refund();

            ValidateResult(_pay);
            OrderIdForRefundCancel = _pay.Order.OrderID;
            OrderRefNumberForRefundCancel = _pay.Transaction.RetrefNum;
        }

        [TestMethod]
        public void RefundCancelTest()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (string.IsNullOrWhiteSpace(OrderRefNumberForRefundCancel) && (sw.ElapsedMilliseconds < 10000))
            {
                System.Threading.Thread.SpinWait(1000);
            }
            sw.Stop();
            sw = null;
            if (string.IsNullOrWhiteSpace(OrderRefNumberForRefundCancel))
                throw new ArgumentNullException("OrderRefNumberForRefundCancel");
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .Order(OrderIdForRefundCancel)
                                    .Amount(95)
                                    .RefundCancel(OrderRefNumberForRefundCancel);

            ValidateResult(_pay);
        }

        [TestMethod]
        public void PreauthSalesTest()
        {
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                    .Order(Guid.NewGuid().ToString("N"))
                                    .Amount(95)
                                    .Preauth();

            ValidateResult(_pay);
            OrderIdForPreAuthSales = _pay.Order.OrderID;
            OrderRefNumberForPreaAuthSales = _pay.Transaction.RetrefNum;
        }

        [TestMethod]
        public void PostauthSalesTest()
        {
            var sw = new Stopwatch();
            sw.Start();
            while ((!PreAuthCancelTestSuccess) && (sw.ElapsedMilliseconds < 10000))
            {
                System.Threading.Thread.SpinWait(1000);
            }
            sw.Stop();
            sw = null;
            OrderIdForPreAuthSales = null;
            OrderRefNumberForPreaAuthSales = null;
            PreauthSalesTest();
            if (string.IsNullOrWhiteSpace(OrderIdForPreAuthSales))
                throw new ArgumentNullException("OrderIdForPreAuthSales");
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    //.CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                    .Order(OrderIdForPreAuthSales)
                                    .Amount(95)
                                    .Postauth(OrderRefNumberForPreaAuthSales);

            ValidateResult(_pay);

        }

        [TestMethod]
        public void PostauthSalesCancelTest()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (string.IsNullOrWhiteSpace(OrderIdForPreAuthSales))
            {
                System.Threading.Thread.SpinWait(1000);
            }
            sw.Stop();
            sw = null;
            OrderIdForPreAuthSales = null;
            OrderRefNumberForPreaAuthSales = null;
            PreauthSalesTest();
            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .Order(OrderIdForPreAuthSales)
                                    .Amount(95)
                                    .PostauthCancel(OrderRefNumberForPreaAuthSales);

            ValidateResult(_pay);
            PreAuthCancelTestSuccess = true;
        }

        [TestMethod]
        public void TCKNVerificationTest()
        {
            var tc_kimlik_no = "000000000000";

            var _pay = new GarantiVPClient()
                                    .Test(true)
                                    .Company(TerminalID, MerchandID, "PROVAUT", UserPassword)
                                    .Customer(customer_email, customer_ipAddress)
                                    .Order(Guid.NewGuid().ToString("N"))
                                    .CreditCard(credit_card_number, credit_card_cvv2, credit_card_month, credit_card_year)
                                    .Amount(1)
                                    .Verification(tc_kimlik_no);

            ValidateResult(_pay);
        }
    }
}
