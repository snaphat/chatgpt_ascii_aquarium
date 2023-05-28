﻿using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    private static readonly Random Random = new Random();
    private const int BorderBuffer = 2;
    private const int FishBuffer = 12;
    private const int BottomBuffer = 7;

    private static bool SeaFloorDrawn;

    static void Main()
    {
        Console.CursorVisible = false;
        Console.WindowHeight = Console.LargestWindowHeight - 10;
        Console.WindowWidth = Console.LargestWindowWidth - 10;

        List<Fish> fishesLeft = GenerateFishes(true);
        List<Fish> fishesRight = GenerateFishes(false);
        List<Bubble> bubbles = GenerateBubbles();

        // Draw the aquarium once at the beginning
        DrawAquarium(fishesLeft, fishesRight, bubbles);

        while (true)
        {
            UpdateAndRedrawFishes(fishesLeft, true);
            UpdateAndRedrawFishes(fishesRight, false);
            UpdateAndRedrawBubbles(bubbles);

            Thread.Sleep(50);
        }
    }

    static void DrawSeaFloor()
    {
        if (!SeaFloorDrawn)
        {
            int seaFloorStart = Console.WindowHeight - BottomBuffer;

            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.SetCursorPosition(i, seaFloorStart);
                Console.Write("~");
            }

            for (int i = 0; i < Console.WindowWidth; i++)
            {
                for (int j = seaFloorStart + 1; j < Console.WindowHeight; j++)
                {
                    Console.SetCursorPosition(i, j);
                    Console.Write(Random.Next(0, 3) > 0 ? "'" : ".");
                }
            }

            SeaFloorDrawn = true;
        }
    }

    static void UpdateAndRedrawFishes(List<Fish> fishes, bool moveLeft)
    {
        foreach (Fish fish in fishes)
        {
            ClearAtPosition(fish.X, fish.Y, fish.Symbol.Length);
            fish.Move(moveLeft);
            DrawFish(fish);
        }
    }

    static void UpdateAndRedrawBubbles(List<Bubble> bubbles)
    {
        foreach (Bubble bubble in bubbles)
        {
            ClearAtPosition(bubble.X, bubble.Y, bubble.Symbol.Length);
            bubble.Rise();
            DrawBubble(bubble);
        }
    }

    static void ClearAtPosition(int x, int y, int length)
    {
        int seaFloorStart = Console.WindowHeight - BottomBuffer;
        for (int i = 0; i < length; i++)
        {
            if (y < seaFloorStart)
            {
                Console.SetCursorPosition(x + i, y);
                Console.Write(' ');
            }
        }
    }

    static void DrawFish(Fish fish)
    {
        int y = Math.Max(0, Math.Min(Console.WindowHeight - 1, fish.Y)); // Ensure Y coordinate is within valid range
        Console.SetCursorPosition(fish.X, y);
        Console.ForegroundColor = fish.Color;
        Console.Write(fish.Symbol);
    }

    static void DrawBubble(Bubble bubble)
    {
        Console.SetCursorPosition(bubble.X, bubble.Y);
        Console.ForegroundColor = bubble.Color;
        Console.Write(bubble.Symbol);
    }

    static void DrawAquarium(List<Fish> fishesLeft, List<Fish> fishesRight, List<Bubble> bubbles)
    {
        Console.Clear();

        foreach (Fish fish in fishesLeft)
        {
            DrawFish(fish);
        }

        foreach (Fish fish in fishesRight)
        {
            DrawFish(fish);
        }

        foreach (Bubble bubble in bubbles)
        {
            DrawBubble(bubble);
        }

        DrawSeaFloor();
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

    static void UpdateFishes(List<Fish> fishes, bool moveLeft)
    {
        foreach (Fish fish in fishes)
        {
            fish.Move(moveLeft);
        }
    }

    static List<Bubble> GenerateBubbles()
    {
        Random random = new Random();
        List<Bubble> bubbles = new List<Bubble>();

        for (int i = 0; i < 30; i++)
        {
            int x = random.Next(2, Console.WindowWidth - 2);
            // Make sure the bubbles start above the sea floor
            int y = random.Next(2, Console.WindowHeight - BottomBuffer - 1);
            string bubbleSymbol = GenerateBubbleSymbol();
            ConsoleColor color = (ConsoleColor)random.Next(1, 16);
            Bubble bubble = new Bubble(x, y, bubbleSymbol, color, BottomBuffer);
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
    static string GenerateBubbleSymbol()
    {
        int size = Random.Next(1, 4);
        return new string('o', size);
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
    public string Symbol { get; }
    public ConsoleColor Color { get; }
    private int BottomBuffer { get; }

    public Bubble(int x, int y, string symbol, ConsoleColor color, int bottomBuffer)
    {
        X = x;
        Y = y;
        Symbol = symbol;
        Color = color;
        BottomBuffer = bottomBuffer;
    }

    public void Rise()
    {
        Y--;
        if (Y < 2)
        {
            Y = Console.WindowHeight - BottomBuffer - 1;
        }
    }
}
