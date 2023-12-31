using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Wabubby {
    public class BackupSaveGame : SaveGame {
        [HideInInspector] public static string CurrentSaveIdentifier = "current-save-";
        [HideInInspector] public static string BackupSaveIdentifier = "backup-save-";
        public Slot BackupSlot;

        public BackupSaveGame(string name, string path, Slot curSlot, Slot backupSlot) : base(name, path, curSlot) {
            BackupSlot = backupSlot;
        }

        public void ShuffleSave(bool isEncrypted) {
            // send the backup to narnia:
            string backupNarnia = $"{SaveSystem.OldSavesDirectoryPath}\\{BackupSlot.Name}";
            if (File.Exists(backupNarnia)) { File.Delete(backupNarnia); }
            File.Move(BackupSlot.Path, backupNarnia); // this will be replaced with an ACTUAL deletion later
            // send the old (working) savefile to the backup path
            File.Move(CurSlot.Path, BackupSlot.Path);
            // save the in-game file to the savefile location
            CurSlot.Save(isEncrypted);
        }

        public void BasicSave(bool isEncrypted) {
            // overwrite current save :-)
            CurSlot.Save(isEncrypted);
        }

        public void Load(bool isEncrypted) {
            try {
                CurSlot.Load(isEncrypted); // attempt to initalize save data
            } catch {
                Debug.LogError($"slot at game {Name} failed to load. your backup savefile has been loaded instead. you can find the [UNPLAYABLE] slot in /old-saves. literally unplayable man. eeeeeee");
                LoadBackup(isEncrypted);
            }
        }

        public void LoadBackup(bool isEncrypted) {
            if (BackupSlot == null) {
                Debug.Log("you done fucked man. there are no backups no more. get me a pizariella");
                return;
            }

            File.Move(CurSlot.Path, $"{SaveSystem.OldSavesDirectoryPath}\\{BackupSlot.Name}"); // this will be replaced with an ACTUAL deletion later
            CurSlot = BackupSlot;
            BackupSlot = null;
            Load(isEncrypted);
            
        }

        // put potential mountains of data into the GARBAGEEEE
        public void DeLoad() {
            CurSlot.DeLoad();
        }
    }
}
