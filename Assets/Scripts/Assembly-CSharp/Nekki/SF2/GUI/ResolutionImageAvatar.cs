namespace Nekki.SF2.GUI
{
	public class ResolutionImageAvatar : ResolutionImage
	{
		protected new static string NormalizeSpriteName(string spriteName)
		{
			return spriteName;
		}

		protected new static string NormalizeAtlasName(string atlasName)
		{
			return atlasName;
		}

		protected override void OnNativeSizeSet()
		{
		}
	}
}
