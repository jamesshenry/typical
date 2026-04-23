using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Typical.Core.Events;
using Typical.Core.Interfaces;
using Typical.Core.Statistics;
using Typical.Core.Text;

namespace Typical.Core.ViewModels;

public partial class TypingViewModel
    : ObservableObject,
        INavigatableView,
        IRecipient<GameResetMessage>
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
    public partial KeystrokeType[] DisplayStates { get; set; } = [];

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

        WeakReferenceMessenger.Default.Register<TypingViewModel, GameResetMessage>(
            this,
            (r, m) => r.Receive(m)
        );
    }

    public bool IsGameOver => _engine.IsOver;

    /// <summary>
    /// Processes input received from the View.
    /// Maps Key events to Core Game Logic.
    /// </summary>
    public async void ProcessInput(char c, bool isBackspace)
    {
        if (_engine.IsOver)
        {
            await InitializeAsync();
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

        WeakReferenceMessenger.Default.Send(
            new GameStateUpdatedMessage(TargetText, _engine.UserInput, snapshot, _engine.IsOver)
        );
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

    public async Task InitializeAsync(TextSample? textSample = null)
    {
        var result = textSample ?? await _textProvider.GetQuoteAsync();
        _engine.LoadText(result);
        TargetText = _engine.TargetText;

        DisplayStates = new KeystrokeType[TargetText.Length];
        Array.Fill(DisplayStates, KeystrokeType.Untyped);
        UpdateState();
    }

    public async void Receive(GameResetMessage message)
    {
        TextSample textSample = message.Settings switch
        {
            QuoteMode q => (await _textProvider.GetQuoteAsync(q.Length)),
            _ => throw new InvalidOperationException(
                $"Unsupported mode settings type: {message.Settings.Value?.GetType().Name ?? message.Settings.GetType().Name}"
            ),
        };

        await InitializeAsync(textSample);
    }
}
