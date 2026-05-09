using LargeFileChallenge.FileGenerator.Abstractions;
using System.Text;

namespace LargeFileChallenge.FileGenerator.Services;

public class FileContentGenerator : IFileContentGenerator
{
    private const int MaxNumber = 100000;

    private readonly Random _random = new((int)DateTime.Now.Ticks & 0x0000FFFF);

    public async Task Generate(
        string sampleFilePath,
        string targetFilePath,
        long targetSize,
        CancellationToken cancellationToken = default)
    {
        var sampleStrings = await File.ReadAllLinesAsync(sampleFilePath, cancellationToken);

        var currentSize = 0L;

        await using var stream = new FileStream(
            targetFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 8192,
            options: FileOptions.Asynchronous);

        while (currentSize < targetSize)
        {
            var shuffledStringsEnumerable = sampleStrings
                .Select(s => $"{_random.Next(MaxNumber)}. {s}")
                .OrderBy(_ => _random.Next());

            var textChunk = string.Join(Environment.NewLine, shuffledStringsEnumerable);

            var bytes = Encoding.UTF8.GetBytes(textChunk);

            await stream.WriteAsync(bytes, cancellationToken);
            await stream.FlushAsync(cancellationToken);

            currentSize += bytes.Length;
        }
    }
}
