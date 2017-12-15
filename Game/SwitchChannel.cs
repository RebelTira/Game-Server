using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_SwitchChannel : Handler
    {
        public override void Handle(User usr)
        {
            int channel = int.Parse(getBlock(0));
            if (channel >= 1 || channel <= 3)
            {
                if (channel == 1 && Configs.Server.Channels.Infantry || channel == 2 && Configs.Server.Channels.Vehicular || channel == 3 && Configs.Server.Channels.Zombie)
                {
                    usr.channel = channel;
                }
                else
                {
                    usr.send(new SP_Chat(Configs.Server.SystemName, SP_Chat.ChatType.Lobby_ToAll, Configs.Server.SystemName + " >> This channel has been disabled, please choose a other one", 999, "NULL"));
                }

                usr.send(new SP_SwitchChannel(usr.channel));

                //RoomList
                usr.lobbypage = 0;
                usr.send(new SP_RoomList(usr, usr.lobbypage, false));
                Managers.UserManager.UpdateUserlist(usr);
            }
            else
            {
                // Fake packet (?)
                usr.disconnect();
            }
        }
    }

    class SP_SwitchChannel : Packet
    {
        public SP_SwitchChannel(int channelId)
        {
            newPacket(28673);
            addBlock(1);
            addBlock(channelId);
        }
    }
}
