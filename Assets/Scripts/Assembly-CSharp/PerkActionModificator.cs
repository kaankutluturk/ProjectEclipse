public class PerkActionModificator : PerkAction
{
	public PerkActionModificator()
	{
		set_Modificator(true);
	}

	public PerkActionModificator(PerkActionModificator source)
		: base(source)
	{
		set_Modificator(source.GetModificator());
	}
}
