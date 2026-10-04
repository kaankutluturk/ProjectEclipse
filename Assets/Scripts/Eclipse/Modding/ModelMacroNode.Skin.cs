using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

// Imported mesh skinning belongs to Eclipse; archival MacroNode math is unchanged.
public partial class ModelMacroNode
{
    private sealed class SkinBinding
    {
        internal ModelNode Start, End;
        internal float Weight, Along, Across;
    }
    private List<SkinBinding> skinBindings;
    private ModelObject skinModel;

    internal void LoadSkinBindings(ModelObject model, XmlNode node)
    {
        int count = int.Parse(node.Attributes["BonesCount"].Value, CultureInfo.InvariantCulture);
        skinModel = model;
        skinBindings = new List<SkinBinding>(count);
        for (int i = 1; i <= count; i++)
        {
            string suffix = i.ToString(CultureInfo.InvariantCulture);
            var start = model.FindNodeOrParent(node.Attributes["BoneStart" + suffix].Value);
            var end = model.FindNodeOrParent(node.Attributes["BoneEnd" + suffix].Value);
            if (start == null || end == null || start == end)
                throw new System.IO.InvalidDataException("Invalid skin bone on " + GetName());
            skinBindings.Add(new SkinBinding {
                Start = start, End = end,
                Weight = float.Parse(node.Attributes["Weight" + suffix].Value, CultureInfo.InvariantCulture),
                Along = float.Parse(node.Attributes["Along" + suffix].Value, CultureInfo.InvariantCulture),
                Across = float.Parse(node.Attributes["Across" + suffix].Value, CultureInfo.InvariantCulture)
            });
        }
    }

    private bool UpdateSkinBindings()
    {
        if (skinBindings == null) return false;
        float x = 0, y = 0;
        foreach (var binding in skinBindings)
        {
            var start = binding.Start.GetStart(); var end = binding.End.GetStart();
            float dx = end.GetX() - start.GetX(), dy = end.GetY() - start.GetY();
            float across = binding.Across * (skinModel.GetModel()?.FacingSign ?? 1);
            // Across is authored in file coordinates (Y up). Runtime Y is down.
            x += binding.Weight * (start.GetX() + binding.Along * dx + across * dy);
            y += binding.Weight * (start.GetY() + binding.Along * dy - across * dx);
        }
        _Start.SetX(x); _Start.SetY(y); _Start.SetZ(0);
        return true;
    }
}
