using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using Mercury.Editor.Models.Messages;

namespace Mercury.Editor.Controls;

public partial class BoolPopup : UserControl, IPopup<RequestBoolPopupMessage,BoolPopupResult> {
    
    public BoolPopup() {
        InitializeComponent();
        DataContext = this;
    }
    
    private TaskCompletionSource<BoolPopupResult>? tcs;

    private bool isCancellable;

    public Task<BoolPopupResult> Request(RequestBoolPopupMessage request) {
        IsVisible = true;
        this.isCancellable = request.IsCancellable;
        PopupTitle.Text = request.Title;
        tcs = new TaskCompletionSource<BoolPopupResult>();
        return tcs.Task;
    }

    [RelayCommand]
    private void Confirm() {
        IsVisible = false;
        tcs?.SetResult(new BoolPopupResult() {
            IsCancelled = false,
            Result = true
        });
    }
    
    [RelayCommand]
    private void Deny() {
        IsVisible = false;
        tcs?.SetResult(new BoolPopupResult() {
            IsCancelled = false,
            Result = false
        });
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key != Key.Escape || !isCancellable || !IsVisible) {
            return;
        }
        e.Handled = true;
        IsVisible = false;
        tcs?.SetResult(new BoolPopupResult() {
            IsCancelled = true,
            Result = default
        });
    }

    private void Dismiss_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        // Se o clique foi fora do popup interno
        if (!isCancellable || !IsVisible || e.Source != DismissBorder) {
            return;
        }
        IsVisible = false;
        tcs?.SetResult(new BoolPopupResult() {
            IsCancelled = true,
            Result = default
        });
    }
}