using StackExchange.Redis;

// connection string to your Redis Cache    
string connectionString = "az204redisth.redis.cache.windows.net:6380,password=hlkumvzIO64LbHbLxpLkmLJQWDBNHORBwAzCaEJqsVc=,ssl=True,abortConnect=False";

var cache = await ConnectionMultiplexer.ConnectAsync(connectionString).ConfigureAwait(true);
await using (cache)
{
    IDatabase db = cache.GetDatabase();

    // Snippet below executes a PING to test the server connection
    var result = await db.ExecuteAsync("ping");
    Console.WriteLine($"PING = {result.Type} : {result}");

    // Call StringSetAsync on the IDatabase object to set the key "test:key" to the value "100"
    var redisKey = "test:key";
    bool setValue = await db.StringSetAsync(redisKey, "100");
    Console.WriteLine($"SET: {setValue}");
    var getValue = (await db.StringGetAsync(redisKey)).ToString();
    Console.WriteLine($"GET: {getValue}");

    // apparently does not work when setting a value after an expiration
    // seems expiration must be set after an assignment.
    db.KeyExpire(redisKey, TimeSpan.FromSeconds(1));

    setValue = await db.StringSetAsync(redisKey, "200");
    Console.WriteLine($"SET: {setValue}");
    getValue = (await db.StringGetAsync(redisKey)).ToString();
    Console.WriteLine($"GET: {getValue}");
    db.KeyExpire(redisKey, TimeSpan.FromSeconds(1));
    await Task.Delay(2000);
    
    // StringGetAsync retrieves the value for the "test" key
    Console.WriteLine(db.KeyExists(redisKey) ? "KeyStillExists (fail) " : "Key deleted (succes)");

    var redisResult = await db.StringGetAsync(redisKey);
    Console.WriteLine(redisResult.IsNull ? "Result is null (success) " : "Result not null (failure)");

}
