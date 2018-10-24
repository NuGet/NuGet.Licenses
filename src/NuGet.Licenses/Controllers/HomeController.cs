using System.Web.Mvc;

namespace NuGet.Licenses.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("https://github.com/NuGet/Home/wiki/Packaging-License-within-the-nupkg");
        }
    }
}