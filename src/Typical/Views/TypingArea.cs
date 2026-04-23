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
        // _correctAttr = new Attribute(Terminal.Gui.Drawing.ColorName16.Blue);
        // _incorrectAttr = new Attribute(Terminal.Gui.Drawing.ColorName16.Red);
        // _untypedAttr = new Attribute(Terminal.Gui.Drawing.ColorName16.Gray);
    }

    public void RefreshText()
    {
        if (Viewport.Width <= 0)
            return;

        _formatter.Text = _viewModel.TargetText;
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

        int yOffset = Math.Max(0, (Viewport.Height - _cachedLines.Count) / 2);
        int globalIdx = 0;
        for (int y = 0; y < _cachedLines.Count; y++)
        {
            string line = _cachedLines[y];
            int xOffset = Math.Max(0, (Viewport.Width - line.Length) / 2);

            for (int x = 0; x < line.Length; x++)
            {
                if (globalIdx >= _viewModel.DisplayStates.Length)
                    break;

                var state = _viewModel.DisplayStates[globalIdx];

                SetAttribute(GetAttributeForState(state));
                AddRune(x + xOffset, y + yOffset, (Rune)line[x]);

                globalIdx++;
            }
        }
        return true;
    }

    private Attribute GetAttributeForState(KeystrokeType state) =>
        state switch
        {
            KeystrokeType.Correct => _correctAttr,
            KeystrokeType.Incorrect => _incorrectAttr,
            _ => _untypedAttr,
        };
}
