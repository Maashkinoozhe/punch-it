using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PIModel
{
    public interface IModelManager<T> where T : PersistentObjectBase
    {
        T Load(string path, string file);
        bool Save(string path, string file, T data);
    }
}
