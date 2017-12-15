
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_Server
{
    internal enum RoomMode : int
    {
        Explosive = 0,
        FFA = 1,            
        FourVersusFour = 2, //>--- 8vs8
        TDM = 3,            //>--- 12vs12, Battle-DM 
        Conquest = 4,       
        BGExplosive = 5,    
        HeroMode = 7,
        TotalWar = 8,       //>--- Falta acabar
        CaptureMode = 9,     //>--- Falta acabar
        Survival = 10,
        Defense = 11,
        TimeAttack = 12,
        Escape = 13,         //>--- Falta acabar
        TankWar = 14,        //>--- Falta acabar
        Annihilation = 15,
        SpecialMode = 16,   //>--- Falta acabar
    }
}
    /*
   { //>---  Original
        Explosive = 0,
        FFA = 1,
        FourVersusFour = 2,
        TDM = 3,
        Conquest = 4,
        BGExplosive = 5,
        TotalWar = 6,
        HeroMode = 7,
        CaptureMode = 9,
        Survival = 10,
        Defense = 11,
        TimeAttack = 12,
        Escape = 13,
        Annihilation = 15,
    }
    */
