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
            var rq = RavenSession.Query<Company, Companies_QueryIndex>()
                                .Search(x => x.Name, term)
                                .Take(5);

            var ravenResults = rq
                .ToList();

            if (ravenResults.Count < 5)
            {
                var suggestionQueryResult = rq.Suggest();

                ravenResults.AddRange(RavenSession.Query<Company, Companies_QueryIndex>()
                                          .Search(x => x.Name, string.Join(" ", suggestionQueryResult.Suggestions))
                                          .Take(5 - ravenResults.Count));
            }

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

            var rq = RavenSession.Query<Company, Companies_QueryIndex>()
                    .Where(x => x.Category == category)
                    .Search(x => x.Name, term, options:SearchOptions.And)
                    .Take(5);

            var ravenResults = rq
                .ToList();

            if (ravenResults.Count < 5)
            {
                var suggestionQueryResult = rq.Suggest();

                ravenResults.AddRange(RavenSession.Query<Company, Companies_QueryIndex>()
                                .Where(x => x.Category == category)
                                .Search(x => x.Name, string.Join(" ", suggestionQueryResult.Suggestions), options:SearchOptions.And)
                                .Take(5 - ravenResults.Count));
            }

            return Json(ravenResults.Select(x => new
            {
                id = x.Id, 
                value = x.Name, 
                description = string.Format("({0}, {1})", x.Category, x.Location)
            }));
}
    }
}
