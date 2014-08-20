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
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Timers;
using gw2map.Model;
using Newtonsoft.Json;

namespace gw2map
{
    public delegate void ChangedEventHandler(object sender, Gw2MapEventArgs e);

    public delegate void StateChangedEventHandler(object sender, Gw2MapStateChangedEventArgs e);

    public class Gw2Map : IDisposable
    {

        private const string NAME = "MumbleLink";
        private const float METER_TO_INCH = 39.3701f;
        private int[] wvwmaps = new int[] {96,94,95,38};
        private Gw2MapState state = Gw2MapState.Detached;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LinkedMem
        {
            public UInt32 uiVersion;
            public UInt32 uiTick;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] fAvatarPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] fAvatarFront;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] fAvatarTop;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] fCameraPosition;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] fCameraFront;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] fCameraTop;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string identity;
            public UInt32 context_len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] context;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)]
            public string description;
        };

        public event ChangedEventHandler Changed;

        public event StateChangedEventHandler StateChanged;

        public Gw2MapState State {
            get { return state; }
            private set
            {
                if (value != state)
                {
                    state = value;
                    OnStateChanged(new Gw2MapStateChangedEventArgs(state));
                    
                }
        } }

        #region Win32

        [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, FileMapProtection flProtect, Int32 dwMaxSizeHi, Int32 dwMaxSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenFileMapping(FileMapAccess DesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr MapViewOfFile(IntPtr hFileMapping, FileMapAccess dwDesiredAccess, Int32 dwFileOffsetHigh, Int32 dwFileOffsetLow, Int32 dwNumberOfBytesToMap);

        [Flags]
        private enum FileMapAccess : uint
        {
            FileMapCopy = 0x0001,
            FileMapWrite = 0x0002,
            FileMapRead = 0x0004,
            FileMapAllAccess = 0x001f,
            fileMapExecute = 0x0020,
        }

        [Flags]
        private enum FileMapProtection : uint
        {
            PageReadonly = 0x02,
            PageReadWrite = 0x04,
            PageWriteCopy = 0x08,
            PageExecuteRead = 0x20,
            PageExecuteReadWrite = 0x40,
            SectionCommit = 0x8000000,
            SectionImage = 0x1000000,
            SectionNoCache = 0x10000000,
            SectionReserve = 0x4000000,
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hFile);

        [DllImport("kernel32")]
        private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        #endregion Win32

        private readonly int MEM_SIZE;

        private IntPtr mappedFile;
        private IntPtr mapView;
        private byte[] readBuffer;
        private byte[] writeBuffer;
        private GCHandle readBufferHandle;
        private GCHandle writeBufferHandle;
        private UnmanagedMemoryStream unmanagedStream;
		private LinkedMem lm = new LinkedMem();
		private Random rand = new Random();
        private uint lastTick = 0;
        private Gw2Coordinates last = new Gw2Coordinates();
        private Timer aTimer;
        private Timer stateTimer; 

        unsafe public Gw2Map()
        {
            MEM_SIZE = Marshal.SizeOf(typeof(LinkedMem));

            //mappedFile = OpenFileMapping(FileMapAccess.FileMapRead, false, NAME);
            mappedFile = OpenFileMapping(FileMapAccess.FileMapAllAccess, false, NAME);
            if (mappedFile == IntPtr.Zero)
            {
                mappedFile = CreateFileMapping(IntPtr.Zero, IntPtr.Zero, FileMapProtection.PageReadWrite, 0, MEM_SIZE, NAME);
                if (mappedFile == IntPtr.Zero)
                    throw new Exception("Unable to create file mapping");
            }

            //mapView = MapViewOfFile(mappedFile, FileMapAccess.FileMapRead, 0, 0, MEM_SIZE);
            mapView = MapViewOfFile(mappedFile, FileMapAccess.FileMapAllAccess, 0, 0, MEM_SIZE);
            if (mapView == IntPtr.Zero)
                throw new Exception("Unable to map view of file");

			lm.name = "Guild Wars 2";
			lm.uiTick = 1;
			lm.fAvatarPosition = new float[3];
			lm.context = new byte[256];
			lm.context_len = 256;

			writeBuffer = new byte[MEM_SIZE];
			writeBufferHandle = GCHandle.Alloc(writeBuffer, GCHandleType.Pinned);
            readBuffer = new byte[MEM_SIZE];
            readBufferHandle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
	
            byte* p = (byte*)mapView.ToPointer();
            //unmanagedStream = new UnmanagedMemoryStream(p, MEM_SIZE, MEM_SIZE, FileAccess.Read);
            unmanagedStream = new UnmanagedMemoryStream(p, MEM_SIZE, MEM_SIZE, FileAccess.ReadWrite);

            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;

            stateTimer = new System.Timers.Timer();
            stateTimer.Elapsed += stateTimer_Elapsed;
            stateTimer.Interval = 10000;
            stateTimer.Enabled = true;

            State = Gw2MapState.Detached;
        }

        void stateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            State = Gw2MapState.Detached;
            stateTimer.Stop();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            long currentTick = 0;
            long oldTick = lastTick;

            Gw2Coordinates previous = last;
            Gw2Coordinates coord = GetCoordinates(out currentTick);
            if (previous != coord && FilterWvwMap(coord.map_id))
            {
                OnChanged(new Gw2MapEventArgs(coord));
            }

            if (currentTick > oldTick)
            {
                stateTimer.Stop();
                stateTimer.Start();

                State = Gw2MapState.Attached;

            }

        }

        private bool FilterWvwMap(int id)
        {
            foreach (int i in wvwmaps)
            {
                if (i == id)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void OnChanged(Gw2MapEventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        protected virtual void OnStateChanged(Gw2MapStateChangedEventArgs e)
        {
            if (StateChanged != null)
                StateChanged(this, e);
        }

		public void Write()
		{
            Gw2CoordinatesIdentity rawIdentity = new Gw2CoordinatesIdentity();
            rawIdentity.commander = true;
            rawIdentity.name = "ZycloniteNetworks";
            rawIdentity.profession = 6;
            rawIdentity.team_color_id = 9;
			lm.uiTick++;
            lm.identity = JsonConvert.SerializeObject(rawIdentity);
            lm.fAvatarPosition[0] = (float)rand.NextDouble() * 100;
            lm.fAvatarPosition[1] = (float)rand.NextDouble() * 100;
            lm.fAvatarPosition[2] = (float)rand.NextDouble() * 100;
            int map = 38;
    		int world = 2006;
			BitConverter.GetBytes(map).CopyTo(lm.context, 28);
			BitConverter.GetBytes(world).CopyTo(lm.context, 36);
			Marshal.StructureToPtr(lm, writeBufferHandle.AddrOfPinnedObject(), false);
            unmanagedStream.Position = 0;
            unmanagedStream.Write(writeBuffer, 0, MEM_SIZE);
		}

		public LinkedMem Read()
        {
            unmanagedStream.Position = 0;
            unmanagedStream.Read(readBuffer, 0, MEM_SIZE);
            return (LinkedMem)Marshal.PtrToStructure(readBufferHandle.AddrOfPinnedObject(), typeof(LinkedMem));
        }

        public Gw2Coordinates GetCoordinates(out long tick)
        {
            tick = lastTick;
            LinkedMem l = Read();
            if(l.name.Equals("Guild Wars 2") && (l.uiTick > lastTick))
            {
                Gw2CoordinatesIdentity rawIdentity = JsonConvert.DeserializeObject<Gw2CoordinatesIdentity>(l.identity);
			    Gw2Coordinates coord = new Gw2Coordinates();
			    float x = l.fAvatarPosition [0];
			    float y = l.fAvatarPosition [2];
			    float z = l.fAvatarPosition [1];
            	coord.x = x * METER_TO_INCH; //west to east
            	coord.y = y * METER_TO_INCH; //north to south
            	coord.z = -z * METER_TO_INCH; //altitude
            	coord.world_id = BitConverter.ToInt32(l.context, 36);
            	coord.map_id = BitConverter.ToInt32(l.context, 28);
				coord.identity = rawIdentity.name;
                coord.profession = rawIdentity.profession;
                coord.team_color_id = rawIdentity.team_color_id;
                coord.commander = rawIdentity.commander;
				lastTick = l.uiTick;
                tick = l.uiTick;
                if (last != coord)
                {
                    last = coord;
                }
			}
			return last;
        }


        public void Dispose()
        {
            if (unmanagedStream != null)
                unmanagedStream.Dispose();
            if (writeBufferHandle.IsAllocated)
                writeBufferHandle.Free();
            if (readBufferHandle.IsAllocated)
                readBufferHandle.Free();
            if (mapView != IntPtr.Zero)
            {
                UnmapViewOfFile(mapView);
                mapView = IntPtr.Zero;
            }
            if (mappedFile != IntPtr.Zero)
            {
                CloseHandle(mappedFile);
                mappedFile = IntPtr.Zero;
            }
            if (aTimer != null)
                aTimer.Dispose();
        }

    }
}
