// ReSharper disable CheckNamespace

namespace Content.Client.Options.UI;

public sealed partial class OptionSlider
{
    /// <summary>
    /// Color of the title label text.
    /// </summary>
    public Color? TextColor
    {
        get => NameLabel.FontColorOverride;
        set => NameLabel.FontColorOverride = value;
    }
}
