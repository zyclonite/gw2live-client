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
    public class Gw2ServerMessageEventArgs
    {
       public string  Message { get; set; }

       public Gw2ServerMessageEventArgs(string message)
        {
            this.Message = message;
        }
    }
}
