using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.FileSorter.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace LargeFileChallenge.FileSorter.Services;

public partial class MultipleFileMerger(IOptions<IoSettings> options) : IMultipleFileMerger
{
    private readonly IoSettings _ioSettings = options.Value;

    public async Task MergeAsync(
        string tempFolder,
        List<string> chunkFileNames,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var readerInfos = new List<MergeReaderInfo>(chunkFileNames.Count);
        try
        {
            for (var chunkCounter = 0; chunkCounter < chunkFileNames.Count; chunkCounter++)
            {
                var fs = new FileStream(
                    Path.Combine(tempFolder, chunkFileNames[chunkCounter]),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: _ioSettings.ReadBufferSize,
                    options: FileOptions.SequentialScan);
                readerInfos.Add(
                    new MergeReaderInfo(
                        new BinaryReader(fs, Encoding.UTF8, leaveOpen: false),
                        chunkCounter));
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

            var pq = new PriorityQueue<MergeQueueItem, SortKey>(readerInfos.Count);

            for (int i = 0; i < readerInfos.Count; i++)
            {
                TryEnqueueRecord(pq, readerInfos[i]);
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
                TryEnqueueRecord(pq, readerInfos[item.ReaderIndex]);
            }

            await writer.FlushAsync(cancellationToken);
        }
        finally
        {
            foreach (var info in readerInfos)
            {
                info.Reader?.Dispose();
            }
            Directory.Delete(tempFolder, true);
        }
    }

    private static void TryEnqueueRecord(
        PriorityQueue<MergeQueueItem, SortKey> queue,
        MergeReaderInfo readerInfo)
    {
        if (readerInfo.Reader == null) return;

        try
        {
            var reader = readerInfo.Reader;
            var number = reader.ReadInt32();
            var text = reader.ReadString();
            queue.Enqueue(new MergeQueueItem(readerInfo.Index, number, text), new SortKey(text, number));
        }
        catch (EndOfStreamException)
        {
            readerInfo.Reader?.Dispose();
            readerInfo.Reader = null;
        }
    }
}
