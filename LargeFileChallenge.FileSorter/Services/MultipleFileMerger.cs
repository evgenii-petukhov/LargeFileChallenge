using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.FileSorter.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace LargeFileChallenge.FileSorter.Services;

public partial class MultipleFileMerger(IOptions<IoSettings> options) : IMultipleFileMerger
{
    private readonly IoSettings _ioSettings = options.Value;

    public async Task MergeAsync(
        List<string> chunkFiles,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var readers = new List<StreamReader>(chunkFiles.Count);
        try
        {
            foreach (var f in chunkFiles)
            {
                var fs = new FileStream(
                    f,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: _ioSettings.ReadBufferSize,
                    options: FileOptions.SequentialScan);
                readers.Add(
                    new StreamReader(
                        fs,
                        Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: true,
                        bufferSize: _ioSettings.ReadBufferSize,
                        leaveOpen: false));
            }

            await using var outFs = new FileStream(
                outputPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: _ioSettings.WriteBufferSize,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            using var writer = new StreamWriter(
                outFs,
                Encoding.UTF8,
                _ioSettings.WriteBufferSize,
                leaveOpen: false);

            var pq = new PriorityQueue<MergeQueueItem, SortKey>(readers.Count);

            for (int i = 0; i < readers.Count; i++)
            {
                await EnqueueLine(pq, readers[i], i, cancellationToken);
            }

            while (pq.Count > 0)
            {
                var item = pq.Dequeue();
                await writer.WriteLineAsync(item.Line);
                await EnqueueLine(pq, readers[item.ReaderIndex], item.ReaderIndex, cancellationToken);
            }

            await writer.FlushAsync(cancellationToken);
        }
        finally
        {
            foreach (var r in readers)
            {
                r.Dispose();
            }
        }
    }

    private static async Task EnqueueLine(
        PriorityQueue<MergeQueueItem, SortKey> queue,
        StreamReader reader,
        int readerIndex,
        CancellationToken cancellationToken)
    {
        var line = await reader.ReadLineAsync(cancellationToken);
        if (line != null)
        {
            var parts = line.Split(". ");
            queue.Enqueue(new MergeQueueItem(readerIndex, line), new SortKey(parts[1], int.Parse(parts[0])));
        }
    }
}
