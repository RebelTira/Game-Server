using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_RoomList : Handler
    {
        public override void Handle(User usr)
        {
            int type = int.Parse(getBlock(1));

            bool waiting = (type == 1 ? false : true);

            int pageIdx = 0;

            if (type == 3)
            {
                pageIdx = int.Parse(getBlock(0));
            }
            else if(type == 2)
            {
                pageIdx = int.Parse(getBlock(0));
                usr.lobbypage = (int)pageIdx / 13;
            }
            else if(type == 1)
            {
                usr.lobbypage = int.Parse(getBlock(0));
            }
            
            usr.send(new SP_RoomList(usr, usr.lobbypage, waiting, pageIdx, type));
        }
    }

    class SP_RoomList : Packet
    {
        public SP_RoomList(User usr, int page, bool waiting = false, int pageIdx = 0, int type = 1)
        {
            newPacket(29184);

            List<Room> Rooms = new List<Room>();

            var ch = ChannelManager.channels[usr.channel];

            int p = (!waiting ? ch.roomToPageCount : ch.availableRoomToPageCount);

            if (page > p) // Lets try to see if the page of the user is higher than supported pages
                page = p;
            
            if (!waiting)
            {
                Rooms = ch.GetRoomListByPage(page);
            }
            else if (pageIdx <= 13) // Load first page
            {
                Rooms = ch.GetAvailableRoomListByPage(0);
            }
            else
            {
                if (type == 2) // Forward
                {
                    Rooms = ch.GetAvailableRoomList().Where(r => r.id >= pageIdx).Take(13).ToList();
                }
                else // Backward
                {
                    Rooms = ch.GetAvailableRoomList().Where(r => r.id <= pageIdx).Skip(pageIdx - 13).Take(13).ToList();
                }
            }

            //29184 1 0 15 13  0 2 2 0 OnlyEngineer 0 16 15 22 0 0 1 1 3 0 0 6 0 1 0 0 0 1 0 0 -1 1 2 2 5 KENWOOD 1 8 4 65 3 2 2 0 3 1 0 0 0 0 0 0 0 1 0 0 -1 2 2 2 0 #HombergArmyFFA 0 16 15 22 0 0 4 1 3 0 0 6 0 1 0 0 0 1 0 0 -1 3 2 2 8 LoSNoMinesTR 0 16 14 15 4 2 1 0 3 1 0 2 11110 0 0 2 0 1 0 0 -1 4 2 2 4 Ufo 0 8 8 12 3 2 1 0 3 0 0 0 0 0 0 0 0 1 0 0 -1 5 2 2 0 Saraah&Asterix 0 16 16 22 0 0 2 1 3 0 0 6 0 1 0 0 0 1 0 0 -1 6 2 2 1 PolishInvasion 0 8 8 37 3 2 1 0 3 0 0 0 0 0 0 0 0 1 0 0 -1 7 2 2 4 Gessato&Bikini! 0 8 8 61 0 1 1 2 3 0 0 0 0 0 0 0 0 1 0 0 -1 8 2 2 0 OnlyEngineer 0 16 14 22 0 0 2 1 3 0 0 6 0 1 0 0 0 1 0 0 -1 9 2 4 0 FFA 1 16 10 22 0 0 4 1 3 0 0 6 0 0 0 0 0 1 0 0 -1 10 2 2 0 Let'splayWarRocktoday!!! 0 8 8 32 0 4 2 2 3 0 0 0 0 1 0 0 0 1 0 0 -1 11 2 2 10 BobbleHeadEvent!!yo~! 0 16 16 77 0 2 2 2 3 0 0 6 0 1 0 0 0 1 0 0 -1 12 1 1 8 TUFAOE 0 16 13 15 4 2 3 0 3 1 0 2 11110 0 0 0 0 1 1 0 -1

            addBlock(type);
            addBlock(page); // Room Page
            addBlock(p - 1); // Page count
            addBlock(Rooms.Count); //Rooms Count

            Rooms.Cast<Room>().ToList().ForEach(r => { addRoomInfo(r); });
        }
    }
}
