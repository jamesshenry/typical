using System.Globalization;
using System.Text;
using Terminal.Gui.Configuration;
using Terminal.Gui.Text;
using Terminal.Gui.ViewBase;
using Typical.Core.Statistics;
using Typical.Core.ViewModels;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace Typical.Views;

public class TypingArea : View
{
    private readonly Attribute _correctAttr;
    private readonly Attribute _incorrectAttr;
    private readonly Attribute _untypedAttr;
    private readonly TextFormatter _formatter = new();
    private List<string> _cachedLines = [];
    private readonly TypingViewModel _viewModel;

    public TypingArea(TypingViewModel viewModel)
    {
        _viewModel = viewModel;
        _formatter.WordWrap = true;
        var errorScheme = SchemeManager.GetScheme(Terminal.Gui.Drawing.Schemes.Error);
        var normalScheme = SchemeManager.GetScheme(Terminal.Gui.Drawing.Schemes.Base);
        _correctAttr = normalScheme!.HotNormal;
        _incorrectAttr = errorScheme!.Active;
        _untypedAttr = normalScheme!.Normal;
    }

    public void Refresh()
    {
        if (Viewport.Width <= 0)
            return;

        _formatter.Text = _viewModel.Target.Text;
        _formatter.ConstrainToWidth = Viewport.Width;
        _formatter.PreserveTrailingSpaces = true;
        _cachedLines = _formatter.GetLines();

        if (Height != _cachedLines.Count)
        {
            Height = _cachedLines.Count;
            SuperView?.SetNeedsLayout();
        }
        SetNeedsDraw();
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        if (_cachedLines.Count == 0 || Viewport.Width == 0)
            return true;

        int globalIdx = 0;
        int yPos = Math.Max(0, (Viewport.Height - _cachedLines.Count) / 2);

        foreach (var line in _cachedLines)
        {
            int xPos = CalculateXOffset(line);

            var enumerator = StringInfo.GetTextElementEnumerator(line);
            while (enumerator.MoveNext())
            {
                string grapheme = enumerator.GetTextElement();

                if (globalIdx >= _viewModel.DisplayStates.Length)
                    break;

                var state = _viewModel.DisplayStates[globalIdx];

                SetAttribute(GetAttributeForState(state));

                Rune r = grapheme.EnumerateRunes().First();

                AddRune(xPos, yPos, r);

                xPos += r.GetColumns();
                globalIdx++;
            }
            yPos++;
        }
        return true;
    }

    private int CalculateXOffset(string line)
    {
        int visualWidth = 0;
        foreach (var rune in line.EnumerateRunes())
        {
            visualWidth += rune.GetColumns();
        }
        return Math.Max(0, (Viewport.Width - visualWidth) / 2);
    }

    private Attribute GetAttributeForState(KeystrokeType state) =>
        state switch
        {
            KeystrokeType.Correct => _correctAttr,
            KeystrokeType.Incorrect => _incorrectAttr,
            _ => _untypedAttr,
        };
}
