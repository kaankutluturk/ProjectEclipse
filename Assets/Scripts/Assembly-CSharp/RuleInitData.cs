public class RuleInitData
{
	public Model OpponentModel;

	public Model PlayerModel;

	public ModelParameters PlayerParameters;

	public ModelParameters OpponentParameters;

	public Location FightLocation;

	public PlayersFightData FightData;

	public RuleInitData(Model _playerModel, Model CKNCPOABFBO, Location _location, PlayersFightData DPONLGICLEH)
	{
		PlayerModel = _playerModel;
		OpponentModel = CKNCPOABFBO;
		FightLocation = _location;
		FightData = DPONLGICLEH;
	}
}
