using System.Linq;
using System.Web.Mvc;
using Raven.Client.Linq;
using RavenSuggest.Models;

namespace RavenSuggest.Controllers
{
    public class HomeController : RavenController
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CompanySuggestions(string term)
        {
            var rq = RavenSession.Query<Company, Companies_QueryIndex>();

            var ravenResults = rq.Search(x => x.Name, string.Format("*{0}*", term), escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards, options: SearchOptions.And)
                                        .Take(5)
                                        .ToList();

            return Json(ravenResults.Select(x => new
            {
                id = x.Id,
                value = x.Name,
                description = string.Format("({0}, {1})", x.Category, x.Location)
            }));
        }

        [HttpPost]
        public JsonResult ScopedCompanySuggestions(string term, string category)
        {
            var rq = RavenSession.Query<Company, Companies_QueryIndex>();

            var ravenResults = rq.Search(x => x.Name, string.Format("*{0}*", term), escapeQueryOptions: EscapeQueryOptions.AllowAllWildcards, options: SearchOptions.And)
                                        .Where(x => x.Category == category)
                                        .Take(5)
                                        .ToList();

            return Json(ravenResults.Select(x => new
            {
                id = x.Id, 
                value = x.Name, 
                description = string.Format("({0}, {1})", x.Category, x.Location)
            }));
}
    }
}
