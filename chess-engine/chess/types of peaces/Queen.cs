using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess.types_of_peaces
{
    class Queen : Peace
    {
        public Queen(Byte iswhitee, int position) : base(iswhitee, position)
        {
            this.type = 4;
        }
    }
}
