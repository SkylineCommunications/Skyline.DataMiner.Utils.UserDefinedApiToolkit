namespace UserDefinedApiToolkit.Tests.Build.TestFiles
{
	using System.Collections.Generic;

	// Reproduces a reported circular reference: Profile has a list of Parameter, and each
	// Parameter references its owning Profile back via Parent.
	public class Profile
	{
		public string Name { get; set; } = string.Empty;

		public List<Parameter> Parameters { get; set; } = new List<Parameter>();
	}

	public class Parameter
	{
		public string Key { get; set; } = string.Empty;

		public Profile Parent { get; set; } = new Profile();
	}
}
