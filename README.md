# 📂 Large File Sorting Challenge

A test task to sort a large text file efficiently under strict memory and time constraints.

## 📄 Input Format

Each line consists of a number and a text part. The text part can repeat across lines.

```
13. Paprika
42. Green onions, sliced
100. Paprika
81. Tofu, cubed
256. Sugar
```

[Test data](https://github.com/evgenii-petukhov/LargeFileChallenge/blob/master/LargeFileChallenge.FileGenerator/SampleStrings.txt) was generated using the Recipes endpoint from [DummyJSON](https://dummyjson.com).

## ⚠️ Constraints

- Max file size: 100GB
- Memory limit: 2–2.5GB
- Time reference: 1GB ~ 30s, 10GB ~ 9min
- Sorting criteria: text part first, then number

## 💡 Approach

Since the input file exceeds physical memory limits, in-memory sorting (e.g. LINQ `OrderBy` or `List.Sort`) isn't viable. The solution uses an external sort strategy:

1. 🔪 Split the input file into smaller chunks
2. 🔤 Sort each chunk in-memory
3. 🧩 Merge all chunks into a single sorted file using a K-way merge algorithm

The optimal chunk size is 10MB — large enough for throughput, small enough to stay within memory bounds.

## 🔧 Key Components

- [FileSplitter](https://github.com/evgenii-petukhov/LargeFileChallenge/blob/master/LargeFileChallenge.FileSorter/Services/FileSplitter.cs) — splits the input without breaking lines, so chunk sizes may slightly exceed 10MB
- [FileContentSorter](https://github.com/evgenii-petukhov/LargeFileChallenge/blob/master/LargeFileChallenge.FileSorter/Services/FileContentSorter.cs) — handles file reading and writing with maximum performance
- [MultipleFileMerger](https://github.com/evgenii-petukhov/LargeFileChallenge/blob/master/LargeFileChallenge.FileSorter/Services/MultipleFileMerger.cs) — implements the K-way merge to stitch chunks into the final output

Both Windows (`\r\n`) and macOS (`\n`) line endings are supported.

## 📊 Complexity

- Chunk sorting: `O(n log n)` — `List.Sort` uses Quicksort, where `n` is the number of lines per chunk
- K-way merge: `O(n log k)` — where `n` is lines per chunk and `k` is the number of chunk files

## 🏆 Best Results

All metrics were measured using the Release build configuration.

| CPU | File Size | Total | Splitting | Sorting | Merging | Peak Memory |
|-----|-----------|-------|-----------|---------|---------|-------------|
| Intel Core i7 2.30GHz | 1GB | 26s | 1s | 12s | 13s | 1.6GB |
| Intel Core i7 2.80GHz | 1GB | 34s | 1s | 18s | 15s | 1GB |
| Intel Core i7 2.30GHz | 10GB | 6m 36s | 36s | 2m 20s | 3m 40s | 2.4GB |
| Intel Core i7 2.80GHz | 10GB | 10m 18s | 2m 8s | 3m 36s | 4m 34s | 2.1GB |

## 🚀 How to Use

There are two main applications: `FileGenerator` and `FileSorter`.

### Generate an input file

1. Launch `FileGenerator`
2. Enter the desired file size, e.g. `1gb`
3. Enter the desired file name, e.g. `1gb`

### Sort the file

1. Copy the generated file to the `FileSorter` folder
2. Launch `FileSorter`
3. Enter the name of the input file, e.g. `1gb`