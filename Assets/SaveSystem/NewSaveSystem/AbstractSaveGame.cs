using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractSaveGame {
    public string Name;
    private AbstractSaveEncoder Encoder;
    public SaveData SaveData;

    public AbstractSaveGame(string name, SaveData saveData) {
        Name = name;
        // Encoder = new ConcreteSaveEncoder(this);
    }

    public virtual void Save() {
        Encoder.Save(SaveData);
    }

    public virtual void Load() {
        SaveData = Encoder.Load();
    }

    public virtual void Delete() {
        Encoder.Delete();
    }
    
}
