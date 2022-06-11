using MaxMind.Db;
using MaxMind.GeoIP2;

namespace Universe.ExternalIp
{
    public class IpAddressDetails
    {
        public class Country
        {
            public string Name { get; set; }
            public string IsoCode { get; set; }
            public bool IsInEuropeanUnion { get; set; }

            public override string ToString()
            {
                return $"{nameof(Name)}: {Name}, {nameof(IsoCode)}: {IsoCode}, {nameof(IsInEuropeanUnion)}: {IsInEuropeanUnion}";
            }
        }

        public static Country GetCountry(string ip)
        {
            using (var reader = new DatabaseReader("Countries.mmf", FileAccessMode.MemoryMapped))
            {
                if (reader.TryCountry(ip, out var countryResponse))
                {
                    return new Country()
                    {
                        Name = countryResponse.Country.Name,
                        IsoCode = countryResponse.Country.IsoCode,
                        IsInEuropeanUnion = countryResponse.Country.IsInEuropeanUnion
                    };
                }

                return null;
            }
        }
    }
}
