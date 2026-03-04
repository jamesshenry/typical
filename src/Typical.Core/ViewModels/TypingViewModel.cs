using System.Security.AccessControl;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Typical.Core.Interfaces;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core.ViewModels;

public partial class TypingViewModel : ObservableObject, IBindableView
{
    private readonly GameEngine _engine;
    private readonly ITextProvider _textProvider;
    private readonly INavigationService _navigationService;
    private readonly ILogger<TypingViewModel> _logger;

    [ObservableProperty]
    private string _targetText = "";

    // [ObservableProperty]
    // private bool _isGameOver;

    [ObservableProperty]
    private double _wpm;

    [ObservableProperty]
    private double _accuracy;

    [ObservableProperty]
    private string _timeElapsed = "00:00";

    [ObservableProperty]
    private KeystrokeType[] _displayStates = [];

    public TypingViewModel(
        GameEngine engine,
        ITextProvider textProvider,
        INavigationService navigationService,
        ILogger<TypingViewModel> logger
    )
    {
        _engine = engine;
        _textProvider = textProvider;
        _navigationService = navigationService;
        _logger = logger;
    }

    public bool IsGameOver => _engine.IsOver;

    /// <summary>
    /// Processes input received from the View.
    /// Maps Key events to Core Game Logic.
    /// </summary>
    public void ProcessInput(char c, bool isBackspace)
    {
        if (_engine.IsOver)
        {
            _navigationService.NavigateTo<SettingsViewModel>();
            return;
        }

        bool accepted = _engine.ProcessKeyPress(c, isBackspace);

        var states = _engine.CharacterStates.ToArray();

        if (!accepted && !isBackspace && c != '\0')
        {
            int pos = _engine.UserInput.Length;
            if (pos < states.Length)
            {
                states[pos] = KeystrokeType.Incorrect;
            }
        }

        DisplayStates = states;
        UpdateState();
    }

    /// <summary>
    /// Synchronizes the Engine state with ViewModel properties.
    /// This triggers PropertyChanged notifications for the View.
    /// </summary>
    private void UpdateState()
    {
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
        var result = await _textProvider.GetTextAsync();
        _engine.LoadText(result);
        TargetText = _engine.TargetText;

        DisplayStates = new KeystrokeType[TargetText.Length];
        Array.Fill(DisplayStates, KeystrokeType.Untyped);
        UpdateState();
    }
}
