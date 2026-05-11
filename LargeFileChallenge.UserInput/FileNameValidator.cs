using LargeFileChallenge.UserInput.Abstractions;

namespace LargeFileChallenge.UserInput;

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
