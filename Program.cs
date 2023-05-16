using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Main()
    {
        Console.CursorVisible = false;
        Console.WindowHeight = Console.LargestWindowHeight - 10;
        Console.WindowWidth = Console.LargestWindowWidth - 10;

        List<Fish> fishesLeft = GenerateFishes(true);
        List<Fish> fishesRight = GenerateFishes(false);
        List<Bubble> bubbles = GenerateBubbles();

        while (true)
        {
            List<Fish> updatedFishesLeft = UpdateFishes(fishesLeft, true);
            List<Fish> updatedFishesRight = UpdateFishes(fishesRight, false);
            List<Bubble> updatedBubbles = UpdateBubbles(bubbles);

            DrawAquarium(updatedFishesLeft, updatedFishesRight, updatedBubbles);

            Thread.Sleep(100);
        }
    }

    static void DrawAquarium(List<Fish> fishesLeft, List<Fish> fishesRight, List<Bubble> bubbles)
    {
        Console.Clear();

        foreach (Fish fish in fishesLeft)
        {
            int y = Math.Max(0, Math.Min(Console.WindowHeight - 1, fish.Y)); // Ensure Y coordinate is within valid range
            Console.SetCursorPosition(fish.X, y);
            Console.ForegroundColor = fish.Color;
            Console.Write(fish.Symbol);
        }

        foreach (Fish fish in fishesRight)
        {
            int x = fish.X + fish.Symbol.Length - 1;
            int y = Math.Max(0, Math.Min(Console.WindowHeight - 1, fish.Y)); // Ensure Y coordinate is within valid range
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = fish.Color;
            Console.Write(fish.Symbol);
        }

        foreach (Bubble bubble in bubbles)
        {
            Console.SetCursorPosition(bubble.X, bubble.Y);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(bubble.Symbol);
        }
    }

    static List<Fish> GenerateFishes(bool moveLeft)
    {
        Random random = new Random();
        List<Fish> fishes = new List<Fish>();

        List<string> fishSymbols = moveLeft ? GenerateFishSymbolsLeft() : GenerateFishSymbolsRight();

        for (int i = 0; i < 10; i++)
        {
            int y = random.Next(2, Console.WindowHeight - 7);
            int x = random.Next(2, Console.WindowWidth - 12);
            string fishSymbol = fishSymbols[random.Next(0, fishSymbols.Count)];
            ConsoleColor color = (ConsoleColor)random.Next(1, 16);
            int speed = random.Next(1, 4);
            int bobRange = random.Next(1, 4);
            int amplitude = random.Next(1, 4);
            Fish fish = new Fish(fishSymbol, x, y, color, speed, bobRange, amplitude);
            fishes.Add(fish);
        }

        return fishes;
    }

    static List<string> GenerateFishSymbolsLeft()
    {
        return new List<string>
        {
            @"<')))><",
            @"<====>",
            @"  _><<_ ",
            @"<(0)^^^>",
        };
    }

    static List<string> GenerateFishSymbolsRight()
    {
        return new List<string>
        {
            @"><((('>",
            @"<====>",
            @" _>><_  ",
            @"<^^^(0)>",
        };
    }

    static List<Fish> UpdateFishes(List<Fish> fishes, bool moveLeft)
    {
        List<Fish> updatedFishes = new List<Fish>();

        foreach (Fish fish in fishes)
        {
            fish.Move(moveLeft);
            updatedFishes.Add(fish);
        }

        return updatedFishes;
    }

    static List<Bubble> GenerateBubbles()
    {
        Random random = new Random();
        List<Bubble> bubbles = new List<Bubble>();

        for (int i = 0; i < 30; i++)
        {
            int x = random.Next(2, Console.WindowWidth - 2);
            int y = random.Next(2, Console.WindowHeight - 2);
            Bubble bubble = new Bubble(x, y);
            bubbles.Add(bubble);
        }

        return bubbles;
    }

    static List<Bubble> UpdateBubbles(List<Bubble> bubbles)
    {
        List<Bubble> updatedBubbles = new List<Bubble>();

        foreach (Bubble bubble in bubbles)
        {
            bubble.Rise();
            updatedBubbles.Add(bubble);
        }

        return updatedBubbles;
    }
}

class Fish
{
    public string Symbol { get; }

    public int X { get; private set; }
    public int Y { get; private set; }
    public ConsoleColor Color { get; }
    public int Speed { get; }
    public int BobRange { get; }
    public int Amplitude { get; }

    private double angle = 0;
    private int bobCounter = 0;
    private bool bobUp = true;

    public Fish(string symbol, int x, int y, ConsoleColor color, int speed, int bobRange, int amplitude)
    {
        Symbol = symbol;
        X = x;
        Y = y;
        Color = color;
        Speed = speed;
        BobRange = bobRange;
        Amplitude = amplitude;
    }

    public void Move(bool moveLeft)
    {
        if (moveLeft)
        {
            X -= Speed;
            if (X < 2)
            {
                X = Console.WindowWidth - 12;
            }
        }
        else
        {
            X += Speed;
            if (X > Console.WindowWidth - 12)
            {
                X = 2;
            }
        }

        // Update the Y position based on the boundaries and bobbing effect
        if (bobUp)
        {
            if (Y > BobRange)
            {
                Y--;
            }
            else
            {
                bobUp = false;
                Y++;
            }
        }
        else
        {
            if (Y < Console.WindowHeight - 8 - BobRange)
            {
                Y++;
            }
            else
            {
                bobUp = true;
                Y--;
            }
        }
    }
}

class Bubble
{
    public int X { get; }
    public int Y { get; private set; }
    public string Symbol { get; } = "o";

    public Bubble(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Rise()
    {
        Y--;
        if (Y < 2)
        {
            Y = Console.WindowHeight - 7;
        }
    }
}