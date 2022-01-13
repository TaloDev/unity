using System;
using UnityEngine;
using TaloGameServices;
using System.Collections.Generic;

public class Loadable : MonoBehaviour, ILoadable
{
    public string id = Guid.NewGuid().ToString();
    public Dictionary<string, object> savedFields = new Dictionary<string, object>();

    protected virtual void OnEnable()
    {
        Talo.Saves.Register(this);
        Talo.Saves.OnSaveChosen += LoadData;
    }

    protected virtual void OnDisable()
    {
        Talo.Saves.OnSaveChosen -= LoadData;
    }

    private void LoadData(GameSave save)
    {
        OnLoaded(Talo.Saves.LoadObject(save, id));
    }

    public virtual void RegisterFields()
    {
        throw new NotImplementedException();
    }

    public virtual void OnLoaded(Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }
}
