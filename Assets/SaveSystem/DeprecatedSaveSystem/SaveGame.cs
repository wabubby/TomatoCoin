using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Wabubby {
    [System.Serializable]
    public class SaveGame
    {
        public string Name;
        public string Path;
        public Slot CurSlot;

        public SaveGame(string name, string path, Slot curSlot) {
            Name = name;
            Path = path;
            CurSlot = curSlot;
        }
        public void Delete() {
            Debug.Log($"Archiving {Path} to {SaveSystem.OldSavesDirectoryPath}/{System.IO.Path.GetFileName(Path)}");
            Directory.Move(Path, $"{SaveSystem.OldSavesDirectoryPath}/{System.IO.Path.GetFileName(Path)}");
        }
        
    }
}
