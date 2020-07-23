using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipManagerMultiLanguage.Models
{
    public class Ship
    {
        public int X, Y;
        public int Size { get; private set; }
        public Direction Dir;

        public Ship(int size)
        {
            Size = size;
        }
    }
}
