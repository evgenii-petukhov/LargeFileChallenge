using LargeFileChallenge.FileSorter.Abstractions;

namespace LargeFileChallenge.FileSorter.Services;

public class FileContentSorter : IFileContentSorter
{
    public async Task SortAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var strings = await File.ReadAllLinesAsync(filePath, cancellationToken);

        await File.WriteAllLinesAsync(
            filePath,
            strings
                .Select(s =>
                {
                    var parts = s.Split(". ");
                    return new
                    {
                        Original = s,
                        Number = int.Parse(parts[0]),
                        Text = parts[1]
                    };
                })
                .OrderBy(item => item.Text)
                .ThenBy(item => item.Number)
                .Select(item => item.Original),
            cancellationToken);
    }
}
