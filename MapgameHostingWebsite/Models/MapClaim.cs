using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MapgameHostingWebsite.Models
{
    public class MapClaim
    {
        public Color[] Colours { get; set; }
        public Location[] Locations { get; set; }
        public int Length { get; set; }

        public MapClaim(Color[] colours, Location[] locations)
        {
            this.Colours = colours;
            this.Locations = locations;
            if (colours.Length == locations.Length)
            {
                this.Length = colours.Length;
            }
        }
    }

    public class Location
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Location(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
