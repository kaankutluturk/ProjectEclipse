using System;
using System.Collections.Generic;

// Player look replacement belongs to Eclipse; recovered model assembly only asks
// whether a look is active and which extra skin documents to load.
public partial class ModelParameters
{
    /// <summary>Set on the campaign roster's own fighter (Shadow); copies keep it.</summary>
    public bool EclipseRosterPlayer;
    /// <summary>Set on this device's own versus fighter: its look is drawn locally only.</summary>
    public bool EclipseVersusLook;
    // Armor/helmet documents whose figures are not drawn under a look. They still load,
    // so the fighter's nodes and edges (and its physics) are the same with or without one.
    private readonly HashSet<string> _eclipseHiddenFigures = new HashSet<string>(StringComparer.Ordinal);

    internal bool HidesEclipseFigures(string path) => _eclipseHiddenFigures.Contains(path);

    private string[] _eclipseAppearanceSkins = Array.Empty<string>();
    private string _eclipseAppearanceVoice;

    /// <summary>Voice for gendered combat sounds: the chosen look's voice when it declares one.</summary>
    public string EclipseVoice => string.IsNullOrEmpty(_eclipseAppearanceVoice) ? Voice : _eclipseAppearanceVoice;

    /// <summary>
    /// Resolves the local look choice for Shadow. Fighters that already carry a
    /// mod character's own body/skins (forms, playable mod warriors) keep them.
    /// </summary>
    private bool ResolveEclipseAppearance()
    {
        _eclipseAppearanceSkins = Array.Empty<string>();
        _eclipseAppearanceVoice = null;
        _eclipseHiddenFigures.Clear();
        if (!(EclipseRosterPlayer || EclipseVersusLook) || !string.IsNullOrEmpty(EclipseBodyModel) || EclipseSkinModels.Length != 0 ||
            !string.IsNullOrEmpty(EclipseCharacterId))
            return false;
        if (!Eclipse.Modding.PlayerAppearance.TryGetLook(out string[] skins, out string voice, EclipseVersusLook)) return false;
        _eclipseAppearanceSkins = skins;
        _eclipseAppearanceVoice = voice;
        return true;
    }

    /// <summary>Records an armor/helmet document whose figures the active look hides.</summary>
    private string HideEclipseFigures(string path)
    {
        _eclipseHiddenFigures.Add(path);
        return path;
    }

    private static string EclipseModelPath(string reference) =>
        reference.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ? reference : reference + ".xml";

    internal bool HasEclipseAuthoredModels =>
        !string.IsNullOrEmpty(EclipseBodyModel) || EclipseSkinModels.Length != 0 || _eclipseAppearanceSkins.Length != 0;

    internal IEnumerable<string> EclipseAuthoredModels()
    {
        if (!string.IsNullOrEmpty(EclipseBodyModel)) yield return EclipseBodyModel;
        foreach (string skin in EclipseSkinModels) yield return skin;
        foreach (string skin in _eclipseAppearanceSkins) yield return skin;
    }
}
