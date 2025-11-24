using System;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace Mercury.Editor.Models;

public class TextCompletionData : ICompletionData
{
    public TextCompletionData(string text)
    {
        Text = text;
    }
    
    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, Text);
    }

    public IImage Image => null!;
    public string Text { get; }
    public object Content {
        get {
            TextBlock tb = new();
            tb.Text = Text;
            return tb;
        }
    }
    public object Description {
        get {
            return Text switch {
                ".ascii" => "Define uma string ASCII sem terminador \\0.",
                ".asciiz" => "Define uma string ASCII com terminador \\0.",
                _ => $"Descrição de {Text}"
            };
        }
    }

    public double Priority => Random.Shared.NextDouble();
}