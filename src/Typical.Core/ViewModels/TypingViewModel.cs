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

public partial class TypingViewModel : ObservableObject, INavigatableView
{
    private readonly TypingTest _Test;
    private readonly ITextProvider _textProvider;
    private readonly IStatsRepository _statsRepository;
    private readonly ILogger<TypingViewModel> _logger;
    private readonly IMessenger _messenger;
    private readonly Timer _refreshTimer;
    private bool _isFinishing;
    public event EventHandler? RefreshRequested;

    [ObservableProperty]
    public required partial TextSample Target { get; set; } = TextSample.Empty;

    [SetsRequiredMembers]
    public TypingViewModel(
        TypingTest Test,
        ITextProvider textProvider,
        IStatsRepository statsRepository,
        INavigationService navigationService,
        ILogger<TypingViewModel> logger,
        IMessenger messenger
    )
    {
        _Test = Test;
        _Test.OnTestFinished += async (s, result) => await HandleTestFinished(result);
        _textProvider = textProvider;
        _statsRepository = statsRepository;
        _logger = logger;
        _messenger = messenger;

        _refreshTimer = new Timer(100);
        _refreshTimer.AutoReset = true;
        _refreshTimer.Elapsed += OnRefreshTimerElapsed;
    }

    /// <summary>
    /// Processes input received from the View.
    /// Maps Key events to Core Test Logic.
    /// </summary>
    public async void ProcessInput(string c, bool isBackspace)
    {
        if (_Test.IsOver)
        {
            await InitializeAsync();
            return;
        }

        bool accepted = _Test.ProcessKeyPress(c, isBackspace);

        UpdateState();
    }

    public void RefreshState() => UpdateState();

    /// <summary>
    /// Synchronizes the Engine state with ViewModel properties.
    /// This triggers PropertyChanged notifications for the View.
    /// </summary>
    private void UpdateState()
    {
        var snapshot = _Test.GetCurrentSnapshot();
        _messenger.Send(new TestSessionUpdatedMessage(snapshot));
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
        if (_Test.IsOver)
        {
            return;
        }

        _Test.Stats.SampleSnapshot();

        RefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    public async Task InitializeAsync(TextSample? textSample = null)
    {
        Target = textSample ?? await _textProvider.GetQuoteAsync();
        _Test.LoadText(Target);
        UpdateState();
    }

    public KeystrokeType GetStatus(int globalIdx)
    {
        return _Test.GetStatus(globalIdx);
    }

    private async Task HandleTestFinished(TestResult result)
    {
        if (_isFinishing)
            return;
        _isFinishing = true;

        try
        {
            await Task.Delay(100);

            await _statsRepository.SaveTestResultAsync(result);
            _messenger.Send(new TestCompletedMessage(result));
        }
        finally
        {
            _isFinishing = false;
        }
    }
}
