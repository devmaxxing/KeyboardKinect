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
        public Rectangle rect { get; set; }
        public KeyReg keyReg { get; set; }
        public List<int> pixelIndices { get; set; }

        public Key(Rectangle r, KeyReg k, List<int> l)
        {
            rect = r;
            keyReg = k;
            pixelIndices = l;
        }
    }
}
