using System.Security.AccessControl;

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
    private string _targetText = "";

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

    [ObservableProperty]
    private IReadOnlyList<KeystrokeType> _characterStates = [];
    [ObservableProperty] private KeystrokeType[] _displayStates = [];

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
        if (!_engine.IsRunning && _engine.IsInitialized)
        {
            _engine.StartNewGame();
            TargetText = _engine.TargetText;
        }

        bool engineAcceptedKey = _engine.ProcessKeyPress(c, isBackspace);

        var display = _engine.CharacterStates.ToArray();

    if (!engineAcceptedKey && !isBackspace && c != '\0')
    {
        int cursor = _engine.UserInput.Length;
        if (cursor < display.Length)
        {
            display[cursor] = KeystrokeType.Incorrect;
        }
    }

        UpdateState();
        DisplayStates = display;
    }

    /// <summary>
    /// Synchronizes the Engine state with ViewModel properties.
    /// This triggers PropertyChanged notifications for the View.
    /// </summary>
    private void UpdateState()
    {
        TypedText = _engine.UserInput;
        IsGameOver = _engine.IsOver;

        var snapshot = _engine.Stats.CreateSnapshot();
        Accuracy = snapshot.Accuracy;
        Wpm = snapshot.WordsPerMinute;
        TimeElapsed = snapshot.ElapsedTime.ToString(@"mm\:ss");
    }

    public KeystrokeType GetStatus(int index)
    {
        return index < 0 || index >= _engine.CharacterStates.Count
            ? KeystrokeType.Untyped
            : _engine.CharacterStates[index];
    }

    public void OnNavigatedTo()
    {
        _logger.LogInformation($"Navigated to {nameof(TypingViewModel)}");
    }

    public void OnNavigatedFrom()
    {
        _logger.LogInformation($"Navigated from {nameof(TypingViewModel)}");
    }

    public async Task InitializeAsync()
    {
        await _engine.InitializeAsync();
        TargetText = _engine.TargetText;
        UpdateState();
    }
}
