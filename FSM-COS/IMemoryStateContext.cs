using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheSingularityWorkshop.FSM_API;

namespace FSM_COS
{
    public interface IMemoryStateContext : IStateContext
    {
        static int TypeSize { get; }
        static int TypeHash { get; }
        static string TypeName { get; }

        void Initialize();
    }
}
