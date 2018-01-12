using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serwer_poker
{
    class Player
    {
        public List<byte> Hand = new List<byte>();
        public int ID;
        public int Chips;
        public bool IsPlaying;
        public bool Fold;

        public void AddCard(byte Card)
        {
            Hand.Add(Card);
        }
    }
}
