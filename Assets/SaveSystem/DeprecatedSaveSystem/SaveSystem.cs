using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;

namespace Wabubby {
    public class SaveSystem : Singleton<SaveSystem> {
        public enum SaveType{
            Backup, Rollback
        }
        public SaveType SaveMethod;

        // public static string SlotSettingsPath;
        [HideInInspector] public static string SavesDirectoryPath;
        [HideInInspector] public static string OldSavesDirectoryPath;


        public static SaveGame noSave = new SaveGame("there are no saves you dum dum", null, null);
        public List<SaveGame> SaveGames = new List<SaveGame>(); // all current savegames in a parrallel list. should probably conform to one structure. yeah ill do that.

        public UnityEvent OnSaveGameAdded;
        public UnityEvent OnCurrentGameChanged;


        public static int CurrentIndex = 0;
        public static SaveGame CurrentSaveGame;
        public static Slot CurrentSlot;
        public static SaveData CurrentSaveData => CurrentSaveGame.CurSlot.SaveDataCon.SaveData;

        public bool DoEncryptSaveData=false;

        public bool LoadAllSavesOnAwake = false;

        protected override void Awake() {
            if (instance == null) {
                Initialize();
            }
            base.Awake();

            // SlotSettingsPath = ConvertToSaveName("SaveSlotSettings");
        }

        private void Initialize() {
            SavesDirectoryPath = $"{Application.persistentDataPath}{"/saves"}";
            OldSavesDirectoryPath = $"{Application.persistentDataPath}{"/old-saves"}";
            Debug.Log(SavesDirectoryPath);
            
            CheckCreateSaveDirectories();

            IdentifyAllSaveGames();
            IdentifyAllSlots();

            CurrentSaveGame = noSave;

            if (LoadAllSavesOnAwake) {
                LoadAllSaveGames();
            }
            
        }

        private void CheckCreateSaveDirectories() {
            if (! JsonSaver.DirectoryExists(SavesDirectoryPath)) { JsonSaver.CreateDirectory(SavesDirectoryPath); }
            if (! JsonSaver.DirectoryExists(OldSavesDirectoryPath)) { JsonSaver.CreateDirectory(OldSavesDirectoryPath); }
        }

        private void ClearSaveGamesList() {
            SaveGames = new List<SaveGame>();
        }

        private void IdentifyAllSaveGames() { // collects all savegames
            ClearSaveGamesList();

            string[] directoryPaths = Directory.GetDirectories(SavesDirectoryPath);
            System.Array.Sort(directoryPaths);

            foreach (string dirPath in directoryPaths) { // scroll thru the entire save directory
                string dirName = Path.GetFileNameWithoutExtension(dirPath);

                Debug.Log($"p:{dirPath}  n:{dirName}");

                if (SaveMethod == SaveType.Backup) {
                    SaveGames.Add(new BackupSaveGame(dirName, dirPath, null, null));
                } else if (SaveMethod == SaveType.Rollback) {}
            }
        }

        private void IdentifyAllSlots() { // gets the names and path of each save without them taking a fat shit on memory
            foreach (SaveGame saveGame in SaveGames) {
                string[] filePaths = Directory.GetFiles(saveGame.Path);

                if (SaveMethod == SaveType.Backup) {
                    foreach (string path in filePaths) {
                        string name = Path.GetFileName(path);
                        if (name.StartsWith(BackupSaveGame.CurrentSaveIdentifier)) { // load in current save for that directory
                            saveGame.CurSlot = new Slot(name, path, null);
                            Debug.Log($"found current save for savegame {Path.GetFileName(saveGame.Path)}");
                        }
                    }
                    foreach (string path in filePaths) {
                        string name = Path.GetFileName(path);
                        if (name.StartsWith(BackupSaveGame.BackupSaveIdentifier)) { // load in backup save for that directory
                            ((BackupSaveGame) saveGame).BackupSlot = new Slot(name, path, null);
                            Debug.Log($"found backup save for savegame {Path.GetFileName(saveGame.Path)}");
                        }
                    }
                } else if (SaveMethod == SaveType.Rollback) {}
            }
        }

        public void LoadAllSaveGames() {
            Debug.Log("loading all data from save games...");
            foreach (SaveGame saveGame in SaveGames) {
                ((BackupSaveGame) saveGame).Load(DoEncryptSaveData);
            }
        }

        public void DeLoadAllSaveGames() {
            Debug.Log("deloading all data from save games (to save memory)");
            foreach (SaveGame saveGame in SaveGames) {
                ((BackupSaveGame) saveGame).DeLoad();
            }
        }

        public void ShuffleSaveCurrentSaveGame() {
            if (CurrentSaveGame == noSave) { return; }
            if (SaveMethod == SaveType.Backup) {
                ((BackupSaveGame) CurrentSaveGame).ShuffleSave(DoEncryptSaveData);
            }
        }

        public void BasicSaveCurrentSaveGame() {
            if (CurrentSaveGame == noSave) { return; }
            if (SaveMethod == SaveType.Backup) {
                ((BackupSaveGame) CurrentSaveGame).BasicSave(DoEncryptSaveData);
            }
        }

        public void DeleteCurrentSaveGame() {
            if (CurrentSaveGame == noSave) { return; }
            CurrentSaveGame.Delete(); // this save game is deleted in memory...

            SaveGames.Remove(CurrentSaveGame);
            CurrentSaveGame = noSave; // and now it's deleted in game. as long as no one keeps sneaky references to it...

            OnCurrentGameChanged.Invoke();
        }

        public void LoadCurrentSaveGame() {
            if (CurrentSaveGame == noSave) { return; }
            if (SaveMethod == SaveType.Backup) {
                ((BackupSaveGame) CurrentSaveGame).Load(DoEncryptSaveData);
            }
        }

        public void AddSaveGame() {
            string name = $"slot-{SaveGames.Count}-{System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";

            Slot slot = new Slot(name, GenerateSavePathName(name, false), ConstructSaveDataContainer()); // new save very exciting :O
            if (SaveMethod == SaveType.Backup) {
                Slot backupSlot = new Slot(name, GenerateSavePathName(name, true), ConstructSaveDataContainer()); // just a filler save.

                SaveGame savegame = new BackupSaveGame(name, $"{SavesDirectoryPath}\\{name}", slot, backupSlot);
                SaveGames.Add(savegame);

                JsonSaver.CreateDirectory(savegame.Path);
                ((BackupSaveGame) savegame).BasicSave(DoEncryptSaveData); // also creates save game directory in the process
                backupSlot.Save(DoEncryptSaveData); // let's also get that into memory

                // man what fine memories. these two file locations will be shuffling around each other for who knows how many years :D so exciting!

            } else if (SaveMethod == SaveType.Rollback) {}
            if (!LoadAllSavesOnAwake){
                slot.DeLoad(); // not confirmed this is the current, so we can take away its babies. i just got irrumuki whatsherface flashbacks. gahh that show is cursed
            }
            
            OnCurrentGameChanged.Invoke();
        }

        private SaveDataContainer ConstructSaveDataContainer() {
            return new SaveDataContainer(new SaveData("New Save"));
        }
    
        public bool IsIndexInBounds(int index) { return index >= 0 && index < SaveGames.Count; }

        public void SwitchCurrentGame(int index) {
            Debug.Log($"attempt change to {index}");

            if (IsIndexInBounds(index)) {
                // de-recognize old slot
                if (CurrentSlot != null && !LoadAllSavesOnAwake) {
                    CurrentSlot.DeLoad();
                }
                
                // make the new one the is do`
                CurrentIndex = index;
                CurrentSaveGame = SaveGames[index];
                CurrentSlot = CurrentSaveGame.CurSlot;
                LoadCurrentSaveGame(); // realize savedata. the save is only identified at this point.
            } else if (SaveMethod == SaveType.Rollback) {}

            OnCurrentGameChanged.Invoke();
        }

        public string[] GetSaveGameNames() {
            string[] slotNames = new string[SaveGames.Count];
            for (int i=0; i<SaveGames.Count; i++) {
                slotNames[i] = SaveGames[i].Name;
            }
            return slotNames;
        }


        public string GenerateSavePathName(string name, bool isBackup) {
            return $"{SavesDirectoryPath}\\{name}\\{(isBackup ? BackupSaveGame.BackupSaveIdentifier : BackupSaveGame.CurrentSaveIdentifier)}{name}.{(DoEncryptSaveData ? "data" : "json")}";
        }
    }
}
