using NUnit.Framework;
using SftpScheduler.BLL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Utility
{
	[TestFixture]
	public class ArrayUtilsTests
	{
		[TestCase("1", "1")]
		[TestCase("3,4", "3,4")]
		[TestCase("7,8,10,11", "7,8,10,11")]
		public void ElementsEqualUnordered_SameItemsOrdered_AreEqual(string arr1, string arr2)
		{
			// setup
			string[] array1 = arr1.Split(',');
			string[] array2 = arr2.Split(',');

			// execute
			bool result = ArrayUtils.ElementsEqualUnordered(array1, array2);

			// assert
			Assert.That(result, Is.True);
		}

		[TestCase("3", "4")]
		[TestCase("7,8,11", "7,11,8,10")]
		[TestCase("15,8,10,11", "8,15,11")]
		public void ElementsEqualUnordered_DifferentItems_AreNotEqual(string arr1, string arr2)
		{
			// setup
			string[] array1 = arr1.Split(',');
			string[] array2 = arr2.Split(',');

			// execute
			bool result = ArrayUtils.ElementsEqualUnordered(array1, array2);

			// assert
			Assert.That(result, Is.False);
		}

		[TestCase("3,4", "4,3")]
		[TestCase("7,8,10,11", "7,11,8,10")]
		[TestCase("15,8,10,11", "8,10,15,11")]
		public void ElementsEqualUnordered_SameItemsNotOrdered_AreEqual(string arr1, string arr2)
		{
			// setup
			string[] array1 = arr1.Split(',');
			string[] array2 = arr2.Split(',');

			// execute
			bool result = ArrayUtils.ElementsEqualUnordered(array1, array2);

			// assert
			Assert.That(result, Is.True);
		}

	}
}
