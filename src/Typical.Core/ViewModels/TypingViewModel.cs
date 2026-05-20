using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Typical.Core.Data;
using Typical.Core.Events;
using Typical.Core.Interfaces;
using Typical.Core.Statistics;
using Typical.Core.Text;
using Timer = System.Timers.Timer;

namespace Typical.Core.ViewModels;

public partial class TypingViewModel
    : ObservableObject,
        INavigatableView,
        IRecipient<GameResetMessage>
{
    private readonly TypingSession _session;
    private readonly ITextProvider _textProvider;
    private readonly IStatsRepository _statsRepository;
    private readonly INavigationService _navigationService;
    private readonly ILogger<TypingViewModel> _logger;
    private readonly IMessenger _messenger;
    private readonly Timer _refreshTimer;

    public event EventHandler? RefreshRequested;

    [ObservableProperty]
    public required partial TextSample Target { get; set; } = TextSample.Empty;

    [SetsRequiredMembers]
    public TypingViewModel(
        TypingSession session,
        ITextProvider textProvider,
        IStatsRepository statsRepository,
        INavigationService navigationService,
        ILogger<TypingViewModel> logger,
        IMessenger messenger
    )
    {
        _session = session;
        _session.OnSessionFinished += async (s, result) => await HandleSessionFinished(result);
        _textProvider = textProvider;
        _statsRepository = statsRepository;
        _navigationService = navigationService;
        _logger = logger;
        _messenger = messenger;

        _refreshTimer = new Timer(100);
        _refreshTimer.AutoReset = true;
        _refreshTimer.Elapsed += OnRefreshTimerElapsed;

        _messenger.Register<TypingViewModel, GameResetMessage>(this, (r, m) => r.Receive(m));
    }

    public bool IsGameOver => _session.IsOver;

    /// <summary>
    /// Processes input received from the View.
    /// Maps Key events to Core Game Logic.
    /// </summary>
    public async void ProcessInput(string c, bool isBackspace)
    {
        if (_session.IsOver)
        {
            await InitializeAsync();
            return;
        }

        bool accepted = _session.ProcessKeyPress(c, isBackspace);

        UpdateState();
    }

    public void RefreshState() => UpdateState();

    /// <summary>
    /// Synchronizes the Engine state with ViewModel properties.
    /// This triggers PropertyChanged notifications for the View.
    /// </summary>
    private void UpdateState()
    {
        var snapshot = _session.CreateSnapshot();
        _messenger.Send(new GameStatsUpdatedMessage(snapshot));
    }

    public void OnNavigatedTo()
    {
        _logger.LogInformation($"Navigated to {nameof(TypingViewModel)}");
        _refreshTimer.Start();
    }

    public void OnNavigatedFrom()
    {
        _logger.LogInformation($"Navigated from {nameof(TypingViewModel)}");
        _refreshTimer.Stop();
    }

    private void OnRefreshTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_session.IsOver)
        {
            return;
        }

        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    public async Task InitializeAsync(TextSample? textSample = null)
    {
        Target = textSample ?? await _textProvider.GetQuoteAsync();
        _session.LoadText(Target);
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

    public KeystrokeType GetStatus(int globalIdx)
    {
        return _session.GetStatus(globalIdx);
    }

    private async Task HandleSessionFinished(GameResult result)
    {
        await _statsRepository.SaveGameResultAsync(result);

        _messenger.Send(new GameCompletedMessage(result));
    }
}
