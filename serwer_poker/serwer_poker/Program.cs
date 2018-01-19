﻿using System;
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
                    try
                    {
                        Decision = int.Parse(decyzja);
                    }
                    catch (Exception ex)
                    {
                        Decision = 0;
                    }
                    
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

        static void Evaluate(List<Player> Players, List<byte> TableCards)
        {
            foreach (Player Player in Players)
            {
                if (!Player.Fold)
                {
                    Console.WriteLine("Gracz " + Player.ID + " nie sfoldował");
                    List<byte> WholeHand = Player.Hand;
                    WholeHand.AddRange(TableCards);
                    Console.WriteLine("cała ręka");
                    foreach (var card in WholeHand)
                    {
                        Console.WriteLine(card);
                    }
                    byte ValueCard;
                    List<byte> ValueHand = new List<byte>();
                    Console.WriteLine("wartości na ręce");
                    foreach (byte Card in WholeHand)
                    {
                        ValueCard = (byte)(Card % 16);
                        ValueHand.Add(ValueCard);
                        Console.WriteLine(ValueCard);
                    }
                    Player.TieBreaker = ValueHand;
                    Player.TieBreaker.Sort();
                    ValueHand.Sort();

                    bool StraightColor = false;
                    bool Quad = false;
                    bool Full = false;
                    bool Color = false;
                    bool Straight = false;
                    bool Three = false;
                    bool DoublePair = false;
                    bool Pair = false;
                    bool HighCard = false;

                    uint StraightColor1 = 0;
                    uint Quad1 = 0;
                    uint Straight1 = 0;
                    uint Three1 = 0;
                    uint Pair1 = 0;
                    uint Pair2 = 0;
                    uint HighCard1 = 0;



                    /*kolor*/

                    List<byte> Hearts = new List<byte>();
                    List<byte> Spades = new List<byte>();
                    List<byte> Diamonds = new List<byte>();
                    List<byte> Clubs = new List<byte>();
                    List<byte> ColorHand = new List<byte>();
                    uint Color1 = 15000000;

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
                    if (ColorHand.Count() >= 5)
                    {
                        Console.WriteLine("jest kolor");
                        foreach (var card in ColorHand)
                        {
                            Console.WriteLine(card);
                        }
                    }
                    else
                        Console.WriteLine("nie ma koloru ani pokera");
                    Console.ReadKey();
                    /*koniec kolor*/

                    /*zapełnianie tiebreakera dla koloru*/
                    if (Color)
                    {
                        Player.TieBreakerColor.AddRange(ColorHand);
                        Player.TieBreakerColor.Sort();
                        foreach (byte Card in ColorHand)
                        {
                            if (Player.TieBreaker.Contains(Card))
                            {
                                Player.TieBreaker.Remove(Card);
                            }
                        }
                    }
                    /*koniec tiebreakera*/


                    /*poker (strit+kolor*/
                    if (Color)
                    {
                        Console.WriteLine("sprawdzam czy jest poker");
                        Console.WriteLine("co najmniej 5 kart");
                        int DifferenceOneColor = 0;
                        StraightColor1 = 0;
                        List<byte> DistinctColor = ColorHand;
                        DistinctColor.Sort();

                        Console.WriteLine("Posortowane karty do pokera:");
                        foreach (byte Card in DistinctColor)
                        {
                            Console.WriteLine(Card);
                        }

                        for (byte Index = 0; Index < 4; Index++)
                        {
                            if (DistinctColor.ElementAt(Index + 1) - DistinctColor.ElementAt(Index) == 1)
                            {
                                DifferenceOneColor++;
                            }
                        }
                        if (DifferenceOneColor >= 4)
                        {
                            Console.WriteLine("jest poker w pierwszych pięciu kartach");
                            StraightColor = true;
                            StraightColor1 = DistinctColor.ElementAt(4);
                            StraightColor1 *= 1000000;
                            /*tiebreker poker z dostępnych 5 kart*/
                            Player.TieBreaker = ValueHand;
                            for (int Index = 0; Index < 4; Index++)
                            {
                                if (Player.TieBreaker.Contains(DistinctColor.ElementAt(Index)))
                                {
                                    Player.TieBreaker.Remove(DistinctColor.ElementAt(Index));
                                }
                            }
                            /*koniec tiebreakera*/
                        }
                        DifferenceOneColor = 0;

                        if (DistinctColor.Contains(14))
                        {
                            bool StraightColorAce = true;
                            for (byte Index = 2; Index < 6; Index++)
                            {
                                if (DistinctColor.Contains(Index))
                                {
                                    StraightColorAce &= true;
                                }
                                else
                                {
                                    StraightColorAce = false;
                                }
                                if (StraightColorAce && (DistinctColor.ElementAt(3) > StraightColor1 / 1000000))
                                {
                                    StraightColor = true;
                                    StraightColor1 = DistinctColor.ElementAt(3);
                                    StraightColor1 *= 1000000;
                                    Console.WriteLine("jest poker z asem na początku");
                                    /*tiebreker poker z dostępnych 5 kart*/
                                    Player.TieBreaker = ValueHand;
                                    for (int Number = 2; Number < 6; Number++)
                                    {
                                        Player.TieBreaker.Remove((byte)Number);
                                    }
                                    Player.TieBreaker.Remove(14);
                                    /*koniec tiebreakera*/
                                }
                            }
                        }

                        if (DistinctColor.Count() >= 6)
                        {
                            Console.WriteLine("jest co najmniej 6 kart");
                            for (byte Index = 1; Index < 5; Index++)
                            {
                                if (DistinctColor.ElementAt(Index + 1) - DistinctColor.ElementAt(Index) == 1)
                                {
                                    DifferenceOneColor++;
                                }
                            }

                            if (DifferenceOneColor >= 4)
                            {
                                Console.WriteLine("jest poker w kartach od 2 do 6");
                                StraightColor = true;
                                StraightColor1 = DistinctColor.ElementAt(5);
                                StraightColor1 *= 1000000;
                                /*tiebreker poker z dostępnych 6 kart*/
                                Player.TieBreaker = ValueHand;
                                for (int Index = 1; Index < 5; Index++)
                                {
                                    if (Player.TieBreaker.Contains(DistinctColor.ElementAt(Index)))
                                    {
                                        Player.TieBreaker.Remove(DistinctColor.ElementAt(Index));
                                    }
                                }
                                /*koniec tiebreakera*/
                            }
                            DifferenceOneColor = 0;

                            if (DistinctColor.Count() >= 7)
                            {
                                Console.WriteLine("co najmniej 7 kart");
                                for (byte Index = 2; Index < 6; Index++)
                                {
                                    if (DistinctColor.ElementAt(Index + 1) - DistinctColor.ElementAt(Index) == 1)
                                    {
                                        DifferenceOneColor++;
                                    }
                                }

                                if (DifferenceOneColor >= 4)
                                {
                                    Console.WriteLine("jest poker w kartach od 3 do 7");
                                    StraightColor = true;
                                    StraightColor1 = DistinctColor.ElementAt(6);
                                    StraightColor1 *= 1000000;
                                    /*tiebreker poker z dostępnych 7 kart*/
                                    Player.TieBreaker = ValueHand;
                                    for (int Index = 2; Index < 6; Index++)
                                    {
                                        if (Player.TieBreaker.Contains(DistinctColor.ElementAt(Index)))
                                        {
                                            Player.TieBreaker.Remove(DistinctColor.ElementAt(Index));
                                        }
                                    }
                                    /*koniec tiebreakera*/
                                }
                                DifferenceOneColor = 0;
                            }
                        }
                    }
                    Console.ReadKey();
                    /*koniec poker*/

                    /*kareta*/
                    if (!StraightColor)
                    {
                        Console.WriteLine("nie ma pokera, sprawdzam karetę");
                        Quad1 = 0;
                        uint Value = 0;
                        for (uint Index = 2; Index <= 14; Index++)
                        {
                            if ((ValueHand.LastIndexOf((byte)Index) - ValueHand.IndexOf((byte)Index) + 1) == 4)
                            {
                                Console.WriteLine("jest kareta");
                                Value = Index * 100000000;
                                Quad1 = Value;
                                Quad = true;
                                Player.TieBreaker = ValueHand;
                                Player.TieBreaker.RemoveAll((Card => Card == Index));
                            }
                        }
                        /*koniec karety*/
                        /*full*/
                        if (!Quad)
                        {
                            Console.WriteLine("nie ma karety, sprawdzam fulla");
                            Three1 = 0;
                            Pair1 = 0;
                            for (uint Index = 2; Index <= 14; Index++)
                            {
                                if ((ValueHand.LastIndexOf((byte)Index) - ValueHand.IndexOf((byte)Index) + 1) == 3)
                                {
                                    Console.WriteLine("znaleziono trójkę do fula: " + Index);
                                    Three1 = Index;
                                    Three = true;
                                }
                                if ((ValueHand.LastIndexOf((byte)Index) - ValueHand.IndexOf((byte)Index) + 1) == 2)
                                {
                                    Console.WriteLine("znaleziono dwójkę do fula: " + Index);
                                    Pair1 = Index;
                                    Pair = true;
                                }
                            }
                            if (Pair && Three)
                            {
                                Console.WriteLine("jest full");
                                Player.TieBreaker = ValueHand;
                                Player.TieBreaker.RemoveAll((Card => Card == Three1));
                                Console.WriteLine("usunięto z tiebreakera: " + Three1);
                                foreach (var card in Player.TieBreaker)
                                {
                                    Console.WriteLine(card);
                                }
                                Player.TieBreaker.RemoveAll((Card => Card == Pair1));
                                Console.WriteLine("usunięto z tiebreakera: " + Pair1);
                                foreach (var card in Player.TieBreaker)
                                {
                                    Console.WriteLine(card);
                                }
                                Three1 *= 100000;
                                Pair1 *= 1000;
                                Console.WriteLine("Wartości punktowe trójki: " + Three1 + " i pary: " + Pair1);
                                Full = true;
                            }
                            /*koniec full*/
                            /*strit*/
                            if (!Full && !Color)
                            {
                                int DifferenceOne = 0;
                                Straight1 = 0;
                                List<byte> StraightHand = DistinctHand(ValueHand);
                                Console.WriteLine("pozbycie się duplikatów wartości");
                                foreach (var item in StraightHand)
                                {
                                    Console.WriteLine(item);
                                }
                                Console.ReadKey();

                                if (StraightHand.ElementAt(StraightHand.Count() - 1) == 14)
                                {
                                    DifferenceOne = 0;
                                    for (int i = 0; i < 5; i++)
                                    {
                                        if (StraightHand.ElementAt(i) == i + 2)
                                        {
                                            DifferenceOne++;
                                        }
                                    }

                                    if (DifferenceOne == 4)
                                    {
                                        Console.WriteLine("jest strit z asem na początku");
                                        Straight1 = (uint)StraightHand.ElementAt(3)*1000000;
                                        Straight = true;
                                        Player.TieBreaker.Clear();
                                        foreach (byte Card in WholeHand)
                                        {
                                            ValueCard = (byte)(Card % 16);
                                            Player.TieBreaker.Add(ValueCard);
                                            Console.WriteLine(ValueCard);
                                        }
                                        for (byte i = 0; i <= 3; i++)
                                        {
                                            Player.TieBreaker.Remove(StraightHand.ElementAt(i));
                                        }
                                        Player.TieBreaker.Remove(14);
                                        Console.WriteLine("Karty na ręce gracza");
                                        foreach (var item in ValueHand)
                                        {
                                            Console.WriteLine(item);
                                        }

                                        Console.WriteLine("Karty do tiebrekera");
                                        foreach (var item in Player.TieBreaker)
                                        {
                                            Console.WriteLine(item);
                                        }
                                        Console.ReadKey();
                                    }
                                }
                                if (StraightHand.Count() >= 5)
                                {
                                    for (byte i = 0; i < 4; i++)
                                    {
                                        if (StraightHand.ElementAt(i + 1) - StraightHand.ElementAt(i) == 1)
                                        {
                                            DifferenceOne++;
                                        }
                                    }
                                    if (DifferenceOne == 4)
                                    {
                                        Console.WriteLine("jest strit z 5");
                                        Straight1 = (uint)StraightHand.ElementAt(4) * 1000000;
                                        Straight = true;
                                        for (byte i = 0; i <= 4; i++)
                                        {
                                            Player.TieBreaker.Remove(StraightHand.ElementAt(i));
                                        }
                                        Console.WriteLine("Karty na ręce gracza");
                                        foreach (var item in ValueHand)
                                        {
                                            Console.WriteLine(item);
                                        }

                                        Console.WriteLine("Karty do tiebrekera");
                                        foreach (var item in Player.TieBreaker)
                                        {
                                            Console.WriteLine(item);
                                        }
                                        Console.ReadKey();

                                    }
                                }
                                if (StraightHand.Count() >= 6)
                                {
                                    DifferenceOne = 0;
                                    for (byte i = 1; i < 5; i++)
                                    {
                                        if (StraightHand.ElementAt(i + 1) - StraightHand.ElementAt(i) == 1)
                                        {
                                            DifferenceOne++;
                                        }
                                    }
                                    if (DifferenceOne == 4)
                                    {
                                        Console.WriteLine("jest strit z 6");
                                        Straight1 = (uint)StraightHand.ElementAt(5) * 1000000; ;
                                        Straight = true;
                                        Player.TieBreaker.Clear();
                                        foreach (byte Card in WholeHand)
                                        {
                                            ValueCard = (byte)(Card % 16);
                                            Player.TieBreaker.Add(ValueCard);
                                            Console.WriteLine(ValueCard);
                                        }
                                        for (byte i = 1; i <= 5; i++)
                                        {
                                            Player.TieBreaker.Remove(StraightHand.ElementAt(i));
                                        }
                                        Console.WriteLine("Karty na ręce gracza");
                                        foreach (var item in ValueHand)
                                        {
                                            Console.WriteLine(item);
                                        }

                                        Console.WriteLine("Karty do tiebrekera");
                                        foreach (var item in Player.TieBreaker)
                                        {
                                            Console.WriteLine(item);
                                        }
                                        Console.ReadKey();
                                    }
                                }
                                if (StraightHand.Count() >= 7)
                                {
                                    DifferenceOne = 0;
                                    for (byte i = 2; i < 6; i++)
                                    {
                                        if (StraightHand.ElementAt(i + 1) - StraightHand.ElementAt(i) == 1)
                                        {
                                            DifferenceOne++;
                                        }
                                    }
                                    if (DifferenceOne == 4)
                                    {
                                        Console.WriteLine("jest strit z 7");
                                        Straight1 = (uint)StraightHand.ElementAt(6) * 1000000; ;
                                        Straight = true;
                                        Player.TieBreaker.Clear();
                                        foreach (byte Card in WholeHand)
                                        {
                                            ValueCard = (byte)(Card % 16);
                                            Player.TieBreaker.Add(ValueCard);
                                            Console.WriteLine(ValueCard);
                                        }
                                        for (byte i = 2; i <= 6; i++)
                                        {
                                            Player.TieBreaker.Remove(StraightHand.ElementAt(i));
                                        }
                                        Console.WriteLine("Karty na ręce gracza");
                                        foreach (var item in ValueHand)
                                        {
                                            Console.WriteLine(item);
                                        }

                                        Console.WriteLine("Karty do tiebrekera");
                                        foreach (var item in Player.TieBreaker)
                                        {
                                            Console.WriteLine(item);
                                        }
                                        Console.ReadKey();
                                    }
                                }
                                /*koniec strita*/
                                /*trójka*/
                                if (!Straight)
                                {
                                    Console.WriteLine("nie ma strita, sprawdzam trójki");
                                    Three1 = 0;
                                    Three = false;
                                    for (uint Index = 2; Index <= 14; Index++)
                                    {
                                        if ((ValueHand.LastIndexOf((byte)Index) - ValueHand.IndexOf((byte)Index) + 1) == 3)
                                        {
                                            Three1 = Index;
                                            Three = true;
                                        }
                                    }
                                    if (Three)
                                    {
                                        Console.WriteLine("jest trójka");
                                        Player.TieBreaker = ValueHand;
                                        Player.TieBreaker.RemoveAll((Card => Card == Three1));
                                        Three1 *= 100000;
                                    }
                                    /*koniec trójki*/

                                    /*dwie pary i para*/
                                    if (!Three)
                                    {
                                        Console.WriteLine("nie ma trójki, sprawdzam pary");
                                        Pair = false;
                                        Pair1 = 0;
                                        Pair2 = 0;
                                        Value = 0;
                                        Console.WriteLine("valuehand");
                                        foreach (var card in ValueHand)
                                        {
                                            Console.WriteLine(card);
                                        }
                                        for (uint Index = 2; Index <= 14; Index++)
                                        {
                                            if ((ValueHand.LastIndexOf((byte)Index) - ValueHand.IndexOf((byte)Index) + 1) == 2)
                                            {
                                                if (Pair1 == 0)
                                                {
                                                    Pair1 = Index;
                                                    Pair = true;
                                                }
                                                else
                                                {
                                                    Pair2 = Pair1;
                                                    Pair1 = Index;
                                                    Pair = false;
                                                    DoublePair = true;
                                                }

                                            }
                                        }

                                        if (DoublePair)
                                        {
                                            Console.WriteLine("są dwie pary");
                                            Player.TieBreaker = ValueHand;
                                            Console.WriteLine("baza kart");
                                            foreach (var card in Player.TieBreaker)
                                            {
                                                Console.WriteLine(card);
                                            }
                                            Player.TieBreaker.RemoveAll((Card => Card == Pair1));
                                            Console.WriteLine("usunięto karty: " + Pair1);
                                            foreach (var card in Player.TieBreaker)
                                            {
                                                Console.WriteLine(card);
                                            }
                                            Player.TieBreaker.RemoveAll((Card => Card == Pair2));
                                            Console.WriteLine("usunięto karty: " + Pair2);
                                            foreach (var card in Player.TieBreaker)
                                            {
                                                Console.WriteLine(card);
                                            }
                                            Pair1 *= 1000;
                                            Pair2 *= 10;
                                        }

                                        if (Pair && !DoublePair)
                                        {
                                            Console.WriteLine("jest jedna para");
                                            Player.TieBreaker = ValueHand;
                                            Player.TieBreaker.RemoveAll((Card => Card == Pair1));
                                            Pair1 *= 10;
                                        }
                                        /*koniec par*/

                                        /*najwyższa karta*/
                                        if (!DoublePair && !Pair)
                                        {
                                            Console.WriteLine("do sprawdzenia zostały tylko najwyższe karty");
                                            int NumberOfCards = ValueHand.Count();
                                            HighCard1 = ValueHand.ElementAt(NumberOfCards - 1);
                                            HighCard = true;
                                            Player.TieBreaker = ValueHand;
                                            Player.TieBreaker.RemoveAll((Card => Card == HighCard1));
                                        }


                                    }
                                }
                            }
                        }
                    }

                    if (StraightColor)
                    {
                        Console.WriteLine("punkty za pokera");
                        Player.ValueOfHand = (StraightColor1 + Color1) * 100;
                    }
                    else if (Quad)
                    {
                        Console.WriteLine("punkty za karetę");
                        Player.ValueOfHand = Quad1;
                    }
                    else if (Full)
                    {
                        Console.WriteLine("punkty za fulla");
                        Player.ValueOfHand = (Three1 + Pair1) * 100;
                    }
                    else if (Color)
                    {
                        Console.WriteLine("punkty za kolor");
                        Player.ValueOfHand = Color1;
                    }
                    else if (Straight)
                    {
                        Console.WriteLine("punkty za strita");
                        Player.ValueOfHand = Straight1;
                    }
                    else if (Three)
                    {
                        Console.WriteLine("punkty za trójkę");
                        Player.ValueOfHand = Three1;
                    }
                    else if (DoublePair)
                    {
                        Console.WriteLine("punkty za dwie pary");
                        Player.ValueOfHand = Pair1 + Pair2;
                    }
                    else if (Pair)
                    {
                        Console.WriteLine("punkty za parę");
                        Player.ValueOfHand = Pair1;
                    }
                    else if (HighCard)
                    {
                        Console.WriteLine("punkty za najwyższą kartę");
                        Player.ValueOfHand = HighCard1;
                    }
                }
                Console.WriteLine("Gracz: " + Player.ID + " ma rękę wartą " + Player.ValueOfHand);
                Console.ReadKey();
                Player.TieBreaker.Sort();
                Console.WriteLine("Ręka do tiebrekera");
                foreach (var card in Player.TieBreaker)
                {
                    Console.WriteLine(card);
                }
            }
        }

        static void WhoWon(List<Player> Players, Table Table)
        {
            uint Max = 0;
            int Who = 0;
            List<Player> SameValue = new List<Player>();
            foreach (Player Player in Players)
            {
                if (!Player.Fold)
                {
                    Max = Players.ElementAt(0).ValueOfHand;
                    Who = Players.ElementAt(0).ID;
                    Console.WriteLine("obecny max " + Max + "obecny gracz " + Who);
                    break;
                }
            }

            foreach (Player Player in Players)
            {
                if (Player.ValueOfHand > Max)
                {
                    Max = Player.ValueOfHand;
                    Who = Player.ID;
                    Console.WriteLine("zmiana maxa na " + Max + "obecny gracz " + Who);
                    SameValue.Clear();
                    SameValue.Add(Player);
                }
                else if (Player.ValueOfHand == Max)
                {
                    SameValue.Add(Player);
                    Console.WriteLine("remis, dodaję gracza " + Player.ID + " do samevalue");
                }
            }

            if (SameValue.Count() <= 1)
            {
                Console.WriteLine("Wygrał gracz " + Who);
                Players.ElementAt(0).Chips += Table.Pot;
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Potzrebny tiebreak ");
                TieBreaker(SameValue, Max, 1, Table);
                Console.ReadKey();
            }
        }

        static void TieBreaker(List<Player> TiedPlayers, uint TiedValue, int Cards, Table Table)
        {
            Console.WriteLine("tiebreaker nr " + Cards);
            uint Max = 0;
            int Who = 0;
            List<Player> SameValue = new List<Player>();
            int LastItem;
            if (TiedValue != 15000000)
            {
                LastItem = TiedPlayers.ElementAt(0).TieBreaker.Count() - Cards;
                if (LastItem < 0)
                {
                    /*remis nie do rozstrzygnięcia*/
                    int NumberOfPlayers = TiedPlayers.Count();
                    Console.WriteLine("Koniec kart w tiebreakerze. Remis między graczami: ");
                    foreach (Player Player in TiedPlayers)
                    {
                        Console.WriteLine(Player.ID);
                        Player.Chips += Table.Pot / NumberOfPlayers;
                    }
                    Console.ReadKey();
                }
                else
                {
                    Max = TiedPlayers.ElementAt(0).TieBreaker.ElementAt(LastItem);
                    Who = TiedPlayers.ElementAt(0).ID;
                    Console.WriteLine("obecny max " + Max + "obecny gracz " + Who);

                    foreach (Player Player in TiedPlayers)
                    {
                        LastItem = Player.TieBreaker.Count() - Cards;
                        if (Player.TieBreaker.ElementAt(LastItem) > Max)
                        {
                            Max = Player.TieBreaker.ElementAt(LastItem);
                            Who = Player.ID;
                            Console.WriteLine("zmiana maxa na " + Max + "obecny gracz " + Who);
                        }
                        else if (Player.TieBreaker.ElementAt(LastItem) == Max)
                        {
                            SameValue.Add(Player);
                            Console.WriteLine("remis w tiebreakerze, dodaję gracza " + Player.ID + " do samevalue");
                        }
                    }

                        if (SameValue.Count() <= 1)
                        {
                            Console.WriteLine("Koniec tiebreakera. Wygrał gracz " + Who);
                            Console.ReadKey();
                        }
                        else
                        {
                            Console.WriteLine("potrzebny dalszy tiebreker");
                            Console.ReadKey();
                            Cards++;
                            TieBreaker(SameValue, TiedValue, Cards, Table);
                        }
                    
                }
            }
            else
            {
                LastItem = TiedPlayers.ElementAt(0).TieBreakerColor.Count() - Cards;
                if (LastItem < 0)
                {
                    /*remis nie do rozstrzygnięcia*/
                    int NumberOfPlayers = TiedPlayers.Count();
                    Console.WriteLine("Remis między graczami: ");
                    foreach (Player Player in TiedPlayers)
                    {
                        Console.WriteLine(Player.ID);
                        Player.Chips += Table.Pot / NumberOfPlayers;
                    }
                    Console.ReadKey();
                }
                else
                {
                    Max = TiedPlayers.ElementAt(0).TieBreakerColor.ElementAt(LastItem);
                    Who = TiedPlayers.ElementAt(0).ID;
                    Console.WriteLine("obecny max " + Max + "obecny gracz " + Who);

                    foreach (Player Player in TiedPlayers)
                    {
                        LastItem = Player.TieBreakerColor.Count() - Cards;
                        if (Player.TieBreakerColor.ElementAt(LastItem) > Max)
                        {
                            Max = Player.TieBreakerColor.ElementAt(LastItem);
                            Who = Player.ID;
                            Console.WriteLine("zmiana maxa na " + Max + "obecny gracz " + Who);
                        }
                        else if (Player.TieBreakerColor.ElementAt(LastItem) == Max)
                        {
                            SameValue.Add(Player);
                            Console.WriteLine("remis w tiebreakerze, dodaję gracza " + Player.ID + " do samevalue");
                        }
                    }
                        if (SameValue.Count() <= 1)
                        {
                            Console.WriteLine("Wygrał gracz " + Who);
                            SameValue.ElementAt(0).Chips += Table.Pot;
                            Console.ReadKey();
                        }
                        else
                        {
                        Console.WriteLine("potrzevbny kolejny tiebreaker");
                            Cards++;
                            TieBreaker(SameValue, Max, Cards, Table);
                        }
                    
                }
            }

        }

        static void PreperationForNextRound(List<Player> Players, Table Table)
        {
            Table.Bid = 0;
            Table.CommunityCards.Clear();
            Table.Pot = 0;
            foreach (Player Player in Players)
            {
                Player.Hand.Clear();
                Player.TieBreaker.Clear();
                Player.TieBreakerColor.Clear();
                Player.Fold = false;
                Player.Check = false;
                Player.Bet = 0;
                Player.ValueOfHand = 0;
            }
        }

        static void Main(string[] args)
        {
            //List<byte> DeckTemplate = new List<byte>();
            Player Player1 = new Player { ID = 1, Chips = 1000, IsPlaying = true, Fold = false, Check = false, Bet = 0 };
            Player Player2 = new Player { ID = 2, Chips = 1000, IsPlaying = true, Fold = false, Check = false, Bet = 0 };
            Player Player3 = new Player { ID = 3, Chips = 1000, IsPlaying = false, Fold = false, Check = false, Bet = 0 };
            Player Player4 = new Player { ID = 4, Chips = 1000, IsPlaying = true, Fold = false, Check = false, Bet = 0 };
            List<Player> AllPlayers = new List<Player>(){ Player1, Player2, Player3, Player4 };
            foreach (Player Player in AllPlayers)
            {
                Console.WriteLine("Gracz " + Player.ID + " gotowy");
            }            
            Table Table = new Table { Pot = 0, Bid = 0 };
            //DeckTemplate = CreateDeck();
            while (AllPlayers.Count() >= 2)
            {
                WhoIsPlaying(AllPlayers);
                foreach (Player Player in AllPlayers)
                {
                    Console.WriteLine("Gracz " + Player.ID + " gra");
                }
                List<byte> DeckToPlay = CreateDeck();//Przypisanie talii do nowej zmiennej, która będzie modyfikowana
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
                Console.WriteLine("Trzecie i ostatnie rozdanie kart na stół");
                Table.Deal(CardsOnTable, 1);
                Console.WriteLine("Kart w talii: " + DeckToPlay.Count());
                Table.ShowCards();
                Console.WriteLine("Licytacja 4");
                NextBetting(AllPlayers, Table);
                Console.ReadKey();
                Evaluate(AllPlayers, Table.CommunityCards);
                Console.ReadKey();
                WhoWon(AllPlayers, Table);
                PreperationForNextRound(AllPlayers, Table);
                Console.ReadKey();
            }
        }
    }
}
