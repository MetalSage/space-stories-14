// ReSharper disable CheckNamespace

namespace Content.Client.Options.UI;

public sealed partial class OptionSlider
{
    public Color? TextColor
    {
        get => NameLabel.FontColorOverride;
        set => NameLabel.FontColorOverride = value;
    }
}
