using System;
using Avalonia.Media.Fonts;

namespace SAAE.Editor;

public class NunitoFontCollection : EmbeddedFontCollection{
    
    public NunitoFontCollection() : base(
        new Uri("fonts:Nunito", UriKind.Absolute),
        new Uri($"avares://SAAE.Editor/Assets/Fonts/Nunito/", UriKind.Absolute)) {
    }
}