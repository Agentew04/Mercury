using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia.Data;
using Avalonia.Data.Converters;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Extensions;
using SAAE.Editor.Localization;
using SAAE.Editor.Models.Messages;
using SAAE.Editor.Views.ExecuteView;
using SAAE.Engine.Memory;
using SAAE.Engine.Mips.Runtime;
using Machine = SAAE.Engine.Mips.Runtime.Machine;

namespace SAAE.Editor.ViewModels.Execute;

public partial class RamViewModel : BaseViewModel<RamViewModel, RamView>, IDisposable {

    [ObservableProperty]
    private ObservableCollection<Location> locations = [];

    [ObservableProperty, NotifyPropertyChangedFor(nameof(Rows))]
    private int selectedSectionIndex;

    public ObservableCollection<RamVisualization> AvailableVisualizationModes { get; private init; } = [];

    [ObservableProperty, NotifyPropertyChangedFor(nameof(Rows))] private int selectedModeIndex;

    private Machine? currentMachine;

    [ObservableProperty]
    private ObservableCollection<RamRow> rows = [];

    [ObservableProperty] private int selectedRowIndex = -1;

    private int currentPage;
    private ulong currentMemoryAccess;

    private const uint BytesPerPage = 256;
    private const uint BytesPerRow = 16;
    private const int RowCount = 16;

    public RamViewModel() {
        WeakReferenceMessenger.Default.Register<RamViewModel, ProgramLoadMessage>(this, OnProgramLoad);
        WeakReferenceMessenger.Default.Register<RamViewModel,LabelFocusMessage>(this,OnLabelFocus);
        LocalizationManager.CultureChanged += OnLocalize;
    }

    private void OnLocalize(CultureInfo cultureInfo) {
        OnPropertyChanged(nameof(AvailableVisualizationModes));
    }

    private static void OnProgramLoad(RamViewModel vm, ProgramLoadMessage msg) {
        // load sectors from elf
        vm.PopulateLocations(msg.Elf);
        if(vm.currentMachine is not null) {
            // desinscreve do evento de acesso de memoria da maquina antiga
            vm.currentMachine.OnMemoryAccess -= vm.OnMemoryAccess;
        }
        vm.currentMachine = msg.Machine;
        vm.currentMachine.OnMemoryAccess += vm.OnMemoryAccess;
        vm.SelectedSectionIndex = vm.Locations.IndexOf(x => x.Name == ".data");
        vm.PopulateRam();
        vm.DisplayRam();
        vm.SelectedRowIndex = -1;
        vm.NextPageCommand.NotifyCanExecuteChanged();
        vm.PreviousPageCommand.NotifyCanExecuteChanged();
    }

    private static void OnLabelFocus(RamViewModel vm, LabelFocusMessage msg) {
        int nearest = -1;
        for (int i = 0; i < vm.Locations.Count; i++) {
            ulong next = i >= vm.Locations.Count-1 ? ulong.MaxValue : vm.Locations[i+1].LoadAddress;
            if (msg.Address >= vm.Locations[i].LoadAddress &&
                msg.Address < next) {
                nearest = i;
                break;
            }
        }

        if (nearest == -1) {
            vm.Logger.LogWarning("Could not find suitable elf section for label {label}", msg.Name);
            return;
        }

        vm.SelectedSectionIndex = nearest;
        
        // find correct page
        long offset = (long)(msg.Address - vm.Locations[nearest].LoadAddress);
        vm.currentPage = (int)(offset / BytesPerPage);
        vm.Logger.LogInformation("Changing RamView to section {section} and page {page}", vm.Locations[nearest].Name, vm.currentPage);
        vm.PopulateRam();
        vm.DisplayRam();
    }
    
    private void OnMemoryAccess(object? sender, MemoryAccessEventArgs e) {
        currentMemoryAccess = e.Address;
        HighlightRow(currentMemoryAccess);
    }

    private void HighlightRow(ulong address) {
        if (address >= Rows[0].RowAddress && address <= Rows[^1].RowAddress + 16) {
            ulong addr = (address - Rows[0].RowAddress) / 16;
            SelectedRowIndex = (int)addr;
            Logger.LogInformation("Highlighting row {row}", SelectedRowIndex);
        }
        else {
            SelectedRowIndex = -1;
        }
    }

    private void PopulateLocations(ELF<uint> elf) {
        Locations.Clear();
        // isolar localizacoes que nos importam
        List<Segment<uint>> segments = elf.Segments
            .Where(x => x.Type == SegmentType.Load)
            .ToList(); 
        List<Section<uint>> sections = elf.Sections
            .Where(x => x.Type == SectionType.ProgBits)
            .ToList();

        // tenta descobrir o nome de cada segmento a partir das secoes
        // descarta secoes sem correspondencia: sao .MIPS.abiflags .reginfo
        foreach (Segment<uint> segment in segments) {
            Section<uint>? candidate = sections.Find(x => x.Offset == segment.Offset);
            if (candidate is null) {
                continue;
            }
            string? name = candidate.Name;
            Location loc = new() {
                LoadAddress = segment.Address,
                Name = name ?? string.Empty
            };
            Locations.Add(loc);
        }
        SelectedSectionIndex = -1;
        SelectedSectionIndex = 0;
        CreateModeList();
        SelectedModeIndex = -1;
        SelectedModeIndex = 0;
    }

    private void CreateModeList() {
        AvailableVisualizationModes.Clear();
        AvailableVisualizationModes.AddRange([
                RamVisualization.Hexadecimal, 
                RamVisualization.Decimal, 
                RamVisualization.Ascii,
                RamVisualization.Float
            ]
        );
    }

    private void PopulateRam() {
        Location loc = Locations[SelectedSectionIndex];
        long addr = loc.LoadAddress;
        
        if (addr < BytesPerPage * currentPage) {
            addr = 0;
        }
        else {
            addr += BytesPerPage * currentPage;
        }

        Rows.Clear();
        if (currentMachine is null) {
            return;
        }
        for (uint i = 0; i < RowCount; i++) {
            uint offset = (uint)(addr + i * BytesPerRow);
            RamRow row = new() {
                RowAddress = offset,
                Data0 = currentMachine.Memory.ReadWord(offset + 0x0),
                Data4 = currentMachine.Memory.ReadWord(offset + 0x4),
                Data8 = currentMachine.Memory.ReadWord(offset + 0x8),
                DataC = currentMachine.Memory.ReadWord(offset + 0xC)
            };
            Rows.Add(row);
        }
        HighlightRow(currentMemoryAccess);
    }

    // calcula o modo de exibicao correto dos dados do visualizador
    private void DisplayRam() {
        if (currentMachine is null) {
            return;
        }
        foreach (RamRow row in Rows) {
            row.Data0String = Display(row.Data0);
            row.Data4String = Display(row.Data4);
            row.Data8String = Display(row.Data8);
            row.DataCString = Display(row.DataC);
        }
        //OnPropertyChanged(nameof(Rows));
        return;
        
        string Display(int data) {
            if (SelectedModeIndex == -1) {
                SelectedModeIndex = 0;
            }
            
            switch (AvailableVisualizationModes[SelectedModeIndex]) {
                case RamVisualization.Hexadecimal:
                    return "0x" + data.ToString("x8");
                case RamVisualization.Decimal:
                    return data.ToString();
                case RamVisualization.Ascii:
                    Span<byte> bytes = stackalloc byte[4];
                    Span<char> chars = stackalloc char[4];
                    switch (currentMachine!.Memory.Endianess) {
                        case Endianess.LittleEndian:
                            BinaryPrimitives.WriteInt32LittleEndian(bytes, data);
                            break;
                        case Endianess.BigEndian:
                            BinaryPrimitives.WriteInt32BigEndian(bytes, data);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    Encoding.ASCII.GetChars(bytes, chars);
                    return $"{chars[0].Escape()} {chars[1].Escape()} {chars[2].Escape()} {chars[3].Escape()}";
                case RamVisualization.Float:
                    float f = BitConverter.Int32BitsToSingle(data);
                    return f.ToString(CultureInfo.CurrentCulture);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    partial void OnSelectedModeIndexChanged(int value) {
        DisplayRam();
    }

    partial void OnSelectedSectionIndexChanged(int value) {
        if (value == -1) {
            return;
        }
        PopulateRam();
        DisplayRam();
    }

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void NextPage() {
        currentPage++;
        PopulateRam();
        DisplayRam();
    }

    [RelayCommand(CanExecute = nameof(CanNavigate))]
    private void PreviousPage() {
        currentPage--;
        PopulateRam();
        DisplayRam();
    }

    private bool CanNavigate() {
        return currentMachine is not null;
    }
    
    public void Dispose() {
        LocalizationManager.CultureChanged -= OnLocalize;
    }
}

public class Location {
    public required string Name { get; init; }
    public required uint LoadAddress { get; init; }
}

public enum RamVisualization {
    Hexadecimal,
    Decimal,
    Ascii,
    Float
}

public class RamVisualizationConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not RamVisualization visu) {
            return null;
        }
        return visu switch {
            RamVisualization.Decimal => RamResources.RamDecModeValue,
            RamVisualization.Ascii => RamResources.RamTextModeValue,
            RamVisualization.Hexadecimal => RamResources.RamHexModeValue,
            RamVisualization.Float => RamResources.RamFloatModeValue,
            _ => BindingNotification.Null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return BindingNotification.Null;
    }
}

public partial class RamRow : ObservableObject {
    public uint RowAddress { get; init; }

    public int Data0 { get; init; }

    [ObservableProperty] private string data0String = string.Empty;

    public int Data4 { get; init; }

    [ObservableProperty] private string data4String = string.Empty;

    public int Data8 { get; init; }

    [ObservableProperty] private string data8String = string.Empty;

    public int DataC { get; init; }

    [ObservableProperty] private string dataCString = string.Empty;
}