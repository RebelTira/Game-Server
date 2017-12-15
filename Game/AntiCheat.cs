using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_AntiCheat : Handler
    {
        enum Subtype
        {
            LoginVerification = 5,
            Reauth = 6,
            Detection = 22,
            ShoxGuardDetection = 43,
            KickRequest = 666,
        }

        enum ShoxViolationType
        {
            NoViolation = 0,
            SystemAPIInterception = 1,
            DisallowedProgramFound = 2,
            ProbeOfDebugging = 3,
            RenderingAPIInterception = 4
        }

        public override void Handle(User usr)
        {
            int OPCode = int.Parse(getBlock(0));
            Subtype theSub = (Subtype)OPCode;
            switch (theSub)
            {
                case Subtype.LoginVerification:
                    {
                        try
                        {
                            //Log.WriteLine(string.Join(" ", getAllBlocks()));
                            //5 24 0e9affa5008c0f2f99dc66286c699cbc 7660 6
                            int ClientVer = int.Parse(getBlock(1));
                            string HWID = getBlock(2);

                            int TicketID = int.Parse(getBlock(3));

                            usr.hwid = HWID;

                            if (Configs.Server.AntiCheat.enabled)
                            {
                                int myTicketId = (int)Math.Ceiling((decimal)(TicketID / 244));

                                if (Managers.BanManager.isHWIDBanned(usr.hwid))
                                {
                                    Log.WriteError(usr.nickname + " -> tried to login with banned hwid!");
                                    usr.disconnect();
                                    return;
                                }

                                if (myTicketId == usr.ticketId)
                                {
                                    if (ClientVer != Configs.Server.ClientVersion && Configs.Server.ClientVersion != -1 && !Configs.Server.Debug) // Server config [x]
                                    {
                                        Log.WriteError(usr.nickname + " tried to login with an older patch!");
                                        usr.send(new SP_WelcomePacket(SP_WelcomePacket.ErrorCodes.ClientVersionMissmatch));

                                        byte[] buffer = (new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Notice1, "Your client version is different, please download patchs!", 999, usr.nickname)).GetBytes();

                                        for (int i = 0; i < 3; i++)
                                            usr.sendBuffer(buffer);

                                        usr.disconnect();
                                    }
                                    else
                                    {
                                        usr.AntiCheatCheck = true;
                                    }
                                }
                                else if (myTicketId == 0)
                                {
                                    usr.send(new SP_CharacterInfo(SP_CharacterInfo.ErrCodes.NormalProcedure));
                                    Log.WriteError(usr.nickname + " invalid TicketID! Please check the DSETUP Client Version");
                                    usr.disconnect();
                                }
                                else
                                {
                                    usr.send(new SP_CharacterInfo(SP_CharacterInfo.ErrCodes.NormalProcedure));
                                    Log.WriteError(usr.nickname + " invalid TicketID!");
                                    usr.disconnect();
                                }
                            }
                        }
                        catch
                        {
                            Log.WriteError(usr.nickname + " tried to login but system got error and he got kicked!");
                            usr.disconnect();
                        }
                        break;
                    }
                case Subtype.Detection:
                    {
                        int DetectionType = int.Parse(getBlock(1));
                        string Detection = "Unknown (IP: " + usr.IP + ")";
                        switch (DetectionType)
                        {
                            case 0: Detection = "Present (maybe Bandicam / Fraps / GameBooster)"; break;
                            case 1: Detection = "Draw Index Primitive"; break;
                            case 2: Detection = "Cheat Engine"; break;
                            case 3: Detection = "End Scene"; break;
                            case 4: Detection = "Draw Index Primitive VMT"; break;
                            case 5: Detection = "End Scene VMT"; break;
                            case 6: // Detection by ID
                                {
                                    int id = int.Parse(getBlock(2));
                                    switch (id)
                                    {
                                        case 0: Detection = "Generic Injector"; break;
                                        case 1: Detection = "XTCheats Client"; break;
                                        case 2: Detection = "NetLimiter"; break;
                                        default: Detection = "Generic Illegal Third Party Tool"; break;
                                    }
                                    break;
                                }
                            case 556: Detection = "Anticheat Detection Occured"; break;
                            case 9991: Detection = "Generic Hack Detection (v2)"; break;
                            default: Detection = "Generic Hack Detection"; break;
                        }
                        DB.RunQuery("INSERT INTO anticheat_logs (userid, description, timestamp) VALUES ('" + usr.userId + "', '" + usr.nickname + " - Detection Type: " + Detection + "', '" + Generic.timestamp + "')");
                        break;
                    }
                case Subtype.KickRequest:
                    {
                        if (!Configs.Server.Debug)
                        {
                            Log.WriteError("Received kick out request [DSETUP.dll] for the user " + usr.nickname);
                            usr.disconnect();
                        }
                        break;
                    }
                case Subtype.ShoxGuardDetection:
                    {
                        ushort detectionType;
                        bool validDetection = ushort.TryParse(getBlock(1), out detectionType);
                        if(validDetection)
                        {
                            usr.lastShoxTick = Generic.timestamp;
                            usr.shoxDetection = true;
                            string violationType = "none";
                            string data = getBlock(2).Replace((char)0x1D, (char)0x20);
                            switch((ShoxViolationType)detectionType)
                            {
                                case ShoxViolationType.DisallowedProgramFound:
                                    {
                                        violationType = "Disallowed Program has been found";
                                        break;
                                    }
                                case ShoxViolationType.ProbeOfDebugging:
                                    {
                                        violationType = "User mode debugging has been detected";
                                        break;
                                    }
                                case ShoxViolationType.RenderingAPIInterception:
                                    {
                                        violationType = "Rendering API Hook has been detected";
                                        break;
                                    }
                                case ShoxViolationType.SystemAPIInterception:
                                    {
                                        violationType = "System API Hook has been detected";
                                        break;
                                    }
                                default:
                                    {
                                        if (data.Contains("reinterpret_cast"))
                                        {
                                            data = data.Replace("reinterpret_cast<", "").Replace(">", "");

                                            string[] valuesStr = data.Split(',');
                                            int[] values = { int.Parse(valuesStr[0]), int.Parse(valuesStr[1]), int.Parse(valuesStr[2]) };
                                            if (values.Length == 3)
                                            {
                                                int a = values[0] ^ values[2];

                                                if (a != values[1])
                                                {
                                                    // Invalid algorithm
                                                    Log.WriteError("[" + usr.nickname + "] Invalid ShoxGuard Algorithm");
                                                    usr.disconnect();
                                                }
                                            }
                                            else
                                            {
                                                // Invalid packet length
                                                Log.WriteError("[" + usr.nickname + "] Invalid packet length");
                                                usr.disconnect();
                                            }
                                        }
                                        else
                                        {
                                            usr.disconnect();
                                        }

                                        usr.shoxDetection = false;
                                        return;
                                    }
                            }
                            if (usr.shoxDetection) return;
                            DB.RunQuery("INSERT INTO anticheat_logs (userid, description, timestamp) VALUES ('" + usr.userId + "', '" + usr.nickname + " - ShoxGuard Detection Type: (" + violationType + ") " + data + "', '" + Generic.timestamp + "')");
                            Log.WriteError("[SHOXGUARD] " + usr.nickname + " has been detected (ViolationType: " + violationType + ", data: " + data);
                        }
                        else
                        {
                            /* Fake packet? */
                            usr.disconnect();
                        }
                        break;
                    }
                default:
                    {
                        Log.WriteError("Unknown subtype of Anti Cheat received [" + OPCode + "]");
                        usr.disconnect();
                        break;
                    }
            }
        }
    }

    class SP_AntiCheat : Packet
    {
        public SP_AntiCheat(User usr)
        {
            newPacket(46723);
            addBlock(0);
            //addBlock((int)Math.Ceiling((decimal)(usr.ticketId * 5.352)));
        }
    }

    class SP_CustomSound : Packet
    {
        public enum Sounds : int
        {
            FirstBlood = 0,
            HeadShot = 1
        }

        public SP_CustomSound(Sounds soundIndex)
        {
            newPacket(46725);
            addBlock((int)soundIndex);
        }

        public SP_CustomSound(int soundIndex)
        {
            newPacket(46725);
            addBlock(soundIndex);
        }
    }
}