using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.Paper.UI;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.RichText;

public sealed class SignatureTagHandler : IMarkupTagHandler
{
    private static int _signatureCounter;

    public SignatureTagHandler()
    {
        IoCManager.InjectDependencies(this);
    }

    public static float FontLineHeight { get; set; } = 16.0f;

    public string Name => "signature";

    public void PushDrawContext(MarkupNode node, MarkupDrawingContext context) { }
    public void PopDrawContext(MarkupNode node, MarkupDrawingContext context) { }

    public string TextBefore(MarkupNode node)
    {
        return "";
    }

    public string TextAfter(MarkupNode node)
    {
        return "";
    }

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        var btn = new Button
        {
            Text = Loc.GetString("paper-signature-sign-button"),
            MinSize = new Vector2(96, FontLineHeight + 4),
            MaxSize = new Vector2(96, FontLineHeight + 4),
            Margin = new Thickness(1, 2, 1, 2),
            StyleClasses = { "ButtonSquare" },
            TextAlign = Label.AlignMode.Center,
        };

        var signatureIndex = GetSignatureIndex(node);
        btn.Name = $"signature_{signatureIndex}";

        btn.OnPressed += _ =>
        {
            var parent = btn.Parent;
            while (parent != null && parent is not PaperWindow)
            {
                parent = parent.Parent;
            }

            if (parent is PaperWindow paperWindow)
            {
                var buttonIndex = CountSignatureButtonsBefore(btn);
                paperWindow.SendSignatureRequest(buttonIndex);
            }
        };

        control = btn;
        return true;
    }

    private static int GetSignatureIndex(MarkupNode node)
    {
        return _signatureCounter++;
    }

    public static void ResetSignatureCounter()
    {
        _signatureCounter = 0;
    }

    private static int CountSignatureButtonsBefore(Control clickedButton)
    {
        var count = 0;
        var root = clickedButton;

        while (root.Parent != null)
        {
            root = root.Parent;
        }

        var found = false;
        CountSignatureButtonsRecursive(root, clickedButton, ref count, ref found);
        return found ? count : 0;
    }

    private static void CountSignatureButtonsRecursive(Control control, Control target, ref int count, ref bool found)
    {
        if (found)
            return;

        if (control is Button btn && btn.Text == Loc.GetString("paper-signature-sign-button"))
        {
            if (control == target)
            {
                found = true;
                return;
            }

            count++;
        }

        foreach (var child in control.Children)
        {
            CountSignatureButtonsRecursive(child, target, ref count, ref found);
        }
    }
}
