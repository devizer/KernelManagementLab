using System;
using NUnit.Framework;

namespace Universe.ExternalIp.Tests
{
    public class FahrenheitTests
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
