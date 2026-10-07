using System.Diagnostics;
using UnityEngine;

namespace Nekki.SF2.GUI.Fight
{
	public class PointsTable : MonoBehaviour
	{
		private int leftScore;

		private int rightScore;

		private int maxScore;

		private Vector2 textSizeDelta = new Vector2(200f, 200f);

		[SerializeField]
		private LabelAlias leftScoreText;

		[SerializeField]
		private LabelAlias delimiterText;

		[SerializeField]
		private LabelAlias rightScoreText;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private PointsTableType tableType;

		public int LeftPoints
		{
			get
			{
				return get_LeftScore();
			}
			set
			{
				set_LeftScore(value);
			}
		}

		public int RightPoints
		{
			get
			{
				return get_RightScore();
			}
			set
			{
				set_RightScore(value);
			}
		}

		public int get_LeftScore()
		{
			return leftScore;
		}

		public void set_LeftScore(int value)
		{
			if (leftScore != value && leftScoreText != null)
			{
				leftScore = value;
				leftScoreText.set_text(leftScore.ToString());
			}
		}

		public int get_RightScore()
		{
			return rightScore;
		}

		public void set_RightScore(int value)
		{
			if (rightScore != value && rightScoreText != null)
			{
				rightScore = value;
				rightScoreText.set_text(rightScore.ToString());
			}
		}

		public PointsTableType get_Type()
		{
			return tableType;
		}

		private void set_Type(PointsTableType value)
		{
			tableType = value;
		}

		public void Init(PointsTableType LFLGCDNKNJI, int LOMKKEAMMIG = 0, int CFMPJLLNCFF = 120)
		{
			set_Type(LFLGCDNKNJI);
			this.maxScore = LOMKKEAMMIG;
			switch (LFLGCDNKNJI)
			{
			case PointsTableType.POINTS_TABLE_CONTEST:
				leftScoreText.set_text(leftScore.ToString());
				rightScoreText.set_text(rightScore.ToString());
				delimiterText.set_text(":");
				break;
			case PointsTableType.POINTS_TABLE_SCORE:
				leftScoreText.set_text(leftScore.ToString());
				rightScoreText.set_text(LOMKKEAMMIG.ToString());
				delimiterText.set_text("/");
				break;
			}
		}
	}
}
