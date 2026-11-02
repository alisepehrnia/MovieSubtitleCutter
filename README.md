# 🎬 MovieSubtitleCutter – SRT Parser & Elasticsearch Subtitle Indexer

A **.NET 8 solution** that parses **SRT subtitle files** and indexes their contents into **Elasticsearch** for full‑text search.  
The repository contains three projects:

- **`SrtParser`** – a dependency‑free library that turns `.srt` streams into structured `SubtitleItem` objects.
- **`IndexerConsole`** – a console app that reads an SRT file, indexes every subtitle line into Elasticsearch, and demonstrates search and delete operations.
- **`SrtParserTester`** – a minimal console tester that calls the parser on a local SRT file.

---

## 📖 Overview

`MovieSubtitleCutter` is built around a small, reusable **SRT parser** (`SrtParser`).  
The parser reads an SRT stream, splits it into subtitle blocks, extracts the timecodes and text, and returns a list of `SubtitleItem` objects. Each item exposes:

- `StartTime` / `EndTime` – in milliseconds.
- `Lines` – the raw subtitle text (formatting preserved).
- `PlaintextLines` – the same text with SRT formatting tags (`{...}`, `<...>`) stripped.

The `IndexerConsole` project then takes those parsed items and **indexes them into Elasticsearch**, so subtitle lines can be searched by keyword.  
The `SrtParserTester` project is a minimal harness for verifying the parser against a local file.

---

## 🗂️ Repository Structure

```text
MovieSubtitleCutter/
├── IndexerConsole/
│   ├── bin/Debug/net8.0/          # Build output (generated)
│   ├── obj/                       # Intermediate build files (generated)
│   ├── IndexerConsole.csproj      # Console app project (net8.0)
│   └── Program.cs                 # Elasticsearch indexing, search, delete logic
├── SrtParser/
│   ├── .vs/                       # Visual Studio local state (generated)
│   ├── bin/Debug/net8.0/          # Build output (generated)
│   ├── obj/                       # Intermediate build files (generated)
│   ├── ISubtitlesParser.cs        # Parser interface
│   ├── SrtParser.cs               # SRT implementation (static class)
│   ├── SrtParser.csproj           # Class library project (net8.0)
│   ├── SrtParser.sln              # Standalone solution (optional)
│   └── SubtitleItem.cs            # Parsed subtitle model
├── SrtParserTester/
│   ├── bin/Debug/net8.0/          # Build output (generated)
│   ├── obj/                       # Intermediate build files (generated)
│   ├── Program.cs                 # Minimal parser test harness
│   └── SrtParserTester.csproj     # Console tester project (net8.0)
├── .gitignore
├── MovieSubtitleCutter.sln        # Main Visual Studio solution
└── README.md
```

## 🛠️ Technologies

|Item|Details|
|---|---|
|**Language**|C#|
|**Framework**|.NET 8.0 (`net8.0`)|
|**Parser dependencies**|None (BCL only: `System.Text`, `System.Text.RegularExpressions`)|
|**Indexer dependency**|**Elastic.Clients.Elasticsearch** (official .NET client)|
|**Subtitle format**|SubRip (`.srt`) only|
|**Build system**|`dotnet` CLI / Visual Studio solution (`MovieSubtitleCutter.sln`)|

---
## 🧩 Project 1 – `SrtParser` (Library)

### What it does

- Parses an SRT stream into `List<SubtitleItem>`.
- Accepts multiple timecode delimiters: `-->`, `- >`, and `->`.
- Returns both **raw** lines (`Lines`) and **plain‑text** lines (`PlaintextLines`).
- Throws clear exceptions for invalid input (non‑seekable stream, malformed SRT, empty result).

### Key files

| File                  | Role                                                                                                 |
| --------------------- | ---------------------------------------------------------------------------------------------------- |
| `ISubtitlesParser.cs` | Interface defining `ParseStream(Stream, Encoding)`. An async signature is present but commented out. |
| `SrtParser.cs`        | Static `SrtParser` class with `ParseStream` and the internal block/timecode parsers.                 |
| `SubtitleItem.cs`     | Model with `StartTime`, `EndTime`, `Lines`, `PlaintextLines`, and a `ToString()` override.           |

### How to use

```csharp
using System.Text;
using SrtParser;
using var stream = File.OpenRead("movie.srt");
var subtitles = SrtParser.SrtParser.ParseStream(stream, Encoding.UTF8);
foreach (var item in subtitles)
{
    Console.WriteLine($"{item.StartTime} ms → {item.EndTime} ms");
    foreach (var line in item.PlaintextLines)
        Console.WriteLine($"  {line}");
}
```


> **Note:** `ParseStream` is a **static method** on the `SrtParser` class (not an instance method, even though `ISubtitlesParser` defines an instance contract). Use `SrtParser.SrtParser.ParseStream(...)` as shown.

### `SubtitleItem` members

| Member           | Type           | Description                                         |
| ---------------- | -------------- | --------------------------------------------------- |
| `StartTime`      | `int`          | Start time in milliseconds.                         |
| `EndTime`        | `int`          | End time in milliseconds.                           |
| `Lines`          | `List<string>` | Raw subtitle lines (formatting preserved).          |
| `PlaintextLines` | `List<string>` | Lines with `{...}` / `<...>` tags removed.          |
| `ToString()`     | `string`       | Returns `start --> end: text` for quick inspection. |

---
## 🧩 Project 2 – `IndexerConsole` (Elasticsearch Indexer)

### What it does

`IndexerConsole/Program.cs` is a demonstration console app that:

1. **Connects** to an Elasticsearch instance at `http://localhost:9200`.
2. (Optionally) **Indexes** parsed subtitle items into an index named `subtitles`.
3. **Searches** the index for a term (the example uses `"بشین"`).
4. **Deletes** documents by query (both `DeleteByQueryAsync` and `DeleteByQuery` are shown, with placeholder index names).

### Key lines in the source

- The Elasticsearch client is created with:
    
    ```csharp
    var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
        .Authentication(new BasicAuthentication("elastic", "loobley@3129"));
    var client = new ElasticsearchClient(settings);
    ```
    
- The indexing call is **commented out**:
    
    ```csharp
        //var x = await client.IndexManyAsync(subItems, "subtitles");
    ```
    
- The search call is **active**:
    
    ```csharp
        var x = await client.SearchAsync(s => s
        .Index("subtitles")
        .Query(q => q.Term(t => t.Field(f => f.PlaintextLines).Value("بشین"))));
    ```
    
- The delete calls contain **placeholder index names** (`"IndexName"`) and should be adjusted before use.
### How to run

1. **Start Elasticsearch** locally on port `9200` (e.g. via Docker: `docker run -p 9200:9200 elasticsearch:8.15.0`).
2. **Set the SRT file path** in `Program.cs`. The file currently hard‑codes a Windows path that does not exist on most machines:
    
    ```csharp
var path = @"G:\Video\About Time (2013) [1080p]\About.Time.2013.1080p.BluRay.x264.YIFY.srt";
    ```
    
    Replace it with the path to a real `.srt` file on your system.
    
3. **Uncomment the indexing line** if you want to populate the index:
    
    ```csharp
        var x = await client.IndexManyAsync(subItems, "subtitles");
    ```
    
1. **Update the authentication** if your Elasticsearch instance does not use the hard‑coded credentials (`user` / `pass`).
    
2. **Run the project**:
    
    ```bash
    cd IndexerConsole
    dotnet run
    ```

> **Important:** The `Program.cs` uses top‑level statements and mixes commented‑out and active code. Review it before running — the delete operations, in particular, use placeholder index names and would delete nothing (or the wrong data) as written.

---
## 🧩 Project 3 – `SrtParserTester` (Parser Harness)

`SrtParserTester/Program.cs` is a **7‑line console harness** that calls the parser on a local SRT file:

```csharp
var path = @"G:\Video\About Time (2013) [1080p]\About.Time.2013.1080p.BluRay.x264.YIFY.srt";
var fstream = new FileStream(path, FileMode.Open);
var subItems = SrtParser.SrtParser.ParseStream(fstream, System.Text.Encoding.UTF8);
Console.ReadKey();
```

Like `IndexerConsole`, it uses a **hard‑coded Windows path**. Change it to a real file before running:

```bash
cd SrtParserTester
dotnet run
```

> The tester does not print anything — it simply parses the file and waits for a key press. Add a `foreach` over `subItems` if you want to inspect the output.

---
## 🚀 Getting Started

### Prerequisites

- **.NET 8 SDK**
- **Elasticsearch** (for `IndexerConsole`) — local or remote.
- An `.srt` file to test with.
### Build the solution

```bash
git clone https://github.com/alisepehrnia/MovieSubtitleCutter.git
cd MovieSubtitleCutter
dotnet build MovieSubtitleCutter.sln
```

# Parser tester (after fixing the hard-coded path)
```bash
cd SrtParserTester
dotnet run
```
# Elasticsearch indexer (after fixing the path and optionally uncommenting indexing)
```bash
cd ../IndexerConsole
dotnet run
```

---
## 🔍 How the Parser Works (Summary)

1. **Stream validation** — checks that the stream is readable and seekable, then resets its position to 0.
2. **Block splitting** — `GetSrtSubTitleParts` reads line by line and yields a block whenever it hits a blank line.
3. **Timecode parsing** — `TryParseTimecodeLine` splits the first non‑empty line on `-->`, `- >`, or `->` and parses both sides into milliseconds.
4. **Text collection** — remaining lines are stored in `Lines`; a regex (`StyledTextRegex`) strips `{...}` and `<...>` into `PlaintextLines`.
5. **Validation** — items are kept only if they have a non‑zero timecode and at least one text line. An empty result throws `ArgumentException`.
---

## ⚠️ Notes & Caveats

- **Hard‑coded paths** — both `IndexerConsole/Program.cs` and `SrtParserTester/Program.cs` contain `G:\Video\...` paths. You **must** change these before the projects will run.
- **Hard‑coded Elasticsearch credentials** — `IndexerConsole` uses `BasicAuthentication("elastic", "loobley@3129")`. Replace with your own credentials or remove authentication for a local dev cluster.
- **Commented‑out indexing** — the `IndexManyAsync` call is commented out, so running the project as‑is will **search an empty or existing index** without first inserting data.
- **Placeholder delete queries** — the delete calls use `"IndexName"` instead of the real index (`"subtitles"`), so they will not affect the intended data.
- **SRT only** — the parser does not handle ASS/SSA, WebVTT, or other subtitle formats.
- **Synchronous parsing only** — the async `ParseStreamAsync` signature in `ISubtitlesParser` is commented out; only the synchronous `ParseStream` is implemented.
- **No unit tests** — `SrtParserTester` is a manual harness, not an automated test suite.
- **Encoding** — you pass the `Encoding` explicitly. `StreamReader` still detects BOMs, but non‑BOM files (e.g. Windows‑1256) need the correct encoding supplied.
- **`IndexerConsole` and `SrtParserTester` are demo code**, not production tools — treat them as examples of how to use the parser and the Elasticsearch client.

---
## 🤝 Contributing

Issues and pull requests are welcome — e.g. fixing the hard‑coded paths, enabling the indexing call, adding unit tests for the parser, or supporting additional subtitle formats.

---
## 📄 License

Feel free to use
