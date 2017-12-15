using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Game_Server.Managers
{
    internal enum PackageType
    {
        NormalItem = 0,
        Package = 1
    }
    internal class PackageItem
    {
        public string item;
        public short days;
    }

    class Item
    {
        public int ID;
        public string Code, Name;
        public int Damage = 0;
        public int Level = 1;
        public bool Premium, Buyable;
        public int BuyType = 0;
        private int[] Price = new int[6];
        private int[] Cash = new int[6];
        private int[] EA = new int[4] { 1, 10, 30, 60};

        private int[] useableBranch = new int[5] { 0, 0, 0, 0, 0 };
        private int[] useableSlot = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0};

        public string[] personal = null;
        public string[] surface = null;

        public int GetEACount(int Type) { return EA[Type]; }
        public int GetPrice(int Type) { return Price[Type]; }
        public int GetCashPrice(int Type) { return Cash[Type]; }
        public int GetUseableSlot() { return Array.IndexOf(useableSlot, 1); }

        public PackageType packageType;
        public List<PackageItem> packageItems = new List<PackageItem>();

        public uint dinarReward = 0;

        public bool accruable = false;
        public ushort maxAccrueCount = 0;

        public bool UseableBranch(int branch) { if (branch < 0 || branch > 5) return false; return this.useableBranch[branch] == 1; }
        public bool UseableSlot(int slot) { if (slot < 0 || slot > 8) return false; return this.useableSlot[slot] == 1; }

        public Item(int ID, string Code, string Name, string Price, string Cash, int BuyType, int Damage, uint dinarReward, byte packageType, string packageItems, string[] Surface, string[] Personal, string UseableBranch, string UseableSlot, bool accruable, ushort maxAccrueCount, int Level, bool Premium, bool Buyable)
        {
            try
            {
                this.ID = ID;
                for (int I = 0; I < 6; I++) { this.Price[I] = -1; this.Cash[I] = -1; }
                
                this.Code = Code;
                this.Name = Name;
                this.BuyType = BuyType;

                string[] SplitPrice = Price.Split(new char[] { ',' });

                for (int I = 0; I < SplitPrice.Length; I++)
                    this.Price[I] = int.Parse(SplitPrice[I]);

                string[] SplitCash = Cash.Split(new char[] { ',' });

                for (int I = 0; I < SplitCash.Length; I++)
                    this.Cash[I] = int.Parse(SplitCash[I]);

                if (UseableBranch != null)
                {
                    var useableData = UseableBranch.Split(',');
                    for (int i = 0; i < useableData.Length; i++)
                    {
                        int val = int.Parse(useableData[i].ToString());
                        if (val == 0 || val == 1)
                        {
                            this.useableBranch[i] = val;
                        }
                    }
                }

                this.packageType = (PackageType)packageType;
                if (packageItems != null && packageItems.Length >= 7) // At least one item (CODE/DAYS)
                {
                    string[] tempItems = packageItems.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tempItems.Length; i++)
                    {
                        string[] data = tempItems[i].Split('/');
                        string itemCode = data[0];
                        short itemDays = 3650; // Retail [10 years]

                        if (data.Length == 2)
                        {
                            itemDays = short.Parse(data[1]);
                        }

                        PackageItem newItem = new PackageItem();
                        newItem.item = itemCode;
                        newItem.days = itemDays;
                        this.packageItems.Add(newItem);
                    }
                }

                if (UseableSlot != null)
                {
                    var useableData = UseableSlot.Split(',');
                    for (int i = 0; i < useableData.Length; i++)
                    {
                        int val = int.Parse(useableData[i].ToString());
                        if (val == 0 || val == 1)
                        {
                            this.useableSlot[i] = val;
                        }
                    }
                }

                this.Damage = Damage;

                this.accruable = accruable;
                this.maxAccrueCount = maxAccrueCount;

                this.surface = Surface;

                this.personal = Personal;

                this.dinarReward = dinarReward;

                this.Level = Level;
                this.Premium = Premium;
                this.Buyable = Buyable;
            }
            catch (Exception ex)
            {
                Log.WriteError("Couldn't parse item code: " + this.Code);
            }
        }
    }

    class ItemManager
    {
        public static string Items = null;
        public static Dictionary<string, Item> CollectedItems = new Dictionary<string, Item>();
        public static List<Item> List = new List<Item>();
        public static string MD5 = null;

        private static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch { return null; }
        }

        public static void LoadItems()
        {
            if (Items == null) return;

            int tempID = -1;

            try
            {
                CollectedItems.Clear();
                MD5 = GetMD5HashFromFile("items.bin");
                string[] Lines = Items.Replace("\r", "").Split(new char[] { '\t', '\n' });
                for (int I = 1; I < Lines.Length; I++)
                {
                    try
                    {
                        if (Lines[I] == "<!--")
                        {
                            string Name = "";
                            string Code = "";
                            bool Buyable = false;
                            string Dinars = "-1,-1,-1,-1,-1";
                            string Cashs = "-1,-1,-1,-1,-1";
                            int BuyType = 3;
                            int Level = 1;
                            int Damage = -1;
                            bool Premium = false;
                            string UseableBranch = null;
                            string UseableSlot = null;
                            
                            byte packageType = 0;
                            string packageItems = null;

                            bool accruable = false;
                            ushort maxAccrueCount = 0;

                            uint dinarReward = 0;

                            string[] preparePersonal = new string[3] { "0", "0", "0" };
                            string[] prepareSurface = new string[3] { "0", "0", "0" };
                            
                            for(int V = 0; V < 300; V++)
                            {
                                string actualLine = Lines[I + V].Trim();
                                string nextLine = Lines[I + (V + 1)].Trim();

                                if(actualLine.Contains("ENGLISH")) { Name = nextLine; }
                                if(actualLine.Contains("CODE")) { Code = nextLine; }
                                if(actualLine.Contains("bBuy")) { Buyable = int.Parse(nextLine) == 1; }
                                if(actualLine.Contains("DinarCost")) { Dinars = nextLine; }
                                if(actualLine.Contains("CashCost")) { Cashs = nextLine; }
                                if(actualLine.Contains("ReqLevel")) { Level = int.Parse(nextLine); }
                                if(actualLine.Contains("bPremiumOnly")) { Premium = int.Parse(nextLine) == 1; }
                                if(actualLine.Contains("Power")) { if(Damage == -1) Damage = int.Parse(nextLine); }
                                if(actualLine.Contains("BuyType")) { BuyType = int.Parse(nextLine); }
                                if(actualLine.Contains("UseableBranch")) { UseableBranch = nextLine; }
                                if(actualLine.Contains("UseableSlot")) { UseableSlot = nextLine; }
                                if(actualLine.Contains("nPackageType")) { packageType = byte.Parse(nextLine); }
                                if(actualLine.Contains("PackageComponent")) { packageItems = nextLine; }
                                if(actualLine.Contains("RewardDinar")) { dinarReward = uint.Parse(nextLine); }

                                if(actualLine.Contains("Low")) 
                                {
                                    preparePersonal[0] = nextLine.Split(',')[0];
                                    prepareSurface[0] = nextLine.Split(',')[1];
                                }

                                if(actualLine.Contains("Middle")) 
                                {
                                    preparePersonal[1] = nextLine.Split(',')[0];
                                    prepareSurface[1] = nextLine.Split(',')[1];
                                }
                                if(actualLine.Contains("High")) 
                                {
                                    preparePersonal[2] = nextLine.Split(',')[0];
                                    prepareSurface[2] = nextLine.Split(',')[1];
                                }

                                if(actualLine.Contains("bAccuruable"))
                                {
                                    accruable = nextLine.Contains("1");
                                }

                                if(actualLine.Contains("AccrueCount"))
                                {
                                    maxAccrueCount = ushort.Parse(nextLine);
                                }
								
                                if(actualLine.Contains("//-->"))
                                {
                                    I += V;
                                    break;
                                }
                            }

                            bool id = false;

                            if (Code.StartsWith("D"))
                            {
                                id = true;
                                tempID++;
                            }

                            Item Item = new Item((id ? tempID : 0), Code, Name, Dinars, Cashs, BuyType, Damage, dinarReward, packageType, packageItems, prepareSurface, preparePersonal, UseableBranch, UseableSlot, accruable, maxAccrueCount, Level, Premium, Buyable);
                            CollectedItems.Add(Code, Item);
                        }
                    }
                    catch (Exception ex) { /*Log.WriteDebug(ex.Message);*/ }
                }
                Log.WriteLine("Successfully loaded [" + CollectedItems.Count + "] Items");
            }
            catch
            {
                Log.WriteError("Error write loading items");
            }
        }

        public static Item GetItemByID(int id)
        {
            return CollectedItems.Values.Where(i => i.ID == id).FirstOrDefault();
        }

        public static string GetItemCodeByID(int id)
        {
            var item = CollectedItems.Values.Where(i => i.ID == id).First();
            if (item != null)
            {
                return item.Code;
            }
            return null;
        }
        
        public static int GetDamage(string Code, int Type = 2)
        {
            try
            {
                if (CollectedItems.ContainsKey(Code))
                {
                    Item Item = (Item)CollectedItems[Code];

                    if (Item.personal != null)
                    {
                        int p = int.Parse(Item.personal[Type]);

                        return int.Parse((Math.Truncate((double)((Item.Damage * p) / 100))).ToString());
                    }
                    else
                        return Item.Damage;
                }
                return 0;
            }
            catch { return 0; }
        }

        public static int GetVehicleDamage(string Code, int Type = 1)
        {
            try
            {
                if (CollectedItems.ContainsKey(Code))
                {
                    Item Item = (Item)CollectedItems[Code];

                    if (Item.surface != null)
                    {
                        int p = int.Parse(Item.surface[Type]);

                        return int.Parse((Math.Truncate((double)((Item.Damage * p) / 100))).ToString());
                    }
                    else
                        return Item.Damage;
                }
                return 0;
            }
            catch { return 0; }
        }

        public static Item GetItem(string Code)
        {
            if (Code.Contains("-")) { return null; }
            Code = Code.ToUpper();
            if (CollectedItems.ContainsKey(Code))
            {
                return (Item)CollectedItems[Code];
            }
            return null;
        }
        public static string DecryptBinRaw(byte[] Raw)
        {
            try
            {
                return DecryptBinRaw(System.Text.Encoding.UTF8.GetString(Raw));
            }
            catch (Exception ex) { Log.WriteError(ex.Message); return null; }
        }

        public static string DecryptBinRaw(string Raw)
        {
            try
            {
                byte[] databuffer = Encoding.UTF8.GetBytes(Raw);

                string str = Encoding.Default.GetString(databuffer);

                byte[] binData = new byte[Raw.Length / 2];

                for (int i = 0; i < binData.Length; i++)
                {
                    binData[i] = Convert.ToByte(str.Substring(i * 2, 2), 0x10);
                    binData[i] = (byte)(binData[i] ^ 0x2A);
                }

                return System.Text.Encoding.UTF8.GetString(binData.ToArray());
            }
            catch (Exception ex) { Log.WriteError(ex.Message); return null; }
        }

        public static string DecryptBinFile(string filename)
        {
            try
            {
                System.IO.StreamReader mReader = new System.IO.StreamReader(System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                string mRet = DecryptBinRaw(mReader.ReadToEnd());
                mReader.Close();
                return Items = mRet;
            }
            catch
            {
                Log.WriteError("Failed to decrypt " + filename);
                return null;
            }
        }
    }
}
