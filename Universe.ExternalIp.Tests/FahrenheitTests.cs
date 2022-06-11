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
            Console.WriteLine($"ip=[{ip}]");
            IpAddressDetails.Country country = IpAddressDetails.GetCountry(ip);
            Console.WriteLine($"country=[{country}]");
            bool isFahrenheit = FahrenheitCountries.Contains(country?.Name);
            Console.WriteLine($"isFahrenheit=[{isFahrenheit}]");


        }
    }
}
