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
        var readers = new List<BinaryReader>(chunkFiles.Count);
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
                readers.Add(new BinaryReader(fs, Encoding.UTF8, leaveOpen: false));
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
                TryEnqueueRecord(pq, readers[i], i);
            }

            var first = true;
            while (pq.Count > 0)
            {
                var item = pq.Dequeue();
                if (!first) writer.Write("\r\n");
                writer.Write(item.Number);
                writer.Write(". ");
                writer.Write(item.Text);
                first = false;
                TryEnqueueRecord(pq, readers[item.ReaderIndex], item.ReaderIndex);
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

    private static void TryEnqueueRecord(
        PriorityQueue<MergeQueueItem, SortKey> queue,
        BinaryReader reader,
        int readerIndex)
    {
        try
        {
            var number = reader.ReadInt32();
            var text = reader.ReadString();
            queue.Enqueue(new MergeQueueItem(readerIndex, number, text), new SortKey(text, number));
        }
        catch (EndOfStreamException) { }
    }
}
