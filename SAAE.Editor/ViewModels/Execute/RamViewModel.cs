using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;
using Microsoft.Extensions.Logging;
using SAAE.Editor.Models.Messages;

namespace SAAE.Editor.ViewModels.Execute;

public partial class RamViewModel : BaseViewModel<RamViewModel> {

    [ObservableProperty]
    private ObservableCollection<Location> locations = [];

    [ObservableProperty]
    private int selectedSectionIndex = 0;
    
    public RamViewModel() {
        WeakReferenceMessenger.Default.Register<ProgramLoadMessage>(this, OnProgramLoad);
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
        //vm.OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedSectionIndex)));
    }
}

public class Location {
    public string Name { get; set; }
    public uint LoadAddress { get; set; }
}