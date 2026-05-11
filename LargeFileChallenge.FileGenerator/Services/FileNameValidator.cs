using LargeFileChallenge.FileGenerator.Abstractions;

namespace LargeFileChallenge.FileGenerator.Services;

public partial class FileNameValidator : IFileNameValidator
{
    public bool IsValid(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var invalidChars = Path.GetInvalidFileNameChars();

        return fileName.IndexOfAny(invalidChars) < 0;
    }
}
