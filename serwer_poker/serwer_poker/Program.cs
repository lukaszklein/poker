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
        static List<byte> DealCards(List<byte> Deck, int NumberOfCards)
        {
            Random RandomNumber = new Random();
            List<byte> DealtCards = new List<byte>();
            int IndexOfCard;
            byte Card;
            for (int i = 0; i < NumberOfCards; i++)
            {
                IndexOfCard = RandomNumber.Next(0, Deck.Count - 1);
                Card = Deck.ElementAt(IndexOfCard);
                DealtCards.Add(Card);
                Deck.RemoveAt(IndexOfCard);
            }
            return DealtCards;
        }

        static void WhoIsPlaying(List<Player> Players)
        {
            foreach (Player Player in Players)
            {
                if(!Player.IsPlaying)
                {
                    Players.Remove(Player);
                }
            }
        }

        ///*Przeciążenie metody DealCards do rozdania kart wspólnych*/
        //static void DealCards(List<byte> Deck, Table Table, int NumberOfCards)
        //{
        //    Random RandomNumber = new Random();          
        //    int IndexOfCard;
        //    byte Card;
        //    for (int i = 0; i < NumberOfCards; i++)
        //    {
        //        IndexOfCard = RandomNumber.Next(0, Deck.Count - 1);
        //        Card = Deck.ElementAt(IndexOfCard);
        //        Table.AddCard(Card);
        //    }
        //}

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
            int IndexofCard = 0;
            List<byte> TempDeck = DealCards(Deck, Players.Count()*2);
            foreach (Player Player in Players)
            {
                Player.AddCard(TempDeck.ElementAt(IndexofCard));
                IndexofCard++;
            }
            foreach (Player Player in Players)
            {
                Player.AddCard(TempDeck.ElementAt(IndexofCard));
                IndexofCard++;
            }
            TempDeck.Clear();
        }

        static void SmallBlind(Player Player, Table Table)
        {
            if (Player.Chips < 50)/*wartości stawek do ustalenia*/
            {
                Table.Pot += Player.Chips;
                Player.Chips = 0;
            }
            else
            {
                Table.Pot += 50;
                Player.Chips -= 50;
            }
        }

        static void BigBlind(Player Player, Table Table)
        {
            if (Player.Chips < 100)/*wartości stawek do ustalenia*/
            {
                Table.Pot += Player.Chips;
                Player.Chips = 0;
            }
            else
            {
                Table.Pot += 100;
                Player.Chips -= 100;
            }
        }

        static void FirstBetting(List<Player> Players, Table Table)
        {
            if(Players.Count() <= 2)
            {
                SmallBlind(Players.ElementAt(0), Table);
                BigBlind(Players.ElementAt(1), Table);
            }
            else
            {
                SmallBlind(Players.ElementAt(1), Table);
                BigBlind(Players.ElementAt(2), Table);
            }

            
        }

        static void Main(string[] args)
        {
            List<byte> DeckTemplate = new List<byte>();
            Player Player1 = new Player { Chips = 1000, IsPlaying = true, ID=1, Fold = false };
            Player Player2 = new Player { Chips = 1000, IsPlaying = true, ID=2, Fold = false };
            Player Player3 = new Player { Chips = 1000, IsPlaying = true, ID=3, Fold = false };
            Player Player4 = new Player { Chips = 1000, IsPlaying = true, ID=4, Fold = false };
            List<Player> AllPlayers = new List<Player>(){ Player1, Player2, Player3, Player4 };
            WhoIsPlaying(AllPlayers);
            Table Table = new Table { Pot = 0 };
            DeckTemplate = CreateDeck();
            List<byte> DeckToPlay = DeckTemplate;//Przypisanie talii do nowej zmiennej, która będzie modyfikowana
            FirstDeal(AllPlayers, DeckToPlay);
            FirstBetting(AllPlayers, Table);
            Console.ReadKey();
        }
    }
}
