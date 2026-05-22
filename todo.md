## Header

- margin top 2 to give some space from menubar.
- not full width
- row of several groups of buttons, example:
  - **toggles**: `[punctuation, numbers]`
  - **modes**: `[time, words, quote, zen, custom]`
  - **mode_options**: `[change]`
- **modes** group is central focal point
  - example: quote mode
    - disables **toggles** group on left
    - **mode_options**: `[all, short, medium, long, thick]` on right
- all buttons reset the typing view state to fresh with new quote
- `change` brings up an options menu

---

# AI Agent System Prompt & Execution Protocol

**ROLE:** You are an expert .NET 11 C# developer adhering to strict Test-Driven Development (TDD) principles.
**STACK:** C# 12/13, .NET 11, CommunityToolkit.Mvvm, Terminal.Gui v2, TUnit, SQLite, DbUp.
**PROTOCOL:**

1. Execute tasks strictly in the sequence provided.
2. For each task, read the target files, make the modifications, and run `dotnet test src/Typical.Tests/Typical.Tests.csproj`.
3. Do not proceed to the next task if the tests fail. Fix the compilation or logic error first.
4. Favor explicit Dependency Injection over static singletons.

---

## 🛠️ Phase 1: Architectural Refactoring for Testability

*Objective: Remove static coupling to allow deterministic testing of time and messaging.*

### Task 1.1: Inject `TimeProvider` into Game Statistics

- **Target File:** `src/Typical.Core/Statistics/GameStats.cs`
- **Action:**
  - Change the parameterless constructor to accept `TimeProvider timeProvider`.
  - Assign it to the `_timeProvider` readonly field.
- **Target File:** `src/Typical.Core/TypingSession.cs`
- **Action:**
  - Update the constructor to accept `TimeProvider timeProvider`.
  - Update the instantiation: `Stats = new GameStats(timeProvider);`.
- **Target File:** `src/Typical.Tests/TypingSessionTests.cs`
- **Action:**
  - Fix compiler errors by passing `TimeProvider.System` (or a `FakeTimeProvider`) to `TypingSession` instantiations in the test setups.

### Task 1.2: Abstract `IMessenger` in ViewModels

- **Target Files:**
  - `src/Typical.Core/ViewModels/TypingViewModel.cs`
  - `src/Typical.Core/ViewModels/SettingsViewModel.cs`
- **Action:**
  - Add `IMessenger messenger` to their constructors.
  - Replace all instances of `WeakReferenceMessenger.Default.Send(...)` with `_messenger.Send(...)`.
- **Target File:** `src/Typical.Tests/NavigationServiceTests.cs`
- **Action:**
  - Fix any compiler errors in the test setups by passing a mock/concrete `IMessenger` to the modified ViewModels.

---

## 🧪 Phase 2: Core Domain Logic Tests

*Objective: Implement tests for the currently empty GameStats test file.*

### Task 2.1: WPM and Accuracy Math Tests

- **Target File:** `src/Typical.Tests/Core/GameStatsTests.cs`
- **Dependencies to use:** `Microsoft.Extensions.TimeProvider.Testing` (`FakeTimeProvider`), `TUnit`.
- **Action:** Implement the following tests:
  1. `CreateSnapshot_CalculatesAccurateWPM_BasedOnTime`:
     - **Setup:** Create `FakeTimeProvider`, pass to `GameStats`. Call `Start()`.
     - **Action:** Record 6 correct characters (e.g., "hello "). Advance `FakeTimeProvider` by exactly 12 seconds. Call `Stop()`.
     - **Assert:** `WPM` should equal `6` (6 chars / 5 = 1.2 words. 1.2 words in 0.2 minutes = 6 WPM). `Accuracy` should equal `100`.
  2. `CreateSnapshot_CalculatesAccuracy_WithErrors`:
     - **Action:** Record 9 `Correct` keystrokes and 1 `Incorrect` keystroke.
     - **Assert:** `Accuracy` equals `90`.
  3. `CreateSnapshot_HandlesZeroElapsed_PreventsDivideByZero`:
     - **Setup:** Start and immediately stop without advancing time.
     - **Assert:** Returns default snapshot without throwing an exception.

---

## 🏗️ Phase 3: ViewModel Behavioral Tests

*Objective: Test state mutations and message handling in ViewModels.*

### Task 3.1: Implement StatsViewModel Tests

- **Target File:** `src/Typical.Tests/StatsViewModelTests.cs`
- **Action:** Implement tests using TUnit:
  1. `Receive_GameStatsUpdatedMessage_UpdatesProperties`:
     - **Setup:** Instantiate `StatsViewModel`.
     - **Action:** Call `Receive(new GameStatsUpdatedMessage(mockSnapshot))`.
     - **Assert:** Verify the ViewModels properties (WPM, Accuracy, ElapsedTime) map exactly to the snapshot's values.

### Task 3.2: Create TypingViewModel Tests

- **Target File:** `src/Typical.Tests/TypingViewModelTests.cs` (Create new file)
- **Action:** Implement tests:
  1. `InitializeAsync_LoadsQuote_FromTextProvider`:
     - **Setup:** Mock `ITextProvider` to return a specific `TextSample`.
     - **Action:** Call `InitializeAsync()`.
     - **Assert:** Verify `ViewModel.Target` equals the mock text.
  2. `ProcessInput_UpdatesEngine_AndBroadcastsMessage`:
     - **Setup:** Inject a mock `IMessenger`. Call `InitializeAsync()`.
     - **Action:** Call `ProcessInput("a", false)`.
     - **Assert:** Verify `_messenger.Send` was called with a `GameStatsUpdatedMessage`.
  3. `Receive_GameResetMessage_ReloadsText_BasedOnSettings`:
     - **Action:** Call `Receive(new GameResetMessage(new QuoteMode(QuoteLength.Short)))`.
     - **Assert:** Verify `ITextProvider.GetQuoteAsync(QuoteLength.Short)` was called.

---

## 🗄️ Phase 4: Data Access Integration Tests

*Objective: Test SQLite repositories against an in-memory database.*

### Task 4.1: Create TextRepository Tests

- **Target File:** `src/Typical.Tests/TextRepositoryTests.cs` (Create new file)
- **Constraint:** Do NOT mock the database. Use SQLite in-memory mode (`Data Source=:memory:`).
- **Setup Block:**
  - Create an in-memory SQLite connection and open it.
  - Instantiate `TypicalDbOptions` pointing to this connection string.
  - Run the `DatabaseMigrator` to apply `Script_00100_CreateQuotesTable` and `Script_00200_SeedInitialQuotes`.
- **Action:** Implement tests:
  1. `GetRandomQuoteAsync_ReturnsSeededQuote`:
     - **Action:** Call `GetRandomQuoteAsync()`.
     - **Assert:** Result is not null, `Text` is populated, `Author` is parsed correctly.
  2. `GetQuoteAsync_MapsDBNullAuthor_ToUnknown`:
     - **Action:** Insert a raw record into the DB with a `NULL` author. Retrieve it via `GetQuoteAsync()`.
     - **Assert:** Verify `Quote.Author` equals `"Unknown"`.

---

## 🚦 Phase 5: Edge Cases

*Objective: Guarantee application stability during anomalous inputs.*

### Task 5.1: Grapheme Cluster Boundary Tests

- **Target File:** `src/Typical.Tests/TypingSessionTests.cs`
- **Action:** Add a test `ProcessKeyPress_WithEmoji_HandlesGraphemesCorrectly`:
  - **Setup:** Load text containing an emoji with a modifier (e.g., `👍🏽`).
  - **Action:** Call `ProcessKeyPress("👍🏽", false)`.
  - **Assert:** Verify `GameStats.TotalPhysicalKeystrokes` counts this as exactly **1** correct keystroke, not multiple bytes.

### Task 5.2: ViewLocator Completeness Test

- **Target File:** `src/Typical.Tests/NavigationServiceTests.cs` (or create `ViewLocatorTests.cs`)
- **Action:** Add a test using Reflection:
  - **Setup:** Find all classes in `Typical.Core` that implement `INavigatableView`.
  - **Action:** Pass each to `ViewLocator.GetView(sp, instance)`.
  - **Assert:** No `ArgumentException` is thrown (proving every Navigatable ViewModel has a mapped View in the UI project).
