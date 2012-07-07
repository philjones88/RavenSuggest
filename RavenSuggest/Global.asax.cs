using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Raven.Client.Document;
using Raven.Client.Indexes;
using RavenSuggest.Controllers;
using RavenSuggest.Models;

namespace RavenSuggest
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static DocumentStore DocumentStore { get; private set; }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // Startup setup tasks to make sure RavenDB database is in correct state for app to run
            InitializeDocumentStore();

            RavenController.DocumentStore = DocumentStore;
        }

        private static void InitializeDocumentStore()
        {
            if (DocumentStore != null) return; // prevent misuse

            DocumentStore = new DocumentStore
            {
                ConnectionStringName = "RavenDB"
            };

            DocumentStore.Initialize();

            // Setup MvcMiniProfiler RavenDB support
            //MvcMiniProfiler.RavenDb.Profiler.AttachTo(DocumentStore);

            TryCreatingIndexesOrRedirectToErrorPage();

            SetupVersioning();

            SeedData();
        }

        private static void TryCreatingIndexesOrRedirectToErrorPage()
        {
            try
            {
                IndexCreation.CreateIndexes(Assembly.GetAssembly(typeof(Company)), DocumentStore);
            }
            catch (WebException e)
            {
                var socketException = e.InnerException as SocketException;
                if (socketException == null)
                    throw;

                switch (socketException.SocketErrorCode)
                {
                    case SocketError.AddressNotAvailable:
                    case SocketError.NetworkDown:
                    case SocketError.NetworkUnreachable:
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionReset:
                    case SocketError.TimedOut:
                    case SocketError.ConnectionRefused:
                    case SocketError.HostDown:
                    case SocketError.HostUnreachable:
                    case SocketError.HostNotFound:
                        // Shameless "borrow" from RacoonBlog, to show a nice page if it can't connect to RavenDB
                        HttpContext.Current.Response.Redirect("~/RavenNotReachable.htm");
                        break;
                    default:
                        throw;
                }
            }
        }

        private static void SetupVersioning()
        {
             // Exclude all documents from versioning
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(new
                {
                    Exclude = true,
                    Id = "Raven/Versioning/DefaultConfiguration",
                });

                session.SaveChanges();
            }
        }

        private static void SeedData()
        {
            using (var session = DocumentStore.OpenSession())
            {
                if (!session.Query<Company, Companies_QueryIndex>().Customize(x => x.WaitForNonStaleResultsAsOfNow()).Any())
                {
                    var locations = new List<string>
                    {
                        "Bristol",
                        "Bath",
                        "Birmingham",
                        "Blackpool",
                        "Bournemouth",
                        "Brighton",
                        "Cardiff",
                        "Exeter",
                        "Leeds",
                        "Liverpool",
                        "London",
                        "Manchester",
                        "Newcastle",
                        "Nottingham",
                        "Oxford"
                    };

                    var categories = new List<string>
                    {
                        "Computer Hardware",
                        "Computer Software",
                        "Marketing",
                        "SEO",
                        "Accountancy",
                        "Design",
                        "Printing",
                        "Catering",
                        "Travel",
                        "Security"
                    };

                    var rand = new Random();

                    string alphabet = "abcdefghijklmnopqrstuvwyxzeeeiouea";

                    Func<char> randomLetter = () => alphabet[rand.Next(alphabet.Length)];

                    Func<int, string> makeName =
                      length => new string(Enumerable.Range(0, length)
                         .Select(x => x == 0 ? char.ToUpper(randomLetter()) : randomLetter())
                         .ToArray());

                    foreach(var location in locations)
                    {
                        foreach(var category in categories)
                        {
                            for (int i = 0; i < 200; i++)
                            {
                                session.Store(new Company
                                {
                                    Name = makeName(rand.Next(6) + 3) + " " + makeName(rand.Next(10) + 1) + " Ltd",
                                    Category = category,
                                    Location = location
                                });
                            }
                        }

                        session.SaveChanges();
                    }
                }
            }
        }
    }
}