using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

public abstract class AbstractSaveEncoder {
    
    public AbstractSaveGame SaveGame;
    public string Path;

    public AbstractSaveEncoder(AbstractSaveGame saveGame) {
        SaveGame = saveGame;
        Path = EncodingConstants.AppPath + $"/{saveGame.Name}";
    }

    // loads from filepath
    public SaveData Load() {
        return JsonEncoder.Load(Path).SaveData;
    }

    // saves to filepath
    public void Save(SaveData saveData) {
        JsonEncoder.Save(new SaveDataContainer(saveData), Path);
    }

    // deletes savegame path
    public void Delete() {
        Directory.Move(Path, $"{EncodingConstants.TrashPath}/{System.IO.Path.GetFileName(Path)}");
    }

}
