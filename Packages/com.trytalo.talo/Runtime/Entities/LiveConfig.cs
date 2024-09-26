using System;
using System.Linq;

namespace TaloGameServices
{
    public class LiveConfig
    {
        private Prop[] props;

        public LiveConfig(Prop[] props)
        {
            this.props = props;
        }

        public T GetProp<T>(string key, T fallback = default(T))
        {
            try
            {
                Prop prop = props.First((prop) => prop.key == key);
                return (T)Convert.ChangeType(prop.value, typeof(T));
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}
