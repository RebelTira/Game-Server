using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Game_Server.Game
{
    class SP_PlayerInfo : Packet
    {
        public SP_PlayerInfo(List<User> users)
        {
            newPacket(29952);
            addBlock(users.Count);
            Log.WriteInfo(">---RoomUDP- PKT 29952 PlayerInfo" );
			//>---     29952 1 1 1 0 0 0 0 1000 0 1000 TIRA 2 1001001 Peralejo 2 1 0 0 0 0 0 1 461237 101447 0 9535277 -1 -1 -1 0 0 0 0 0 0 0 
			//>--- UDP 29952 1 2 2 1 0 0 0 -1 0 -1 TIRA1 2 1001001 Peralejo 0 1 0 0 0 3 0 1 460104 101586 0 9299684 -1 -1 -1 0 0 0 0 0 0 0 
            foreach (User player in users)
            {
                addBlock(player.userId);        // UserID
                addBlock(player.sessionId);     // SessionID
                addBlock(player.roomslot);      // Room Slot
                addBlock(player.isReady ? 1 : 0);       //Room Ready State of player(0 = not ready, 1 = ready)
                addBlock(player.room.GetSide(player));
                addBlock(player.weapon); // Weapon-ID
                addBlock(player.Health); // ?
                addBlock(player.Class); // ?
                // -- addBlock(player.Health);
                addBlock(0); //>  +++
                addBlock(player.nickname);
                addBlock((player.clan != null ? player.clan.id : -1)); // Clan ID
                if (player.clan != null && !player.clanPending) addBlock(player.clan.iconid); else addBlock(-1); // Clan Icon <- Remove if chapter 1
                addBlock((player.clan != null ? player.clan.name : "NULL")); // Clan Name
                addBlock((player.clan != null && !player.clanPending ? player.clan.clanRank(player) : 0)); // Clan Rank (5 = Master, 2 = User)
                addBlock(1);        // 1? Send From Login
                addBlock(0);       // 0? Send From Login
                addBlock(0);   // 910 (Always)? Send From Login (910 G1, 410 NX , 300 KR , 100 PH, INVALID TW)
                addBlock(0); // <- To remove if chapter 1
                addBlock((player.room.supermaster && player.roomslot == player.room.master) ? 0 : player.premium);// Premium Type
                addBlock(0); // PC Item on Korean
                addBlock(player.HasSmileBadge); // <- To remove if chapter 1
                addBlock(player.kills);         // player Kills
                addBlock(player.deaths);        // player Deaths
                addBlock(0);
                addBlock(player.exp);
                addBlock((player.currentVehicle == null ? -1 : player.currentVehicle.ID));
                addBlock((player.currentSeat == null ? -1 : player.currentSeat.ID));
                addBlock(player.classCode); // <- To remove if chapter 1
                addBlock(0); // Medal ID
                addBlock(0);
                addBlock(0);
                addBlock(0);
                addBlock(0);
                addBlock(0);
                addBlock(0);
            }
        }
    }

    class SP_PlayerInfoSpectate : Packet
    {
        public SP_PlayerInfoSpectate(User usr, Room Room) // Leave UDP
        {
            newPacket(29953);
            addBlock(1);
            addBlock(0);
            addBlock(usr.spectatorId); // Spectator ID
            addBlock(usr.userId);
            addBlock(usr.sessionId);
            addBlock("0"); // Nickname
            addBlock(usr.accesslevel);
        }

        public SP_PlayerInfoSpectate(User usr) // Join UDP
        {
            newPacket(29953);
            addBlock(1);
            addBlock(1);
            addBlock(usr.spectatorId);  // Spectator ID
            addBlock(usr.userId);
            addBlock(usr.sessionId);
            addBlock("0"); // Nickname
            addBlock(usr.accesslevel);
            addBlock(usr.RemoteIP);
            addBlock(usr.RemotePort);
            addBlock(usr.LocalIP);
            addBlock(usr.LocalPort);
            addBlock(0);
        }
    }
}