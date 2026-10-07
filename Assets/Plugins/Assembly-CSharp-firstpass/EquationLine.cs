public class EquationLine
{
	public float A;

	public float CoefficientB;

	public float ConstantC;

	public EquationLine()
	{
		A = 0f;
		CoefficientB = 0f;
		ConstantC = 0f;
	}

	public EquationLine(float a, float coefficientB = 0f, float constantC = 0f)
	{
		this.A = a;
		this.CoefficientB = coefficientB;
		this.ConstantC = constantC;
	}
}
