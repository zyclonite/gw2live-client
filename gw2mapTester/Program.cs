/*
 * gw2live - GuildWars 2 Map Location Stub
 * 
 * Website: http://gw2map.com
 *
 * Copyright 2013   zyclonite    networx
 *                  http://zyclonite.net
 * Developer: Manuel Bauer
 */
using System;
using System.Timers;
using System.Net;
using System.IO;
using System.Globalization;


namespace gw2map
{
    class Program
    {
        private static Gw2LocationStub stub;

        public static void Main(string[] args)
        {
            Console.WriteLine("starting app, press 'q' to quit");

            stub = new Gw2LocationStub();

            Gw2Map n = new Gw2Map();
            n.Changed += n_Changed;


            while (Console.Read() != 'q') ;
        }

        static void n_Changed(object sender, Gw2MapEventArgs e)
        {
            Console.WriteLine(e.Coordinates.x + " " + e.Coordinates.y + e.Coordinates.profession);
        }
    }
}
