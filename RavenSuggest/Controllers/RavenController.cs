using System.Web.Mvc;
using Raven.Client;

namespace RavenSuggest.Controllers
{
    public abstract partial class RavenController : Controller
    {
        public static IDocumentStore DocumentStore { get; set; }

        private IDocumentSession _ravenSession;

        public IDocumentSession RavenSession
        {
            get { return _ravenSession ?? (_ravenSession = DocumentStore.OpenSession()); }
            set { _ravenSession = value; }
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            using (_ravenSession)
            {
                if (_ravenSession != null && filterContext.Exception == null)
                    _ravenSession.SaveChanges();
            }
        }
    }
}
