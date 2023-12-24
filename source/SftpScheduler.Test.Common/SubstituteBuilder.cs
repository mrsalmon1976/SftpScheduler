using AutoBogus;
using NSubstitute;
using System.Linq.Expressions;
using System.Reflection;

namespace SftpScheduler.Test.Common
{
    public class SubstituteBuilder<T> where T : class
    {
        private T _instance;

        public SubstituteBuilder()
        {
            _instance = Substitute.For<T>();
        }

        public T Build()
        {
            return _instance;
        }

        public SubstituteBuilder<T> WithProperty<TValue>(Expression<Func<T, TValue>> memberLamda, TValue value)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    property.SetValue(_instance, value, null);
                }
            }

            return this;
        }

        public SubstituteBuilder<T> WithRandomProperties()
        {
            var faker = new AutoFaker<T>();
            faker.Populate(_instance);
            return this;
        }


    }
}