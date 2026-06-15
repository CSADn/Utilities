using PCMediaAutomation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCMediaAutomation.Controllers
{
    public class HomeController : Controller
    {
        private Aimp _aimp;

        public HomeController()
        {
            _aimp = new Aimp();
        }


        public ActionResult Index()
        {
            var model = new Models.Home.Index
            {
                Volume = _aimp.GetVolume()
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult Play()
        {
            _aimp.Play();
            return Json(true);
        }

        [HttpPost]
        public JsonResult Pause()
        {
            _aimp.Pause();
            return Json(true);
        }

        [HttpPost]
        public JsonResult Stop()
        {
            _aimp.Stop();
            return Json(true);
        }

        [HttpPost]
        public JsonResult GetVolume()
        {
            return Json(_aimp.GetVolume());
        }

        [HttpPost]
        public JsonResult SetVolume(float value)
        {
            _aimp.SetVolume(value);
            return Json(true);
        }

        [HttpPost]
        public JsonResult BDay()
        {
            _aimp.LaunchBDay();
            return Json(true);
        }
    }
}