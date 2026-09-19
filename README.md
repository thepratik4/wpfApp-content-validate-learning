# Learning Content Validator

A production-style, modular C#/.NET Windows desktop application built with WPF, MVVM, and Microsoft Extensions to validate pre-release digital courseware and learning packages.

Designed as an exemplary, enterprise-grade reference architecture for studying modern C#/.NET engineering practices and preparing for a **Software Engineer / Test Automation Engineer** role at **Dassault Systèmes**.

---

## 1. Project Overview

Digital learning platforms, engineering training portals (such as 3DEXPERIENCE and CAD course repositories), and enterprise LMS platforms receive course packages from various authoring teams. These packages contain heterogeneous assets:
- Course syllabi and reference documentation (**PDF**)
- Interactive web modules (**HTML**, CSS, JS)
- Media tutorials (**MP4**, WEBM)
- Graphical assets and diagrams (**PNG**, JPG, SVG)
- Package manifests and structural schemas (**JSON**)
- Bundled offline exercise files (**ZIP**)

Releasing invalid packages leads to broken links, runtime crashes, missing lesson metadata, and degraded user experiences. 

**Learning Content Validator** provides a desktop interface for scanning learning package folders, validating assets against configurable compliance rules, reporting issues with severity badges, and exporting audit reports in JSON and CSV formats.

---

## 2. Architecture & Clean Architecture Principles

The solution follows **Clean Architecture** (Ports & Adapters / Hexagonal principles), strictly enforcing dependency direction toward the domain:

```
+-------------------------------------------------------------+
|               LearningContentValidator.App                  |
|          (WPF Views, ViewModels, Converters, DI)            |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|          LearningContentValidator.Infrastructure            |
| (FileScanner, ContentValidationService, Exporters, Logging) |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|             LearningContentValidator.Domain                 |
|       (Core Models, Interfaces, Rules, ValidationContext)   |
+-------------------------------------------------------------+
```

### Architectural Highlights
1. **Domain Layer (Zero External Dependencies)**: Contains enterprise models (`ContentFile`, `ValidationResult`, `ValidationSummary`, `ValidationOptions`), interfaces (`IFileScanner`, `IValidationRule`, `IContentValidationService`, `IReportGenerator`), and concrete validation rules.
2. **Infrastructure Layer**: Implements I/O boundaries—recursive file system crawling (`FileScanner`), multi-rule orchestration (`ContentValidationService`), report generation (`JsonReportGenerator`, `CsvReportGenerator`), and thread-safe file logging (`FileLogger`).
3. **App Layer (Presentation)**: Implements pure **MVVM (Model-View-ViewModel)**. The code-behind `MainWindow.xaml.cs` contains zero validation or file-system logic. `App.xaml.cs` acts as the **Composition Root**, configuring `Microsoft.Extensions.DependencyInjection`.
4. **Test Suite**: Automated NUnit 4 suite with isolated unit tests, Moq-based behavioral tests, and temporary-directory integration tests.

---

## 3. Project Structure

```
LearningContentValidator/
│
├── src/
│   ├── LearningContentValidator.Domain/             # Core models, interfaces, and rules
│   │   ├── Interfaces/
│   │   │   ├── IFileScanner.cs                      # File crawler contract
│   │   │   ├── IValidationRule.cs                   # Extensible rule contract
│   │   │   ├── IContentValidationService.cs         # Service orchestration contract
│   │   │   └── IReportGenerator.cs                  # Report exporter contract
│   │   ├── Models/
│   │   │   ├── ContentFile.cs                       # File metadata entity
│   │   │   ├── ContentFileType.cs                   # Enum (Pdf, Html, Json, Image, Video, Zip)
│   │   │   ├── ValidationIssue.cs                   # Individual error/warning record
│   │   │   ├── ValidationResult.cs                  # File-level validation outcome
│   │   │   ├── ValidationSummary.cs                 # Aggregate run statistics
│   │   │   ├── ValidationOptions.cs                 # Configurable engine settings
│   │   │   ├── ValidationProgressReport.cs          # UI progress notification payload
│   │   │   └── ValidationReport.cs                  # Exportable data transfer object
│   │   └── Validation/
│   │       ├── ValidationContext.cs                 # Package-wide index (O(1) lookups)
│   │       ├── ValidationRuleBase.cs                # Abstract base class (Template Method)
│   │       └── Rules/
│   │           ├── FileSizeRule.cs                  # Checks maximum file byte limits
│   │           ├── ExtensionRule.cs                 # Enforces allowed extension whitelist
│   │           ├── EmptyFileRule.cs                 # Detects 0-byte placeholder files
│   │           ├── FileNameRule.cs                  # Rejects illegal chars & reserved names
│   │           ├── RequiredMetadataRule.cs          # Validates JSON schema & mandatory fields
│   │           └── DuplicateFileNameRule.cs         # Flags package-wide duplicate file names
│   │
│   ├── LearningContentValidator.Infrastructure/      # External I/O and implementations
│   │   ├── FileSystem/
│   │   │   └── FileScanner.cs                       # Resilient recursive directory scanner
│   │   ├── Validation/
│   │   │   └── ContentValidationService.cs          # Async engine with cancellation & progress
│   │   ├── Reporting/
│   │   │   ├── JsonReportGenerator.cs               # System.Text.Json report exporter
│   │   │   ├── CsvReportGenerator.cs                # RFC 4180 compliant CSV exporter
│   │   │   └── ReportGeneratorFactory.cs            # Strategy resolver factory
│   │   └── Logging/
│   │       ├── FileLogger.cs                        # Thread-safe text file logger
│   │       ├── FileLoggerProvider.cs                # Microsoft.Extensions.Logging provider
│   │       └── FileLoggerExtensions.cs              # ILoggingBuilder registration helper
│   │
│   └── LearningContentValidator.App/                # WPF Desktop Presentation Layer
│       ├── Converters/
│       │   ├── StatusToBrushConverter.cs            # Visual status badges (Green/Amber/Red)
│       │   ├── ByteSizeConverter.cs                 # Human-readable bytes (KB, MB, GB)
│       │   └── InverseBooleanConverter.cs           # Boolean negation for UI state
│       ├── Services/
│       │   ├── IDialogService.cs                    # FolderBrowser and SaveFileDialog abstraction
│       │   └── WindowsDialogService.cs              # Concrete WPF native dialog implementation
│       ├── ViewModels/
│       │   ├── ViewModelBase.cs                     # INotifyPropertyChanged with CallerMemberName
│       │   ├── RelayCommand.cs                      # Synchronous ICommand
│       │   ├── AsyncRelayCommand.cs                 # Asynchronous ICommand with re-entrancy lock
│       │   ├── FileResultViewModel.cs               # Per-file presentation wrapper
│       │   └── MainViewModel.cs                     # Central coordinator ViewModel
│       ├── Views/
│       │   ├── MainWindow.xaml                      # Responsive XAML layout
│       │   └── MainWindow.xaml.cs                   # Code-behind (DI DataContext only)
│       ├── App.xaml                                 # Application markup
│       └── App.xaml.cs                              # Composition Root (DI container setup)
│
├── tests/
│   └── LearningContentValidator.Tests/              # NUnit 4 automated test suite
│       ├── TestHelpers/
│       │   └── TempDirectoryFixture.cs              # RAII-style temp folder lifecycle helper
│       ├── Unit/
│       │   ├── FileSizeRuleTests.cs                 # Unit tests for FileSizeRule
│       │   ├── ExtensionRuleTests.cs                # Unit tests for ExtensionRule
│       │   ├── EmptyFileRuleTests.cs                # Unit tests for EmptyFileRule
│       │   ├── FileNameRuleTests.cs                 # Unit tests for FileNameRule
│       │   ├── RequiredMetadataRuleTests.cs         # Unit tests for RequiredMetadataRule
│       │   ├── DuplicateFileNameRuleTests.cs        # Unit tests for DuplicateFileNameRule
│       │   ├── ValidationSummaryTests.cs            # Unit tests for LINQ metrics calculations
│       │   └── ReportGeneratorTests.cs              # Unit tests for JSON/CSV serializers
│       └── Integration/
│           ├── FileScannerTests.cs                  # Real filesystem crawler integration tests
│           └── ContentValidationServiceTests.cs     # Engine orchestration & Moq mock tests
│
├── samples/
│   └── CoursePackage_Physics101/                    # Ready-to-run demo course package
├── Jenkinsfile                                      # Declarative CI/CD pipeline for Windows agents
├── .gitignore                                       # Git ignore for .NET, VS, and Rider
└── LearningContentValidator.sln                     # Complete Visual Studio solution file
```

---

## 4. How the Application Works

1. **Folder Selection**: The user selects a root course package folder using the native folder browser dialog or by typing the path directly.
2. **Recursive Crawling (`FileScanner`)**:
   - Traverses directories asynchronously using `DirectoryInfo.GetFiles()` and `GetDirectories()`.
   - Filters out temporary and hidden files (`.git`, `.vs`, `Thumbs.db`, `desktop.ini`, `~$*`).
   - Resilient against I/O exceptions: if an individual file or directory is inaccessible (`UnauthorizedAccessException`), the scanner logs a warning and proceeds without crashing.
3. **Context Pre-indexing (`ValidationContext`)**:
   - The discovered files are indexed into a `ValidationContext`.
   - Pre-computes a lookup table `FilesByName = files.ToLookup(f => f.FileName, StringComparer.OrdinalIgnoreCase)`. This ensures duplicate file detection across folders runs in $O(1)$ query time per file rather than $O(N^2)$.
4. **Extensible Rule Execution (`ContentValidationService`)**:
   - Executes each registered `IValidationRule` in sequence for each file.
   - Rules can flag `Error` (e.g. oversized file, corrupt JSON, disallowed extension) or `Warning` (e.g. 0-byte file, duplicate file name).
   - If a custom rule throws an unexpected exception, the service isolates the fault, logs the technical trace, and records an Error issue for that rule without aborting the session.
   - Cooperative cancellation via `CancellationToken` allows the user to stop the scan instantly.
   - Progress is dispatched back to the UI via `IProgress<ValidationProgressReport>` without blocking the UI thread.
5. **Real-time Dashboard & LINQ Filtering**:
   - Aggregates metrics (`TotalFiles`, `ValidFiles`, `InvalidFiles`, `WarningCount`, `ErrorCount`, `Duration`) using LINQ.
   - DataGrid displays results with colored status badges (Green = Valid, Amber = Warning, Red = Invalid).
   - Instant filtering buttons (`All`, `Errors Only`, `Warnings Only`, `Valid Only`) use in-memory LINQ queries.
   - Selecting any row displays the full list of rule violations and exact error descriptions in the details drawer.
6. **Report Generation (`IReportGenerator`)**:
   - Export results to **JSON** (`JsonReportGenerator`) or **CSV** (`CsvReportGenerator`).
   - JSON uses indented `System.Text.Json` serialization.
   - CSV follows RFC 4180 standards with proper double-quote escaping.

---

## 5. Technologies Used

- **Language**: C# 12 / C# 13 (Nullable reference types enabled, Pattern matching, String interpolation, Raw string literals)
- **Runtime**: .NET 8 (with `<RollForward>Major</RollForward>` support for forward compatibility)
- **UI Framework**: Windows Presentation Foundation (WPF) with XAML data binding
- **Dependency Injection**: `Microsoft.Extensions.DependencyInjection`
- **Logging**: `Microsoft.Extensions.Logging` (Console + Custom Thread-Safe File Logger)
- **Configuration**: `Microsoft.Extensions.Options`
- **Testing Framework**: NUnit 4, NUnit3TestAdapter, Microsoft.NET.Test.Sdk
- **Mocking Library**: Moq 4
- **Build System**: dotnet CLI / MSBuild
- **Continuous Integration**: Jenkins Declarative Pipeline (`Jenkinsfile`)

---

## 6. How to Build & Run

### Prerequisites
- Windows 10 or 11
- .NET 8 SDK or .NET 10 SDK (installed)

### 1. Build the Solution
```powershell
dotnet build LearningContentValidator.sln
```

### 2. Run All Unit & Integration Tests
```powershell
dotnet test LearningContentValidator.sln
```
Output:
```
Passed!  - Failed: 0, Passed: 56, Skipped: 0, Total: 56, Duration: ~1 sec
```

### 3. Run the Desktop Application
```powershell
dotnet run --project src/LearningContentValidator.App/LearningContentValidator.App.csproj
```

### 4. Test with the Bundled Sample Package
When the application opens:
1. Click **Browse...**
2. Select the `samples/CoursePackage_Physics101` directory inside the repository.
3. Click **Start Validation**.
4. Observe the progress bar, the metric tiles, and the colored status badges in the results grid.
5. Try clicking the filter buttons (**Errors Only**, **Warnings Only**, **Valid Only**).
6. Click **Export Report (JSON)** and **Export Report (CSV)** to verify export functionality.

---

## 7. C++ Concepts vs C# Concepts (Quick Reference for Dassault Systèmes)

Dassault Systèmes products (CATIA, SOLIDWORKS, 3DEXPERIENCE) are heavily engineered with C++ and C#. Here is how core concepts map across both languages in this project:

| Concept | C++ | C# (.NET) | Implementation in This Project |
| :--- | :--- | :--- | :--- |
| **Pure Interface** | Abstract class with pure virtual methods (`virtual void Foo() = 0;`) | `interface IFoo` | `IValidationRule`, `IFileScanner`, `IReportGenerator` |
| **Abstract Base** | Class with pure virtual + implemented methods | `abstract class FooBase` | `ValidationRuleBase` (Template Method pattern) |
| **Dynamic Array** | `std::vector<T>` | `List<T>` | `List<ContentFile>`, `List<ValidationIssue>` |
| **Hash Map** | `std::unordered_map<Key, Val>` | `Dictionary<TKey, TValue>` | Extension-to-type mapping in `ContentFile` |
| **Hash Multimap** | `std::unordered_multimap<Key, Val>` | `ILookup<TKey, TElement>` | `ValidationContext.FilesByName` via LINQ `.ToLookup()` |
| **Unique Set** | `std::unordered_set<T>` | `HashSet<T>` | `ValidationOptions.AllowedExtensions` |
| **Memory / Lifetime** | RAII (`std::unique_ptr`, destructors) | GC + `IDisposable` (`using` statement) | `TempDirectoryFixture` deterministic test cleanup |
| **Function Pointers** | `std::function<void()>`, lambdas | Delegates (`Action`, `Func<T>`) | `RelayCommand`, LINQ lambdas `f => f.IsValid` |
| **Async Operations** | `std::future`, `std::async`, threads | `Task`, `Task<T>`, `async / await` | `ScanAsync`, `ValidateFilesAsync`, `GenerateReportAsync` |
| **Generics / Templates** | C++ Templates (compile-time expansion) | C# Generics (reified type metadata) | `SetProperty<T>`, `IEnumerable<T>`, `IOptions<T>` |
| **String Type** | `std::string` / `wchar_t*` (value/buffer) | `System.String` (immutable reference type) | String manipulation, escaping in `CsvReportGenerator` |

---

## 8. Continuous Integration (Jenkins CI/CD)

The repository includes a declarative `Jenkinsfile` configured for Windows build agents:
1. **Checkout**: Pulls code from Git.
2. **Environment Diagnostic**: Logs `dotnet --info` for environment consistency.
3. **Restore**: Executes `dotnet restore LearningContentValidator.sln`.
4. **Build**: Executes `dotnet build --configuration Release --no-restore`.
5. **Test**: Executes `dotnet test --no-build --logger "trx;LogFileName=test_results.trx"`.
6. **Publish**: Packages the WPF application using `dotnet publish`.
7. **Archive**: Archives build binaries and publishes the TRX test results for Jenkins test trend graphing.

---

## 9. Interview Discussion (Dassault Systèmes Preparation)

Use this section to articulate your design choices, trade-offs, and technical rationale during your technical interview.

### Q1: Why did you choose WPF?
> **Answer**: WPF provides a mature, hardware-accelerated UI framework with first-class data binding and XAML separation. Unlike WinForms, WPF natively supports MVVM, template-based UI customization (e.g., custom status badge cells), asynchronous event dispatching via `Dispatcher`, and vector scaling. For desktop engineering tools on Windows, WPF remains the enterprise standard for stability and rich desktop controls.

### Q2: Why did you use MVVM?
> **Answer**: Model-View-ViewModel separates presentation logic from UI controls. In traditional code-behind architectures, UI event handlers directly touch file systems and controls, making automated unit testing virtually impossible. With MVVM:
> - ViewModels have zero knowledge of WPF controls (`Button`, `DataGrid`).
> - We can test complete user workflows (button clicks, command execution, filtering) in automated NUnit tests by instantiating the ViewModel and verifying state changes.
> - The UI designer can redesign XAML without touching business logic.

### Q3: Why use interfaces?
> **Answer**: Interfaces define contracts and enforce the **Interface Segregation Principle (ISP)** and **Dependency Inversion Principle (DIP)**. High-level orchestrators like `ContentValidationService` depend on the `IValidationRule` abstraction, not concrete rule classes. This allows new rules to be created independently and permits mocking dependencies with Moq during testing.

### Q4: Why use Dependency Injection?
> **Answer**: DI eliminates hardcoded dependencies and service locators. In this project:
> - `MainViewModel` declares what it needs in its constructor (`IFileScanner`, `IContentValidationService`, `ReportGeneratorFactory`, `IDialogService`, `ILogger`).
> - At runtime, `App.xaml.cs` (the Composition Root) resolves and injects concrete implementations.
> - In unit tests, we inject mock or stub implementations without touching the ViewModel's internal logic.

### Q5: Why use NUnit?
> **Answer**: NUnit is a battle-tested enterprise testing framework with rich assertion syntax (`Assert.That(actual, Is.EqualTo(expected))`), parameterization support (`[TestCase]`), lifecycle fixtures (`[SetUp]`, `[TearDown]`), and seamless integration with CI test runners via TRX loggers. It provides clean Arrange-Act-Assert readability.

### Q6: How did you structure the validation engine?
> **Answer**: I implemented the **Strategy Pattern** combined with the **Template Method Pattern**:
> - Every rule implements `IValidationRule` and inherits from `ValidationRuleBase`.
> - A `ValidationContext` holds pre-computed indices (such as an `ILookup<string, ContentFile>`).
> - `ContentValidationService` acts as the engine, executing rules across files.
> - This decouples the rules from each other: each rule has a single responsibility (SRP).

### Q7: How would you add a new validation rule?
> **Answer**: Following the **Open/Closed Principle (OCP)**:
> 1. Create a new class inheriting from `ValidationRuleBase` (e.g., `VideoBitrateRule`).
> 2. Implement `ValidateAsync(ContentFile file, ValidationContext context)`.
> 3. Register it in `App.xaml.cs`: `services.AddSingleton<IValidationRule, VideoBitrateRule>();`.
> 
> *Zero lines of code need to be modified in `ContentValidationService` or `MainViewModel`.*

### Q8: How did you prevent the UI from freezing?
> **Answer**: 
> 1. Heavy file system I/O (`FileScanner.ScanAsync`) and file reads (`RequiredMetadataRule`) run asynchronously on thread pool threads via `Task.Run` or native async I/O (`File.ReadAllTextAsync`).
> 2. ViewModels use `AsyncRelayCommand` with `async/await`.
> 3. Progress updates are dispatched using `IProgress<T>`, which automatically captures the UI `SynchronizationContext` and marshals updates back to the UI thread safely.

### Q9: How did you handle exceptions?
> **Answer**: I avoided blanket `catch (Exception)` blocks at low levels. Instead, I established clean architectural boundaries:
> - In `FileScanner`: Handled specific I/O exceptions (`UnauthorizedAccessException`, `PathTooLongException`, `DirectoryNotFoundException`) on a per-file/directory basis so inaccessible files are logged and skipped without halting the whole scan.
> - In `ContentValidationService`: Isolated rule execution inside a try/catch boundary so a buggy third-party rule cannot crash the entire validation session.
> - In `App.xaml.cs`: Handled `DispatcherUnhandledException` as a top-level fallback to log unexpected crashes and display a user-friendly dialog.

### Q10: How did you test file-system operations?
> **Answer**: I avoided depending on machine paths (`C:\Users\...`). Instead, I created a `TempDirectoryFixture` implementing `IDisposable`. In the test `[SetUp]`, it creates an isolated directory in `Path.GetTempPath()` with a unique GUID. The tests create actual files, run the scanner/rules, assert outcomes, and in `[TearDown]`, the fixture deletes the directory and all contents recursively (RAII pattern).

### Q11: What is the difference between unit and integration testing here?
> **Answer**:
> - **Unit Tests** test components in total isolation without touching the operating system. For example, `FileSizeRuleTests` and `ValidationSummaryTests` verify in-memory logic using fabricated `ContentFile` models.
> - **Integration Tests** verify components interacting with external systems or multiple layers together. For example, `FileScannerTests` hits the actual disk file system to verify recursive directory enumeration and attribute handling.

### Q12: How would you make tests less flaky?
> **Answer**:
> 1. Never share state or mutable directories across tests. Each test runs in its own unique `TempDirectoryFixture`.
> 2. Avoid hardcoded `Thread.Sleep()`. Use async completion or explicit waits.
> 3. Avoid hardcoded path separators (use `Path.Combine` instead of string concatenation).
> 4. Ensure files are properly closed and disposed after writing before invoking scanner assertions.

### Q13: How would you run this in CI?
> **Answer**: Using our declarative `Jenkinsfile`. The build agent runs `dotnet restore`, `dotnet build --no-restore`, and `dotnet test --logger "trx;LogFileName=test_results.trx"`. The TRX logger generates standard XML test results that Jenkins parses to show test status and historical trends.

### Q14: What happens when Jenkins reports a failed test?
> **Answer**: 
> 1. The `dotnet test` command exits with a non-zero exit code.
> 2. The pipeline marks the stage as failed.
> 3. In the `post` block, the `mstest` step parses the TRX file and identifies the exact test method, exception message, and stack trace.
> 4. Jenkins flags the build status as **UNSTABLE** or **FAILURE** and notifies the team.

### Q15: What are build artifacts?
> **Answer**: Build artifacts are the output binaries, executables, symbols, and test reports generated during compilation. In our pipeline, `dotnet publish` produces the self-contained WPF executable and dependent DLLs in `artifacts/publish`, while `dotnet test` produces `test_results.trx` in `artifacts/test-results`. The `archiveArtifacts` step preserves them in Jenkins.

### Q16: How would you parallelize tests?
> **Answer**:
> - In NUnit, add `[Parallelizable(ParallelScope.All)]` or `[assembly: Parallelizable]`.
> - Because our unit tests are purely in-memory and our integration tests use unique isolated GUID folders via `TempDirectoryFixture`, tests can run concurrently across CPU cores without race conditions or file lock collisions.

### Q17: How would you improve the application for thousands of files?
> **Answer**:
> 1. **Streaming / Yield Enumeration**: Use `Directory.EnumerateFiles()` instead of `GetFiles()` to process files in an asynchronous pipeline (`IAsyncEnumerable<ContentFile>`) without loading thousands of `FileInfo` objects into memory up front.
> 2. **Parallel Validation**: Use `Parallel.ForEachAsync` with a configurable `MaxDegreeOfParallelism` (e.g. `Environment.ProcessorCount`) to validate independent files concurrently across CPU cores.
> 3. **Virtualization in UI**: Enable WPF DataGrid UI virtualization (`VirtualizingStackPanel.IsVirtualizing="True"`) so the UI only instantiates visual elements for visible rows on screen.

### Q18: How would you monitor this application in production?
> **Answer**:
> - Incorporate structured logging with correlation IDs and export logs to a centralized log aggregator (such as Seq, Elasticsearch/Logstash, or Azure Application Insights).
> - Add telemetry metrics for scan durations, package sizes, and rule failure frequencies.
> - Implement crash dump reporting via `AppDomain.CurrentDomain.UnhandledException` using tools like Windows Error Reporting or Sentry.

---

## 10. Top 10 Files to Study First

If you are reading this codebase to prepare for your interview, study these files in this exact order:

1. [`ContentFile.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/src/LearningContentValidator.Domain/Models/ContentFile.cs) - See clean encapsulation, properties, dictionary lookups, and file type classification.
2. [`IValidationRule.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/src/LearningContentValidator.Domain/Interfaces/IValidationRule.cs) - Understand the core interface contract enabling the Open/Closed Principle.
3. [`ValidationRuleBase.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/src/LearningContentValidator.Domain/Validation/ValidationRuleBase.cs) - Study abstract classes, polymorphism, and the Template Method pattern.
4. [`RequiredMetadataRule.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/src/LearningContentValidator.Domain/Validation/Rules/RequiredMetadataRule.cs) - Learn async I/O, `System.Text.Json` parsing, and defensive exception handling.
5. [`ValidationContext.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/src/LearningContentValidator.Domain/Validation/ValidationContext.cs) - Learn LINQ `ToLookup` for $O(1)$ duplicate checking across large datasets.
6. [`FileScanner.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/src/LearningContentValidator.Infrastructure/FileSystem/FileScanner.cs) - Study recursive directory traversal and fault-tolerant exception handling.
7. [`ContentValidationService.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/src/LearningContentValidator.Infrastructure/Validation/ContentValidationService.cs) - Study orchestration, `CancellationToken`, `IProgress<T>`, and LINQ aggregation.
8. [`MainViewModel.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/src/LearningContentValidator.App/ViewModels/MainViewModel.cs) - Master MVVM command bindings, async execution, cancellation, and observable collections.
9. [`FileScannerTests.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/tests/LearningContentValidator.Tests/Integration/FileScannerTests.cs) - Learn how to test real filesystem interactions cleanly with `TempDirectoryFixture`.
10. [`ContentValidationServiceTests.cs`](file:///D:/C%23%20learning/WpfApp1-validate-content/tests/LearningContentValidator.Tests/Integration/ContentValidationServiceTests.cs) - Learn when and how to use Moq to verify multi-rule behavior and fault isolation.

