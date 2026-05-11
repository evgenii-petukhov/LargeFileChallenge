namespace LargeFileChallenge.UserInput;

public class UserInputConstants
{
    public static readonly HashSet<string> TerminateCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "EXIT",
        "QUIT"
    };

    public static readonly Dictionary<string, long> UnitMultipliers = new(StringComparer.Ordinal)
    {
        { "B", 1L },
        { "KB", 1024L },
        { "MB", 1024L * 1024L },
        { "GB", 1024L * 1024L * 1024L },
        { "TB", 1024L * 1024L * 1024L * 1024L }
    };
}
