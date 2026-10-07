using System.Xml;

public class DeviceComparison : ComparisonExpression
{
	public enum DeviceParameter
	{
		PARAMETER_NONE = 0,
		MEMORY_TOTAL = 1,
		MEMORY_FREE = 2,
		CORES_COUNT = 3
	}

	private DeviceParameter parameter;

	public DeviceComparison(XmlNode node)
		: base(node)
	{
		if (node.Attributes != null)
		{
			parameter = GetParameterFromString((node.Attributes["Value"] != null) ? node.Attributes["Value"].Value : null);
			_thanValue = float.Parse(node.Attributes["Than"].Value);
		}
		UpdateParameter();
	}

	public static DeviceParameter GetParameterFromString(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			AdvLog.LogError("DeviceComparison::GetParameterFromString - empty parameter name");
			return DeviceParameter.PARAMETER_NONE;
		}
		switch (name)
		{
		case "_DeviceTotalMem":
			return DeviceParameter.MEMORY_TOTAL;
		case "_DeviceFreeMem":
			return DeviceParameter.MEMORY_FREE;
		case "_DeviceCoresNum":
			return DeviceParameter.CORES_COUNT;
		default:
			AdvLog.LogError(string.Format("DeviceComparison::GetParameterFromString - unknown type: {0}", name));
			return DeviceParameter.PARAMETER_NONE;
		}
	}

	public void UpdateParameter()
	{
		switch (parameter)
		{
		case DeviceParameter.MEMORY_TOTAL:
			_actualValue = SystemProperties.GetDeviceInfo().TotalRam / 1024;
			break;
		case DeviceParameter.MEMORY_FREE:
			_actualValue = SystemProperties.GetDeviceInfo().FreeRam / 1024;
			break;
		case DeviceParameter.CORES_COUNT:
			_actualValue = SystemProperties.GetDeviceInfo().CpuCount;
			break;
		default:
			AdvLog.LogError(string.Format("DeviceComparison::UpdateParameter - unknown type: {0}", parameter));
			break;
		}
	}
}
