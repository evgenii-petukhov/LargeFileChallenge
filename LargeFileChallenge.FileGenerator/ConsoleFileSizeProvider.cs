namespace LargeFileChallenge.FileGenerator;

public class ConsoleFileSizeProvider(FileSizeParser fileSizeParser)
{
    private readonly FileSizeParser _fileSizeParser = fileSizeParser;

    private readonly HashSet<string> _terminateCommands = ["EXIT", "QUIT"];

    public (bool terminate, long size) GetFileSize()
    {
        var terminate = false;
        var targetSize = 0L;

        do
        {
            Console.WriteLine("Enter target size (e.g., 100MB, 10GB, 1TB). Type 'exit' or 'quit' to close");
            try
            {
                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please enter a valid target size.");
                    continue;
                }

                if (_terminateCommands.Contains(input))
                {
                    terminate = true;
                    break;
                }

                targetSize = _fileSizeParser.Parse(input!);

                Console.WriteLine(targetSize > 0
                    ? $"Target size set to {targetSize} bytes"
                    : "Please enter a positive target size");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("An unexpected exception occurred");
                Console.WriteLine(e);
            }
        } while (!terminate && targetSize <= 0);

        return (terminate, targetSize);
    }
}
