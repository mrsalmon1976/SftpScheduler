using Bogus;
using System.Net;

namespace SftpScheduler.Test.Common
{
    public static class RandomData
    {
        private static Faker _faker = new Faker();

        public static bool Bool()
        {
            return _faker.Random.Bool();
        }

        public static string Base64String()
        {
            int length = _faker.Random.Int(100, 1000);
            byte[] data = _faker.Random.Bytes(length);
            return Convert.ToBase64String(data);
        }

        public static byte[] Bytes(int? length = null)
        {
            int datalength = (length ?? _faker.Random.Int(100, 1000));
            return _faker.Random.Bytes(datalength);
        }

        public static decimal Decimal(decimal min = 0.0M, decimal max = 100.00M)
        {
            return _faker.Random.Decimal(min, max);
        }


        public static string Email()
        {
            return _faker.Internet.Email();
        }

        public static string GuidString()
        {
            return System.Guid.NewGuid().ToString();
        }

        public static Guid Guid()
        {
            return System.Guid.NewGuid();
        }

        public static HttpMethod HttpMethod()
        {
            int i = Number(1, 5);
            switch (i)
            {
                case 1:
                    return System.Net.Http.HttpMethod.Get;
                case 2:
                    return System.Net.Http.HttpMethod.Post;
                case 3:
                    return System.Net.Http.HttpMethod.Put;
                case 4:
                    return System.Net.Http.HttpMethod.Delete;
                case 5:
                    return System.Net.Http.HttpMethod.Patch;
            }

            throw new Exception("Unexpected number for switch method");
        }

        public static HttpStatusCode HttpStatusCode()
        {
            var codes = Enum.GetValues(typeof(HttpStatusCode));
            int i = Number(0, codes.Length - 1);
            return (HttpStatusCode)codes.GetValue(i)!;
        }


        public static IPAddress IPAddress()
        {
            byte[] data = Bytes(4);
            data[0] |= 1;
            return new IPAddress(data);
        }

        public static int Number(int min = 0, int max = 100)
        {
            return _faker.Random.Number(min, max);
        }

        //public static string String()
        //{
        //    int length = Number(5, 20);
        //    return String(length);
        //}

        //public static string String(int length = 10)
        //{
        //    return Nanoid.Generate(Nanoid.Alphabets.Letters, length);
        //}

        public static string StringParagraph(int minSentenceCount = 3)
        {
            return _faker.Lorem.Paragraph(minSentenceCount);
        }

        public static string StringWord()
        {
            return _faker.Lorem.Word();
        }

        public static string Url()
        {
            return _faker.Internet.Url();
        }
    }
}
