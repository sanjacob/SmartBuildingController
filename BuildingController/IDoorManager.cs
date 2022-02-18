using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding
{
    interface IDoorManager
    {
        public bool OpenDoor(int doorID);
        public bool LockDoor(int doorID);
        public bool OpenAllDoors();
        public bool LockAllDoors();
    }
}
