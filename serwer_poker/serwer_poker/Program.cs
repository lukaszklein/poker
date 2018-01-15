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

        static void WhoIsPlaying(List<Player> Players)
        {
            bool AllClear = false;
            while (!AllClear)
            {
                foreach (Player Player in Players)
                {
                    if (!Player.IsPlaying)
                    {
                        Players.Remove(Player);
                        break;
                    }
                    else
                        AllClear = true;
                }
            }
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
                Deck.RemoveAt(IndexOfCard);
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

        //static void DealOnTable(Table Table, List<byte> Deck, int NumberOfCards)
        //{
        //    Table.CommunityCards = DealCards(Deck, NumberOfCards);
        //}

        static void SmallBlind(Player Player, Table Table)
        {
            if (Player.Chips < 50)/*wartości stawek do ustalenia*/
            {
                Table.Pot += Player.Chips;
                Player.Bet = Player.Chips;
                Player.Chips = 0;
            }
            else
            {
                Table.Pot += 50;
                Player.Bet = 50;
                Player.Chips -= 50;
            }
        }

        static void BigBlind(Player Player, Table Table)
        {
            if (Player.Chips < 100)/*wartości stawek do ustalenia*/
            {
                Table.Pot += Player.Chips;
                Player.Bet = Player.Chips;
                Table.Bid = Player.Chips;//jak bigblind
                Player.Chips = 0;
            }
            else
            {
                Table.Pot += 100;
                Player.Bet = 100;
                Player.Chips -= 100;
                Table.Bid = 100;//jak bigblind
            }
        }

        static void Call(Player Player, Table Table)
        {
            int Difference = Table.Bid - Player.Bet;
            Table.Pot += Difference;
            Player.Bet += Difference;
            Player.Chips -= Difference;
        }

        static void Raise(Player Player, Table Table, int Bid)
        {
            Table.Pot += Bid;
            Player.Bet += Bid;
            Table.Bid += Bid;
            Player.Chips -= Bid;
        }

        static void FirstBetting(List<Player> Players, Table Table)
        {
            int IndexOfPlayer;
            if (Players.Count() == 2)
            {
                SmallBlind(Players.ElementAt(0), Table);
                BigBlind(Players.ElementAt(1), Table);
                Players.ElementAt(1).Check = true;
                IndexOfPlayer = 0;
            }
            if (Players.Count() == 3)
            {
                SmallBlind(Players.ElementAt(1), Table);
                BigBlind(Players.ElementAt(2), Table);
                Players.ElementAt(2).Check = true;
                IndexOfPlayer = 0;
            }
            else
            {
                SmallBlind(Players.ElementAt(1), Table);
                BigBlind(Players.ElementAt(2), Table);
                Players.ElementAt(2).Check = true;
                IndexOfPlayer = 3;
            }

            Betting(Players, Table, IndexOfPlayer);
        }

        static void NextBetting(List<Player> Players, Table Table)
        {
            
            int IndexOfPlayer;
            if (Players.Count() <= 3)
            {
                IndexOfPlayer = 0;
            }
            else
            {
                IndexOfPlayer = 3;
            }

            Betting(Players, Table, IndexOfPlayer);
        }

        static void Betting(List<Player> Players, Table Table, int IndexOfPlayer)
        {
            int Decision = 0;
            bool ContinueBetting = true;
            while (ContinueBetting)
            {
                bool Control = true;
                if (!Players.ElementAt(IndexOfPlayer).Fold)
                {
                    Console.WriteLine("tura gracza " + Players.ElementAt(IndexOfPlayer).ID);
                    Console.WriteLine("aktualna stawka: " + Table.Bid + " żetonów");
                    Console.WriteLine("aktualna pula " + Table.Pot + " żetonów");
                    Console.WriteLine("do tej pory dałeś " + Players.ElementAt(IndexOfPlayer).Bet + " żetonów");
                    Console.WriteLine("masz aktualnie " + Players.ElementAt(IndexOfPlayer).Chips + " żetonów");
                    Console.WriteLine("Stan graczy:");
                    foreach (var Player in Players)
                    {
                        Console.WriteLine("Gracz " + Player.ID);
                        Console.WriteLine("Fold:" + Player.Fold);
                        Console.WriteLine("Check:" + Player.Check);
                    }
                    /*decyzja gracza zapisana do zmiennej
                     przesyłana jako int:
                     -1 - fold, 0 - call/check, wartość_int - raise, max_int - all in*/
                    Console.WriteLine("podejmij decyzję");
                    string decyzja = Console.ReadLine();

                    Decision = int.Parse(decyzja);
                    Console.WriteLine("podjęto decyzję " + Decision);
                    switch (Decision)
                    {
                        case -1:/*fold*/
                            {
                                Console.WriteLine("fold");
                                Players.ElementAt(IndexOfPlayer).Fold = true;
                                break;
                            }
                        case 0:/*call/check*/
                            {
                                if (Table.Bid == Players.ElementAt(IndexOfPlayer).Bet)
                                {
                                    Console.WriteLine("check");
                                    Players.ElementAt(IndexOfPlayer).Check = true;
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("call");
                                    Call(Players.ElementAt(IndexOfPlayer), Table);
                                    break;
                                }
                            }
                        case 5000:/*all in*/
                            {
                                Console.WriteLine("all in");
                                Call(Players.ElementAt(IndexOfPlayer), Table);
                                Raise(Players.ElementAt(IndexOfPlayer), Table, Players.ElementAt(IndexOfPlayer).Chips);
                                foreach (Player Player in Players)
                                {
                                    if (!Player.Fold)
                                    {
                                        Player.Check = false;
                                    }
                                }
                                break;
                            }
                        default:/*call+raise*/
                            {
                                Console.WriteLine("raise");
                                Call(Players.ElementAt(IndexOfPlayer), Table);
                                Raise(Players.ElementAt(IndexOfPlayer), Table, Decision);
                                foreach (Player Player in Players)
                                {
                                    if (!Player.Fold)
                                    {
                                        Player.Check = false;
                                    }
                                }
                                break;
                            }
                    }

                }

                if (IndexOfPlayer == Players.Count() - 1)
                {
                    IndexOfPlayer = 0;
                }
                else
                {
                    IndexOfPlayer++;
                }


                foreach (Player Player in Players)
                {
                    if (!Player.Fold)
                    {
                        Control &= Player.Check;
                    }
                }

                ContinueBetting = !Control;
                Console.WriteLine("zmienna continuebetting: " + ContinueBetting);
            }

            foreach (Player Player in Players)
            {
                if (!Player.Fold)
                {
                    Player.Check = false;
                }
            }
        }

            static void Main(string[] args)
        {
            List<byte> DeckTemplate = new List<byte>();
            Player Player1 = new Player { ID = 1, Chips = 1000, IsPlaying = true, Fold = false, Check = false, Bet = 0 };
            Player Player2 = new Player { ID = 2, Chips = 1000, IsPlaying = true, Fold = false, Check = false, Bet = 0 };
            Player Player3 = new Player { ID = 3, Chips = 1000, IsPlaying = false, Fold = false, Check = false, Bet = 0 };
            Player Player4 = new Player { ID = 4, Chips = 1000, IsPlaying = true, Fold = false, Check = false, Bet = 0 };
            List<Player> AllPlayers = new List<Player>(){ Player1, Player2, Player3, Player4 };
            foreach (Player Player in AllPlayers)
            {
                Console.WriteLine("Gracz " + Player.ID + " gotowy");
            }
            WhoIsPlaying(AllPlayers);
            foreach (Player Player in AllPlayers)
            {
                Console.WriteLine("Gracz " + Player.ID + " gra");
            }
            Table Table = new Table { Pot = 0, Bid = 0 };
            DeckTemplate = CreateDeck();
            List<byte> DeckToPlay = DeckTemplate;//Przypisanie talii do nowej zmiennej, która będzie modyfikowana
            Console.WriteLine("Kart w talii: " + DeckToPlay.Count());
            Console.WriteLine("Rozdanie kart graczom");
            FirstDeal(AllPlayers, DeckToPlay);
            Console.WriteLine("Kart w talii: " + DeckToPlay.Count());
            foreach (Player Player in AllPlayers)
            {
                Player.ShowCards();
            }
            Console.WriteLine("Licytacja 1");
            FirstBetting(AllPlayers, Table);
            Console.WriteLine("Rozdanie kart na stół");
            DealCards(DeckToPlay, Table, 3);
            Console.WriteLine("Kart w talii: " + DeckToPlay.Count());
            Table.ShowCards();
            Console.WriteLine("Licytacja 2");
            NextBetting(AllPlayers, Table);
            Console.WriteLine("Rozdanie kart na stół");
            DealCards(DeckToPlay, Table, 1);
            Console.WriteLine("Kart w talii: " + DeckToPlay.Count());
            Table.ShowCards();
            Console.WriteLine("Licytacja 3");
            NextBetting(AllPlayers, Table);
            Console.WriteLine("Rozdanie kart na stół");
            DealCards(DeckToPlay, Table, 1);
            Console.WriteLine("Kart w talii: " + DeckToPlay.Count());
            Table.ShowCards();
            Console.WriteLine("Licytacja 4");
            NextBetting(AllPlayers, Table);
            Console.ReadKey();
        }
    }
}
