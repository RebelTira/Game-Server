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

namespace Game_Server.Room_Data
{
    class RoomHandler_TotalWarSpawnVehicle : RoomDataHandler
    {
        public override void Handle(User usr, Room room)
        {

            //                         30000   0 6 2 166 0 0 1 2 0 37 1 0 0 0 
            //                         30000 1 0 6 2 166 0 0 1 5 37 1 2 3 Hp 0 <- Serv
            //                         30000   0 0 2 166 0 0 1 7 0 39 2 0 0 0 
            //To fix
/*
            int VehicleID = int.(sendBlocks[10]);
            int WarPoint = 5 * (int.(sendBlocks[8]) + 1);

            usr.TotalWarPoint -= WarPoint;

           // usr.WaitingVID = VehicleID;
           // usr.WaitingVIDTimeStamp = Stuff.currTimeStamp + 5;

            sendBlocks[8] = usr.TotalWarPoint.ToString();
            sendBlocks[9] = sendBlocks[10];

            //System.Threading.Thread oThread = new System.Threading.Thread(usr.TotalWarSpawnVehicle);
            //oThread.Start();

*/

/*
                        int VehicleID = int.Parse(sendBlocks[10]);
                        int WarPoint = 5 * (int.Parse(sendBlocks[8]) + 1);

                       usr.TotalWarPoint -= WarPoint;

                       usr.WaitingVID = VehicleID;
                       usr.WaitingVIDTimeStamp = Stuff.currTimeStamp + 5;

                        sendBlocks[8] =usr.TotalWarPoint.ToString();
                        sendBlocks[9] = sendBlocks[10];

                        System.Threading.Thread oThread = new System.Threading.Thread(usr.TotalWarSpawnVehicle);
                        oThread.Start();
*/
        }
    }
}
