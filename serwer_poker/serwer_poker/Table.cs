using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serwer_poker
{
    class Table
    {
        public List<byte> CommunityCards = new List<byte>();
        public int Pot;

        public void AddCard(byte Card)
        {
            CommunityCards.Add(Card);
        }
    }
}
