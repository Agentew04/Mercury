using System;
using Avalonia.Media.Fonts;

namespace Mercury.Editor;

public class NunitoFontCollection : EmbeddedFontCollection{
    
    public NunitoFontCollection() : base(
        new Uri("fonts:Nunito", UriKind.Absolute),
        new Uri($"avares://Mercury.Editor/Assets/Fonts/Nunito/", UriKind.Absolute)) {
    }
}