using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyCOMComponent
{
    [ComVisible(true)]
    [Guid("A6C141A9-28AE-4147-B3C0-09D46E0EFF15")]
    public interface IExpress
    {
        string minus(int a, int b);
        string divide(int a, int b);
    }
}
