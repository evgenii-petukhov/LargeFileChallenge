using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.FileSorter.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace LargeFileChallenge.FileSorter.Services;

public class FileContentSorter(IOptions<IoSettings> options) : IFileContentSorter
{
    private readonly IoSettings _ioSettings = options.Value;

    public async Task SortAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var records = new List<(int Number, string Text)>();

        await using var readFs = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: _ioSettings.ReadBufferSize,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        using (var reader = new StreamReader(
            readFs,
            Encoding.UTF8,
            bufferSize: _ioSettings.ReadBufferSize,
            leaveOpen: false))
        {
            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                var sep = line.IndexOf('.');
                records.Add((int.Parse(line.AsSpan(0, sep)), line[(sep + 2)..]));
            }
        }

        records.Sort(static (a, b) =>
        {
            var cmp = string.CompareOrdinal(a.Text, b.Text);
            return cmp != 0 ? cmp : a.Number.CompareTo(b.Number);
        });

        await using var writeFs = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: _ioSettings.WriteBufferSize,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        await using var writer = new BinaryWriter(writeFs, Encoding.UTF8, leaveOpen: false);

        foreach (var (number, text) in records)
        {
            writer.Write(number);
            writer.Write(text);
        }
    }
}
