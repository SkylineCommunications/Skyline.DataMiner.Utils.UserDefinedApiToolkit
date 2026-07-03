namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using System;
	using System.Linq;
	using System.Xml.Linq;

	internal class ControllerUnit
	{
		internal ControllerUnit(Type controllerType, XDocument? xmlDocs)
		{
			ControllerType = controllerType;
			XmlDocs = xmlDocs;
		}

		public Type ControllerType { get; }

		public XDocument? XmlDocs { get; }

		public string GetRoute()
		{
			var attr = ControllerType.GetCustomAttributesData()
				.FirstOrDefault(a => a.AttributeType.Name == "Route" ||
									 a.AttributeType.Name == "RouteAttribute");

			return attr?.ConstructorArguments[0].Value as string ?? "/";
		}
	}
}
