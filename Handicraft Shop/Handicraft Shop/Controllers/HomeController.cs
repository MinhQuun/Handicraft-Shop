using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Handicraft_Shop.Models;

namespace Handicraft_Shop.Controllers
{
    public class HomeController : Controller
    {
        HandicraftShopDataContext data = new HandicraftShopDataContext();
        public ActionResult Index()
        {
            List < SANPHAM > sp= data.SANPHAMs.ToList();
            return View(sp);
        }
        public ActionResult()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}