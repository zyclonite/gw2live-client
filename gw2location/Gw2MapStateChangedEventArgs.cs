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
    public enum Gw2MapState
    {
        Attached, Detached
    }

    public class Gw2MapStateChangedEventArgs : EventArgs
    {
        public Gw2MapState MapState { get; set; }

        public Gw2MapStateChangedEventArgs(Gw2MapState state)
        {
            this.MapState = state;
        }
    }
}
