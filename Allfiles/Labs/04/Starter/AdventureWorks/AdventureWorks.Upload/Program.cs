namespace AdventureWorks.Upload
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text.Json;
    using System.IO;
    using Microsoft.Azure.Cosmos;

    public class Program
    {
        private const string EndpointUrl = "https://az204thpolycosmos.documents.azure.com:443/";
        private const string AuthorizationKey = "LQQeka5dLKRLa3Lqd4KuiIeqw06NP4S65AINvoxKJvN9eBDkYhz76W0zzGtfLEoEKhYcGRw1kMa6ACDb3DNwzg==";
        private const string DatabaseName = "Retail";
        private const string ContainerName = "Online";
        private const string PartitionKey = "/Category";
        private const string JsonFilePath = "S:\\Github-Titusvh\\AZ204-Courseware\\Allfiles\\Labs\\04\\Starter\\AdventureWorks\\AdventureWorks.Upload\\models.json";

        static private int amountToInsert;
        static List<Model> models;

        static async Task Main(string[] args)
        {
            try
            {
                // <CreateClient>
                CosmosClient cosmosClient = new CosmosClient(EndpointUrl, AuthorizationKey, new CosmosClientOptions() { AllowBulkExecution = true });
                // </CreateClient>

                // <Initialize>
                Console.WriteLine($"Creating a database if not already exists...");
                Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(Program.DatabaseName);

                // Configure indexing policy to exclude all attributes to maximize RU/s usage
                Console.WriteLine($"Creating a container if not already exists...");
                await database.DefineContainer(Program.ContainerName, PartitionKey)
                        .WithIndexingPolicy()
                            .WithIndexingMode(IndexingMode.Consistent)
                            .WithIncludedPaths()
                                .Attach()
                            .WithExcludedPaths()
                                .Path("/*")
                                .Attach()
                        .Attach()
                    .CreateAsync();
                // </Initialize>

                //using (StreamReader reader = new StreamReader(File.OpenRead(JsonFilePath)))
                //{
                //    string json = await reader.ReadToEndAsync();
                //    models = JsonSerializer.Deserialize<List<Model>>(json);
                //    amountToInsert = models.Count;
                //}
                var json = await File.ReadAllTextAsync(JsonFilePath);
                models = JsonSerializer.Deserialize<List<Model>>(json);
                amountToInsert = models.Count;

                // Prepare items for insertion
                Console.WriteLine($"Preparing {amountToInsert} models to insert...");

                // Create the list of Tasks
                Console.WriteLine("Starting...");
                Stopwatch stopwatch = Stopwatch.StartNew();
                // <ConcurrentTasks>
                Container container = database.GetContainer(ContainerName);

                await Parallel.ForEachAsync(models, async(model, ct) =>
                    {
                        try
                        {
                            await container.CreateItemAsync(model, new PartitionKey(model.Category))
                                .ConfigureAwait(false);
                        }
                        catch (AggregateException ex)
                        {
                            if (ex.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is
                                CosmosException cosmosException)
                            {
                                Console.WriteLine(
                                    $"Received {cosmosException.StatusCode} ({cosmosException.Message}).");
                            }
                            else
                            {
                                Console.WriteLine($"Exception {ex.InnerExceptions.FirstOrDefault()}.");
                            }
                        }
                        catch (CosmosException ex)
                        {
                            Console.WriteLine($"Received {ex.StatusCode} ({ex.Message}).");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception {ex}.");
                        }
                    }
                ).ConfigureAwait(false);
                
                stopwatch.Stop();

                Console.WriteLine($"Finished writing {amountToInsert} items in {stopwatch.Elapsed}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public class Model
        {
            public string id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public string Photo { get; set; }
            public IList<Product> Products { get; set; }
        }

        public class Product
        {
            public string id { get; set; }
            public string Name { get; set; }
            public string Number { get; set; }
            public string Category { get; set; }
            public string Color { get; set; }
            public string Size { get; set; }
            public decimal? Weight { get; set; }
            public decimal ListPrice { get; set; }
            public string Photo { get; set; }
        }
    }
}
