using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace API.Controllers
{
    public class DespensaController : Controller
    {
        // GET: Despensa
        public ActionResult Index()
        {
            return View();
        }
    }
}