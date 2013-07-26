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
using System.Timers;

namespace gw2map
{
    public class Gw2LocationStub
    {
        private Gw2Map map;
        private Timer aTimer = new Timer();

        public Gw2LocationStub()
        {
            map = new Gw2Map();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 2000;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            Push();
        }

        public void Push()
        {
            map.Write();
        }
    }
}
