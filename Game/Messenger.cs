using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Game_Server.Game
{
    class CP_Messenger : Handler
    {
        internal enum Subtype
        {
            FriendList = 5606,
            SendMessage = 5608,
            AvailableToChat = 5609,
            FriendAccept = 5610,
            DeleteFriend = 5611,
            BlockUnblock = 5612,
            FriendRequest = 5615,
            FriendDecline = 5616,
        }

        public override void Handle(User usr)
        {
            Subtype type = (Subtype)int.Parse(getBlock(0));
            switch (type)
            {
                case Subtype.FriendList:
                    {
                        //32256 1 5606 3 <- [Count] 0 FrostyPrime 1 0 1 SiroSick 1 0 -1 tishina 1 4
                        usr.send(new SP_MessengerFriends(usr));
                        break;
                    }
                case Subtype.AvailableToChat:
                    {
                        string nickname = getBlock(1);
                        if (nickname.Length > 0 && nickname.Length <= 32)
                        {
                            Messenger vm = usr.GetFriend(nickname);
                            
                            if (vm != null && (vm.status == 0 || vm.status == 1) && vm.isOnline)
                            {
                                usr.send(new SP_Unknown(32256, 1, 5609, vm.nickname, 0));
                            }
                        }

                        break;
                    }
                case Subtype.SendMessage:
                    {
                        //32256 5608 xK1llSt3am Wefracomestai?t'appoziooh <- Client
                        //32256 1 5608 Realiity xK1llSt3am Wefracomestai?t'appoziooh 

                        string nickname = getBlock(1), message = "";
                        if (nickname.Length > 0 && nickname.Length <= 32)
                        {
                            message = getBlock(2).Trim();

                            User friend = Managers.UserManager.GetUser(nickname);
                            if (friend != null)
                            {
                                Messenger msg = usr.GetFriend(friend.userId);
                                if (msg != null && (msg.status == 0 || msg.status == 1) && msg.isOnline)
                                {
                                    byte[] buffer = (new SP_MessengerMessage(usr.nickname, friend.nickname, message).GetBytes());
                                    usr.sendBuffer(buffer);
                                    friend.sendBuffer(buffer);
                                }
                            }
                        }

                        break;
                    }
                case Subtype.FriendRequest:
                    {
                        //32256 1 5615 xK1llSt3am Realiity <- Server
                        string nickname = getBlock(1);
                        if (nickname.Length > 0 && nickname.Length <= 32)
                        {
                            DataTable dt = DB.RunReader("SELECT * FROM users WHERE nickname='" + nickname + "'");
                            if (dt.Rows.Count > 0)
                            {
                                DataRow row = dt.Rows[0];
                                int friendId = int.Parse(row["id"].ToString());
                                if (friendId != usr.userId)
                                {
                                    if (usr.userId == -1 || friendId <= 0) return;
                                    DB.RunQuery("INSERT INTO friends (id1, id2, requesterid, status) VALUES ('" + usr.userId + "', '" + friendId + "', '" + usr.userId + "', '5')");
                                    User Friend = Managers.UserManager.GetUser(friendId);

                                    byte[] buffer = (new SP_MessengerFriendRequest(usr.nickname, nickname)).GetBytes();

                                    if (Friend != null)
                                    {
                                        Friend.AddFriend(usr.userId, usr.userId);
                                        Friend.sendBuffer(buffer);
                                        Friend.send(new SP_MessengerFriends(usr));
                                    }

                                    usr.AddFriend(friendId, usr.userId);
                                    usr.sendBuffer(buffer);
                                    usr.send(new SP_MessengerFriends(usr));
                                }
                            }
                            else
                            {
                                usr.send(new SP_Messenger(SP_Messenger.Subtype.InvalidNickname));
                            }
                        }

                        break;
                    }
                case Subtype.FriendAccept:
                    {
                        //5610 xK1llSt3am 0  <- Client
                        string nickname = getBlock(1).Trim();

                        if (nickname.Length > 0 && nickname.Length <= 32) // Check for the nickname length for safety
                        {
                            DataTable dt = DB.RunReader("SELECT * FROM users WHERE nickname='" + DB.Stripslash(nickname) + "'");
                            if (dt.Rows.Count > 0)
                            {
                                DataRow row = dt.Rows[0];
                                int friendId = int.Parse(row["id"].ToString());
                                if (friendId != usr.userId)
                                {
                                    DB.RunQuery("UPDATE friends SET requesterid='-1', status='1' WHERE id1='" + friendId + "' AND id2='" + usr.userId + "' OR id1='" + usr.userId + "' AND id2='" + friendId + "'");

                                    User friend = Managers.UserManager.GetUser(friendId);
                                    if (friend != null)
                                    {
                                        Messenger u = friend.GetFriend(usr.userId);
                                        if (u != null)
                                        {
                                            u.status = 1;
                                            u.isOnline = true;
                                        }
                                        friend.send(new SP_MessengerFriends(friend));
                                    }

                                    Messenger u2 = usr.GetFriend(friendId);
                                    if (u2 != null)
                                    {
                                        if (friend != null)
                                        {
                                            u2.isOnline = true;
                                        }
                                        u2.status = 1;
                                    }

                                    usr.send(new SP_MessengerFriends(usr));
                                }
                            }
                        }

                        break;
                    }
                case Subtype.FriendDecline:
                    {
                        string nickname = getBlock(1).Trim();
                        if (nickname.Length > 0 && nickname.Length <= 32) // Check for the nickname length for safety
                        {
                            int friendId = int.Parse(DB.RunReaderOnce("id", "SELECT * FROM users WHERE nickname='" + DB.Stripslash(nickname) + "'").ToString());
                            if (friendId > 0) // If the user is in the database
                            {
                                DB.RunQuery("DELETE FROM friends WHERE id1='" + friendId + "' AND id2='" + usr.userId + "' OR id1='" + usr.userId + "' AND id2='" + friendId + "'");

                                User friend = Managers.UserManager.GetUser(friendId);
                                if (friend != null)
                                {
                                    usr.RemoveFriend(usr.userId);
                                    friend.send(new SP_MessengerFriends(friend));
                                }

                                usr.RemoveFriend(friendId);
                                usr.send(new SP_MessengerFriends(usr));
                            }
                        }

                        break;
                    }
                case Subtype.DeleteFriend:
                    {
                        string nickname = getBlock(1);
                        if (nickname.Length > 0 && nickname.Length <= 32)
                        {
                            int friendId = int.Parse(DB.RunReaderOnce("id", "SELECT * FROM users WHERE nickname='" + DB.Stripslash(nickname) + "'").ToString());
                            if (friendId > 0)
                            {
                                DB.RunQuery("DELETE FROM friends WHERE id1='" + friendId + "' AND id2='" + usr.userId + "' OR id1='" + usr.userId + "' AND id2='" + friendId + "'");

                                User friend = Managers.UserManager.GetUser(friendId);
                                if (friend != null)
                                {
                                    Messenger u;
                                    friend.Friends.TryRemove(usr.userId, out u);
                                    friend.send(new SP_MessengerFriends(usr));
                                }

                                Messenger f;
                                usr.Friends.TryRemove(friendId, out f);
                                usr.send(new SP_MessengerFriends(usr));
                            }
                        }

                        break;
                    }
                case Subtype.BlockUnblock:
                    {
                        string nickname = getBlock(1);
                        if (nickname.Length > 0 && nickname.Length <= 32)
                        {
                            int friendId = int.Parse(DB.RunReaderOnce("id", "SELECT * FROM users WHERE nickname='" + DB.Stripslash(nickname) + "'").ToString());
                            if (friendId > 0)
                            {
                                Messenger v = usr.GetFriend(friendId);

                                v.status = v.status == 1 ? 2 : 1;

                                DB.RunQuery("UPDATE friends SET status='" + v.status + "' WHERE id1='" + friendId + "' AND id2='" + usr.userId + "' OR id1='" + usr.userId + "' AND id2='" + friendId + "'");

                                User friend = Managers.UserManager.GetUser(friendId);
                                if (friend != null)
                                {
                                    friend.send(new SP_MessengerFriends(friend));

                                    Messenger u2 = usr.GetFriend(friendId);
                                    if (u2 != null)
                                    {
                                        u2.status = v.status;
                                    }
                                }

                                usr.send(new SP_MessengerFriends(usr));
                            }
                        }
                        break;
                    }
            }
        }
    }

    class SP_Messenger : Packet
    {
        internal enum Subtype
        {
            InvalidNickname = -11
        }

        public SP_Messenger(Subtype Subtype)
        {
            newPacket(32256);
            addBlock((int)Subtype);
        }
    }

    class SP_MessengerFriends : Packet
    {
        public SP_MessengerFriends(User usr)
        {
            newPacket(32256);
            addBlock(1);
            addBlock(5606);
            addBlock(usr.Friends.Count);
            foreach (Messenger friend in usr.Friends.Values)
            {
                if (friend.id > 0 && friend != null)
                {
                    addBlock(1);
                    addBlock(friend.nickname);
                    addBlock((Managers.UserManager.GetUser(friend.id) != null ? 1 : 0));
                    addBlock((friend.requesterId == usr.userId && friend.status == 5) ? 4 : friend.status);
                }
            }
        }
    }

    class SP_MessengerMessage : Packet
    {
        public SP_MessengerMessage(string User, string Friend, string Message)
        {
            newPacket(32256);
            addBlock(1);
            addBlock(5608);
            addBlock(User);
            addBlock(Friend);
            addBlock(Message);
        }
    }

    class SP_MessengerFriendRequest : Packet
    {
        public SP_MessengerFriendRequest(string User, string Friend)
        {
            newPacket(32256);
            addBlock(1);
            addBlock(5615);
            addBlock(User);
            addBlock(Friend);
        }
    }
}
