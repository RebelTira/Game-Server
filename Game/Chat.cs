using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Game_Server.Managers;

namespace Game_Server.Game
{
    class CP_Chat : Handler
    {
        internal enum ChatType
        {
            Unknown = 2,
            LobbyToChannel = 3,
            RoomToAll = 4,
            RoomToTeam = 5,
            Whisper = 6,
            LobbyToAllChannels = 8,
            RadioChat = 9,
            Clan = 10
        }

        public override void Handle(User usr)
        {
            /*if (usr.LastChatTick > Environment.TickCount) return;
            usr.LastChatTick = Environment.TickCount + 200;*/

            ChatType type = (ChatType)int.Parse(getBlock(0));
            int targetId = int.Parse(getBlock(1));

            string tarGetUser = getBlock(2);
            string message = getBlock(3);

            string sMessage = message.Split(new string[] { ">>" + (char)0x1D }, StringSplitOptions.None)[1].Replace((char)0x1D, (char)0x20);

            if (usr.isCommand(sMessage) || usr.channel == -1) return;

            // Wordfilter

            /*string[] m = sMessage.Split(' ');

            for (int i = 0; i < m.Length; i++)
            {
                m[i] = WordFilterManager.Replace(m[i]);
            }

            message = usr.nickname + (char)0x1D + ">>" + (char)0x1D + string.Join(" ", m);*/

            if (sMessage.Length < 170)
            {
                int currentTs = Generic.timestamp;
                
                if (usr.mutedexpire > currentTs)
                {
                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(usr.mutedexpire);
                    string day = dt.ToString("HH:ss");
                    if (dt.Day != DateTime.Now.Day && dt.Month != DateTime.Now.Month && dt.Year != DateTime.Now.Year)
                    {
                        day = " of the " + dt.ToString("dd/MM/yyyy");
                    }
                    usr.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> You are muted, you will be unmuted at " + day + "!", 999, Configs.Server.SystemName));
                    return;
                }

               
                switch (type)
                {
                    case ChatType.Unknown: // ?
                        {
                            if (tarGetUser.Length > 0)
                            {
                                User Target = UserManager.GetUser(tarGetUser);
                                if (Target != null && Target.sessionId > 0 && (Target.GMMode == false || usr.rank >= 4))
                                {
                                    byte[] buffer = (new SP_Chat(usr, SP_Chat.ChatType.Whisper, message, Target.sessionId, Target.nickname)).GetBytes();

                                    usr.sendBuffer(buffer);

                                    if (!usr.Equals(Target))
                                    {
                                        Target.sendBuffer(buffer);
                                    }
                                }
                                else
                                    usr.send(new SP_Chat(SP_Chat.ErrorCodes.ErrorUser, tarGetUser + (char)0x1D));
                            }
                            break;
                        }
                    case ChatType.LobbyToChannel: // Lobby2Channel
                        {
                            if (usr.room != null) return;
                            if (usr.chatColor == System.Drawing.Color.Empty || usr.rank < 2)
                            {
                                if (usr.rank > 2) targetId = 999;
                                UserManager.sendToChannel(usr.channel, false, new SP_Chat(usr, SP_Chat.ChatType.Room_ToAll, message, targetId, tarGetUser));
                            }
                            else
                            {
                                UserManager.sendToChannel(usr.channel, false, new SP_ColoredChat(message, SP_ColoredChat.ChatType.Normal, usr.chatColor));
                            }
                            break;
                        }
                    case ChatType.RoomToAll: // Room2All
                        {
                            if (usr.room != null)
                            {
                                if (usr.chatColor != System.Drawing.Color.Empty && !usr.room.gameactive && usr.rank >= 2)
                                {
                                    usr.room.send(new SP_ColoredChat(message, SP_ColoredChat.ChatType.Normal, usr.chatColor));
                                }
                                else
                                {
                                    if (usr.HasItem("CC02") && usr.room.status == 1 && usr.roomslot == usr.room.master && usr.rank < 3) //>--- SUPERMASTER
                                    {
                                        targetId = 998;
                                    }
                                    else if (usr.rank > 2)
                                    {
                                        targetId = 999;
                                    }

                                    usr.room.send(new SP_Chat(usr, SP_Chat.ChatType.Room_ToAll, message, targetId, tarGetUser));
                                }
                            }
                            else
                                usr.disconnect();
                            break;
                        }
                    case ChatType.RoomToTeam: // Room2Team
                        {
                            if (usr.room != null)
                            {
                                if (usr.rank > 2) targetId = 999;
                                byte[] buffer;

                                buffer = (new SP_Chat(usr, SP_Chat.ChatType.Room_ToTeam, message, targetId, tarGetUser)).GetBytes();
                                
                                int mySide = usr.room.GetSide(usr);

                                foreach (User u in usr.room.users.Values.Where(u => usr.room.GetSide(u) == mySide))
                                {
                                    u.sendBuffer(buffer);
                                }
                                foreach (User u in usr.room.spectators.Values)
                                {
                                    u.sendBuffer(buffer);
                                }
                            }
                            break;
                        }
                    case ChatType.Whisper: // Whisper
                        {
                            if (tarGetUser.Length > 0)
                            {
                                User Target = UserManager.GetUser(tarGetUser);
                                if (Target != null && Target.sessionId > 0 && (Target.GMMode == false || usr.rank >= 4))
                                {
                                    byte[] buffer = (new SP_Chat(usr, SP_Chat.ChatType.Whisper, message, Target.sessionId, Target.nickname)).GetBytes();
                                    
                                    usr.sendBuffer(buffer);

                                    if (!usr.Equals(Target))
                                    {
                                        Target.sendBuffer(buffer);
                                    }
                                }
                                else
                                    usr.send(new SP_Chat(SP_Chat.ErrorCodes.ErrorUser, tarGetUser + (char)0x1D));
                            }
                            break;
                        }
                    case ChatType.LobbyToAllChannels: //Lobby2AllChannels
                        {
                            if (usr.room != null) return;
                            if (usr.chatColor == System.Drawing.Color.Empty || usr.rank < 2)
                            {
                                if (usr.rank > 2) targetId = 999;
                                UserManager.sendToChannel(1, false, new SP_Chat(usr, SP_Chat.ChatType.Lobby_ToAll, message, targetId, tarGetUser));
                                UserManager.sendToChannel(2, false, new SP_Chat(usr, SP_Chat.ChatType.Lobby_ToAll, message, targetId, tarGetUser));
                                UserManager.sendToChannel(3, false, new SP_Chat(usr, SP_Chat.ChatType.Lobby_ToAll, message, targetId, tarGetUser));
                            }
                            else
                            {
                                UserManager.sendToChannel(1, false, new SP_ColoredChat(message, SP_ColoredChat.ChatType.Normal, usr.chatColor));
                                UserManager.sendToChannel(2, false, new SP_ColoredChat(message, SP_ColoredChat.ChatType.Normal, usr.chatColor));
                                UserManager.sendToChannel(3, false, new SP_ColoredChat(message, SP_ColoredChat.ChatType.Normal, usr.chatColor));
                            }
                            break;
                        }
                    case ChatType.RadioChat:
                            {
                                if(usr.room == null) return;

                                byte[] buffer = (new SP_Chat(usr, SP_Chat.ChatType.RadioChat, message, targetId, tarGetUser)).GetBytes();

                                int mySide = usr.room.GetSide(usr);

                                foreach (User u in usr.room.users.Values.Where(u => usr.room.GetSide(u) == mySide))
                                {
                                    u.sendBuffer(buffer);
                                }

                                break;
                            }
                    case ChatType.Clan: // Clan
                        {
                            if (usr.clan != null)
                            {
                                int clanrank = usr.clan.clanRank(usr);
                                if (clanrank != 9)
                                {
                                    foreach (User u in usr.clan.Users.Values)
                                    {
                                        bool colored = u.room != null && u.playing;

                                        u.send(new SP_Chat(usr, SP_Chat.ChatType.Clan, usr.nickname + " >> " + sMessage, usr.clanId, u.nickname));
                                    }
                                }
                            }
                            else
                            {
                                usr.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Whisper, Configs.Server.SystemName + " >> Chat available after creating or being accepted from a clan", usr.sessionId, usr.nickname));
                            }
                            break;
                        }
                    default:
                        {
                            Log.WriteDebug("New unknow operation for chat: " + type);
                            Log.WriteDebug("Blocks: " + string.Join(" ", getAllBlocks));
                            break;
                        }
                }

                bool chatevent = Configs.Server.ChatEvent.enabled;
                int eventId = Configs.Server.ChatEvent.eventId; // 5 Halloween - 6 Christmas
                string chateventMessage = Configs.Server.ChatEvent.message;

                if (chatevent)
                {
                    if (type == ChatType.LobbyToChannel || type == ChatType.LobbyToAllChannels)
                    {
                        if (!usr.CheckForEvent(eventId))
                        {
                            if (sMessage.Replace((char)0x1D, (char)0x20) == chateventMessage)
                            {
                                string[] weaps = Configs.Server.ChatEvent.items;
                                int r = Generic.random(0, weaps.Length - 1);
                                int days = Generic.random(1, 3);

                                Item item = ItemManager.GetItem(weaps[r]);
                                if (item != null)
                                {
                                    string popup = Configs.Server.ChatEvent.popupMessage;
                                    popup = popup.Replace("%item%", item.Name);
                                    popup = popup.Replace("%days%", days.ToString());

                                    usr.send(new SP_CustomMessageBox(popup));
                                    if (item.Code.ToUpper().StartsWith("B"))
                                    {
                                        Inventory.AddCostume(usr, item.Code, days);
                                    }
                                    else
                                    {
                                        Inventory.AddItem(usr, item.Code, days);
                                    }

                                    usr.AddEvent(eventId, !Configs.Server.ChatEvent.daily);
                                    usr.send(new SP_UpdateInventory(usr, usr.expiredItems));
                                }
                            }
                        }
                    }
                }

                /* usr.send(new SP_Chat_EVENT(User, ItemCode)); */
            }
            else
            {
                // Crash message (BOT ?)
                usr.disconnect();
            }
        }
    }

    class SP_Chat : Packet
    {
        internal enum ChatType : int
        {
            Notice1 = 1,
            Notice2,
            Lobby_ToChannel,
            Room_ToAll,
            Room_ToTeam,
            Whisper,
            Lobby_ToAll = 8,
            RadioChat = 9,
            Clan = 10
        }

        internal enum ErrorCodes : int
        {
            ErrorUser = 95040
        }

        public SP_Chat(string Name, ChatType Type, string Message, uint TargetID, string TargetName)
        {
            newPacket(29696);
            addBlock(1);
            addBlock(0);
            addBlock(Name);
            addBlock((int)Type);
            addBlock(TargetID);
            addBlock(TargetName);
            addBlock(Message);
        }

        public SP_Chat(User usr, ChatType Type, string Message, long TargetID, string TargetName)
        {

            newPacket(29696);
            addBlock(1);
            addBlock(usr.sessionId);
            addBlock(usr.nickname);
            addBlock((int)Type);
            addBlock(TargetID);
            addBlock(TargetName);
            addBlock(Message);
        }

        public SP_Chat(ErrorCodes ErrCode, params object[] Params)
        {
            newPacket(29696);
            addBlock((int)ErrCode);
            Params.ToList().ForEach(obj => { addBlock(obj); });
        }
    }

    class SP_ColoredChat : Packet
    {
        internal enum ChatType : int
        {
            Normal = 0,
            Clan = 2
        }

        public SP_ColoredChat(string Message, ChatType type, System.Drawing.Color color)
        {
            newPacket(29697);
            addBlock(1);
            addBlock(Message);
            addBlock((int)type);
            addBlock((int)color.R);
            addBlock((int)color.G);
            addBlock((int)color.B);
        }
    }
}