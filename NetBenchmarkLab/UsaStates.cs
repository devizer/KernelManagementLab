using System;
using System.Collections.Generic;
using System.Linq;

namespace NetBenchmarkLab
{
    public class UsaStates
    {
        private static readonly string Raw = @"
Alabama:AL
Alaska:AK
Arizona:AZ
Arkansas:AR
California:CA
Colorado:CO
Connecticut:CT
Delaware:DE
Florida:FL
Georgia:GA
Hawaii:HI
Idaho:ID
Illinois:IL
Indiana:IN
Iowa:IA
Kansas:KS
Kentucky:KY
Louisiana:LA
Maine:ME
Maryland:MD
Massachusetts:MA
Michigan:MI
Minnesota:MN
Mississippi:MS
Missouri:MO
Montana:MT
Nebraska:NE
Nevada:NV
New Hampshire:NH
New Jersey:NJ
New Mexico:NM
New York:NY
North Carolina:NC
North Dakota:ND
Ohio:OH
Oklahoma:OK
Oregon:OR
Pennsylvania:PA
Rhode Island:RI
South Carolina:SC
South Dakota:SD
Tennessee:TN
Texas:TX
Utah:UT
Vermont:VT
Virginia:VA
Washington:WA
West Virginia:WV
Wisconsin:WI
Wyoming:WY

District of Columbia:DC
";

        private static Lazy<List<KeyValuePair<string,string>>> Pairs = new Lazy<List<KeyValuePair<string, string>>>(() =>
        {
            return Raw.Split(new[] {'\r', '\n'})
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Select(x => new KeyValuePair<string, string>(x.Split(':')[1], x.Split(':')[0]))
                .ToList();
        }); 
        
        public static Dictionary<string,string> StateAbbreviations 
        {
            get
            {
                var ret = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var pair in Pairs.Value) ret[pair.Key] = pair.Value;
                
                return ret;
            }
        }
    }
}