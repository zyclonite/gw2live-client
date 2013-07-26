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

namespace gw2map
{
    public class Gw2MapEventArgs : EventArgs
    {
        public Gw2Coordinates Coordinates { get; set; }

        public Gw2MapEventArgs(Gw2Coordinates coordinates)
        {
            this.Coordinates = coordinates;
        }
    }
}
