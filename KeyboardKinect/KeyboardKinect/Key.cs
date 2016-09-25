using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace KeyboardKinect
{
    class Key
    {
        Rectangle rect;
        KeyReg keyReg;
        List<int> pixelIndices;

        public Key(Rectangle r, KeyReg k, List<int> l)
        {
            rect = r;
            keyReg = k;
            pixelIndices = l;
        }
    }
}
