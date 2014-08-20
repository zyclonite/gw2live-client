/*
 * gw2live - GuildWars 2 Location Library
 * 
 * Website: http://gw2map.com
 *
 * Copyright 2013   zyclonite    networx
 *                  http://zyclonite.net
 * Developer: Manuel Bauer
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gw2map.Model
{
    public class Gw2Coordinates : ICloneable
    {
        public string identity { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public int world_id { get; set; }
        public int map_id { get; set; }
        public int profession { get; set; }
        public bool commander { get; set; }
        public int team_color_id { get; set; }
        public long channel { get; set; }


        public Gw2Coordinates()
        {
            this.x = 0f;
            this.y = 0f;
            this.z = 0f;
        }

        public override bool Equals(object obj)
        {
            return (Gw2Coordinates)obj == this;
        }

        public static bool operator ==(Gw2Coordinates a, Gw2Coordinates b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.x == b.x && a.y == b.y && a.commander == b.commander;
        }

        public static bool operator !=(Gw2Coordinates a, Gw2Coordinates b)
        {
            return !(a == b);
        }

        public object Clone()
        {
            var n = new Gw2Coordinates();

            n.identity = this.identity;
            n.map_id = this.map_id;
            n.world_id = this.world_id;
            n.x = this.x;
            n.y = this.y;
            n.z = this.y;
            n.channel = this.channel;
            n.commander = this.commander;
            n.profession = this.profession;
            n.team_color_id = this.team_color_id;
            return (object)n;
        }


    }
}
