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

        /*Usuwanie z listy graczy nieaktywnych użytkowników*/
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
                    {
                        AllClear = true;
                    }
                }
            }
        }

        /*Rozdawanie NumberOfCards kart z talii Deck*/
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

        /*Pierwsza licytacja obejmująca Blindy*/
        static void FirstBetting(List<Player> Players, Table Table)
        {
            /*Ustalenie rozpoczynającego gracza i przydzielenie blindów*/
            int IndexOfPlayer;
            if (Players.Count() == 2)
            {
                SmallBlind(Players.ElementAt(0), Table);
                BigBlind(Players.ElementAt(1), Table);
                IndexOfPlayer = 0;
            }
            else if (Players.Count() == 3)
            {
                SmallBlind(Players.ElementAt(1), Table);
                BigBlind(Players.ElementAt(2), Table);
                IndexOfPlayer = 0;
            }
            else
            {
                SmallBlind(Players.ElementAt(1), Table);
                BigBlind(Players.ElementAt(2), Table);
                IndexOfPlayer = 3;
            }

            Betting(Players, Table, IndexOfPlayer);
        }

        /*Każda następna licytacja*/
        static void NextBetting(List<Player> Players, Table Table)
        {
            /*Ustalenie rozpoczynającego gracza*/
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

        /*Metoda licytacji*/
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
                        case 5000:/*all in - wartość do edycji*/
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

                /*Ustalenie następnego gracza do licytacji*/
                if (IndexOfPlayer == Players.Count() - 1)
                {
                    IndexOfPlayer = 0;
                }
                else
                {
                    IndexOfPlayer++;
                }

                /*Sprawdzenie czy niefoldujący gracze dokonali checka*/
                foreach (Player Player in Players)
                {
                    if (!Player.Fold)
                    {
                        Control &= Player.Check;
                    }
                }

                /*Jeśli wszyscy schekowali, to następuje wyjście z licytacji*/
                ContinueBetting = !Control;
                Console.WriteLine("zmienna continuebetting: " + ContinueBetting);
            }

            /*Przygotowanie do następnej rundy*/
            foreach (Player Player in Players)
            {
                if (!Player.Fold)
                {
                    Player.Check = false;
                }
            }
        }

        /*Usunięcie z ręki powtarzających się wartości kart na potrzeby ewaluacji*/
        static List<byte> DistinctHand(List<byte> Deck)
        {
            List<byte> DistinctDeck = new List<byte>();
            byte Card;
            for (int Index = 0; Index <= Deck.Count() - 2; Index++)
            {
                if (Deck.ElementAt(Index) != Deck.ElementAt(Index + 1))
                {
                    Card = Deck.ElementAt(Index);
                    DistinctDeck.Add(Card);
                }
            }
            Card = Deck.ElementAt(6);
            if (!DistinctDeck.Contains(Card))
            {
                DistinctDeck.Add(Card);
            }

            return DistinctDeck;
        }

        /*Określenie wartości ręki każdego gracza*/
        static void Evaluate(List<Player> Players, List<byte> TableCards)
        {
            foreach (Player Player in Players)
            {
                if (!Player.Fold)
                {
                    List<byte> WholeHand = Player.Hand;
                    WholeHand.AddRange(TableCards);

                    byte ValueCard;
                    List<byte> ValueHand = new List<byte>();
                    foreach (byte Card in WholeHand)
                    {
                        ValueCard = (byte)(Card % 16);
                        ValueHand.Add(ValueCard);
                    }

                    ValueHand.Sort();
                    int HowMany;

                    /*Sprawdzanie ręki pod kątem występowania par, trójek, karet oraz dwóch par i fula*/
                    uint Pair1 = 0;
                    uint Pair2 = 0;
                    uint Three1 = 0;
                    uint Quad1 = 0;
                    uint Value = 0;
                    bool HighCard = false;
                    bool Pair = false;
                    bool Three = false;
                    bool Quad = false;

                    for (byte Index = 2; Index <= 14; Index++)
                    {
                        if (ValueHand.LastIndexOf(Index) == -1)
                        {
                            HowMany = 0;
                        }
                        else
                        {
                            HowMany = ValueHand.LastIndexOf(Index) - ValueHand.IndexOf(Index) + 1;
                        }
                        switch (HowMany)
                        {
                            case 0:
                                {
                                    Console.WriteLine("brak karty: " + Index);
                                    break;
                                }
                            case 1:
                                {
                                    Console.WriteLine("pojedyncza karta: " + Index);
                                    if (Index > Player.HighCard)
                                    {
                                        Player.HighCard = Index;
                                    }
                                    HighCard = true;
                                    break;
                                }
                            case 2:
                                {
                                    Console.WriteLine("para: " + Index);
                                    Value = (uint)Index * 1000;
                                    if (Value > Pair2)
                                    {
                                        Pair1 = Pair2 / 100;
                                        Pair2 = Value;
                                    }
                                    Pair = true;
                                    break;
                                }
                            case 3:
                                {
                                    Console.WriteLine("trojka: " + Index);
                                    Value = (uint)Index * 100000;
                                    if (Value > Three1)
                                    {
                                        Three1 = Value;
                                    }
                                    Three = true;
                                    break;
                                }
                            case 4:
                                {
                                    Console.WriteLine("kareta: " + Index);
                                    Value = (uint)Index * 100000000;
                                    if (Value > Quad1)
                                    {
                                        Quad1 = Value;
                                    }
                                    Quad = true;
                                    break;
                                }
                        }
                    }

                    /*Sprawdzanie ręki pod kątem występowania stritów*/
                    List<byte> DistinctPlayerHand = DistinctHand(ValueHand);/*usunięcie z ręki duplikatów*/
                    bool Straight = false;
                    uint Straight1 = 0;
                    int DifferenceOne = 0;

                    if (!Quad && DistinctPlayerHand.Count() >= 5)
                    {
                        if (DistinctPlayerHand.Contains(14))
                        {
                            Straight = true;
                            for (byte Index = 2; Index < 6; Index++)
                            {
                                if (DistinctPlayerHand.Contains(Index))
                                {
                                    Straight &= true;
                                }
                                else
                                {
                                    Straight = false;
                                }
                            }
                            if (Straight)
                            {
                                Straight1 = DistinctPlayerHand.ElementAt(3);
                                Straight1 *= 1000000;
                                Console.WriteLine("jest taki strit");
                            }
                            else Console.WriteLine("nie ma takiego strita");
                        }
                        Console.WriteLine("sprawdzam strita w pierwszych pięciu kartach");
                        for (byte Index = 0; Index < 4; Index++)
                        {
                            if (DistinctPlayerHand.ElementAt(Index + 1) - DistinctPlayerHand.ElementAt(Index) == 1)
                            {
                                DifferenceOne++;
                            }
                        }
                        if (DifferenceOne >= 4)
                        {
                            Straight = true;
                            Straight1 = DistinctPlayerHand.ElementAt(4);
                            Straight1 *= 1000000;
                            Console.WriteLine("jest taki strit");
                        }
                        else
                        {
                            Straight = false;
                            Console.WriteLine("nie ma takiego strita");
                        }
                        DifferenceOne = 0;

                        if (DistinctPlayerHand.Count() >= 6)
                        {
                            Console.WriteLine("co najmniej 6 kart");
                            Console.WriteLine("sprawdzam strita od 2 do szóstej karty");
                            for (byte Index = 1; Index < 5; Index++)
                            {
                                if (DistinctPlayerHand.ElementAt(Index + 1) - DistinctPlayerHand.ElementAt(Index) == 1)
                                {
                                    DifferenceOne++;
                                }
                            }

                            if (DifferenceOne >= 4)
                            {
                                Straight = true;
                                Straight1 = DistinctPlayerHand.ElementAt(5);
                                Straight1 *= 1000000;
                                Console.WriteLine("jest taki strit");
                            }
                            else
                            {
                                Straight = false;
                                Console.WriteLine("nie ma takiego strita");
                            }
                            DifferenceOne = 0;

                            if (DistinctPlayerHand.Count() == 7)
                            {
                                Console.WriteLine("jest 7 kart");
                                Console.WriteLine("sprawdzam strita od 3 do siódmej karty");
                                for (byte Index = 2; Index < 6; Index++)
                                {
                                    if (DistinctPlayerHand.ElementAt(Index + 1) - DistinctPlayerHand.ElementAt(Index) == 1)
                                    {
                                        DifferenceOne++;
                                    }
                                }

                                if (DifferenceOne >= 4)
                                {
                                    Straight = true;
                                    Straight1 = DistinctPlayerHand.ElementAt(6);
                                    Straight1 *= 1000000;
                                    Console.WriteLine("jest taki strit");
                                }
                                else
                                {
                                    Straight = false;
                                    Console.WriteLine("nie ma takiego strita");
                                }
                                DifferenceOne = 0;
                            }
                        }
                    }
                    Console.WriteLine("wartość straight1: " + Straight1);
                    /*koniec strita*/

                    /*kolor*/
                    bool Color = false;
                    List<byte> Hearts = new List<byte>();
                    List<byte> Spades = new List<byte>();
                    List<byte> Diamonds = new List<byte>();
                    List<byte> Clubs = new List<byte>();
                    List<byte> ColorHand = new List<byte>();
                    uint Color1 = 15000000;
                    WholeHand.Sort();

                    foreach (byte Card in WholeHand)
                    {
                        if (Card < 16)
                        {
                            Hearts.Add(Card);
                        }
                        else if (16 <= Card && Card < 32)
                        {
                            Spades.Add(Card);
                        }
                        else if (32 <= Card && Card < 48)
                        {
                            Diamonds.Add(Card);
                        }
                        else if (48 <= Card)
                        {
                            Clubs.Add(Card);
                        }
                    }
                    if (Hearts.Count() >= 5)
                    {
                        byte ColorCard;
                        Color = true;
                        foreach (byte Card in Hearts)
                        {
                            ColorCard = (byte)(Card % 16);
                            ColorHand.Add(ColorCard);
                        }
                        ColorHand.Sort();
                    }
                    else if (Spades.Count >= 5)
                    {
                        byte ColorCard;
                        Color = true;
                        foreach (byte Card in Spades)
                        {
                            ColorCard = (byte)(Card % 16);
                            ColorHand.Add(ColorCard);
                        }
                        ColorHand.Sort();
                    }
                    else if (Diamonds.Count >= 5)
                    {
                        byte ColorCard;
                        Color = true;
                        foreach (byte Card in Diamonds)
                        {
                            ColorCard = (byte)(Card % 16);
                            ColorHand.Add(ColorCard);
                        }
                        ColorHand.Sort();
                    }
                    else if (Clubs.Count >= 5)
                    {
                        byte ColorCard;
                        Color = true;
                        foreach (byte Card in Clubs)
                        {
                            ColorCard = (byte)(Card % 16);
                            ColorHand.Add(ColorCard);
                        }
                        ColorHand.Sort();
                    }

                    /*sprawdzanie pokera (strit + kolor)*/
                    bool StraightColor = false;
                    int DifferenceOneColor = 0;
                    uint StraightColor1 = 0;
                    if (Color)
                    {
                        for (byte Index = 0; Index < 4; Index++)
                        {
                            if (ColorHand.ElementAt(Index + 1) - ColorHand.ElementAt(Index) == 1)
                            {
                                DifferenceOneColor++;
                            }
                        }
                        if (DifferenceOne >= 4)
                        {
                            StraightColor = true;
                            StraightColor1 = ColorHand.ElementAt(4);
                            StraightColor1 *= 1000000;
                        }
                        else
                        {
                            StraightColor = false;
                        }
                        DifferenceOneColor = 0;

                        if (DistinctPlayerHand.Contains(14))
                        {
                            StraightColor = true;
                            for (byte Index = 2; Index < 6; Index++)
                            {
                                if (ColorHand.Contains(Index))
                                {
                                    StraightColor &= true;
                                }
                                else
                                {
                                    StraightColor = false;
                                }
                                if (StraightColor)
                                {
                                    StraightColor1 = DistinctPlayerHand.ElementAt(3);
                                    StraightColor1 *= 1000000;
                                }
                            }
                        }

                        if (ColorHand.Count() >= 6)
                        {
                            for (byte Index = 1; Index < 5; Index++)
                            {
                                if (ColorHand.ElementAt(Index + 1) - ColorHand.ElementAt(Index) == 1)
                                {
                                    DifferenceOneColor++;
                                }
                            }

                            if (DifferenceOneColor >= 4)
                            {
                                StraightColor = true;
                                StraightColor1 = ColorHand.ElementAt(5);
                                StraightColor1 *= 1000000;
                            }
                            else
                            {
                                StraightColor = false;
                            }
                            DifferenceOneColor = 0;

                            if (ColorHand.Count() == 7)
                            {
                                for (byte Index = 2; Index < 6; Index++)
                                {
                                    if (ColorHand.ElementAt(Index + 1) - ColorHand.ElementAt(Index) == 1)
                                    {
                                        DifferenceOneColor++;
                                    }
                                }

                                if (DifferenceOneColor >= 4)
                                {
                                    StraightColor = true;
                                    StraightColor1 = ColorHand.ElementAt(6);
                                    StraightColor1 *= 1000000;
                                }
                                else
                                {
                                    StraightColor = false;
                                }
                                DifferenceOneColor = 0;
                            }
                        }
                    }

                    /*Przyznawanie graczowi wartości ręki*/
                    if (HighCard && !Pair && !Three && !Quad && !Color && !Straight && !StraightColor)
                    {
                        Player.ValueOfHand = Player.HighCard;
                    }
                    else if (Pair1 == 0 && Pair2 > 0 && !Three && !Quad && !Color && !Straight && !StraightColor)
                    {
                        Player.ValueOfHand = Pair2;
                    }
                    else if (Pair1 > 0 && Pair2 > 0 && !Three && !Quad && !Color && !Straight && !StraightColor)
                    {
                        Player.ValueOfHand = Pair1 + Pair2;
                    }
                    else if (Three && !Pair && !Quad && !Color && !Straight && !StraightColor)
                    {
                        Player.ValueOfHand = Three1;
                    }
                    else if (Straight && !Quad && !Color && !StraightColor)
                    {
                        Player.ValueOfHand = Straight1;
                    }
                    else if (Color && !Quad && !StraightColor)
                    {
                        Player.ValueOfHand = Color1;
                    }
                    else if (Three && Pair && !Quad && !StraightColor)
                    {
                        Player.ValueOfHand = (Three1 + Pair2) * 100;
                    }
                    else if (Quad && !StraightColor)
                    {
                        Player.ValueOfHand = Quad1;
                    }
                    else if (StraightColor && Color)
                    {
                        Player.ValueOfHand = (StraightColor1 + Color1) * 100;
                    }
                }
                Console.WriteLine("Gracz: " + Player.ID + " ma rękę wartą " + Player.ValueOfHand + " punktów. Najwyższa karta: " + Player.HighCard);
                Console.ReadKey();
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
            Console.WriteLine("Rozdanie kart graczom");
            FirstDeal(AllPlayers, DeckToPlay);
            foreach (Player Player in AllPlayers)
            {
                Player.ShowCards();
            }
            List<byte> CardsOnTable = DealCards(DeckToPlay, 5);
            Console.WriteLine("Licytacja 1");
            FirstBetting(AllPlayers, Table);
            Console.WriteLine("Pierwsze rozdanie kart na stół");
            Table.Deal(CardsOnTable, 3);
            Table.ShowCards();
            Console.WriteLine("Licytacja 2");
            NextBetting(AllPlayers, Table);
            Console.WriteLine("Drugie rozdanie kart na stół");
            Table.Deal(CardsOnTable, 1);
            Table.ShowCards();
            Console.WriteLine("Licytacja 3");
            NextBetting(AllPlayers, Table);
            Console.WriteLine("Trzecie i ostatnieozdanie kart na stół");
            Table.Deal(CardsOnTable, 1);
            Console.WriteLine("Kart w talii: " + DeckToPlay.Count());
            Table.ShowCards();
            Console.WriteLine("Licytacja 4");
            NextBetting(AllPlayers, Table);
            Console.ReadKey();
            Evaluate(AllPlayers, Table.CommunityCards);
            Console.ReadKey();
        }
    }
}
