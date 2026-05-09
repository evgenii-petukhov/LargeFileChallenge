namespace LargeFileChallenge.FileGenerator.Abstractions;

public interface IFileSizeParser
{
    long Parse(string input);
}
