using System;
using NUnit.Framework;
using Universe.NUnitTests;

namespace Universe.ExternalIp.Tests
{
    [TestFixture]
    public class FahrenheitTests : NUnitTestsBase
    {
        [Test]
        public void Test1()
        {
            string ip = ExternalIpFetcher.Fetch();
            Console.WriteLine($"External IP Address is '{ip}'");
            IpAddressDetails.Country country = IpAddressDetails.GetCountry(ip);
            Console.WriteLine($"Country by External IP Address is '{country}'");
            bool isFahrenheit = FahrenheitCountries.Contains(country?.Name);
            Console.WriteLine($"Is Fahrenheit by External IP Address is '{isFahrenheit}'");
        }
    }
}
