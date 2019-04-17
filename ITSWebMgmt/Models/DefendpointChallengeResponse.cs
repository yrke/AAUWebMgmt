using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class DefendpointChallengeResponse
    {
        public string Data;
        public DefendpointChallengeResponse(string result)
        {
            Data = result;
        }
    }
}
