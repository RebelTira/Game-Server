using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Game_Server.GameModes;
using Game_Server.Room_Data;

using Game_Server.Game;
using System.Collections;

namespace Game_Server.GameModes
{
    class TimeAttack
    {
        ~TimeAttack()
        {
            GC.Collect();
        }
        public int LastTick = 0;
        public bool LastWave = false;
        public bool PreparingStage = false;
        public bool respawnThisWave = false;
        public Room room = null;

        public int Stage = 0;
        public int sleepBeforeEverything = 5;
        public int TimeAttackTime = 0;
        public int zombieForStage = 0;
        public int waitForPrepare = -1;
        public Stopwatch time;
        public int stage1ZombieCount;

        //>--- +++
        public bool end1, end2, end3, final3 = false;
        public bool newid2 = false;
        public int waitBoxOut = -1;
        public int waitBox = 0; 
        public int done = 0; 
        public int vidas = 0; 
        public int vivos = 0; 
        public int[] chooses = new int[4] { -1, -1, -1, -1 };
        public int[] rslots = new int[4] { -1, -1, -1, -1 }; 
        public string[] prizes = new string[4]; 
        public int days = 7; 

        //>-------------------------------------------------------------------------------------------------------------
        public void RunTimeAttack()
        {

            stage1ZombieCount = (room.zombiedifficulty > 0 ? 50 : 30); //>--- 500 : 300 Nº de Zs para acabar Stage 1
            zombieForStage = stage1ZombieCount; //>--- +++
            if (LastTick != DateTime.Now.Second)
            {
                LastTick = DateTime.Now.Second;

                if (this.room.SendFirstWave) //>--- Solo una vez
                {
                    
                    for (int i = 4; i < 32; i++) //>--- +++ Rutina de suicidio para que salgan Zs al jugar por segunda vez sin recrear sala
                    {
                        Zombie zombie = room.GetZombieByID(i);
                        room.send(new SP_EntitySuicide(i));
                        zombie.Health = 0;
                        zombie.respawn = Generic.timestamp + 4;
                    }
                    foreach (User u in room.users.Values)
                    {
                        vidas += u.room.timeAttackSpawns; //>--- Total de vidas de los jugadores
                        Log.WriteInfo(">---T.Attack-78  vidas: " + vidas);
                    }
                    Log.WriteInfo(">---T.Attack-80 --- SOLO UNA VEZ ---   SEND NEW-2 - Stage: " + Stage);
                    this.room.FirstWaveSent = true;

                    room.timeattack.PrepareNewStage(2);

                    this.room.SendFirstWave = false; //>-- Para que no se repita
                    Stage = 1; //>--- ++
                }

                //>--->>>> RUNNING  -----------------              
                if (this.room.zombieRunning) //>--- +++
                {

                    if (this.waitForPrepare > 0) //>--- Se pone a 1 en RoomZombie
                    {
                        waitForPrepare = -1;
                        //>--- +++ ------------------
                        if (end1 || end2 || end3)
                        {
                            Log.WriteInfo(">---T.Attack-100 --- end1: " + end1 + " end2: " + end2 + " end3: " + end3);
                            if (end1) { Stage = 2; end1 = false; room.timeleft += 480000; } //>--- 8 min. mas
                            else if (end2) { Stage = 3; end2 = false; room.timeleft += 360000; } //>--- 6 min. mas
                            else if (end3)
                            {

                                if (room.zombiedifficulty > 0)
                                {
                                    Stage = 4; end3 = false; room.timeleft += 240000; //>--- 4 min. mas
                                }
                                else
                                {
                                    Log.WriteInfo(">---T.Attack-111 --- final3 = true ");
                                    string itemCode = "";   //>--- +++
                                    for (int i = 0; i < 4; i++)
                                    {
                                        room.timeattack.prizes[i] = RandItem(itemCode); //>--- Premios al vector 
                                        Log.WriteInfo(">---TA-110 Premios room.timeattack.prizes[" + i + "]  " + room.timeattack.prizes[i]);
                                    }
                                    return; //>--- para que no arranque otro PrepareNewStage al acabar el 3 en facil
                                }
                            }
                            //>--- +++ ------------------
                            Log.WriteInfo(">---T.Attack-117 --- Stage: " + Stage);
                            room.timeattack.PrepareNewStage(4);
                            room.timeattack.sleepBeforeEverything = 5; //>--- 5 Orig. Tiempo entre antesala y nuevo Stage
                            room.SpawnedZombies = 0;
                            room.KilledZombies = 0;
                            newid2 = true; //>--- Nuevo SP_ZombieNewStage 2

                            Log.WriteInfo(">---T.Attack-124  sleepBeforeEverything = 5.  <<<<<<<<<<<<<<< Nuevo STAGE: " + Stage);

                        }
                    }

                    if (this.sleepBeforeEverything > 0)
                    {
                        Log.WriteInfo(">---T.Attack-131  Bucle sleepBeforeEverything: " + sleepBeforeEverything);
                        this.sleepBeforeEverything--;
                        return;
                    }
                    Random rnd = new Random();

                    switch (Stage)
                    {
                        case 0:
                            {
                                //Log.WriteInfo(">---T.Attack-141 Stage = 0 - No Spawn ");
                                break;
                            }
                        case 1:
                            {
                                room.SpawnZombie(0);
                                if (rnd.Next(0, 2) == 0) room.SpawnZombie(1);
                                if (room.SpawnedZombies > 30)
                                {
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (rnd.Next(0, 5) == 0) room.SpawnZombie(5);
                                    }
                                    if (rnd.Next(0, 6) == 0) room.SpawnZombie(6);
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(2);
                                }
                                if (room.SpawnedZombies > 100)
                                {
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(3);
                                    if (rnd.Next(0, 5) == 0) room.SpawnZombie(8);
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(7);
                                }
                                if (room.SpawnedZombies > 200)
                                {
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(4);
                                    if (room.zombiedifficulty > 0 && (rnd.Next(0, 5) == 0)) { room.SpawnZombie(9); }
                                }
                                //Log.WriteInfo(">---T.Attack - Stage 1 - SpawnedZombies: " + room.SpawnedZombies + " KilledZombies: " + room.KilledZombies );
                                break;

                            }
                        case 2:
                            {
                                room.SpawnZombie(0);
                                if (rnd.Next(0, 2) == 0) room.SpawnZombie(1);
                                if (rnd.Next(0, 2) == 0) room.SpawnZombie(14); //>--- Bomber
                                if (room.SpawnedZombies > 30)
                                {
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (rnd.Next(0, 5) == 0) room.SpawnZombie(5);
                                    }
                                    if (rnd.Next(0, 6) == 0) room.SpawnZombie(6);
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(2);
                                }
                                if (room.SpawnedZombies > 100)
                                {
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(3);
                                    if (rnd.Next(0, 5) == 0) room.SpawnZombie(8);
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(7);
                                }
                                if (room.SpawnedZombies > 200)
                                {
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(4);
                                    if (room.zombiedifficulty > 0 && (rnd.Next(0, 5) == 0)) { room.SpawnZombie(9); }
                                }
                                //Log.WriteInfo(">---T.Attack - Stage 2 - SpawnedZombies: " + room.SpawnedZombies + " KilledZombies: " + room.KilledZombies);
                                break;

                            }
                        case 3:
                            {
                                if (!end3)//>--- Para que no sigan saliendo Zs despues de acabar
                                {
                                    room.SpawnZombie(0);
                                    if (rnd.Next(0, 2) == 0) room.SpawnZombie(1);
                                    if (rnd.Next(0, 2) == 0) room.SpawnZombie(15); //>--- Defender
                                    if (room.SpawnedZombies > 30)
                                    {
                                        for (int i = 0; i < 3; i++)
                                        {
                                            if (rnd.Next(0, 5) == 0) room.SpawnZombie(5);
                                        }
                                        if (rnd.Next(0, 6) == 0) room.SpawnZombie(6);
                                        if (rnd.Next(0, 3) == 0) room.SpawnZombie(2);
                                    }
                                    if (room.SpawnedZombies > 100)
                                    {
                                        if (rnd.Next(0, 3) == 0) room.SpawnZombie(3);
                                        if (rnd.Next(0, 5) == 0) room.SpawnZombie(8);
                                        if (rnd.Next(0, 3) == 0) room.SpawnZombie(7);
                                    }
                                    if (room.SpawnedZombies > 200)
                                    {
                                        if (rnd.Next(0, 3) == 0) room.SpawnZombie(4);
                                        if (room.zombiedifficulty > 0 && (rnd.Next(0, 5) == 0)) { room.SpawnZombie(9); }
                                    }
                                }
                                //Log.WriteInfo(">---T.Attack - Stage 3 - SpawnedZombies: " + room.SpawnedZombies + " KilledZombies: " + room.KilledZombies);
                                break;
                            }
                        case 4:
                            {
                                room.SpawnZombie(0);
                                if (rnd.Next(0, 2) == 0) room.SpawnZombie(1);

                                if (room.SpawnedZombies > 30)
                                {
                                    if (room.spawnedBreakers == 0 && room.KilledZombies >= 20) { room.SpawnZombie(16); } //>--- Breaker
                                    if (room.mapid == 55) room.SpawnZombie(10); //>--- Buster
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (rnd.Next(0, 5) == 0) room.SpawnZombie(5);
                                        if (rnd.Next(0, 6) == 0) room.SpawnZombie(6);
                                    }
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(2);
                                }
                                if (room.SpawnedZombies > 100)
                                {
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(3);
                                    if (rnd.Next(0, 5) == 0) room.SpawnZombie(8);
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(7);
                                }
                                if (room.SpawnedZombies > 200)
                                {
                                    if (rnd.Next(0, 3) == 0) room.SpawnZombie(4);
                                    if (room.zombiedifficulty > 0 && (rnd.Next(0, 5) == 0)) { room.SpawnZombie(9); }
                                }
                                //Log.WriteInfo(">---T.Attack - Stage 4 - SpawnedZombies: " + room.SpawnedZombies + " KilledZombies: " + room.KilledZombies);
                                break;

                            }
                    }
                    if (this.room.SleepTime > 0)
                    {
                        Log.WriteInfo(">---T.Attack-290  Bucle SleepTime: " + room.SleepTime);
                        this.room.SleepTime--;

                        if (this.room.SleepTime == 0 && newid2) //>--- Empieza el nuevo Stage
                        {
                            newid2 = false;
                            Log.WriteInfo(">---T.Attack-297  if SleepTime = 0 - SP_ZombieNewStage(this.room, 2) newid2 = false; ");
                            this.room.send(new SP_ZombieNewStage(this.room, 2));
                        }
                        return;
                    }
                    Log.WriteInfo(">---T.Attack-236  --- KilledZombies: " + room.KilledZombies + " zombieForStage: " + zombieForStage);
                    if (this.Stage == 1 && this.room.KilledZombies >= this.zombieForStage) //>--- &&&&&&&&&&&&&&&&&&&& FINAL STAGE 1
                    {
                        Log.WriteInfo(">---T.Attack-305 FINAL Stage 1 (end1 = true) - PrepareNewStage(3) ");
                        PrepareNewStage(3);
                        sleepBeforeEverything = 5; 
                        room.SleepTime = 5; 
                        end1 = true;  
                    }
                } 
                //>---- <<<<   Running 
            }
        }

        public void Update()
        {
            if (this.waitBox > 0 && final3) 
            {
                this.waitBox--;
                Log.WriteInfo(">---TA-296 >--- room.timeattack.waitBox:  " + waitBox + " done: " + done);
                return; //>--- Para que de tiempo a elegir
//>--- Espera que elijan caja

//>--- Si se agota el tiempo y falta alguien por escoger...
                if (this.waitBox <= 0 && done < room.users.Count)
                {
                    waitBox = -1;
                    Log.WriteInfo(">---TA-303 >--- ----------- Tiempo agotado ------------- ");
                    foreach (User u in room.users.Values)
                    {
                        if (u.BoxChoose <= -1) //>--- Si este usuario no ha elegido...
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (chooses[i] == -1)
                                {
                                    chooses[i] = u.userId;
                                    rslots[i] = u.roomslot;
                                    u.BoxChoose = i;
                                    u.SelBox = true;
                                    done++;

                                    //>--- Nuevo paquete, para que salga el nombre del jugador bajo la caja
                                    room.send(new SP_ZombieEndGameChoose(i, u.roomslot));

                                    Log.WriteInfo(">---TA-321 >---Para quien no ha elegido [caja]:  " + i + " user: " + u.userId);
                                    break;
                                }
                            }
                        }
                    }
                    waitBoxOut = 15; //>--- Cuenta atras despues de elegir regalo
                    
                }
                //>--- Todos tienen caja. Ver regalos e inventario
            }

            if (waitBoxOut > 0) //>--- Es -1 hasta que se completan las elecciones
            {
                waitBoxOut--;
                Log.WriteInfo(">---TA-341 >--- room.timeattack.waitBoxOut:  " + waitBoxOut);
                if (waitBoxOut > 0)
                {
                    return;
                }
                    waitBoxOut = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        Log.WriteInfo(">---TA-319  caja[" + i + "] user.Id:  " + room.timeattack.chooses[i] + " prize:  " + room.timeattack.prizes[i]);
                    }
                    foreach (User u in room.users.Values)
                    {
                        if (u.BoxChoose != -1) 
                        {
                            Log.WriteInfo(">---TA-324   SP_ZombieEndGameItem --- PACKET-2-- caja: [" + u.BoxChoose + "] userId: " + room.timeattack.chooses[u.BoxChoose] + " prize:  " + room.timeattack.prizes[u.BoxChoose] + " slot: " + room.timeattack.rslots[u.BoxChoose]);
                            room.send(new SP_ZombieEndGameItem(u.userId, u.BoxChoose, room.timeattack.prizes[u.BoxChoose], room.timeattack.rslots[u.BoxChoose], days));

                            Inventory.AddItem(u, room.timeattack.prizes[u.BoxChoose], days);
                            // u.send(new SP_Chat("SYSTEM", SP_Chat.ChatType.Whisper, "SYSTEM >> You got '" + room.timeattack.prizes[u.BoxChoose] + "' for " + days + " days.", 999, "NULL"));
                        }
                    }
            }

            RunTimeAttack();
            //Log.WriteInfo(">---TA-361   waitBoxOut:  " + waitBoxOut + " final3:  " + final3 + " done: " + done);
            if (this.room.timeleft <= 0) EndGameTA();
            vivos = (room.users.Values.Where(r => r.Health > 0)).Count();
            if (vivos == 0 && vidas == 0) EndGameTA();
            if (room.BossKilled == true)  EndGameTA();          
            if (done >= room.users.Count) 
            {
                Thread.Sleep(5000);
                EndGameTA();
            }
        }

        private void EndGameTA() //>--- +++
        {
            Stage = 0; 

            foreach (User u in room.users.Values)
                u.send(new SP_ScoreboardInformations(room));

            room.EndGame();
            return;
        }

        //>----------------------------------------------------------------------------------------------------------------------

        public void PrepareNewStage(int id)
        {

            this.room.send(new SP_ZombieNewStage(this.room, id));
            if (id == 4)
            {
                room.send(new SP_ScoreboardInformations(room));
                time.Reset();
                this.room.SleepTime = 5; //>---  Lo que tarda en cambiar al nuevo Stage
            }
        }

        //>----------------------------------------------------------------------------------------------------------------------

        public TimeAttack(Room room)
        {
            
            time = new Stopwatch();

            time.Start();
            this.room = room;
            this.Stage = 0;
            this.sleepBeforeEverything = 5; //>--- Tiempo desde spawn en Stage 1 hasta spawn Zs
            this.PreparingStage = this.respawnThisWave = room.zombieRunning = room.SendFirstWave = room.FirstWaveSent = room.BossKilled = false;
            room.SleepTime = done = 0;  
            room.ZombiePoints = room.SpawnedZombieplayers = room.SpawnedZombies = room.KilledZombies = room.KillsBeforeDrop = room.ZombieSpawnPlace = 0;

            room.spawnedMadmans = room.spawnedManiacs = room.spawnedGrinders = room.spawnedGrounders = room.spawnedHeavys = room.spawnedGrowlers = 0;
            room.spawnedLovers = room.spawnedHandgemans = room.spawnedEnvys = room.spawnedClaws = room.spawnedMadSoldiers = room.spawnedMadPrisoners = room.spawnedLadys = room.spawnedMidgets = 0;
            room.spawnedChariots = room.spawnedCrushers = room.spawnedBusters = room.spawnedCrashers = room.spawnedBombers = room.spawnedDefenders = room.spawnedBreaker2s = room.spawnedSuperHeavys = 0;
            room.spawnedztipo0 = room.spawnedztipo1 = room.spawnedztipo2 = room.spawnedztipo3 = 0;

            //>---  -------------- Nuevos +++ 
        }
        private string RandItem(string itemCode) //>--- Premios aleatorios al acabar TA Easy
        {
            string prize = "";
            var rnd = new Random();
            Thread.Sleep(1000);
            int ri = rnd.Next(0, 4); //>--- aleatorio entre 0 - 3
            Thread.Sleep(500);
            switch (ri)
            {
                case 1: { prize = "DA12"; break; }  //>--- DA12 DA_SW_MP1BD  --- DB23 DB_PT92 --- DA68 IF_Dagger
                case 2: { prize = "DA18"; break; } //>--- DA70 DA_Handbell ---   DA18 DA_FacaWotan
                case 3: { prize = "DM09"; break; } //>--- DM09 DM_Skull_Grenade --- DM08 DM_EasterEgg  --- D908 D9_GRENADE_BOW_Skull_8th
                default: { prize = "DA46"; break; } //>--- DA74 DA_Kris_Dual --- DA46 DA_Gurkha_Skull 
            }
            return prize;
        }
    }
    class PACKET_TIMEATTACK_ALL : Packet
    {
        public PACKET_TIMEATTACK_ALL(ushort packetId, params object[] par)
        {
            newPacket(packetId);
            foreach (var p in par)
            {
                addBlock(p);
            }
        }
    }
    class PACKET_TIMEATTACK_END : Packet
    {
        public PACKET_TIMEATTACK_END(Room room)
        {
            //Log.WriteInfo(">---T.Attack-453 Llamada desde Room-1327 - 30053, 1, 0 - Escoger regalo ");
            newPacket(30053);
            addBlock(1); //>-- Escoger regalo
            addBlock(0); //>--- ???
        }
    }
}
 
        

