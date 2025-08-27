using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using SAAE.Editor.Models.Messages;

namespace SAAE.Editor.Controls;

public partial class TextPopup : UserControl, IPopup<RequestTextPopupMessage, TextPopupResult> {

    private TaskCompletionSource<TextPopupResult>? tcs;

    private bool isCancellable;
    
    public TextPopup() {
        InitializeComponent();
        DataContext = this;
    }
    
    public Task<TextPopupResult> Request(RequestTextPopupMessage request) {
        IsVisible = true;
        isCancellable = request.IsCancellable;
        PopupTitle.Text = request.Title ?? string.Empty;
        PopupTitle.IsVisible = request.Title is not null;
        TextBox.Text = string.Empty;
        TextBox.Watermark = request.Watermark;
        TextBox.Focus();
        CancelButton.IsVisible = isCancellable;
        tcs = new TaskCompletionSource<TextPopupResult>();
        return tcs.Task;
    }

    [RelayCommand]
    private void Enter() {
        string input = TextBox.Text ?? string.Empty;
        IsVisible = false;
        tcs?.SetResult(new TextPopupResult() {
            IsCancelled = false,
            Result = input
        }); // Finaliza a Task
    }

    [RelayCommand]
    private void Cancel() {
        IsVisible = false;
        tcs?.SetResult(new TextPopupResult() {
            Result = string.Empty,
            IsCancelled = true
        });
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key != Key.Escape || !isCancellable || !IsVisible) {
            return;
        }
        e.Handled = true;
        IsVisible = false;
        tcs?.SetResult(new TextPopupResult() {
            Result = string.Empty,
            IsCancelled = true
        });
    }

    private void Dismiss_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        // Se o clique foi fora do popup interno
        if (!isCancellable || !IsVisible) {
            return;
        }
        if (e.Source == DismissBorder)
        {
            IsVisible = false;
            tcs?.SetResult(new TextPopupResult() {
                Result = string.Empty,
                IsCancelled = true
            });
        }
    }

    
}