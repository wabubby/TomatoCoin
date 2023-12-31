using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wabubby {
    public class RollbackSaveGame : SaveGame
    {
        public List<Slot> RollbackSlots;

        public RollbackSaveGame(string name, string path, Slot curSlot, List<Slot> rollbackSlots) : base(name, path, curSlot) {
            RollbackSlots = rollbackSlots;
        }
    }
}

