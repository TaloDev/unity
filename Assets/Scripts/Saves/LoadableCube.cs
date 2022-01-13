using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadableCube : Loadable
{
    public override void RegisterFields()
    {
        savedFields.Add("x", transform.position.x);
        savedFields.Add("y", transform.position.y);
        savedFields.Add("z", transform.position.z);

        savedFields.Add("r.x", transform.rotation.x);
        savedFields.Add("r.y", transform.rotation.y);
        savedFields.Add("r.z", transform.rotation.z);

        savedFields.Add("s.x", transform.localScale.x);
        savedFields.Add("s.y", transform.localScale.y);
        savedFields.Add("s.z", transform.localScale.z);
    }

    public override void OnLoaded(Dictionary<string, object> data)
    {
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
