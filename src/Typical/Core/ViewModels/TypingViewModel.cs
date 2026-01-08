using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Typical.Core.Interfaces;

namespace Typical.Core.ViewModels;

public class TypingViewModel : ObservableObject, IBindableView
{
    public TypingViewModel(ILogger<TypingViewModel> logger)
    {
        _logger = logger;
    }

    private readonly string _targetText = "The quick brown fox jumped over the lazy dog.";
    private readonly ILogger<TypingViewModel> _logger;
    private string _typedText = "";

    public string TargetText => _targetText;
    public string TypedText
    {
        get => _typedText;
        set
        {
            if (_typedText != value)
            {
                _typedText = value;
                OnPropertyChanged();
            }
        }
    }

    internal TypingResult GetStatus(int index)
    {
        if (index >= _typedText.Length)
            return TypingResult.Untyped;
        return _typedText[index] == _targetText[index]
            ? TypingResult.Correct
            : TypingResult.Incorrect;
    }

    public void OnNavigatedTo()
    {
        _logger.LogInformation($"Navigated to {nameof(TypingViewModel)}");
    }

    public void OnNavigatedFrom()
    {
        _logger.LogInformation($"Navigated from {nameof(TypingViewModel)}");
    }
}
