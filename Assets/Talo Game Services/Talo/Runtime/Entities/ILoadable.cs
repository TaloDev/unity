using System.Collections.Generic;

namespace TaloGameServices
{
    public interface ILoadable
    {
        void RegisterFields();

        void OnLoaded(Dictionary<string, object> data);
    }
}
