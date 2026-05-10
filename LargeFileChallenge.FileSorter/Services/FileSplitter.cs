using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.FileSorter.Models;
using Microsoft.Extensions.Options;

namespace LargeFileChallenge.FileSorter.Services;

public class FileSplitter(IOptions<IoSettings> options) : IFileSplitter
{
    private readonly IoSettings _ioSettings = options.Value;

    public async Task SplitAsync(
        string inputFilePath,
        string tempFolderPath,
        long chunkSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(chunkSize, 1);

        if (!File.Exists(inputFilePath))
        {
            throw new FileNotFoundException(inputFilePath);
        }

        if (!Directory.Exists(tempFolderPath))
        {
            Directory.CreateDirectory(tempFolderPath);
        }

        int fileIndex = 0;
        int bytesRead = 0;
        int readBufferSize = _ioSettings.ReadBufferSize;

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
            var targetFilePath = Path.Combine(tempFolderPath, $"chunk_{fileIndex}.txt");
            await using var targetFile = new FileStream(
                targetFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: _ioSettings.WriteBufferSize,
                options: FileOptions.Asynchronous);

            long nextChunkOffset = -1L;
            long bytesReadInChunk = 0L;

            do
            {
                bytesRead = await sourceFile.ReadAsync(buffer.AsMemory(0, readBufferSize), cancellationToken);
                if (bytesRead < readBufferSize)
                {
                    await targetFile.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    return;
                }

                if (bytesReadInChunk < chunkSize)
                {
                    await targetFile.WriteAsync(buffer, cancellationToken);
                    bytesReadInChunk += bytesRead;
                    continue;
                }

                int newLineIndex;

                for (newLineIndex = bytesRead - 1; newLineIndex >= 0; newLineIndex--)
                {
                    if (buffer[newLineIndex] == '\n')
                    {
                        break;
                    }
                }

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
    }
}
