using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serwer_poker
{
    class Program
    {
        /*Tworzenie talii złożonej z 52 kart do wykorzystania przy rozgrywce.
         * Karty są zapisywane jako zmienne byte i opisywane w następujący sposób:
         * - 4 najmniej znaczące bity odpowiadają za wartość karty według starszeństwa kart w Texas Hold'em:
         *      0010 - 2, 0011 - 3, 0100 - 4, 0101 - 5, 0110 - 6, 0111 - 7, 1000 - 8, 1001 - 9, 1010 - 10,
         *      1011 - walet(11), 1100 - dama(12), 1101 - król(13), 1110 - as(14)
         * - piąty i szósty bit odpowiadają za kolor karty:
         *      00 - kier(0), 01 - pik(16), 10 - karo(32), 11 - trefl(48)
         * - siódmy i ósmy bit są niewykorzystane
         * Przykład:
         * 6 kier - 00000110, as karo - 00101110
         Przy tworzeniu talii stosowane są liczby w systemie dziesiętnym*/
        static List<byte> CreateDeck()
        {
            byte Card;
            List<byte> TempDeck = new List<byte>();
            for (int i = 0; i <= 48; i+=16)
            {
                for (int j = 2; j <= 14; j++)
                {
                    Card = (byte)(i + j);
                    TempDeck.Add(Card);
                }
            }
            return TempDeck;
        }

        /*Rozdawanie NumberOfCards kart z talii Deck graczowi Player*/
        static void DealCards(List<byte> Deck, Player Player, int NumberOfCards)
        {
            Random RandomNumber = new Random();
            int IndexOfCard;
            byte Card;
            for (int i = 0; i < NumberOfCards; i++)
            {
                IndexOfCard = RandomNumber.Next(0, Deck.Count - 1);
                Card = Deck.ElementAt(IndexOfCard);
                Deck.RemoveAt(IndexOfCard);
                Player.AddCard(Card);
            }
        }

        /*Przeciążenie metody DealCards do rozdania kart wspólnych*/
        static void DealCards(List<byte> Deck, Table Table, int NumberOfCards)
        {
            Random RandomNumber = new Random();
            int IndexOfCard;
            byte Card;
            for (int i = 0; i < NumberOfCards; i++)
            {
                IndexOfCard = RandomNumber.Next(0, Deck.Count - 1);
                Card = Deck.ElementAt(IndexOfCard);
                Table.AddCard(Card);
            }
        }

        /*Zmiana kolejności graczy po rundzie*/
        static void OrderPlayers(List<Player> ListOfPlayers)
        {
            Player TempPlayer = new Player();
            TempPlayer = ListOfPlayers.ElementAt(0);
            int Index = 1;
            while(Index <= 3)
            {
                ListOfPlayers.Insert(Index - 1, ListOfPlayers.ElementAt(Index));
            }
            ListOfPlayers.Insert(Index, TempPlayer);
        }

        /*Pierwsze rozdanie kart*/
        static void FirstDeal(List<Player> Players, List<byte> Deck)
        {
            foreach (Player Player in Players)
            {
                if (Player.IsPlaying)
                {
                    DealCards(Deck, Player, 1);
                }
            }
            foreach (Player Player in Players)
            {
                if (Player.IsPlaying)
                {
                    DealCards(Deck, Player, 1);
                }
            }
        }

        static void Main(string[] args)
        {
            List<byte> DeckTemplate = new List<byte>();
            Player Player1 = new Player { Chips = 1000, IsPlaying = true, ID=1 };
            Player Player2 = new Player { Chips = 1000, IsPlaying = true, ID=2 };
            Player Player3 = new Player { Chips = 1000, IsPlaying = true, ID=3 };
            Player Player4 = new Player { Chips = 1000, IsPlaying = true, ID=4 };
            List<Player> AllPlayers = new List<Player>(){ Player1, Player2, Player3, Player4 };
            Table Table = new Table();
            DeckTemplate = CreateDeck();
            List<byte> DeckToPlay = DeckTemplate;//Przypisanie talii do nowej zmiennej, która będzie modyfikowana
            FirstDeal(AllPlayers, DeckToPlay);
            Console.WriteLine(DeckToPlay.Count);
            Console.ReadKey();
            foreach (var Player in AllPlayers)
            {
                Console.WriteLine("Ręka gracza nr " + Player.ID);
                foreach (var item in Player.Hand)
                {
                    Console.WriteLine("Karta: " + item);
                }
            }
            Console.ReadKey();
        }
    }
}
