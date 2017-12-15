using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server.Game
{
    class CP_ItemEquipment : Handler
    {
        internal enum EquipmentType
        {
            Equip = 0,
            Unequip = 1,
            SixSlot = 3
        }

        public override void Handle(User usr)
        {
            int equip_int = int.Parse(getBlock(0));
            EquipmentType equip = (EquipmentType)equip_int;
            int Class = int.Parse(getBlock(1));
            string type = getBlock(2);
            int inventoryid = int.Parse(getBlock(3));
            string itemcode = getBlock(4);
            int targetSlot = int.Parse(getBlock(5));

            bool defaultWeapon = usr.IsWhitelistedWeapon(itemcode);
            if (usr.room != null && usr.IsAlive()) return;

            //if (usr.room != null && usr.room.mode == (int)RoomMode.FFA && usr.room.ffa != null && usr.room.ffa.isGunGame)
            //{
            //    usr.send(new SP_CustomMessageBox("You cannot equip any item in Gun Game mode."));
            //    return;
            //}

            if (usr.HasItem(itemcode) || defaultWeapon || itemcode.Contains("-"))
            {
                Managers.Item item = Managers.ItemManager.GetItem(itemcode);

                if (targetSlot >= 4)
                {
                    int t = targetSlot - 4;
                    string[] SplitSlots = usr.AvailableSlots.Split(new char[] { ',' });
                    if (SplitSlots[t] != "T")
                        return;
                }

                switch (equip)
                {
                    case EquipmentType.Equip:
                        {
                            if (item == null) return;
                            if (!item.UseableBranch(Class))
                            {
                                Log.WriteError(usr.nickname + " tried to equip " + item.Name + " in the class " + Class + " [NOT ALLOWED!]");
                                usr.disconnect();
                                return;
                            }
/*
                            if (!item.UseableSlot(targetSlot))
                            {
                                Log.WriteError(usr.nickname + " tried to equip " + item.Name + " in the slot " + targetSlot + " [NOT ALLOWED!]");  // >--- Fallo en slot 7
                                usr.disconnect();
                                return;
                            }
*/
                            if (type == "I")
                            {
                                itemcode = Inventory.calculateInventory(inventoryid);
                            }

                            if (targetSlot < 0 || targetSlot > 8)
                            {
                                Log.WriteDebug("User " + usr.nickname + " tried to equip slot " + targetSlot);
                                usr.disconnect();
                                return;
                            }

                            bool alreadyEquipped = (usr.equipment[Class, targetSlot] == itemcode);

                            if (!alreadyEquipped)
                            {
                                /* Skip if is already equipped to another check */
                                for (int i = 0; i < 8; i++)
                                {
                                    if (usr.equipment[Class, i] == itemcode)
                                    {
                                        alreadyEquipped = true;
                                        break;
                                    }
                                }
                            }

                            if (!alreadyEquipped)
                            {
                                usr.equipment[Class, targetSlot] = itemcode;
                                usr.LoadRetails();
                            }
                            else
                            {
                                usr.send(new SP_Itemequipment(SP_Itemequipment.ErrorCodes.AlreadyEquipped));
                            }

                            break;
                        }
                    case EquipmentType.Unequip:
                        {
                            targetSlot = int.Parse(getBlock(3));

                            if (targetSlot < 0 || targetSlot > 8)
                            {
                                Log.WriteDebug("User " + usr.nickname + " tried to unequip slot " + targetSlot);
                                usr.disconnect();
                                return;
                            }

                            if (usr.equipment[Class, targetSlot] != "^")
                            {
                                usr.equipment[Class, targetSlot] = "^";
                                usr.LoadRetails();
                            }

                            break;
                        }
                    case EquipmentType.SixSlot:
                        {
                            if (itemcode.Contains("-")) // When equipping 2 items
                            {
                                if (targetSlot != 5) return; // Double item only on 6th slot
                                string[] codeInfo = itemcode.Split('-');

                                if (!usr.HasItem(codeInfo[0]) || !usr.HasItem(codeInfo[1]) || !Inventory.isPXItem(codeInfo[0]) || !Inventory.isPXItem(codeInfo[1])) return;

                                int index1 = usr.GetItemIndex(codeInfo[0]);
                                int index2 = usr.GetItemIndex(codeInfo[1]);

                                itemcode = Inventory.calculateInventory(index1) + "-" + Inventory.calculateInventory(index2);
                            }
                            else // When unequipping one of two px
                            {
                                if (!Inventory.isPXItem(itemcode)) return;
                                int itemIndex = usr.GetItemIndex(itemcode);

                                itemcode = Inventory.calculateInventory(itemIndex);
                            }

                            usr.equipment[Class, 5] = itemcode;
                            break;
                        }
                }

                string equipmentString = usr.GetEquipment(Class);
                usr.send(new SP_Itemequipment(Class, equipmentString));
                DB.RunQuery("UPDATE equipment SET class" + Class + "='" + equipmentString + "' WHERE ownerid='" + usr.userId + "'");
            }
            else
            {
                // Cheat Engine / ArtMoney or whatever?
                Log.WriteError(usr.nickname + " tried to equip " + itemcode + " but he haven't it!");
                usr.disconnect();
            }
        }
    }

    class SP_Itemequipment : Packet
    {
        internal enum ErrorCodes
        {
            AlreadyEquipped = 97090
        }

        public SP_Itemequipment(ErrorCodes Code)
        {
            newPacket(29970);
            addBlock((int)Code);
        }

        public SP_Itemequipment(int Class, string equipment)
        {
            newPacket(29970);
            addBlock(1);
            addBlock(Class);
            addBlock(equipment);
        }
    }
}
