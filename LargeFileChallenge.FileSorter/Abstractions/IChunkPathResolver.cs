namespace LargeFileChallenge.FileSorter.Abstractions;

public interface IChunkPathResolver
{
    string Resolve(string tempFolderPath, int fileIndex);
}
