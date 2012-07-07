using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace RavenSuggest.Models
{
    public class Companies_QueryIndex : AbstractIndexCreationTask<Company>
    {
        public Companies_QueryIndex()
        {
            Map = companies => from company in companies
                               select new
                                {
                                    company.Name,
                                    company.Category,
                                    company.Location
                                };

            Indexes.Add(x => x.Name, FieldIndexing.Analyzed);
        }
    }
}