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
        public int Chips;
        public bool IsPlaying;
        public bool Fold;
        public bool Check;
        public int Bet;
        public int ID;

        public void AddCard(byte Card)
        {
            Hand.Add(Card);
        }

        public void ShowCards()
        {
            Console.WriteLine("Ręka gracza " + ID);
            foreach (var Card in Hand)
            {
                    Console.WriteLine(Card);
            }
        }
    }
}
