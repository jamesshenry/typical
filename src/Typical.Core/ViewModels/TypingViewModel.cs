using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Typical.Core.Interfaces;
using Typical.Core.Statistics;

namespace Typical.Core.ViewModels;

public partial class TypingViewModel : ObservableObject, IBindableView
{
    private readonly GameEngine _engine;
    private readonly ILogger<TypingViewModel> _logger;

    [ObservableProperty]
    private string _targetText = "The quick brown fox jumped over the lazy dog.";

    [ObservableProperty]
    private string _typedText = "";

    [ObservableProperty]
    private bool _isGameOver;

    [ObservableProperty]
    private double _wpm;

    [ObservableProperty]
    private double _accuracy;

    [ObservableProperty]
    private string _timeElapsed = "00:00";

    public TypingViewModel(GameEngine engine, ILogger<TypingViewModel> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    /// <summary>
    /// Processes input received from the View.
    /// Maps Key events to Core Game Logic.
    /// </summary>
    public void ProcessInput(char c, bool isBackspace)
    {
        if (IsGameOver)
            return;

        // Pass to engine
        bool handled = _engine.ProcessKeyPress(c, isBackspace);

        if (handled)
        {
            UpdateState();
        }
    }

    /// <summary>
    /// Synchronizes the Engine state with ViewModel properties.
    /// This triggers PropertyChanged notifications for the View.
    /// </summary>
    private void UpdateState()
    {
        TypedText = _engine.UserInput;
        TargetText = _engine.TargetText;
        IsGameOver = _engine.IsOver;

        // Pull current stats snapshot
        // Assuming GameEngine exposes a way to get the latest stats
        // If not, calculate accuracy manually here
        if (TypedText.Length > 0)
        {
            int correct = 0;
            for (int i = 0; i < TypedText.Length; i++)
                if (TypedText[i] == TargetText[i])
                    correct++;

            Accuracy = (double)correct / TypedText.Length * 100;
        }
    }

    public KeystrokeType GetStatus(int index)
    {
        return index >= TypedText.Length ? KeystrokeType.Untyped
            : TypedText[index] == TargetText[index] ? KeystrokeType.Correct
            : KeystrokeType.Incorrect;
    }

    public void OnNavigatedTo()
    {
        _logger.LogInformation($"Navigated to {nameof(TypingViewModel)}");
    }

    public void OnNavigatedFrom()
    {
        _logger.LogInformation($"Navigated from {nameof(TypingViewModel)}");
    }

    /// <summary>
    /// Updates WPM and Timer independently of keystrokes.
    /// </summary>
    private async Task UpdateStatsLoop()
    {
        while (!IsGameOver)
        {
            // Assuming GameEngine/Stats provides these values
            // Calculate WPM: (characters / 5) / minutes
            // This ensures the UI updates even if the user stops typing
            Wpm = CalculateWpm();

            // Example of a simple timer logic
            // TimeElapsed = ...

            await Task.Delay(500); // Update twice per second for smoothness
        }
    }

    private double CalculateWpm()
    {
        var snapShot = _engine.Stats.CreateSnapshot();
        return snapShot.WordsPerMinute;
    }
}
