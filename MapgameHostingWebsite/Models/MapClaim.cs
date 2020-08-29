using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MapgameHostingWebsite.Models
{
    public class MapClaim
    {
        public Color[] colours { get; set; }
        public Location[] locations { get; set; }
        public int length { get; set; }

        public MapClaim(Color[] colours, Location[] locations)
        {
            this.colours = colours;
            this.locations = locations;
            if (colours.Length == locations.Length)
            {
                this.length = colours.Length;
            }
        }
    }

    public class Location
    {
        public int x { get; set; }
        public int y { get; set; }

        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
