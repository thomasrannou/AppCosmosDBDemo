using Microsoft.Azure.Cosmos.Samples.Web.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Microsoft.Azure.Cosmos.Samples.Web.Services;

public class CosmosService : ICosmosService
{ 
    private readonly CosmosClient _client;

    private IConfiguration _configuration;
    
    public CosmosService()
    {
        _client = new CosmosClient(
            connectionString: "AccountEndpoint=https://arwcosmosdb.documents.azure.com:443/;AccountKey=I3q2zhjin1IG6JwcA9zphfoFI2A1VBm3eAvbe59LsffPwV7GvcsHtY5aJHuoDOXTge6zWSMJRdmXACDbjczu3Q==;"
        ,
        new CosmosClientOptions()
        {
            ApplicationRegion = Regions.NorthEurope,
        });
    }

    private Container container
    {
        get => _client.GetDatabase("cosmicworks").GetContainer("products");
    }

    public async Task<IEnumerable<Product>> RetrieveAllProductsAsync()
    { 
        var queryable = container.GetItemLinqQueryable<Product>();

        using FeedIterator<Product> feed = queryable
            .Where(p => p.price < 2000m)
            .OrderByDescending(p => p.price)
            .ToFeedIterator();

        List<Product> results = new();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            foreach (Product item in response)
            {
                results.Add(item);
            }
        }

        return results;
    }

    public async Task<IEnumerable<Product>> RetrieveActiveProductsAsync()
    { 
        string sql = """
        SELECT
            p.id,
            p.categoryId,
            p.categoryName,
            p.sku,
            p.name,
            p.description,
            p.price,
            p.tags
        FROM products p
        JOIN t IN p.tags
        WHERE t.name = @tagFilter
        """;

        var query = new QueryDefinition(
            query: sql
        )
            .WithParameter("@tagFilter", "Tag-75");

        using FeedIterator<Product> feed = container.GetItemQueryIterator<Product>(
            queryDefinition: query
        );

        List<Product> results = new();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            foreach (Product item in response)
            {
                results.Add(item);
            }
        }

        return results;
    }
}
