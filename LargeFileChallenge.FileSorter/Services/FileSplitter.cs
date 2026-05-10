using LargeFileChallenge.FileSorter.Abstractions;

namespace LargeFileChallenge.FileSorter.Services;

public class FileSplitter : IFileSplitter
{
    public async Task SplitAsync(
        string inputFilePath,
        string tempFolderPath,
        int chunkSize,
        CancellationToken cancellationToken = default)
    {
        var buffer = new byte[chunkSize];
        using var fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
        int bytesRead;
        int fileIndex = 0;
        var isTempFolderCreated = false;
        while ((bytesRead = fs.Read(buffer, 0, chunkSize)) > 0)
        {
            if (bytesRead < chunkSize) break;

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
                if (buffer[newLineIndex - 1] == '\r')
                {
                    newLineIndex--;
                    fs.Position += newLineIndex - bytesRead + 2;
                }
                else
                {
                    fs.Position += newLineIndex - bytesRead + 1;
                }
            }

            if (!isTempFolderCreated && !Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
                isTempFolderCreated = true;
            }

            var targetFilePath = Path.Combine(tempFolderPath, $"chunk_{fileIndex++}.txt");
            await using var stream = new FileStream(
                targetFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 8192,
                options: FileOptions.Asynchronous);

            await stream.WriteAsync(buffer.AsMemory(0, newLineIndex), cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
    }
}
