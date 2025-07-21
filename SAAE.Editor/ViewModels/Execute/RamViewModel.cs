using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Localization;
using SAAE.Editor.Models.Messages;

namespace SAAE.Editor.ViewModels.Execute;

public partial class RamViewModel : BaseViewModel<RamViewModel>, IDisposable {

    [ObservableProperty]
    private ObservableCollection<Location> locations = [];

    [ObservableProperty]
    private int selectedSectionIndex = 0;

    public ObservableCollection<RamVisualization> AvailableVisualizationModes { get; private set; } = [];

    [ObservableProperty] private int selectedModeIndex = 0;
    
    public RamViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoad);
        LocalizationManager.CultureChanged += OnLocalize;
    }

    private void OnLocalize(CultureInfo cultureInfo) {
        //CreateModeList();
        OnPropertyChanged(nameof(AvailableVisualizationModes));
    }

    private static void OnProgramLoad(object recipient, ProgramLoadMessage msg) {
        RamViewModel vm = (RamViewModel)recipient;
        // load sectors from elf
        vm.Locations.Clear();
        // isolar localizacoes que nos importam
        ELF<uint> elf = msg.Elf;
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
            vm.Locations.Add(loc);
        }
        vm.SelectedSectionIndex = -1;
        vm.SelectedSectionIndex = 0;
        vm.CreateModeList();
        vm.SelectedModeIndex = -1;
        vm.SelectedModeIndex = 0;
    }

    private void CreateModeList() {
        AvailableVisualizationModes.Clear();
        AvailableVisualizationModes.AddRange(
            [RamVisualization.Hexadecimal, RamVisualization.Decimal, RamVisualization.Ascii ]
            );
    }

    public void Dispose() {
        LocalizationManager.CultureChanged -= OnLocalize;
    }
}

public class Location {
    public string Name { get; set; }
    public uint LoadAddress { get; set; }
}

public enum RamVisualization {
    Hexadecimal,
    Decimal,
    Ascii
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
            _ => throw new NotSupportedException()
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}