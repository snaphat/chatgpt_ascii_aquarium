using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    private static readonly Random Random = new Random();
    private const int BorderBuffer = 2;
    private const int FishBuffer = 12;
    private const int BottomBuffer = 7;
    private static int WindowWidth = Console.WindowWidth;
    private static int WindowHeight = Console.WindowHeight;
    private static bool SeaFloorDrawn;

    static void Main()
    {
        Console.CursorVisible = false;
        Console.WindowHeight = Console.LargestWindowHeight - 10;
        Console.WindowWidth = Console.LargestWindowWidth - 10;

        List<Fish> fishesLeft = GenerateFishes(true);
        List<Fish> fishesRight = GenerateFishes(false);
        List<Bubble> bubbles = GenerateBubbles();
        List<SeaPlant> seaPlants = GenerateSeaPlants(BottomBuffer);

        DrawAquarium(fishesLeft, fishesRight, bubbles, seaPlants);

        while (true)
        {
            if (WindowWidth != Console.WindowWidth || WindowHeight != Console.WindowHeight)
            {
                WindowWidth = Console.WindowWidth;
                WindowHeight = Console.WindowHeight;
                SeaFloorDrawn = false; // reset SeaFloorDrawn flag
                fishesLeft = GenerateFishes(true);
                fishesRight = GenerateFishes(false);
                bubbles = GenerateBubbles();
                seaPlants = GenerateSeaPlants(BottomBuffer);
                DrawAquarium(fishesLeft, fishesRight, bubbles, seaPlants);
            }
            else
            {
                UpdateAndRedrawFishes(fishesLeft, true);
                UpdateAndRedrawFishes(fishesRight, false);
                UpdateAndRedrawBubbles(bubbles);
                DrawSeaPlants(seaPlants);
            }

            Thread.Sleep(50);
        }
    }

    static void DrawSeaFloor()
    {
        if (!SeaFloorDrawn)
        {
            int seaFloorStart = Math.Max(Math.Min(Console.WindowHeight - BottomBuffer, Console.WindowHeight - 1), 0);

            for (int i = 0; i < Math.Min(Console.WindowWidth, Console.BufferWidth); i++)
            {
                Console.SetCursorPosition(i, seaFloorStart);
                Console.Write("~");
            }

            for (int i = 0; i < Math.Min(Console.WindowWidth, Console.BufferWidth); i++)
            {
                for (int j = seaFloorStart + 1; j < Math.Min(Console.WindowHeight, Console.BufferHeight); j++)
                {
                    // Make sure 'i' and 'j' are within the current console window width and height
                    int safeI = Math.Min(i, Console.WindowWidth - 1);
                    int safeJ = Math.Min(j, Console.WindowHeight - 1);

                    Console.SetCursorPosition(safeI, safeJ);
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
        for (int i = 0; i < length; i++)
        {
            int safeX = Math.Max(0, Math.Min(Console.WindowWidth - 1, x + i));
            int safeY = Math.Max(0, Math.Min(Console.WindowHeight - 1, y));

            Console.SetCursorPosition(safeX, safeY);
            Console.Write(' ');
        }
    }

    static void DrawSeaPlants(List<SeaPlant> seaPlants)
    {
        foreach (SeaPlant seaPlant in seaPlants)
        {
            for (int j = 0; j < seaPlant.Height; j++)
            {
                if (seaPlant.X < Console.WindowWidth && seaPlant.Y - j < Console.WindowHeight && seaPlant.Y - j >= 0)
                {
                    Console.SetCursorPosition(seaPlant.X, seaPlant.Y - j);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("|");
                }
            }
        }
    }

    static void DrawFish(Fish fish)
    {
        int y = Math.Max(0, Math.Min(Console.WindowHeight - 1, fish.Y)); // Ensure Y coordinate is within valid range
        int x = Math.Max(0, fish.X); // Ensure X coordinate is non-negative
        if (x < Console.WindowWidth && y < Console.WindowHeight)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = fish.Color;
            Console.Write(fish.Symbol);
        }
    }

    static void DrawBubble(Bubble bubble)
    {
        int x = Math.Max(0, bubble.X); // Ensure X coordinate is non-negative
        int y = Math.Max(0, bubble.Y); // Ensure Y coordinate is non-negative
        if (x < Console.WindowWidth && y < Console.WindowHeight)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = bubble.Color;
            Console.Write(bubble.Symbol);
        }
    }

    static void DrawAquarium(List<Fish> fishesLeft, List<Fish> fishesRight, List<Bubble> bubbles, List<SeaPlant> seaPlants)
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
        DrawSeaPlants(seaPlants);
    }

    static List<Fish> GenerateFishes(bool moveLeft)
    {
        List<Fish> fishes = new List<Fish>();

        List<string> fishSymbols = moveLeft ? GenerateFishSymbolsLeft() : GenerateFishSymbolsRight();

        for (int i = 0; i < 10; i++)
        {
            int y = Random.Next(2, Math.Max(2, Console.WindowHeight - 7));
            int x = Random.Next(2, Math.Max(2, Console.WindowWidth - 12));
            string fishSymbol = fishSymbols[Random.Next(0, fishSymbols.Count)];
            ConsoleColor color = (ConsoleColor)Random.Next(1, 16);
            int speed = Random.Next(1, 4);
            int bobRange = Random.Next(1, 4);
            int amplitude = Random.Next(1, 4);
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
        List<Bubble> bubbles = new List<Bubble>();

        for (int i = 0; i < 30; i++)
        {
            int x = Random.Next(2, Console.WindowWidth - 2);

            // Make sure the bubbles start above the sea floor
            int minY = Math.Max(2, Console.WindowHeight - BottomBuffer - 1);
            int y = Random.Next(2, minY);

            string bubbleSymbol = GenerateBubbleSymbol();
            ConsoleColor color = (ConsoleColor)Random.Next(1, 16);
            Bubble bubble = new Bubble(x, y, bubbleSymbol, color, BottomBuffer);
            bubbles.Add(bubble);
        }

        return bubbles;
    }

    static List<SeaPlant> GenerateSeaPlants(int bottomBuffer)
    {
        List<SeaPlant> seaPlants = new List<SeaPlant>();
        int seaFloorStart = Console.WindowHeight - bottomBuffer;

        for (int i = 0; i < Console.WindowWidth; i += Random.Next(2, 10))
        {
            int plantHeight = Random.Next(2, 7);
            seaPlants.Add(new SeaPlant(i, seaFloorStart, plantHeight));
        }

        return seaPlants;
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

class SeaPlant
{
    public int X { get; }
    public int Y { get; }
    public int Height { get; }

    public SeaPlant(int x, int y, int height)
    {
        X = x;
        Y = y;
        Height = height;
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
