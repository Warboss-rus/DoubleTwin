using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleTwin
{
    interface PlayerInterface
    {
        sTableCard MakeMove(Table table);
        bool IsBot();
		List<sCard> GetCards();
    }
}
