using System;
using System.Linq;

namespace TaloGameServices
{
    public class EntityWithProps
    {
        public Prop[] props;

        public string GetProp(string key, string fallback = null)
        {
            Prop prop = props.FirstOrDefault((prop) => prop.key == key && prop.value != null);
            return prop?.value ?? fallback;
        }

        public void SetProp(string key, string value)
        {
            if (GetProp(key) != null)
            {
                props = props.Select((prop) =>
                {
                    if (prop.key == key) prop.value = value;
                    return prop;
                }).ToArray();
            }
            else
            {
                var propList = props.ToList();
                propList.Add(new Prop((key, value)));
                props = propList.ToArray();
            }
        }

        public void DeleteProp(string key)
        {
            Prop prop = props.FirstOrDefault((prop) => prop.key == key);
            if (prop == null) throw new Exception($"Prop with key {key} does not exist");

            prop.value = null;
        }
    }
}