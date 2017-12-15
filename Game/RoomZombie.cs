using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Game_Server.GameModes;
using Game_Server.Room_Data;
using System.Diagnostics;
using System.Threading;

namespace Game_Server.Game
{
    class SP_ZombieDrop : Packet
    {
        public SP_ZombieDrop(User usr, int ID, int UsementID, int Type)
        {
            newPacket(30000);
            addBlock(1);
            addBlock(ID);
            addBlock(17);
            addBlock(2);
            addBlock(901);
            addBlock(UsementID);
            addBlock(0);
            addBlock(-1); // Useless
            addBlock(Type); // (0 = Respawn, 1 = Medic, 2 = Ammo, 3 = Repair)
            addBlock(usr.sessionId + UsementID);
            addBlock(ID);
            addBlock(UsementID);
            addBlock(13);
            addBlock(UsementID);
            addBlock(13);
            addBlock(UsementID);
        }
    }

    class SP_ZombieNewWave : Packet
    {
        public SP_ZombieNewWave(int Wave, bool LastWave = false)
        {
            newPacket(13431);
            addBlock(1);
            addBlock(13);
            addBlock(Wave);
            addBlock(LastWave ? 1 : 0);
        }
    }

    class SP_ZombieEndGameChoose : Packet
    {
        public SP_ZombieEndGameChoose(int choose, int rs)
        {
            newPacket(30053);
            addBlock(6);
            addBlock(0);
            addBlock(choose);
            addBlock(rs);
        }
    }

    class SP_ZombieEndGameItem : Packet
    {
        public SP_ZombieEndGameItem( int usrID, int choose, string ItemCode, int rs, int days )
        {
            newPacket(30053);
            addBlock(6);
            addBlock(1);  //>---  +++  
            addBlock(rs); //>--- 0 - con rs aqui...
            addBlock(choose);
            addBlock(ItemCode);  //>---  +++ 
            addBlock(usrID); //>--- ...y userID aqui OK
            addBlock(days);  //>---  +++  
        }
    }

    class CP_ZombieNewStage : Handler
    {
        public override void Handle(User usr)
        {
            int v = int.Parse(getBlock(0));  
            int doorNumber = int.Parse(getBlock(1));
            Log.WriteInfo(">---RoomZombie-83 --- v: " + v);
            Room room = usr.room;
         
            if (room != null)
            {
                Zombie zombie;
                if (v == 4)
                {
                    //Log.WriteInfo(">---RoomZombie-94 --- v = 4 - waitForPrepare=1 - SUICIDIO - doorNumber: " + doorNumber);
                    room.timeattack.waitForPrepare = 1;
                    room.timeattack.time.Stop(); 
                    for (int i = 4; i < 32; i++)
                    {
                        zombie = room.GetZombieByID(i);
                        room.send(new SP_EntitySuicide(i));
                        zombie.Health = 0;
                        zombie.respawn = Generic.timestamp + 4;
                    }
                }
                else if (v == 6) //>--- Al pulsar sobre la caja elegida...
                    
                {
                    room.timeattack.waitForPrepare = 1; 
                    room.timeattack.time.Stop();
                    //> ------------------------------------------------------ Seleccion caja ---------------------------------------------------------- 
                    if (room.mode == 12) //>--- No es necesario, a menos que lo use en EscapeMode
                    {
                        Log.WriteInfo(">---RoomZombie-110 Users; Id: " + usr.userId + " Slot: " + usr.roomslot + " SelBox: " + usr.SelBox);
                        Log.WriteInfo(">---RoomZombie-111  SelBox: " + usr.SelBox);
                        if (usr.SelBox) return; //>--- Si ya has elegido, return
                        usr.SelBox = true;

                        int choose = int.Parse(getBlock(1)); //>--- Elecion de caja. 0-3
                        usr.BoxChoose = choose;
                        Log.WriteInfo(">---RoomZombie-115  getBlock(1): " + getBlock(1) + " getBlock(1): " + getBlock(2));
                        
                        room.send(new SP_ZombieEndGameChoose(choose, usr.roomslot));  //>--- Nuevo paquete, para que salga el nombre del jugador bajo la caja
                        //Log.WriteInfo(">---RoomZombie-122   SP_ZombieEndGameChoose - PACKET-1 - choose: " + choose + "  u.roomslot: " + usr.roomslot);

                        room.timeattack.chooses[usr.BoxChoose] = usr.userId;   //>--- Copia userId en la celda (caja) elegida de la matriz 
                        room.timeattack.rslots[usr.BoxChoose] = usr.roomslot;  //>--- Copia roomSlot en la celda paralela
                        room.timeattack.done++; //>--- +++ para contar los users que han elegido

                        //Log.WriteInfo(">---RoomZombie-130  room.done: " + room.timeattack.done);

                        for (int i = 0; i < 4; i++)
                        {
                            Log.WriteInfo(">---RoomZombie-135  caja [" + i + "] userId:  " + room.timeattack.chooses[i] + " premio: " + room.timeattack.prizes[i] + " roomSlot: " + room.timeattack.rslots[i]);
                        }

                        if (room.timeattack.done >= room.users.Count)
                        {
                            room.timeattack.waitBoxOut = 15; //>--- Cuenta atras despues de elegir regalo
                            //Log.WriteInfo(">---RoomZombie-139 >--- room.timeattack.done:  " + room.timeattack.done + " room.users.Count: " + room.users.Count);
                            //Thread.Sleep(5000);
                        }

                    }
                }
            }
        }    
    }
        
           
    class SP_ZombieNewStage : Packet
    {
        public SP_ZombieNewStage(Room Room, int nuevo)
        {
            newPacket(30053);
            addBlock(nuevo); // 1 - Choose your prize - 3 Fin Stage
            if (nuevo == 2) //>--- +++
                 addBlock(Room.timeattack.zombieForStage);

        }
    }

    class SP_ZombieSpawn : Packet
    {
        public SP_ZombieSpawn(int Slot, int FollowUser, int Place, int ZombieType, int health)
        {
            newPacket(13432);
            addBlock(Slot);
            addBlock(FollowUser);
            addBlock(ZombieType); 
            addBlock(Place);
            addBlock(health);
        }
    }

    class SP_ZombieChangeTarget : Packet
    {
         public SP_ZombieChangeTarget(Room Room, int RoomSlot)
         {
                List<Zombie> list = Room.ZombieFollowers(RoomSlot);
                newPacket(13433);
                addBlock(RoomSlot);
                addBlock(list.Count);
                foreach (Zombie z in list)
                {
                    addBlock(z.ID);
                    addBlock(Room.master);
                }
         }
    }

    class SP_ZombieSkillpointUpdate : Packet
    {
         public SP_ZombieSkillpointUpdate(User usr)
         {
                newPacket(31495);
                addBlock(usr.roomslot);
                addBlock(usr.skillPoints);
         }
    }

    class CP_ZombieSkillPointRequest : Handler
    {
         public override void Handle(User usr)
         {
             Room Room = usr.room;
             if (Room != null && Room.gameactive && Room.channel == 3 && Room.mode == (int)RoomMode.Defense)
             {
                int Type = int.Parse(getBlock(1));
                bool send = true;
                switch (Type)
                {
                    case 1:
                        if (usr.skillPoints < 5) { usr.disconnect(); send = false; }
                        break;
                    case 2:
                        if (usr.skillPoints < 10) { usr.disconnect(); send = false; }
                        break;
                    case 3:
                        if (usr.skillPoints < 20) { usr.disconnect(); send = false; }
                        break;
                    default:
                        {
                            send = false;
                        }
                        break;
                }
                if (send)
                {
                    usr.skillPoints = 0;
                    Room.send(new SP_Unknown(31492, getAllBlocks));
                }

            }
         }
    }

    class CP_ZombieMultiPlayer : Handler
    {
        public override void Handle(User usr)
        {
            if (usr.channel == 3)
            {
                Room Room = usr.room;
                if (Room != null)
                {
                    if (Room.users.Count > 1)
                    {
                        Room.send(new SP_Unknown(31490, getAllBlocks));
                    }
                }
            }

        }
     }
}


