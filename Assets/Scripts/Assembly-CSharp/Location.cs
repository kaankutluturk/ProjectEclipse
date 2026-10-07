using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Location
{
	public const int ZDelta = -3;

	private const string ParamsFileName = "params.xml";

	private static string sharedLocationName;

	private static string legacyLocationPath;

	private static string legacyLocationMusic;

	private string artworkFolderName;

	private bool _preferCustomLayout;

	private static readonly Dictionary<string, string> MissingArtworkFallbacks = new Dictionary<string, string>
	{
		// The other former entries (road, magic_rocks, stone_dragon, flooded_village and the
		// _small/_thorny variants) now ship their own upscaled artwork and params
		// (Tools/Recovery/ImportUpscaledLocations.py); redirecting them would load another layout.
		{ "emerald_forest_new", "emerald_forest" },
		// The archived bridge layout has no installed artwork. Its preserved
		// roster id uses the recovered bridge arena until that art is recovered.
		{ "bridge", "night_bridge" }
	};

	public string name;

	private string encounterMusic;

	public List<string> musics = new List<string>();

	public int gridSize;

	public float floorHeight;

	public float positionY;

	public float wallWidth;

	public float width;

	public float height;

	public float minWidth;

	public float minWidthRatio;

	public Vector3f playerStartPosition = new Vector3f();

	public Vector3f enemyStartPosition = new Vector3f();

	public Color modelsColor;

	public List<LocationSelector> layers;

	public LocationSelector gameLayer;

	public static string SharedLocationName
	{
		get
		{
			return GetSharedLocationName();
		}
	}

	public Vector3f ModelsCenter
	{
		get
		{
			return GetModelsCenter();
		}
	}

	public Vector2f HalfSize
	{
		get
		{
			return GetHalfSize();
		}
	}

	public string RandomMusic
	{
		get
		{
			return GetRandomMusic();
		}
	}

	private string ParamsPath
	{
		get
		{
			return GetParamsPath();
		}
	}

	private string TexturesPath
	{
		get
		{
			return GetTexturesPath();
		}
	}

	public Location()
	{
		name = GameUtils.DefaultLocation;
		gridSize = 0;
		floorHeight = 0f;
		positionY = 0f;
		wallWidth = 0f;
		width = 0f;
		height = 0f;
		gameLayer = null;
		layers = null;
	}

	// Resolve on entry: the training definition may predate a quest's dojo choice.
	// Ordinary encounters retain their own location, even while a dojo is selected.
	public static string ResolveEntryLocation(BattleType type, string encounterLocation)
	{
		if (type != BattleType.FightNone) return encounterLocation;
		_loadedDojoEncounter = encounterLocation;
		return _loadedDojo = ResolveDojo(encounterLocation);
	}

	private static string _loadedDojo;
	private static string _loadedDojoEncounter;

	private static string ResolveDojo(string encounterLocation)
	{
		string fallback = !string.IsNullOrWhiteSpace(GameUtils.DefaultLocation) ? GameUtils.DefaultLocation : encounterLocation;
		return Eclipse.Modding.ModRuntime.ResolveDojoLocation(fallback);
	}

	// A menu preview resolves the same saved/mod choice without changing the
	// currently loaded training scene's selection bookkeeping.
	public static Location CreateDojoPreview()
	{
		var location = new Location(ResolveDojo("dojo"), string.Empty);
		location.init();
		return location;
	}

	// True when the dojo choice now resolves to a different location than the
	// one the current dojo scene loaded, so reopening the dojo must reload it.
	public static bool DojoSelectionChanged()
	{
		return _loadedDojo != null && ResolveDojo(_loadedDojoEncounter) != _loadedDojo;
	}

	public Location(string locationName, string musicName, bool preferCustomLayout = false)
	{
		_preferCustomLayout = preferCustomLayout;
		GameLog.Write("Location:" + locationName);
		name = locationName;
		gridSize = 0;
		floorHeight = 0f;
		positionY = 0f;
		wallWidth = 0f;
		width = 0f;
		height = 0f;
		encounterMusic = musicName;
		gameLayer = null;
		layers = null;
	}

	public void init()
	{
		artworkFolderName = name;
		Eclipse.Modding.ExternalLocationRuntime.Entry externalLocation;
		bool hasExternalLocation = Eclipse.Modding.ExternalLocationRuntime.TryGet(name, out externalLocation);
		string value;
		if (!hasExternalLocation && !_preferCustomLayout && MissingArtworkFallbacks.TryGetValue(artworkFolderName, out value))
		{
			Debug.Log("[Location] Using installed artwork '" + value + "' for newer location '" + artworkFolderName + "'.");
			artworkFolderName = value;
		}
		// Recovered raid artwork belongs to the new combined-layer layouts.
		// The embedded legacy params split those layers into obsolete tiles.
		XmlDocument xmlDocument = hasExternalLocation ? OpenExternalLocationDocument(externalLocation.Params) :
			(_preferCustomLayout ? XmlUtils.OpenXMLDocument(GetParamsPath(), string.Empty) : OpenInstalledLocationDocument());
		if (xmlDocument == null)
		{
			xmlDocument = hasExternalLocation ? null :
				(_preferCustomLayout ? OpenInstalledLocationDocument() : XmlUtils.OpenXMLDocument(GetParamsPath(), string.Empty));
		}
		if (xmlDocument == null || xmlDocument["Root"] == null)
		{
			Debug.LogWarning("[Location] Missing or invalid location '" + name + "'; using dojo fallback.");
			hasExternalLocation = false;
			artworkFolderName = "dojo";
			xmlDocument = OpenInstalledLocationDocument();
			if (xmlDocument == null)
			{
				xmlDocument = XmlUtils.OpenXMLDocument(GetParamsPath(), string.Empty);
			}
		}
		if (xmlDocument == null || xmlDocument["Root"] == null)
		{
			Debug.LogError("[Location] Dojo fallback data is missing; location cannot be initialized.");
			return;
		}
		musics.Clear();
		if (hasExternalLocation && !string.IsNullOrEmpty(externalLocation.MusicAsset))
		{
			musics.Add(externalLocation.MusicAsset);
		}
		else if (hasExternalLocation && !string.IsNullOrEmpty(xmlDocument["Root"].GetAttribute("Music")))
		{
			// Mod location choices have the same priority as its single-track setting.
			musics.AddRange(xmlDocument["Root"].GetAttribute("Music").Split('|'));
		}
		else if (encounterMusic != string.Empty)
		{
			musics.Add(encounterMusic);
		}
		else
		{
			// Several installed layouts leave music to their encounter. Previews
			// have no encounter track; an absent Music attribute must not abort art loading.
			string[] collection = xmlDocument["Root"].GetAttribute("Music").Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
			musics = new List<string>(collection);
		}
		XmlAttribute frictionAttribute = xmlDocument["Root"].Attributes["FrictionForce"];
		PhysicsController.SetFrictionForce(frictionAttribute.ParseFloat(PhysicsController.GetFriction()));
		wallWidth = xmlDocument["Root"].Attributes["Wall"].ParseFloat();
		floorHeight = xmlDocument["Root"].Attributes["Floor"].ParseFloat();
		positionY = xmlDocument["Root"].Attributes["PositionY"].ParseFloat();
		modelsColor = ColorUtils.ParseHexColor(xmlDocument["Root"].Attributes["Color"].GetStringOrDefault());
		width = xmlDocument["Root"].Attributes["Width"].ParseFloat();
		height = xmlDocument["Root"].Attributes["Height"].ParseFloat();
		minWidth = xmlDocument["Root"].Attributes["MinWidth"].ParseFloat(width);
		minWidthRatio = minWidth / width;
		gridSize = xmlDocument["Root"].Attributes["GridSize"].ParseInt();
		layers = new List<LocationSelector>();
		XmlNode xmlNode = xmlDocument["Root"];
		int num = 0;
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.NodeType != XmlNodeType.Element || childNode.Name != "Layer")
				continue;
			ParseLayer(childNode, num);
			num += -3;
		}
	}

	private XmlDocument OpenExternalLocationDocument(XmlDocument parameters)
	{
		if (parameters == null)
		{
			Debug.LogWarning("[Location] Missing external parameters for '" + name + "'.");
			return null;
		}
		return (XmlDocument)parameters.CloneNode(true);
	}

	private XmlDocument OpenInstalledLocationDocument()
	{
		string text = ResourceManager.GetBundledText(GetParamsPath());
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		try
		{
			XmlDocument document = new XmlDocument();
			document.LoadXml(text);
			return document;
		}
		catch (XmlException exception)
		{
			Debug.LogWarning("[Location] Invalid bundled params for '" + artworkFolderName + "': " + exception.Message);
			return null;
		}
	}

	public static string GetSharedLocationName()
	{
		return sharedLocationName;
	}

	public Vector3f GetModelsCenter()
	{
		Vector3f center = Vector3f.op_Addition(playerStartPosition, enemyStartPosition);
		center.Multiply(0.5f);
		return center;
	}

	public Vector2f GetHalfSize()
	{
		return new Vector2f(width / 2f, height / 2f);
	}

	public string GetRandomMusic()
	{
		return NekkiMath.RandomElement(musics);
	}

	public void Clear()
	{
	}

	private string GetParamsPath()
	{
		return string.Format("{0}/{1}/{2}", SF2Paths.GetLocationsPath(), artworkFolderName, "params.xml");
	}

	private string GetTexturesPath()
	{
		return string.Format("Textures/Locations/{0}", artworkFolderName);
	}

	private string ResolveLayerTexturePath(string path)
	{
		if (Eclipse.Modding.AssetId.TryParse(path, out _))
		{
			return path.TrimEnd('/');
		}
		return "Textures/" + path.Trim('/');
	}

	private void ParseLayer(XmlNode node, int depth)
	{
		int num = 0;
		int num2 = node.Attributes["Scaling"].ParseInt();
		LocationSelector selector = new LocationSelector(depth);
		string text = null;
		text = ((node.Attributes["Path"] == null) ? GetTexturesPath() : ResolveLayerTexturePath(node.Attributes["Path"].GetStringOrDefault()));
		selector.SetScalingEnabled(num2 > 0);
		selector.set_Type(node.Attributes["Type"].ParseInt());
		selector.SetFactor(node.Attributes["Factor"].ParseFloat());
		selector.SetAtlasName(node.Attributes["Atlas"].GetStringOrDefault());
		if (!string.IsNullOrEmpty(selector.GetAtlasName()))
		{
			selector.SetCocosAnimationData(CocosAnimationData.Create(string.Format("{0}/{1}_xml.xml", text, selector.GetAtlasName()), true));
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			switch (childNode.Name)
			{
			case "ModelsViewer":
				playerStartPosition.Set(childNode.Attributes["PlayerPositionX"].ParseFloat(), childNode.Attributes["PlayerPositionY"].ParseFloat());
				enemyStartPosition.Set(childNode.Attributes["EnemyPositionX"].ParseFloat(), childNode.Attributes["EnemyPositionY"].ParseFloat());
				gridSize = num;
				break;
			case "ParticleEffect":
				ParseParticleEffect(childNode, selector, num, 0);
				break;
			case "NewParticleEffect":
				ParseParticleEffect(childNode, selector, num, 1);
				break;
			case "Image":
			case "SpriteMask":
				ParseImage(childNode, text, selector, num);
				break;
			case "SimpleEffect":
				ParseSimpleEffect(childNode, text, selector, num);
				break;
			}
		}
		layers.Add(selector);
		if (selector.GetIsGameLayer())
		{
			gameLayer = selector;
		}
	}

	private void ParseImage(XmlNode node, string texturePath, LocationSelector selector, int index)
	{
		string imageName = node.Attributes["ClassName"].GetStringOrDefault();
		Sprite sprite = LocationSpriteCache.GetSprite(texturePath, imageName, selector.GetAtlasName());
		if (sprite == null)
		{
			GameLog.Write("Pic: {0}", imageName);
			return;
		}
		GameObject gameObject = new GameObject(imageName);
		SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = sprite;
		spriteRenderer.flipX = node.Attributes["FlipX"].ParseInt() != 0;
		spriteRenderer.flipY = node.Attributes["FlipY"].ParseInt() != 0;
		SpriteMaskInteraction maskInteraction;
		if (System.Enum.TryParse(node.Attributes["MaskInteraction"].GetStringOrDefault(), out maskInteraction))
			spriteRenderer.maskInteraction = maskInteraction;
		if (node.Name == "SpriteMask")
		{
			spriteRenderer.enabled = false;
			gameObject.AddComponent<SpriteMask>().sprite = sprite;
		}
		if (node.Attributes["Color"] != null)
		{
			spriteRenderer.color = ColorUtils.ParseHexColor(node.Attributes["Color"].Value);
		}
		float num = node.Attributes["X"].ParseFloat();
		float num2 = 0f - node.Attributes["Y"].ParseFloat();
		float num3 = 0f;
		float num4 = 0f;
		float num5 = node.Attributes["Width"].ParseFloat();
		float num6 = node.Attributes["Height"].ParseFloat();
		float x = sprite.rect.size.x;
		float y = sprite.rect.size.y;
		bool flag = false;
		if (selector.GetCocosAnimationData() != null)
		{
			CocosAnimationData.SpriteFrameCocos frame = selector.GetCocosAnimationData().GetFrames().Find((CocosAnimationData.SpriteFrameCocos cocosFrame) => cocosFrame.get_Name() == imageName);
			if (frame != null)
			{
				flag = frame.GetRotated();
				num3 = frame.GetOffset().x;
				num4 = frame.GetOffset().y;
				x = frame.GetSourceSize().x;
				y = frame.GetSourceSize().y;
			}
		}
		Vector3 vector = default(Vector3);
		gameObject.transform.localPosition = new Vector3(num + num3, num2 + num4, 0f);
		if (flag)
		{
			gameObject.transform.Rotate(0f, 0f, 90f);
			vector = new Vector3(num6 / y, num5 / x, 1f);
		}
		else
		{
			vector = new Vector3(num5 / x, num6 / y, 1f);
		}
		// Qualified sprites carry their own pixels-per-unit import density.
		if (Eclipse.Modding.AssetId.TryParse(texturePath, out _))
		{
			vector.x *= sprite.pixelsPerUnit;
			vector.y *= sprite.pixelsPerUnit;
		}
		if (texturePath.Replace('\\', '/').TrimEnd('/').EndsWith("/dojo", System.StringComparison.OrdinalIgnoreCase) &&
            (imageName == "background_1" || imageName == "background_2"))
        {
            float width = flag ? sprite.bounds.size.y * vector.y : sprite.bounds.size.x * vector.x;
            if (width > 0f && num5 > 0f)
            {
                if (flag) vector.y *= num5 / width;
                else vector.x *= num5 / width;
            }
        }
        gameObject.transform.localScale = vector;
		gameObject.transform.localPosition = new Vector3(num + num3 * vector.x, num2 + num4 * vector.y, 0f);
		selector.AddImage(gameObject, index);
	}

	private void ParseSimpleEffect(XmlNode node, string texturePath, LocationSelector selector, int index)
	{
		bool flag = false;
		string text = texturePath;
		ChangingSprite effect = null;
		string text2 = node.Attributes["PictureLocation"].GetStringOrDefault();
		if (text2 == "global")
		{
			text = string.Empty;
			text = "Textures/Location_effects/";
		}
		if (node.Attributes["Path"] != null)
		{
			text = "Textures/" + node.Attributes["Path"].GetStringOrDefault();
		}
		string text3 = node.Attributes["Type"].GetStringOrDefault();
		if (text3 == "Picture")
		{
			string imageName = node.Attributes["ClassName"].GetStringOrDefault();
			CocosAnimationData animationData = selector.GetCocosAnimationData();
			CocosAnimationData.SpriteFrameCocos frame = animationData == null ? null : animationData.GetFrames().Find((CocosAnimationData.SpriteFrameCocos cocosFrame) => cocosFrame.get_Name() == imageName);
			if (LocationSpriteCache.GetSprite(text, imageName, selector.GetAtlasName()) == null)
			{
				Debug.LogWarning("[Location] Missing picture effect '" + imageName + "' in " + name);
				return;
			}
			effect = new ChangingSprite(ChangingSprite.SpriteEffectType.PictureBased);
			effect.InitPicture(text, imageName, selector.GetAtlasName(), frame, node.Attributes["Width"].ParseFloat(), node.Attributes["Height"].ParseFloat());
			SpriteRenderer renderer = effect.SpriteObject.GetComponent<SpriteRenderer>();
			renderer.flipX = node.Attributes["FlipX"].ParseInt() != 0;
			renderer.flipY = node.Attributes["FlipY"].ParseInt() != 0;
		}
		if (text3 == "Sequention")
		{
			if (GameUtils.SequencesDisabled)
			{
				return;
			}
			effect = new ChangingSprite(ChangingSprite.SpriteEffectType.AtlasBased);
			if (!effect.InitAtlasAnimation(node.Attributes["ClassName"].GetStringOrDefault(), text.TrimEnd('/') + "/Atlases/", node.Attributes["Speed"].ParseFloat(), node.Attributes["Offset"].ParseFloat(), node.Attributes["Width"].ParseFloat(), node.Attributes["Height"].ParseFloat()))
			{
				effect = null;
				return;
			}
			flag = true;
			effect.SetPause(node.Attributes["Pause"].ParseFloat());
		}
		if (effect == null)
		{
			Debug.LogWarning("[Location] Unsupported effect type '" + text3 + "' in " + name);
			return;
		}
		effect.SetPosition(node.Attributes["X"].ParseFloat(), 0f - node.Attributes["Y"].ParseFloat());
		foreach (XmlNode childNode in node.ChildNodes)
		{
			string name = childNode.Name;
			if (name == "OscillationX")
			{
				effect.SetOscillationXOffset(childNode.Attributes["Offset"].ParseFloat());
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					effect.AddOscillationXKeyframe(childNode2.Attributes["Period"].ParseFloat(), childNode2.Attributes["Value"].ParseFloat(), childNode2.Attributes["Ease"].ParseFloat());
				}
			}
			if (name == "OscillationY")
			{
				effect.SetOscillationYOffset(childNode.Attributes["Offset"].ParseFloat());
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					effect.AddOscillationYKeyframe(childNode3.Attributes["Period"].ParseFloat(), childNode3.Attributes["Value"].ParseFloat(), childNode3.Attributes["Ease"].ParseFloat());
				}
			}
			if (name == "Transparency")
			{
				effect.SetTransparencyOffset(childNode.Attributes["Offset"].ParseFloat());
				foreach (XmlNode childNode4 in childNode.ChildNodes)
				{
					effect.AddTransparencyKeyframe(childNode4.Attributes["Period"].ParseFloat(), childNode4.Attributes["Value"].ParseFloat(), childNode4.Attributes["Ease"].ParseFloat());
				}
			}
			if (name == "Rotation")
			{
				effect.SetRotationOffset(childNode.Attributes["Offset"].ParseFloat());
				foreach (XmlNode childNode5 in childNode.ChildNodes)
				{
					effect.AddRotationKeyframe(childNode5.Attributes["Period"].ParseFloat(), childNode5.Attributes["Value"].ParseFloat(), childNode5.Attributes["Ease"].ParseFloat());
				}
			}
			if (name == "Speed")
			{
				effect.SetSpeed(childNode.Attributes["X"].ParseFloat(), childNode.Attributes["Y"].ParseFloat());
			}
			if (name == "ReappearX")
			{
				effect.SetReappearX(childNode.Attributes["Min"].ParseFloat(), childNode.Attributes["Max"].ParseFloat());
			}
			if (name == "ReappearY")
			{
				effect.SetReappearY(childNode.Attributes["Min"].ParseFloat(), childNode.Attributes["Max"].ParseFloat());
			}
		}
		selector.AddSimpleEffect(effect, index);
	}

	private void ParseParticleEffect(XmlNode node, LocationSelector selector, int index, int particleType)
	{
		if (!GameUtils.ParticlesDisabled)
		{
			string particlePath = "Textures/Location_effects/Particles/" + node.Attributes["ClassName"].GetStringOrDefault();
			ChangingSprite effect = new ChangingSprite(ChangingSprite.SpriteEffectType.ParticleBased);
			float posX = node.Attributes["X"].ParseFloat();
			float num = node.Attributes["Y"].ParseFloat();
			int num2 = node.Attributes["MiddleColor"].ParseInt();
			bool flag = false;
			switch (particleType)
			{
			case 0:
				flag = effect.InitParticles(particlePath, posX, 0f - num);
				break;
			case 1:
				flag = effect.InitParticles(particlePath, posX, 0f - num);
				break;
			default:
				GameLog.Write("unknown type in parseParticleEffect");
				break;
			}
			if (flag)
			{
				selector.AddParticleEffect(effect, index);
			}
		}
	}
}
