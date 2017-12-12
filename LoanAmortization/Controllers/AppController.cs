using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LoanAmortization.Models;
using PagedList;

namespace LoanAmortization.Controllers
{
    public class AppController : Controller
    {
        // GET: App
        public ViewResult Index(int? LoanPrincipal,int? NoOfPaymentYears,
            int? NoOfYearlyInstallmentalPayments, int? InterestRate,
            string PaymentStartDate , string sort, int? page)
        {
            SearchFormView loan = new SearchFormView();
            var model = new List<LoanPaymentView>();
            if (!object.Equals(PaymentStartDate, null) && !object.Equals(NoOfYearlyInstallmentalPayments, null) &&
                !object.Equals(LoanPrincipal, null)&& !object.Equals(NoOfPaymentYears, null))
            {
                loan.LoanPrincipal = (LoanPrincipal ?? 0);
                loan.NoOfPaymentYears = (NoOfPaymentYears ?? 0);
                loan.NoOfYearlyInstallmentalPayments = (NoOfYearlyInstallmentalPayments ?? 12);
                loan.InterestRate = (InterestRate ?? 1);
                loan.PaymentStartDate = object.Equals(PaymentStartDate, null) ? DateTime.Now.ToString() : PaymentStartDate;

                model = GetLoanViewData(loan);
                ViewBag.Principal = LoanPrincipal;
                ViewBag.PaymentStartDate = PaymentStartDate;
                ViewBag.InterestRate = InterestRate;
                ViewBag.YearsOfPayment = NoOfPaymentYears;
                ViewBag.PaymentsYearly = NoOfYearlyInstallmentalPayments;
                ViewBag.Interest = model.Last().CummulativeInterestAmount;
                ViewBag.Payment = model.Last().ScheduledPayment;
                ViewBag.PaymentNumber = model.Last().PaymentNo;
                
            }
            else
            {
                if (!object.Equals(ViewBag.Principal, null) && !object.Equals(page, null))
                {
                    loan.LoanPrincipal = ViewBag.Principal;
                    loan.NoOfPaymentYears = ViewBag.NoOfPaymentYears;
                    loan.NoOfYearlyInstallmentalPayments = ViewBag.PaymentsYearly;
                    loan.InterestRate = ViewBag.InterestRate;
                    loan.PaymentStartDate = ViewBag.PaymentStartDate;
                }
                else
                {
                    ViewBag.Status = "Please Fill all the Compulsory Fields";
                }
                

            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(model.ToPagedList(pageNumber,pageSize));
        }
        public ActionResult Loan()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Loan(string Principal,string PaymentYears)
        {
            ViewBag.Principal = Principal;
            ViewBag.PaymentYears = PaymentYears;
            return View();
        }

        private List<LoanPaymentView> GetLoanViewData(SearchFormView loan)
        {
            List<LoanPaymentView> listloan = new List<LoanPaymentView>();
            int TotalNumberofPayments = loan.NoOfPaymentYears * loan.NoOfYearlyInstallmentalPayments;
            double scheduledpayment = GetScheduledPayment(TotalNumberofPayments, loan.LoanPrincipal, loan.InterestRate);
            double startingbalance; double cummulativeInterest = 0;
            DateTime lastpaymenttime = DateTime.Parse(loan.PaymentStartDate);
            startingbalance = loan.LoanPrincipal;

            for (int i = 1; i <= TotalNumberofPayments; i++)
            {
                LoanPaymentView payment = new LoanPaymentView();
                payment = GetPayment(startingbalance, loan.InterestRate, scheduledpayment);
                payment.PaymentNo = i;
                payment.ScheduledPayment = scheduledpayment.ToString("n2"); ;
                cummulativeInterest += Convert.ToDouble(payment.InterestAmount);
                payment.CummulativeInterestAmount = cummulativeInterest.ToString("n2");
                payment.paymentDate = lastpaymenttime.AddMonths(1).ToShortDateString();
                startingbalance = Convert.ToDouble(payment.EndingBalance);
                lastpaymenttime = DateTime.Parse(payment.paymentDate);
                listloan.Add(payment);

            }

            return listloan;
        }

        private LoanPaymentView GetPayment(double startingbalance, double InterestRate, double scheduledpayment)
        {
            LoanPaymentView payment = new LoanPaymentView();
            // payment.ScheduledPayment = scheduledpayment;
            payment.BeginningBalance = startingbalance.ToString("n2");

            double firstmul = 100 * 12;
            double monthlyInterest = startingbalance * InterestRate / firstmul;
            double _interest, _principal, _ending;
            _interest = Math.Round(monthlyInterest, 2);
            _principal = Math.Round((scheduledpayment - monthlyInterest), 2);
            _ending = Math.Round(startingbalance - _principal, 2);
            payment.InterestAmount = _interest.ToString("n2");
            payment.PrincipalAmount = _principal.ToString("n2");
            payment.EndingBalance = _ending.ToString("n2");
            return payment;
        }

        private double GetScheduledPayment(int totalNumberofPayments, double loanPrincipal, double interestRate)
        {
            double firstmul = 100 * 12;
            double intRate = interestRate / firstmul;
            double monthly = (loanPrincipal * (Math.Pow((1 + intRate), totalNumberofPayments)) *
                intRate / (Math.Pow((1 + intRate), totalNumberofPayments) - 1));
            monthly = Math.Round(monthly, 2); ;
            return monthly;
        }

    }
}