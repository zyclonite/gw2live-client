using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gw2mapClient.Controller
{
    public interface IMessager
    {
        void DispatchMessage(string message);
    }
}
