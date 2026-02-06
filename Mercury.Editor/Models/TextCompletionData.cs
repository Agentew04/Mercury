using System;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Mercury.Editor.Localization;

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
                ".macro" => FileEditorResources.IntellisenseMacroStartDescriptionValue,
                ".text" => FileEditorResources.IntellisenseTextSectionDescriptionValue,
                ".data" => FileEditorResources.IntellisenseDataSectionDescriptionValue,
                ".ascii" => FileEditorResources.IntellisenseAsciiDataDescriptionValue,
                ".asciiz" => FileEditorResources.IntellisenseAsciizDataDescriptionValue,
                ".word" => FileEditorResources.IntellisenseWordDataDescriptionValue,
                ".space" => FileEditorResources.IntellisenseSpaceDataDescriptionValue,
                ".globl" => FileEditorResources.IntellisenseGloblDescriptionValue,
                ".align" => FileEditorResources.IntellisenseAlignDescriptionValue,
                ".float" => FileEditorResources.IntellisenseFloatDataDescriptionValue,
                ".double" => FileEditorResources.IntellisenseDoubleDataDescriptionValue,
                ".endmacro" => FileEditorResources.IntellisenseMacroEndDescriptionValue,
                _ => Text
            };
        }
    }

    public double Priority => Random.Shared.NextDouble();
}