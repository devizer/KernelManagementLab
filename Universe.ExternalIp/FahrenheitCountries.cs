using System;
using System.Collections.Generic;
using System.Linq;

namespace Universe.ExternalIp
{
    public class FahrenheitCountries
    {
        public static readonly HashSet<string> FahrenheitCountriesSet = new HashSet<string>(Enumerable.Empty<string>(), StringComparer.InvariantCultureIgnoreCase)
        {
            "Montserrat",
            "Palau",
            "British Virgin Islands",
            "Turks And Caicos Islands", "Saint Kitts And Nevis", "St Kitts and Nevis", "Marshall Islands", "Bermuda",
            "Cayman Islands", "Antigua And Barbuda", "Micronesia", "Federated States of Micronesia", "Bahamas",
            "Belize", "Liberia", "United States"
        };

        public static bool Contains(string countryName)
        {
            return FahrenheitCountriesSet.Contains(countryName);
        }
    }
}
