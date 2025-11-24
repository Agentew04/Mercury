using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mercury.Editor.Models.Messages;
using Mercury.Editor.Views.ExecuteView;
using Mercury.Engine.Common;

namespace Mercury.Editor.ViewModels.Execute;

public partial class OutputViewModel : BaseViewModel<OutputViewModel, OutputView> {

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCommand))] private string inputText = string.Empty;

    // manually implemented text property
    private string? textCached;
    public string Text =>textCached ??= sb.ToString();

    private void TriggerTextUpdate() {
        textCached = null;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Text)));
        ClearCommand.NotifyCanExecuteChanged();
        GetView()?.OutputScroller?.ScrollToEnd();
    }

    private readonly StringBuilder sb = new();
    private ChannelReader<char>? srOut;
    private ChannelReader<char>? srErr;
    private ChannelWriter<char>? swIn;
    private CancellationTokenSource? cts;
    private const int PoolingIntervalMs = 100;

    public OutputViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoaded);
    }

    private static void OnProgramLoaded(object receiver, ProgramLoadMessage msg) {
        OutputViewModel vm = (OutputViewModel)receiver;
        // dispose old objects
        vm.cts?.Cancel();
        vm.cts = new CancellationTokenSource();
        vm.srErr = null;
        vm.srOut = null;
        vm.swIn = null;
        vm.sb.Clear();

        // create new objects
        vm.swIn = msg.MipsMachine.StdIn.Writer;
        vm.srOut = msg.MipsMachine.StdOut.Reader;
        vm.srErr = msg.MipsMachine.StdErr.Reader;

        _ = vm.ReadStdOut(vm.cts.Token);
        _ = vm.ReadStdErr(vm.cts.Token);

        vm.TriggerTextUpdate();
    }
    
    private async Task ReadStdOut(CancellationToken token) {
        StringBuilder outSb = new();
        while (!token.IsCancellationRequested && srOut is not null) {
            int available = srOut.Count;
            if(available == 0) {
                await Task.Delay(PoolingIntervalMs, token);
                continue;
            }

            for (int i = 0; i < available && !token.IsCancellationRequested; i++) {
                outSb.Append(await srOut.ReadAsync(token));
            }
            lock (sb) {
                sb.Append(outSb);
            }
            outSb.Clear();
            TriggerTextUpdate();
        }
    }

    private async Task ReadStdErr(CancellationToken token) {
        StringBuilder errSb = new();
        while (!token.IsCancellationRequested && srErr is not null) {
            int available = srErr.Count;
            if(available == 0) {
                await Task.Delay(PoolingIntervalMs, token);
                continue;
            }

            for (int i = 0; i < available && !token.IsCancellationRequested; i++) {
                errSb.Append(await srErr.ReadAsync(token));
            }
            lock (sb) {
                sb.Append(errSb);
            }
            errSb.Clear();
            TriggerTextUpdate();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task Send() {
        if (swIn is null) {
            return;
        }

        sb.AppendLine(InputText);
        await swIn!.WriteAsync(InputText + '\n');
        InputText = string.Empty;
        TriggerTextUpdate();
    }

    [RelayCommand(CanExecute = nameof(CanClear))]
    private void Clear() {
        sb.Clear();
        TriggerTextUpdate();
    }

    private bool CanSend() {
        return !string.IsNullOrEmpty(InputText) && swIn is not null;
    }

    private bool CanClear() {
        return sb.Length > 0;
    }
}