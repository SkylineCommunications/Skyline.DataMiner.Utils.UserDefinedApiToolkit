namespace UserDefinedApiToolkit.Tests.Build.TestFiles
{
	using System.Collections.Generic;

	public class GenericDocumentationFixture<T>
	{
		public void Get()
		{
		}

		public void GetVector(int[] value)
		{
		}

		public void GetJaggedArray(int[][] value)
		{
		}

		public void GetMatrix(int[,] value)
		{
		}

		public void GetCube(int[,,] value)
		{
		}

		public void GetGenericValue(T value)
		{
		}

		public void GetGenericVector(T[] value)
		{
		}

		public void GetGenericValues<TValue>(T outer, TValue inner)
		{
		}

		public void GetGenericPair<TFirst, TSecond>(TFirst first, TSecond second)
		{
		}

		public void GetNested(Dictionary<string, int>.KeyCollection value)
		{
		}

		public void GetNestedGeneric(Dictionary<T, List<int>> value)
		{
		}

		public void GetNestedType(DocumentationOuter<T>.Inner<int> value)
		{
		}

		public void GetByReference(ref int value)
		{
		}

		public void GetOutValue(out int value)
		{
			value = 0;
		}

		public void GetOverloaded(int value)
		{
		}

		public void GetOverloaded(string value)
		{
		}

		public void GetGeneric<TValue>(TValue value)
		{
		}
	}

	public class DocumentationOuter<T>
	{
		public class Inner<TValue>
		{
		}
	}
}
