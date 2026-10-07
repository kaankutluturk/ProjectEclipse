using System.Collections.Generic;

public class ProfilePerkContainer
{
	public int Level;

	public List<ProfilePerk> Perks = new List<ProfilePerk>();

	public ProfilePerkContainer(int _level)
	{
		Level = _level;
	}
}
