namespace TaloGameServices
{
    public struct LoadableData
    {
        public readonly string id;
        public readonly Loadable loadable;
        public readonly string name;

        public LoadableData(Loadable loadable)
        {
            id = loadable.Id;
            this.loadable = loadable;

            var go = loadable.gameObject;
            name = go.name;
            while (go.transform.parent != null)
            {
                go = go.transform.parent.gameObject;
                name = $"{go.name}.{name}";
            }
        }
    }
}
