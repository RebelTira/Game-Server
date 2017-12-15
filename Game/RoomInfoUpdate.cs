using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_RoomInfoUpdate : Handler
    {
        public override void Handle(User usr)
        {
            Room Room = usr.room;
            if (Room != null && !Room.gameactive)
            {
                //0 aaa 0 NULL 1 0 24 0 -1 4 3 0 0 1 3 2 11001
                // Log.WriteDebug(string.Join(" ", getAllBlocks())); //>--- Activo esto y da error en getAllBlocks
                Room.name = getBlock(1);
                Room.enablepassword = int.Parse(getBlock(2));
                Room.password = getBlock(3);
                Room.maxusers = int.Parse(getBlock(6));
                Room.zombiedifficulty = int.Parse(getBlock(8));

                //Log.WriteInfo(">---RoomInfoUpdate-23 Room.mode: " + Room.mode);
                if (Room.mode == (int)RoomMode.Explosive || Room.mode == (int)RoomMode.HeroMode)
                {
                    Room.rounds = int.Parse(getBlock(7));
                }
                else
                {
                    Room.rounds = int.Parse(getBlock(9));
                }
                Room.timelimit = int.Parse(getBlock(10));
                //Room.ping = int.Parse(getBlock(11));
                Room.mapid = int.Parse(getBlock(13));
                Room.levellimit = byte.Parse(getBlock(5));

                Room.new_mode = byte.Parse(getBlock(15));
                Room.new_mode_sub = int.Parse(getBlock(16));
                //Log.WriteInfo(">---RoomInfoUpdate-37 Room.new_mode: " + Room.new_mode + ", pone a 6 si es mayor");
                if (Room.new_mode > 6) Room.new_mode = 6;
                Room.send(new SP_RoomInfoUpdate(usr.room));
                Room.ch.UpdateLobby(Room);
            }
        }
    }

    class SP_RoomInfoUpdate : Packet
    {
        public SP_RoomInfoUpdate(Room Room)
        {
            newPacket(29201);
            addBlock(Room.id);
            addBlock(Room.name);
            addBlock(Room.enablepassword);
            addBlock(Room.password);
            addBlock(Room.maxusers);
            addBlock(0); // Ping
            addBlock(Room.levellimit);
            addBlock(Room.rounds);
            addBlock(Room.zombiedifficulty);
            addBlock(Room.rounds);
            addBlock(Room.timelimit);
            addBlock(0); // Ping
            addBlock(Room.autostart);
            addBlock(Room.mapid);
            addBlock(Room.mode);
            addBlock(Room.new_mode);
            addBlock(Room.new_mode_sub);
        }
    }
}
