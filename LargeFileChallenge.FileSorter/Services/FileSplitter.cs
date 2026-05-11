using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.FileSorter.Models;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace LargeFileChallenge.FileSorter.Services;

public class FileSplitter(IOptions<IoSettings> options) : IFileSplitter
{
    private readonly IoSettings _ioSettings = options.Value;

    public async Task<List<string>> SplitAsync(
        string inputFilePath,
        string tempFolderPath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException(inputFilePath);
        }

        if (!Directory.Exists(tempFolderPath))
        {
            Directory.CreateDirectory(tempFolderPath);
        }

        var outputFiles = new List<string>();

        int fileIndex = 0;
        int bytesRead = 0;
        int readBufferSize = _ioSettings.ReadBufferSize;
        long chunkSize = _ioSettings.ChunkSize;

        var buffer = new byte[readBufferSize];
        await using var sourceFile = new FileStream(
            inputFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: readBufferSize,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        do
        {
            var targetFilePath = Path.Combine(
                tempFolderPath,
                string.Format(CultureInfo.InvariantCulture, _ioSettings.ChunkNameTemplate!, fileIndex));
            await using var targetFile = new FileStream(
                targetFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: _ioSettings.WriteBufferSize,
                options: FileOptions.Asynchronous);
            outputFiles.Add(targetFilePath); 

            long nextChunkOffset = -1L;
            long bytesReadInChunk = 0L;

            do
            {
                bytesRead = await sourceFile.ReadAsync(buffer.AsMemory(0, readBufferSize), cancellationToken);
                if (bytesRead < readBufferSize)
                {
                    await targetFile.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    return outputFiles;
                }

                if (bytesReadInChunk < chunkSize)
                {
                    await targetFile.WriteAsync(buffer, cancellationToken);
                    bytesReadInChunk += bytesRead;
                    continue;
                }

                int newLineIndex = buffer.AsSpan(0, bytesRead).LastIndexOf((byte)'\n');

                if (newLineIndex > -1)
                {
                    var hasCarriageReturn = newLineIndex > 0 && buffer[newLineIndex - 1] == '\r';
                    nextChunkOffset = sourceFile.Position - readBufferSize + newLineIndex + (hasCarriageReturn ? 2 : 1);
                    await targetFile.WriteAsync(buffer.AsMemory(0, newLineIndex - (hasCarriageReturn ? 1 : 0)), cancellationToken);
                    sourceFile.Position = nextChunkOffset;
                    break;
                }

                await targetFile.WriteAsync(buffer, cancellationToken);
                bytesReadInChunk += bytesRead;
            } while (bytesReadInChunk < chunkSize || nextChunkOffset == -1);

            fileIndex++;
        } while (bytesRead == readBufferSize);

        return outputFiles;
    }
}
