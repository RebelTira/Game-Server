using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;


using Game_Server.Game;
using System.Collections;

namespace Game_Server.GameModes
{
    class Escape
    {
        ~Escape()
        {
            GC.Collect();
        }
        public int LastTick = 0;
        public Stopwatch time;
        public Room room = null;



        public Escape(Room room)
        {
            time = new Stopwatch();
            time.Start();
            this.room = room;


        }
        public void Update()
        {
            //if (room.AliveEscape(true) == 0 && room.EscapeZombie == 0) room.endGame();
            if (room.AliveEscape(false) == 0 && room.EscapeHuman == 0) room.EndGame();
        }
    }
        class CP_EscapeMode : Handler
    {
        public override void Handle(User usr)
        {
            Room room = usr.room;
            int blck0 = Convert.ToInt32(getBlock(0));
            int blck1 = Convert.ToInt32(getBlock(1));
            int blck2 = Convert.ToInt32(getBlock(2));
            int blck3 = Convert.ToInt32(getBlock(3));
            int blck4 = Convert.ToInt32(getBlock(4));
            int blck5 = Convert.ToInt32(getBlock(5));
            int ThisTimeStamp = 0;
            Log.WriteInfo("HANDLE_ESCAPE-25 room: " + room);
            Log.WriteInfo("HANDLE_ESCAPE-26 nums: " + blck0 + blck1 + blck2 + blck3 + blck4 + blck5);
            if (blck1 == 1 && blck2 == 6)
            {
                usr.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Room_ToAll, "SYSTEM >> A por el ;)!", 999, "NULL"));
                usr.send(new PACKET_3(blck3, room)); //>--- 31507 0 0 room blck3 0 0 -1 0
            }
            if (blck0 == 0 && blck1 == 0 && blck2 == 0 || blck2 == 1 && blck4 != 0)
                ThisTimeStamp = Generic.timestamp;
            {
                if (blck4 == 0 && ThisTimeStamp >= Generic.timestamp)
                {
                    room.InHacking = true;
                    usr.send(new PACKET_HACKING_ESCAPE(0, room.EscapeHack, 0));
                    room.EscapeHack += 10;
                    ThisTimeStamp += 2;
                    usr.send(new PACKET_HACKING_ESCAPE(2, room.EscapeHack, 0));
                }
                else if (blck4 == room.EscapeHack)
                {
                    if (room.HackingPause % 3 == 0) room.EscapeHack += 10;
                    usr.send(new PACKET_HACKING_ESCAPE(2, room.EscapeHack, 0));
                }

            }
        }
    }
    class PACKET_ESCAPE_MODE1 : Packet
    {
        public PACKET_ESCAPE_MODE1(int UID)
        {
            // S=> 831664759 31505 1 1 12 1 GetSide (0)
            // S=> 835753467 31505 1 1 0 0 GetSide (1)

            newPacket(31505);
            addBlock(1);
            addBlock(UID);
            addBlock(12);
            addBlock(1);
        }

    }
    class PACKET_ESCAPE_MODE2 : Packet
    {
        public PACKET_ESCAPE_MODE2()
        {
            // S=> 831664759 31505 1 1 12 1 GetSide (0)
            // S=> 835753467 31505 1 1 0 0 GetSide (1)

            newPacket(31505);
            addBlock(1);
            addBlock(1);
            addBlock(0);
            addBlock(0);
        }
    }


    class PACKET_ESCAPE_MODE3 : Packet
    {
        public PACKET_ESCAPE_MODE3()
        {
            //834515335 31505 1 0 2  0 0 12 1 1 0 0 1 12 32 3000 8000 8000 9000 12000 10000 10000 9000 12000 14000 3000 3000 14000 25000 10000 10000 10000 10000 10000 1000 10000 10000 10000 10000 8000 8000 8000 8000 8000 10000 5000 4000000

            newPacket(31505);
            addBlock(1);
            addBlock(0);
            addBlock(2);
            addBlock("");
            addBlock(0);
            addBlock(0);
            addBlock(12);
            addBlock(1);
            addBlock(1);
            addBlock(0);
            addBlock(0);
            addBlock(1);
            addBlock(12);
            addBlock(32);
            addBlock(3000);
            addBlock(8000);
            addBlock(8000);
            addBlock(9000);
            addBlock(12000);

        }
    }

    class PACKET_ESCAPE_MODE : Packet
    {
        public PACKET_ESCAPE_MODE()
        {
            // S=> 834487238 31505 1 0 2  0 0 12 1 0

            newPacket(31505);
            addBlock(1);
            addBlock(0);
            addBlock(2);
            addBlock("");
            addBlock(0);
            addBlock(0);
            addBlock(12);
            addBlock(1);
            addBlock(0);
        }

    }

    class PACKET_1 : Packet
    {
        public PACKET_1()
        {
            //1° = 834508912 31507 0 12 1 6 0 0 -1 0

            newPacket(31507);
            addBlock(0);
            addBlock(12);
            addBlock(1);
            addBlock(1);
            addBlock(6);
            addBlock(0);
            addBlock(0);
            addBlock(-1);
            addBlock(0);

        }

    }

    class PACKET_2 : Packet
    {
        public PACKET_2(int num)
        {
            //S=> 834541187 31507 0 12 1 5 0 0 -1 0
            //    833363252 31507 0 12 1 6 0 0 -1 0

            newPacket(31507);
            addBlock(0);
            addBlock(12);
            addBlock(1);
            addBlock(num);
            addBlock(0);
            addBlock(0);
            addBlock(-1);
            addBlock(0);
        }

    }

    class PACKET_HACKING_ESCAPE : Packet
    {
        public PACKET_HACKING_ESCAPE(int Id, int count, int Timer)
        {
            //S=> 834837857 31507 0 0 0 2 0 10 -1 0

            newPacket(31507);
            addBlock(0);
            addBlock(0);
            addBlock(0);
            addBlock(Id);
            addBlock(0);
            addBlock(count);
            addBlock(-1);
            addBlock(Timer);

        }
    }

    class PACKET_3 : Packet
    {
        public PACKET_3(int blck3, Room Room)
        {
            //S=> 834542789 31507 0 0 1 6 0 0 -1 0

            newPacket(31507);
            addBlock(0);
            addBlock(0);
            addBlock(Room.id);
            addBlock(blck3);
            addBlock(0);
            addBlock(0);
            addBlock(-1);
            addBlock(0);

        }

    }
}
