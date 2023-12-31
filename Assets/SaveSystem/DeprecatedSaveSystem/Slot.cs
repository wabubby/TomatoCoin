using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Wabubby {
    public class Slot
    {
        public SaveGame SaveGame;
        public string Name;
        public string Path;
        public SaveDataContainer SaveDataCon;

        public Slot(string name, string path, SaveDataContainer saveData) {
            Name = name;
            Path = path;
            SaveDataCon = saveData;
        }

        public void DeLoad() {
            SaveDataCon = null;
        }

        public void Load(bool isEncypted) {
            if (isEncypted) { SaveDataCon = JsonSaver.LoadEncrypted(Path); }
            else { SaveDataCon = JsonSaver.Load(Path); } 
        }

        public void Save(bool isEncypted) {
            if (isEncypted) { JsonSaver.SaveEncrypted(SaveDataCon, Path); }
            else { JsonSaver.Save(SaveDataCon, Path); }
        }

        public void SetPath(string newPath) {
            File.Move(Path, newPath);
            Path = newPath;

            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        }

        public void SetDirectoryPath(string newPath) {

        }

        public Slot DeepCopy() {
            string json = JsonUtility.ToJson(this);
            Slot deepCopy = JsonUtility.FromJson<Slot>(json);

            return deepCopy;
        }

    }
}
