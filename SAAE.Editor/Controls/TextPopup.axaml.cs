using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace SAAE.Editor.Controls;

public partial class TextPopup : UserControl {

    private TaskCompletionSource<string?>? tcs;

    private bool isCancellable;
    
    public TextPopup() {
        InitializeComponent();
        DataContext = this;
    }

    public Task<string?> Request(string? title, string? watermark, bool isCancellable) {
        IsVisible = true;
        this.isCancellable = isCancellable;
        PopupTitle.Text = title ?? string.Empty;
        PopupTitle.IsVisible = title is not null;
        TextBox.Text = string.Empty;
        TextBox.Watermark = watermark;
        TextBox.Focus();
        CancelButton.IsVisible = isCancellable;
        tcs = new TaskCompletionSource<string?>();
        return tcs.Task;
    }

    [RelayCommand]
    private void Enter() {
        string input = TextBox.Text ?? string.Empty;
        IsVisible = false;
        tcs?.SetResult(input); // Finaliza a Task
    }

    [RelayCommand]
    private void Cancel() {
        IsVisible = false;
        tcs?.SetResult(null);
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key != Key.Escape || !isCancellable || !IsVisible) {
            return;
        }
        e.Handled = true;
        IsVisible = false;
        tcs?.SetResult(null);
    }

    private void Dismiss_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        // Se o clique foi fora do popup interno
        if (!isCancellable || !IsVisible) {
            return;
        }
        if (e.Source == DismissBorder)
        {
            IsVisible = false;
            tcs?.SetResult(null);
        }
    }
}