using System.Collections.Generic;
using UnityEngine;
using TaloGameServices;

public class LoadableCube : Loadable
{
    public override void RegisterFields()
    {
        RegisterField("x", transform.position.x);
        RegisterField("y", transform.position.y);
        RegisterField("z", transform.position.z);

        RegisterField("r.x", transform.rotation.x);
        RegisterField("r.y", transform.rotation.y);
        RegisterField("r.z", transform.rotation.z);

        RegisterField("s.x", transform.localScale.x);
        RegisterField("s.y", transform.localScale.y);
        RegisterField("s.z", transform.localScale.z);
    }

    public override void OnLoaded(Dictionary<string, object> data)
    {
        if (HandleDestroyed(data)) return;

        transform.position = new Vector3(
            (float)data["x"],
            (float)data["y"],
            (float)data["z"]
        );

        transform.rotation = Quaternion.Euler(
            (float)data["r.x"],
            (float)data["r.y"],
            (float)data["r.z"]
        );

        transform.localScale = new Vector3(
            (float)data["s.x"],
            (float)data["s.y"],
            (float)data["s.z"]
        );
    }
}
