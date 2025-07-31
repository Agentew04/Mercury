using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models.Messages;
using SAAE.Engine.Common;

namespace SAAE.Editor.ViewModels.Execute;

public partial class OutputViewModel : BaseViewModel<OutputViewModel> {

    public ScrollViewer? OutputScroller { get; set; }
    
    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCommand))] private string inputText = string.Empty;

    // manually implemented text property
    private string? textCached;
    public string Text =>textCached ??= sb.ToString();

    private void TriggerTextUpdate() {
        textCached = null;
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Text)));
        ClearCommand.NotifyCanExecuteChanged();
        OutputScroller?.ScrollToEnd();
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
        vm.swIn = msg.Machine.StdIn?.Writer;
        vm.srOut = msg.Machine.StdOut?.Reader;
        vm.srErr = msg.Machine.StdErr?.Reader;

        _ = vm.ReadStdOut(vm.cts.Token);
        _ = vm.ReadStdErr(vm.cts.Token);

        // _ = Task.Run(async () => {
        //     ChannelWriter<char> writer = msg.Machine.StdOut!.Writer;
        //     for (int i = 0; i < 100;i++) {
        //         await writer.WriteAsync(i + ".");
        //         await Task.Delay(1000);
        //         await writer.WriteAsync("\n");
        //         await Task.Delay(1000);
        //         vm.Logger.LogInformation("Writing to stdout");
        //     }
        // });
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
        await swIn!.WriteAsync(InputText);
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