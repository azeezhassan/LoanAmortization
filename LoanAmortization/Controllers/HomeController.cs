using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LoanAmortization.Models;

namespace LoanAmortization.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult LoanApp()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoanApp(SearchFormView loan,int pagenumber=1)
        {
            List<LoanPaymentView> view = new List<LoanPaymentView>();
            if (ModelState.IsValid)
            {
                view = GetLoanViewData(loan);
            }
            ViewBag.Search = loan;
            
            ViewBag.PageNumber = pagenumber;
            ViewBag.Principal = loan.LoanPrincipal;
            ViewBag.Interest = view.Last().CummulativeInterestAmount;
            ViewBag.Payment = view.Last().ScheduledPayment;
            ViewBag.PaymentNumber = view.Last().PaymentNo;
            int resultrange,noOfPages;
            if (pagenumber == 1)
            {
                resultrange = 0;
            }
            else
            {
                resultrange = 11 * pagenumber;
            }
            noOfPages = (view.Count()/10) + 1;
            view = view.Skip(resultrange).Take(10).ToList();
            ViewBag.LoanData = view;
            ViewBag.Pages = noOfPages;
            return View(loan);
        }

        public ViewResult LoanData(List<LoanPaymentView> LoanData)
        {
            List <LoanPaymentView> data= new List<LoanPaymentView>();
            if (LoanData.Any())
            {
                data = LoanData;
            }
            return View(data);
        }


        private List<LoanPaymentView> GetLoanViewData(SearchFormView loan)
        {
            List<LoanPaymentView> listloan = new List<LoanPaymentView>();
            int TotalNumberofPayments = loan.NoOfPaymentYears * loan.NoOfYearlyInstallmentalPayments;
            double scheduledpayment = GetScheduledPayment(TotalNumberofPayments,loan.LoanPrincipal,loan.InterestRate);
            double startingbalance; double cummulativeInterest=0;
            DateTime lastpaymenttime=DateTime.Parse(loan.PaymentStartDate);
            startingbalance = loan.LoanPrincipal;
            
            for (int i = 1; i <= TotalNumberofPayments; i++)
            {
                LoanPaymentView payment = new LoanPaymentView();
                payment = GetPayment(startingbalance,loan.InterestRate, scheduledpayment);
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
            _interest = Math.Round(monthlyInterest,2);
            _principal = Math.Round((scheduledpayment - monthlyInterest),2);
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
            monthly= Math.Round(monthly, 2); ;
            return monthly;
        }
    }
}