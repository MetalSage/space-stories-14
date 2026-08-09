using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.Paper.UI;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.RichText;

public sealed class CheckTagHandler : IMarkupTagHandler
{
    private static int _checkCounter;

    public static float FontLineHeight { get; set; } = 16.0f;

    public string Name => "check";

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
            Text = "☐",
            MinSize = new Vector2(FontLineHeight + 2, FontLineHeight + 2),
            MaxSize = new Vector2(FontLineHeight + 2, FontLineHeight + 2),
            Margin = new Thickness(1, 0, 1, 0),
            StyleClasses = { "ButtonSquare" },
            TextAlign = Label.AlignMode.Center,
        };

        var checkIndex = GetCheckIndex(node);
        btn.Name = $"check_{checkIndex}";

        btn.OnPressed += _ =>
        {
            var parent = btn.Parent;
            while (parent != null && parent is not PaperWindow)
            {
                parent = parent.Parent;
            }

            if (parent is PaperWindow paperWindow)
            {
                var buttonIndex = CountCheckButtonsBefore(btn);
                paperWindow.OpenCheckDialog(buttonIndex);
            }
        };

        control = btn;
        return true;
    }

    private static int GetCheckIndex(MarkupNode node)
    {
        return _checkCounter++;
    }

    public static void ResetCheckCounter()
    {
        _checkCounter = 0;
    }

    private static int CountCheckButtonsBefore(Control clickedButton)
    {
        var count = 0;
        var root = clickedButton;

        while (root.Parent != null)
        {
            root = root.Parent;
        }

        var found = false;
        CountCheckButtonsRecursive(root, clickedButton, ref count, ref found);
        return found ? count : 0;
    }

    private static void CountCheckButtonsRecursive(Control control, Control target, ref int count, ref bool found)
    {
        if (found)
            return;

        if (control is Button btn && (btn.Text == "☐" || btn.Text == "✔" || btn.Text == "✖"))
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
            CountCheckButtonsRecursive(child, target, ref count, ref found);
        }
    }

    private static string ReplaceNthCheckTag(string text, int index, string replacement)
    {
        const string checkTag = "[check]";
        var currentIndex = 0;
        var pos = 0;

        while (pos < text.Length)
        {
            var foundPos = text.IndexOf(checkTag, pos);
            if (foundPos == -1)
                break;

            if (currentIndex == index)
                return text.Substring(0, foundPos) + replacement + text.Substring(foundPos + checkTag.Length);

            currentIndex++;
            pos = foundPos + checkTag.Length;
        }

        return text;
    }
}
