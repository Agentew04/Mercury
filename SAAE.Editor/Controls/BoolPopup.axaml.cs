using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;

namespace SAAE.Editor.Controls;

public partial class BoolPopup : UserControl {
    
    public BoolPopup() {
        InitializeComponent();
        DataContext = this;
    }
    
    private TaskCompletionSource<bool?>? tcs;

    private bool isCancellable;

    public Task<bool?> Request(string title, bool isCancellable) {
        IsVisible = true;
        this.isCancellable = isCancellable;
        PopupTitle.Text = title;
        tcs = new TaskCompletionSource<bool?>();
        return tcs.Task;
    }

    [RelayCommand]
    private void Confirm() {
        IsVisible = false;
        tcs?.SetResult(true); // Finaliza a Task
    }
    
    [RelayCommand]
    private void Deny() {
        IsVisible = false;
        tcs?.SetResult(false); // Finaliza a Task
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
        if (!isCancellable || !IsVisible || e.Source != DismissBorder) {
            return;
        }
        IsVisible = false;
        tcs?.SetResult(null);
    }
}