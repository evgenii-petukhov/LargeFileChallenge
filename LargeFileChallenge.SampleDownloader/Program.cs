using LargeFileChallenge.SampleDownloader;

var httpClient = new HttpClient();

var dummyJsonClient = new DummyJsonClient(httpClient);

var sampleStrings = await dummyJsonClient.GetAllIngredientsAsync();

await File.WriteAllLinesAsync("example.txt", sampleStrings);

