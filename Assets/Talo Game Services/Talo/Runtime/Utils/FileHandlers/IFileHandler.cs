using System;

namespace TaloGameServices
{
    internal interface IFileHandler<T>
    {
        public T ReadContent(string path)
        {
            throw new NotImplementedException();
        }

        public void WriteContent(string path, T content)
        {
            throw new NotImplementedException();
        }
    }
}
