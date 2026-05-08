using LargeFileChallenge.SampleDownloader.Models;
using System.Text.Json;

namespace LargeFileChallenge.SampleDownloader;

public class DummyJsonClient(HttpClient httpClient)
{
    private const string Url = "https://dummyjson.com/recipes?limit=100";

    private readonly HttpClient _httpClient = httpClient;

    private readonly JsonSerializerOptions _deserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<string>> GetAllIngredientsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var recipeResponse = JsonSerializer.Deserialize<RecipeResponse>(content, _deserializeOptions);

        if (recipeResponse?.Recipes == null)
        {
            return [];
        }

        return [.. recipeResponse.Recipes
            .SelectMany(recipe => recipe.Ingredients ?? Enumerable.Empty<string>())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)];
    }
}
