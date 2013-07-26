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

namespace gw2mapClient.Controller
{
    public interface IStateListener
    {
        void NotifyState(gw2mapClient.Controller.Gw2Controller.RecordingState state);
    }
}
