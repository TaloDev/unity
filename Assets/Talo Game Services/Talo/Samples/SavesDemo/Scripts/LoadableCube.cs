using System.Collections.Generic;
using UnityEngine;

namespace TaloGameServices.Sample.SavesDemo
{
    public class LoadableCube : Loadable
    {
        private Vector3 originalPos;

        private void Start()
        {
            originalPos = transform.position;
        }

        public void MoveToOriginalPos()
        {
            transform.position = originalPos;
        }

        private void OnMouseDrag()
        {
            var nextPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            nextPos.z = 0;
            transform.position = nextPos;
        }

        public override void RegisterFields()
        {
            RegisterField("x", transform.position.x);
            RegisterField("y", transform.position.y);
            RegisterField("z", transform.position.z);
        }

        public override void OnLoaded(Dictionary<string, object> data)
        {
            if (HandleDestroyed(data)) return;

            transform.position = new Vector3(
                (float)data["x"],
                (float)data["y"],
                (float)data["z"]
            );
        }
    }
}
