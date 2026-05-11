using LargeFileChallenge.FileSorter.Abstractions;
using LargeFileChallenge.FileSorter.Models;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace LargeFileChallenge.FileSorter.Services;

public class ChunkPathResolver(IOptions<IoSettings> options) : IChunkPathResolver
{
    private readonly IoSettings _ioSettings = options.Value;

    public string Resolve(string tempFolderPath, int fileIndex)
    {
        return Path.Combine(
            tempFolderPath,
            string.Format(CultureInfo.InvariantCulture, _ioSettings.ChunkNameTemplate!, fileIndex));
    }
}
