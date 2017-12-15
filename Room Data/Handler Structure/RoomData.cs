/*
 _____   ___ __  __  _____ _____   ___  _  __              ___   ___   __    __ 
/__   \ /___\\ \/ /  \_   \\_   \ / __\( )/ _\            / __\ /___\ /__\  /__\
  / /\///  // \  /    / /\/ / /\// /   |/ \ \            / /   //  /// \// /_\  
 / /  / \_//  /  \ /\/ /_/\/ /_ / /___    _\ \          / /___/ \_/// _  \//__  
 \/   \___/  /_/\_\\____/\____/ \____/    \__/          \____/\___/ \/ \_/\__/  
__________________________________________________________________________________

Created by: ToXiiC
Thanks to: CodeDragon, Kill1212, CodeDragon

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.Game;

namespace Game_Server.Room_Data
{
    class CP_RoomData : Handler
    {
        internal enum DropType
        {
            Respawn = 0,
            Medic = 1,
            Ammo = 2,
            Repair = 3
        }

        public override void Handle(User usr)
        {
            
            if (blocks.Length >= 6 && usr.room != null)
            {
                Room room = usr.room;
                int roomSlot = int.Parse(getBlock(0));
                int roomId = int.Parse(getBlock(1));
                // Log.WriteInfo(">---RoomData-40 roomSlot: " + roomSlot + " roomId: " + roomId);
                if (roomSlot == usr.roomslot && roomId == room.id)
                    
                {
                    Subtype type = (Subtype)int.Parse(getBlock(3));

                    object[] sendBlocks = new object[blocks.Length - 1];
                    Array.Copy(blocks, sendBlocks, sendBlocks.Length);

                    /* Handle each subtype (if handler is available) */
                    using (RoomDataHandler handler = Managers.RoomPacketManager.ParsePacket((int)type, sendBlocks))
                    {
                        if (handler != null)
                        {
                            handler.Handle(usr, room);

                            /* Send out packet to the room*/
                            sendBlocks = handler.sendBlocks; 

                            int subtypeId = int.Parse(sendBlocks[3].ToString());
                            Log.WriteInfo(">---RoomData-60   subtypeId: " + subtypeId + ", type: " + type);
                            if (handler.sendPacket)
                            {
                                //Log.WriteInfo(">---RoomData-67 subtypeId: " + Subtype.ServerRoomReady); //>--- ServerRoomReady en TimeAttack
                                    if (subtypeId == (int)Subtype.ServerRoomReady) 
                                    { 
                                    if (!room.firstingame)
                                    {
                                        room.RespawnAllVehicles();
                                        room.firstingame = true;
                                    }

                                    Log.WriteInfo(">---RoomData-74  firstingame");

                                    if (room.MapData != null)
                                    {
                                        usr.send(new SP_RoomMapData(room));
                                        usr.send(new SP_RoomInitializeUsers(room));
                                    }

                                    usr.mapLoaded = true;
                                    Log.WriteDebug(">---RoomData-82 MapLoaded "); //>--- +++
                                    usr.send(new SP_RoomData(sendBlocks)); 
                                }
                            
                                else if (subtypeId == (int)Subtype.BackToRoom)
                                {
                                    Log.WriteInfo(">---RoomData-87  BackToRoom");
                                    usr.send(new SP_RoomData(sendBlocks));
                                }
                               
                                else if (subtypeId == (int)Subtype.VoteKick)
                                    
                                {
                                    byte[] buffer = (new SP_RoomData(sendBlocks)).GetBytes();
                                    int usrside = room.GetSide(usr);
                               
                                    foreach (User u in room.users.Values)
                                    {
                                        if (room.GetSide(u) == usrside)
                                        {
                                            u.sendBuffer(buffer);
                                        }
                                    }
                                }
                                else
                                {
                                    room.send(new SP_RoomData(sendBlocks)); //>--- ºººººº CREA LA SALA ºººººº
                                }
                                if  (handler.lobbychanges )
                                {
                                    //Log.WriteInfo(">---RoomData-111  room.ch.UpdateLobby(room)");
                                    room.ch.UpdateLobby(room); 
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    class SP_RoomData : Packet
    {
        public SP_RoomData(params object[] Params)
        {
            newPacket(30000);
            addBlock(1); // Success
            Params.ToList().ForEach(r => { addBlock(r); });
        }
    }

    class SP_RoomDataNewRound : Packet
    {
        public SP_RoomDataNewRound(Room Room, int WinningTeam, bool Prepare)
        {
            //Log.WriteInfo(">---RoomData-136  Prepare: " + Prepare);
            int Code = (Prepare == true ? 6 : 5);
            newPacket(30000);
            addBlock(1);
            addBlock(-1);
            addBlock(Room.id);
            addBlock(1);
            addBlock(Code);
            addBlock(0);
            addBlock(1);
            addBlock(WinningTeam);
            addBlock(Room.DerbRounds);
            addBlock(Room.NIURounds);
        }
    }

    class SP_InitializeNewRound : Packet
        
    {
        public SP_InitializeNewRound(Room r)
        {
            //Log.WriteInfo(">---RoomData-159  r.mode: " + r.mode);
            newPacket(30000); 
            addBlock(1); // Success
            addBlock(-1); // ??
            addBlock(r.id); // Room ID
            addBlock(2);
            addBlock(403); // Sub id? >--- ServerRoomReady
            addBlock(0);
            addBlock(1);
            addBlock(3);
            addBlock(363);
            addBlock(0);
            addBlock(0);
            addBlock(r.mode == (int)RoomMode.Annihilation ? 1 : 0);
            Fill(0, 4);
        }
    }
}
