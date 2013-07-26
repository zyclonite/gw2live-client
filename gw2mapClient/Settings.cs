/*
 * gw2live - GuildWars 2 Map Client
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
using System.Threading.Tasks;

namespace gw2mapClient
{
    public class Settings
    {
        public string BaseUrl { get { return "http://gw2map.com" ; } }
        public string Gw2RegHive { get { return "SOFTWARE\\Wow6432Node\\ArenaNet\\Guild Wars 2"; } }
        public string gw2RegKey { get { return "Path"; } }
    }

}
