using System;
using System.Linq;

namespace ITSWebMgmt.Models
{
    public class Fileshare
    {
        public string Fileshareraw { get; }
        public string Domain { get; }
        public string Type { get; }
        public string Name { get; }
        public string Access { get; }

        public Fileshare(string value)
        {
            this.Fileshareraw = value;

            var split = value.Split(',');
            var oupath = split.Where<string>(s => s.StartsWith("OU=")).ToArray<string>();
            int count = oupath.Count();

            if (count == 3 && oupath[count - 1].Equals("OU=Groups") && oupath[count - 2].Equals("OU=Resource Access"))
            {
                //This is a group access group
                var groupname = split[0].Replace("CN=", "");
                var groupNameSplit = groupname.Split('_');

                var type = groupNameSplit[2];
                if (type.Equals("generic"))
                {
                    type = "fileshares";
                }
                else
                {
                    type = "department";
                }
                var domain = groupNameSplit[1];

                //XXX: Can do this with array copy and a join simpler 
                string nameString = null;
                for (int i = 3; i < groupNameSplit.Length - 1; i++)
                {
                    if (nameString == null)
                    {
                        nameString = groupNameSplit[i];
                    }
                    else
                    {
                        nameString = nameString + "_" + groupNameSplit[i];
                    }
                }

                var access = groupNameSplit[groupNameSplit.Length - 1];

                this.Name = nameString;
                this.Domain = domain;
                this.Type = type;
                this.Access = access;

            }
            else
            {
                throw new FormatException("invalid location for filesharegroup");
            }
        }
    };
}