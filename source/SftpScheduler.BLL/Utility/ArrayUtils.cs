namespace SftpScheduler.BLL.Utility
{
	public class ArrayUtils
	{
		public static bool ElementsEqualUnordered(object[] arr1, object[] arr2)
		{
			if (arr1 == null || arr2 == null)
			{
				return false;
			}

			var count1 = arr1.Length;
			var count2 = arr2.Length;

			if (count1 != count2)
			{
				return false;
			}

			for (int i=0; i<count1; i++)
			{
				if (!arr2.Contains(arr1[i]))
				{
					return false;
				}
			}

			return true;

		}
		
	}
}
