using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Eclipse.Modding;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.Multiplayer
{
    // The Moveset Lab's "new weapon" dialog: a weapon in the Lab's mod (weapons/weapons.json)
    // that starts as a copy of a game weapon (its subtype, so its moves, and its model), with
    // an optional model and icon of its own. New weapons load when the game starts.
    // lol
    public sealed partial class LocalVersusMenu
    {
        private sealed class LabWeaponDraft
        {
            public string Name = string.Empty;
            public WeaponDefinition Base;
            /// <summary>An imported model file (.xml or .modelz), or null.</summary>
            public string ModelFile;
            /// <summary>The base weapon's model text, copied into the mod, or null.</summary>
            public string ModelText;
            public string IconFile;
            public int Price = 100;
            /// <summary>One weapon in each hand, built from a one-hand model.</summary>
            public bool Dual;
        }

        private LabWeaponDraft labWeaponDraft;

        private sealed class LabModWeapon { public string Id, Name, Subtype; }
        // The Lab mod's own weapons (weapons/weapons.json), loaded into the running game.
        private readonly List<LabModWeapon> labModWeapons = new List<LabModWeapon>();

        /// <summary>
        /// Loads the Lab mod's weapons into the running game and the versus roster, so the
        /// preview and training can wear them before a restart registers them for real.
        /// </summary>
        private void LoadLabWeaponsLive()
        {
            labModWeapons.Clear();
            ModWeaponDocument document;
            try { document = ModWeaponWriter.Load(ModRuntime.Host.ModsRoot, labModId); }
            catch (Exception exception) { SetStatus("Weapons not loaded: " + exception.Message); return; }
            if (document.Weapons.Count == 0) return;
            ModRuntime.AddLabMod(labModId);
            var items = ListSF.GetItems();
            foreach (var weapon in document.Weapons)
            {
                string id = weapon.ItemId(labModId);
                if (items.GetItemByName(id) == null)
                {
                    try
                    {
                        string model = weapon.Model.IndexOf(':') >= 0 ? weapon.Model : labModId + ":" + weapon.Model;
                        if (weapon.Model.IndexOf(':') < 0 && ModRuntime.TryReadLabModelText(AssetId.Parse(model)) == null)
                            throw new InvalidOperationException("its model " + weapon.Model + " is missing");
                        var node = new XmlDocument().CreateElement("Item");
                        node.SetAttribute("Name", id);
                        node.SetAttribute("Model", model);
                        node.SetAttribute("Text", weapon.Name);
                        node.SetAttribute("TextButton", weapon.Name);
                        node.SetAttribute("Type", "Weapon");
                        node.SetAttribute("SubType", weapon.Subtype);
                        if (weapon.TacticSubtype != null) node.SetAttribute("TacticSubtype", weapon.TacticSubtype);
                        node.SetAttribute("Level", "1");
                        node.SetAttribute("UpgradeLevel", "100");
                        items.AddExternalItem(node);
                        ModRuntime.AddLabItem(id);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning("[Moveset Lab] Could not load weapon " + id + ": " + exception.Message);
                        continue;
                    }
                }
                VersusRoster.AddExtra(LoadoutSlot.Weapon, id, weapon.Name);
                labModWeapons.Add(new LabModWeapon { Id = id, Name = weapon.Name, Subtype = weapon.Subtype });
            }
        }

        /// <summary>Shows the scope of <paramref name="subtype"/> with weapon <paramref name="id"/> chosen.</summary>
        private void SelectLabWeapon(string subtype, string id)
        {
            int scope = labScopes.IndexOf(subtype);
            if (scope < 0) { OnLabScopeChanged(); return; }
            labScope = scope;
            labWeapon = -1;
            OnLabScopeChanged();
            labWeapon = labWeapons.FindIndex(w => w.Id == id);
            OnLabScopeChanged();
        }

        /// <summary>Weapons the preview can wear, all subtypes, game weapons first.</summary>
        private static List<WeaponDefinition> LabBaseWeapons()
        {
            var result = new List<WeaponDefinition>();
            var content = ModRuntime.Scripts?.Content;
            if (content == null) return result;
            foreach (var weapon in content.Weapons)
            {
                if (string.IsNullOrEmpty(weapon.SubType) || !weapon.HasModel) continue;
                string runtime = weapon.IsCore && !string.IsNullOrEmpty(weapon.LegacyName) ? weapon.LegacyName : weapon.Id.ToString();
                if (ListSF.GetItems().GetItemByName(runtime) != null) result.Add(weapon);
            }
            return result.OrderBy(w => w.IsCore ? 0 : 1).ThenBy(w => LabWeaponTitle(w), StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static string LabWeaponTitle(WeaponDefinition weapon)
        {
            string runtime = weapon.IsCore && !string.IsNullOrEmpty(weapon.LegacyName) ? weapon.LegacyName : weapon.Id.ToString();
            return VersusRoster.Find(LoadoutSlot.Weapon, runtime)?.Name ?? LocalizationManager.GetStringOrDefault(runtime, runtime);
        }

        private void ShowLabNewWeapon()
        {
            if (labWeaponDraft == null)
            {
                labWeaponDraft = new LabWeaponDraft();
                var bases = LabBaseWeapons();
                string current = LabCurrentWeapon?.Id;
                labWeaponDraft.Base = bases.FirstOrDefault(w => w.Id.ToString() == current) ?? bases.FirstOrDefault(w => w.SubType == LabSubtype) ?? bases.FirstOrDefault();
            }
            var draft = labWeaponDraft;
            var layer = OpenLabLayer(true);
            var card = LabModal(layer, "NEW WEAPON", 580, 520);
            LabNoteText(card, "A new weapon for " + (labModName ?? labModId) + ". It starts as a copy of a game weapon: the same moves (its subtype) and the same model, until you give it your own.");

            var nameRow = LabHBox(card, 28, 6, null);
            var caption = LabText(nameRow, "Name", 12, LabDim, TextAnchor.MiddleLeft); LabSize(caption, 110);
            var nameRect = Rect(nameRow, "Name"); LabSize(nameRect, -1, 28, 1);
            var nameField = LabInput(nameRect, draft.Name, "Weapon name", InputField.ContentType.Standard);
            nameField.characterLimit = 48;
            nameField.onValueChanged.AddListener(text => draft.Name = text);

            var baseRow = LabHBox(card, 28, 6, null);
            var baseCaption = LabText(baseRow, "Based on", 12, LabDim, TextAnchor.MiddleLeft); LabSize(baseCaption, 110);
            Button baseButton = null;
            baseButton = LabBtn(baseRow, draft.Base == null ? "Choose..." : LabWeaponTitle(draft.Base) + "   (" + ClassName(draft.Base.SubType) + ")", () =>
            {
                var items = LabBaseWeapons().Select(w => new LabChoice
                {
                    Label = LabWeaponTitle(w), Detail = ClassName(w.SubType) + (w.IsCore ? "" : "  " + w.Id.Namespace.Value), Current = w == draft.Base,
                    Pick = () => { draft.Base = w; draft.ModelText = null; ShowLabNewWeapon(); },
                }).ToList();
                OpenLabChoices((RectTransform)baseButton.transform, items, 380);
            }, -1, LabStyle.Normal, 12);

            var modelRow = LabHBox(card, 26, 6, null);
            var modelCaption = LabText(modelRow, "Model", 12, LabDim, TextAnchor.MiddleLeft); LabSize(modelCaption, 110);
            string modelLabel = draft.ModelFile != null ? System.IO.Path.GetFileName(draft.ModelFile) : draft.ModelText != null ? "copy of the base model (editable)" : "the base weapon's model";
            var modelValue = LabText(modelRow, modelLabel, 12, LabFore, TextAnchor.MiddleLeft); LabSize(modelValue, -1, 26, 1);
            var modelButtons = LabHBox(card, 24, 6, null);
            LabSpacer(modelButtons, 110);
            LabBtn(modelButtons, "IMPORT MODEL...", () => PickLabWeaponFile(draft, true), -1, LabStyle.Normal, 11);
            LabBtn(modelButtons, "COPY BASE MODEL", () => CopyLabBaseModel(draft), -1, LabStyle.Normal, 11);
            if (draft.ModelFile != null || draft.ModelText != null)
                LabBtn(modelButtons, "USE BASE", () => { draft.ModelFile = null; draft.ModelText = null; ShowLabNewWeapon(); }, -1, LabStyle.Quiet, 11);

            var handsRow = LabHBox(card, 26, 6, null);
            var handsCaption = LabText(handsRow, "Hands", 12, LabDim, TextAnchor.MiddleLeft); LabSize(handsCaption, 110);
            LabBtn(handsRow, "ONE WEAPON", () => { draft.Dual = false; ShowLabNewWeapon(); }, -1, draft.Dual ? LabStyle.Normal : LabStyle.On, 11);
            LabBtn(handsRow, "ONE IN EACH HAND", () => { draft.Dual = true; ShowLabNewWeapon(); }, -1, draft.Dual ? LabStyle.On : LabStyle.Normal, 11,
                "Copies a one-hand model into the other hand, as dual weapons such as daggers are built");

            var iconRow = LabHBox(card, 26, 6, null);
            var iconCaption = LabText(iconRow, "Shop icon", 12, LabDim, TextAnchor.MiddleLeft); LabSize(iconCaption, 110);
            var iconValue = LabText(iconRow, draft.IconFile != null ? System.IO.Path.GetFileName(draft.IconFile) : "none", 12, LabFore, TextAnchor.MiddleLeft); LabSize(iconValue, -1, 26, 1);
            LabBtn(iconRow, "IMPORT PNG...", () => PickLabWeaponFile(draft, false), 110, LabStyle.Normal, 11);

            LabNumber(card, "Shop price (coins)", draft.Price, 100, "0", 10, 0, 1000000, value => draft.Price = (int)Math.Round(value), null, "What the weapon costs in the weapon shop.");

            foreach (string line in LabWeaponModelChecks(draft)) LabNoteText(card, line);

            var buttons = LabHBox(card, 28, 6, null);
            LabSpacer(buttons, 0, 1);
            LabBtn(buttons, "CANCEL", () => { labWeaponDraft = null; CloseLabPopup(); }, 90, LabStyle.Quiet, 12);
            LabBtn(buttons, "CREATE", () => CreateLabWeapon(draft), 100, LabStyle.Primary, 12);
            if (draft.Name.Length == 0) { nameField.Select(); nameField.ActivateInputField(); }
        }

        private void PickLabWeaponFile(LabWeaponDraft draft, bool model)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            SetStatus("Importing files works on desktop.");
#else
            string file;
            try
            {
                file = model ? Eclipse.UI.ModZipPicker.PickDesktop("Import a weapon model", "Eclipse model geometry", "xml", "modelz")
                    : Eclipse.UI.ModZipPicker.PickDesktop("Import a shop icon", "PNG images", "png");
            }
            catch (Exception exception) { SetStatus("Could not open the file picker: " + exception.Message); return; }
            if (string.IsNullOrEmpty(file)) return;
            if (model)
            {
                string text = ReadLabModelText(file, out string error);
                if (text == null) { SetStatus("Not imported: " + error); return; }
                draft.ModelFile = file; draft.ModelText = null;
            }
            else
            {
                try
                {
                    var bytes = System.IO.File.ReadAllBytes(file);
                    if (bytes.Length < 8 || bytes[0] != 0x89 || bytes[1] != 0x50 || bytes[2] != 0x4E || bytes[3] != 0x47) { SetStatus("Not imported: the file is not a PNG image."); return; }
                    if (bytes.Length > 8 * 1024 * 1024) { SetStatus("Not imported: icons must be under 8 MB."); return; }
                }
                catch (Exception exception) { SetStatus("Not imported: " + exception.Message); return; }
                draft.IconFile = file;
            }
            ShowLabNewWeapon();
#endif
        }

        /// <summary>The XML of a model file (.modelz is gzip of the XML), or null with <paramref name="error"/>.</summary>
        private static string ReadLabModelText(string file, out string error)
        {
            error = null;
            try
            {
                if (new System.IO.FileInfo(file).Length > 16 * 1024 * 1024) { error = "model files must be under 16 MB."; return null; }
                byte[] data = System.IO.File.ReadAllBytes(file);
                if (file.EndsWith(".modelz", StringComparison.OrdinalIgnoreCase))
                    using (var input = new System.IO.MemoryStream(data))
                    using (var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
                    using (var output = new System.IO.MemoryStream())
                    {
                        gzip.CopyTo(output);
                        data = output.ToArray();
                    }
                string text = new System.Text.UTF8Encoding(false, true).GetString(data);
                var document = new XmlDocument { XmlResolver = null };
                document.LoadXml(text);
                if (document.DocumentElement?.Name != "Scene") { error = "the file is not Eclipse model geometry (its root must be <Scene>)."; return null; }
                return text;
            }
            catch (Exception exception) { error = exception.Message; return null; }
        }

        private static List<string> LabModelEdges(string text)
        {
            var edges = new List<string>();
            try
            {
                var document = new XmlDocument { XmlResolver = null };
                document.LoadXml(text);
                var list = document.DocumentElement?["Edges"];
                if (list != null) foreach (XmlNode node in list.ChildNodes) if (node.NodeType == XmlNodeType.Element) edges.Add(node.Name);
            }
            catch (Exception) { }
            return edges;
        }

        /// <summary>
        /// Attack parts the base subtype's moves hit with that a custom model lacks: the game
        /// finds a weapon's hitting edges by name, so those attacks would never connect.
        /// </summary>
        private List<string> LabWeaponModelChecks(LabWeaponDraft draft)
        {
            var notes = new List<string>();
            if (draft.Base == null) { notes.Add("<color=#DE543E>Choose a game weapon to start from.</color>"); return notes; }
            string text = LabDraftModelText(draft, out string dualNote);
            if (dualNote != null) notes.Add(dualNote);
            if (text == null) { notes.Add("Uses " + LabWeaponTitle(draft.Base) + "'s model, so every " + ClassName(draft.Base.SubType) + " move hits as usual."); return notes; }
            var edges = new HashSet<string>(LabModelEdges(text), StringComparer.Ordinal);
            var needed = labBaseline.Values.Where(m => m.WeaponGroups.Any(g => g.Contains(draft.Base.SubType)))
                .SelectMany(m => m.Attacks).SelectMany(a => a.Attack.Edges)
                .Select(e => e.EndsWith("_1", StringComparison.Ordinal) || e.EndsWith("_2", StringComparison.Ordinal) ? e.Substring(0, e.Length - 2) : e)
                .Where(e => e.StartsWith("WEAPON", StringComparison.Ordinal)).Distinct(StringComparer.Ordinal).ToList();
            var missing = needed.Where(e => !edges.Contains(e) && !edges.Contains(e + "_1") && !edges.Contains(e + "_2")).ToList();
            notes.Add("The model has " + edges.Count + " edges.");
            if (missing.Count == 0) notes.Add("It has every weapon part " + ClassName(draft.Base.SubType) + " moves hit with.");
            else notes.Add("<color=#DE543E>Missing parts that " + ClassName(draft.Base.SubType) + " moves hit with: " + string.Join(", ", missing.Take(6)) + (missing.Count > 6 ? "..." : "") +
                ". Name the model's edges the same, or change those attacks' hitting parts in the HITBOX tab.</color>");
            return notes;
        }

        /// <summary>
        /// The model text the weapon will get: imported, copied, or (for two hands) the base
        /// model, made two-handed when asked. Null keeps the base weapon's model as it is.
        /// </summary>
        private string LabDraftModelText(LabWeaponDraft draft, out string dualNote)
        {
            dualNote = null;
            string text = draft.ModelText ?? (draft.ModelFile != null ? ReadLabModelText(draft.ModelFile, out _) : null);
            if (!draft.Dual) return text;
            if (text == null && draft.Base != null)
            {
                try { text = ModRuntime.Host.TypedAssets.LoadModelText(draft.Base.Model); }
                catch (Exception exception) { Debug.LogWarning("[Moveset Lab] Could not read " + draft.Base.Model + ": " + exception.Message); }
                if (string.IsNullOrEmpty(text)) { dualNote = "<color=#DE543E>The base model could not be read, so it cannot be put in both hands. Import the model instead.</color>"; return null; }
            }
            if (text == null) return null;
            string dual = DualWieldModel(text, out bool already);
            if (dual == null) { dualNote = "<color=#DE543E>The model could not be copied into the other hand.</color>"; return text; }
            dualNote = already ? "The model already holds a weapon in each hand." :
                "The model is copied into the other hand: its parts ending _1 (or without a number) get a _2 twin on the second hand." +
                (LabIsDualSubtype(draft.Base?.SubType) ? "" : " " + ClassName(draft.Base?.SubType ?? "") + " moves use one hand; base the weapon on a two-weapon class such as Daggers, Knives or Axes so its moves use both.");
            return dual;
        }

        /// <summary>Whether the game's own weapons of <paramref name="subtype"/> hold one weapon in each hand.</summary>
        private bool LabIsDualSubtype(string subtype)
        {
            if (string.IsNullOrEmpty(subtype)) return false;
            return labBaseline.Values.Where(m => m.WeaponGroups.Any(g => g.Contains(subtype))).SelectMany(m => m.Attacks)
                .SelectMany(a => a.Attack.Edges).Any(e => e.StartsWith("WEAPON", StringComparison.Ordinal) && e.EndsWith("_2", StringComparison.Ordinal));
        }

        /// <summary>
        /// A weapon in each hand from a one-hand model. The game's dual weapons are a hand-1
        /// model (parts named ..._1, attached to Weapon-Node1_1..4_1) plus the same parts named
        /// ..._2 on Weapon-Node1_2..4_2 with the same weights. Parts without a number become
        /// ..._1 and ..._2, as dual weapons name them. Returns null when the XML cannot be read.
        /// </summary>
        private static string DualWieldModel(string text, out bool already)
        {
            already = false;
            try
            {
                var document = new XmlDocument { XmlResolver = null, PreserveWhitespace = false };
                document.LoadXml(text);
                if (document.FirstChild is XmlDeclaration declaration) document.RemoveChild(declaration);
                var scene = document.DocumentElement;
                if (scene == null || scene.Name != "Scene") return null;
                if (text.IndexOf("Weapon-Node1_2", StringComparison.Ordinal) >= 0 || text.IndexOf("Weapon-Node4_2", StringComparison.Ordinal) >= 0)
                {
                    already = true;
                    return document.OuterXml;
                }
                string Base(string name) => name.EndsWith("_1", StringComparison.Ordinal) ? name.Substring(0, name.Length - 2) : name;
                var sections = new[] { "Nodes", "Edges", "Figures" }.Select(n => scene[n]).Where(n => n != null).ToList();
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var section in sections)
                    foreach (XmlNode node in section.ChildNodes) if (node.NodeType == XmlNodeType.Element) names.Add(node.Name);
                // Hand 1 keeps or gains _1; hand 2 is the _2 twin. Anchors are the hand's own.
                string Hand(string value, string suffix)
                {
                    if (names.Contains(value)) return Base(value) + suffix;
                    if (value.StartsWith("Weapon-Node", StringComparison.Ordinal) && value.EndsWith("_1", StringComparison.Ordinal)) return value.Substring(0, value.Length - 2) + suffix;
                    return value;
                }
                foreach (var section in sections)
                {
                    var originals = section.ChildNodes.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Element).Cast<XmlElement>().ToList();
                    foreach (var original in originals)
                    {
                        foreach (string suffix in new[] { "_1", "_2" })
                        {
                            var copy = document.CreateElement(Base(original.Name) + suffix);
                            foreach (XmlAttribute attribute in original.Attributes) copy.SetAttribute(attribute.Name, Hand(attribute.Value, suffix));
                            section.InsertBefore(copy, original);
                        }
                        section.RemoveChild(original);
                    }
                }
                return document.OuterXml;
            }
            catch (Exception) { return null; }
        }

        private void CopyLabBaseModel(LabWeaponDraft draft)
        {
            if (draft.Base == null) return;
            string text = null;
            try { text = ModRuntime.Host.TypedAssets.LoadModelText(draft.Base.Model); }
            catch (Exception exception) { Debug.LogWarning("[Moveset Lab] Could not read " + draft.Base.Model + ": " + exception.Message); }
            if (string.IsNullOrEmpty(text)) { SetStatus("The base model could not be read. Extract it with Tools/AssetPacker instead."); return; }
            draft.ModelText = text; draft.ModelFile = null;
            SetStatus("The base model will be copied into the mod's assets/models folder, ready to edit.");
            ShowLabNewWeapon();
        }

        private void CreateLabWeapon(LabWeaponDraft draft)
        {
            string name = (draft.Name ?? string.Empty).Trim();
            if (name.Length == 0) { SetStatus("Type a name for the weapon."); return; }
            if (draft.Base == null) { SetStatus("Choose a game weapon to start from."); return; }
            string root = ModRuntime.Host.ModsRoot;
            try
            {
                var document = ModWeaponWriter.Load(root, labModId);
                string baseId = MovesetClipFile.SafeName(name).Replace('-', '_');
                if (baseId.Length > ModWeaponEntry.MaxIdLength - 3) baseId = baseId.Substring(0, ModWeaponEntry.MaxIdLength - 3).TrimEnd('_');
                if (baseId.Length == 0 || !char.IsLetter(baseId[0])) baseId = "weapon_" + baseId;
                string id = baseId;
                for (int n = 2; document.Weapons.Any(w => w.Id == id); n++) id = baseId + "_" + n;
                string modFolder = System.IO.Path.Combine(root, labModId);
                var entry = new ModWeaponEntry { Id = id, Name = name, Subtype = draft.Base.SubType, TacticSubtype = draft.Base.TacticSubtype, Price = draft.Price };
                string modelText = LabDraftModelText(draft, out _);
                if (modelText != null)
                {
                    string models = System.IO.Path.Combine(modFolder, "assets", "models");
                    System.IO.Directory.CreateDirectory(models);
                    // An imported .modelz is kept as it is unless it was made two-handed.
                    if (!draft.Dual && draft.ModelText == null && draft.ModelFile != null && draft.ModelFile.EndsWith(".modelz", StringComparison.OrdinalIgnoreCase))
                        System.IO.File.Copy(draft.ModelFile, System.IO.Path.Combine(models, id + ".modelz"), true);
                    else
                        System.IO.File.WriteAllText(System.IO.Path.Combine(models, id + ".xml"), modelText, new System.Text.UTF8Encoding(false));
                    entry.Model = "models/" + id;
                }
                else entry.Model = draft.Base.Model.ToString();
                if (draft.IconFile != null)
                {
                    string textures = System.IO.Path.Combine(modFolder, "assets", "textures"), sprites = System.IO.Path.Combine(modFolder, "assets", "sprites");
                    System.IO.Directory.CreateDirectory(textures); System.IO.Directory.CreateDirectory(sprites);
                    System.IO.File.Copy(draft.IconFile, System.IO.Path.Combine(textures, id + "_icon.png"), true);
                    System.IO.File.WriteAllText(System.IO.Path.Combine(sprites, id + "_icon.asset"),
                        "type=sprite\ntexture=textures/" + id + "_icon.png\npixels_per_unit=100\n", new System.Text.UTF8Encoding(false));
                    entry.Icon = "sprites/" + id + "_icon";
                }
                document.Weapons.Add(entry);
                ModWeaponWriter.Save(root, labModId, labModName, document);
                // Save the moves too, so the mod on disk is complete and consistent.
                if (labCopy != null && labCopy.IsDirty) ApplyLab();
                labWeaponDraft = null;
                CloseLabPopup();
                LoadLabWeaponsLive();
                SelectLabWeapon(entry.Subtype, entry.ItemId(labModId));
                SetStatus("Created " + name + ". It is ready here for the preview and training; give it moves of its own with a weapon copy. It joins the weapon shop when the game next starts.");
            }
            catch (Exception exception) { SetStatus("Weapon not created: " + exception.Message); }
        }
    }
}
