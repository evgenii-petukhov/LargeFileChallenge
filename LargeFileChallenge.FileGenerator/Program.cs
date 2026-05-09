using LargeFileChallenge.FileGenerator;

var consoleFileSizeProvider = new ConsoleFileSizeProvider(new FileSizeParser());

var (terminate, targetSize) = consoleFileSizeProvider.GetFileSize();

if (terminate) {
    Console.WriteLine("Exiting...");
    return;
}

var fileContentGenerator = new FileContentGenerator();
await fileContentGenerator.Generate("SampleStrings.txt", "LargeFile.txt", targetSize);

