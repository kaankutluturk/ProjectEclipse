public class RuleInitData
{
	public Model OpponentModel;

	public Model PlayerModel;

	public ModelParameters PlayerParameters;

	public ModelParameters OpponentParameters;

	public Location FightLocation;

	public PlayersFightData FightData;

	public RuleInitData(Model _playerModel, Model opponentModel, Location _location, PlayersFightData fightData)
	{
		PlayerModel = _playerModel;
		OpponentModel = opponentModel;
		FightLocation = _location;
		FightData = fightData;
	}
}
