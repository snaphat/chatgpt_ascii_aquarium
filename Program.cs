using System;
using System.Collections.Generic;
using System.IO;
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
        // Removed forced resizing to play better with Windows Terminal and other modern terminals
        
        List<Fish> fishesLeft = GenerateFishes(true);
        List<Fish> fishesRight = GenerateFishes(false);
        List<Bubble> bubbles = GenerateBubbles();
        List<SeaPlant> seaPlants = GenerateSeaPlants(BottomBuffer);
        Castle castle = GenerateCastle();
        List<Jellyfish> jellyfishList = GenerateJellyfish();
        List<Crab> crabs = GenerateCrabs(BottomBuffer);
        TreasureChest treasureChest = GenerateTreasureChest(BottomBuffer);
        Submarine submarine = GenerateSubmarine();
        List<Starfish> starfishList = GenerateStarfish(BottomBuffer);
        Shark shark = GenerateShark();

        DrawAquarium(fishesLeft, fishesRight, bubbles, seaPlants, castle, jellyfishList, crabs, treasureChest, submarine, starfishList, shark);

        while (true)
        {
            if (Console.WindowWidth < 1 || Console.WindowHeight < 1)
            {
                continue; // don't execute the function if the console is too small
            }

            try
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
                    castle = GenerateCastle();
                    jellyfishList = GenerateJellyfish();
                    crabs = GenerateCrabs(BottomBuffer);
                    treasureChest = GenerateTreasureChest(BottomBuffer);
                    submarine = GenerateSubmarine();
                    starfishList = GenerateStarfish(BottomBuffer);
                    shark = GenerateShark();
                    DrawAquarium(fishesLeft, fishesRight, bubbles, seaPlants, castle, jellyfishList, crabs, treasureChest, submarine, starfishList, shark);
                }
                else
                {
                    UpdateAndRedrawFishes(fishesLeft, true);
                    UpdateAndRedrawFishes(fishesRight, false);
                    UpdateAndRedrawBubbles(bubbles);
                    UpdateAndRedrawJellyfish(jellyfishList);
                    UpdateAndRedrawCrabs(crabs);
                    UpdateAndRedrawSubmarine(submarine);
                    shark.Clear();
                    shark.Update();
                    shark.Draw();
                    treasureChest.Update(bubbles);
                    treasureChest.Draw();
                    DrawCastle(castle);
                    DrawSeaPlants(seaPlants);
                    foreach (var s in starfishList) s.Draw();
                }

                Thread.Sleep(50);
            }
            catch (ArgumentOutOfRangeException)
            {
                // The console window was probably resized to a very small size
                continue; // skip this iteration of the loop
            }
            catch (IOException)
            {
                // The console window was probably resized to zero size
                continue; // skip this iteration of the loop
            }
        }
    }

    public static void SafeSetCursorPosition(int left, int top)
    {
        try
        {
            if (left >= 0 && left < Console.BufferWidth && top >= 0 && top < Console.BufferHeight)
            {
                Console.SetCursorPosition(left, top);
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            // Ignore if still out of range due to race condition
        }
    }

    public static void SafeDrawString(string text, int x, int y, ConsoleColor color)
    {
        if (y < 0 || y >= Console.WindowHeight) return;

        int drawX = x;
        string visiblePart = text;

        if (drawX < 0)
        {
            if (Math.Abs(drawX) >= text.Length) return;
            visiblePart = text.Substring(Math.Abs(drawX));
            drawX = 0;
        }

        if (drawX + visiblePart.Length > Console.WindowWidth)
        {
            int length = Console.WindowWidth - drawX;
            if (length <= 0) return;
            visiblePart = visiblePart.Substring(0, length);
        }

        SafeSetCursorPosition(drawX, y);
        Console.ForegroundColor = color;
        Console.Write(visiblePart);
    }

    public static void SafeClearString(int x, int y, int length)
    {
        SafeDrawString(new string(' ', length), x, y, Console.ForegroundColor);
    }

    static void DrawSeaFloor()
    {
        if (!SeaFloorDrawn)
        {
            int seaFloorStart = Math.Max(Math.Min(Console.WindowHeight - BottomBuffer, Console.WindowHeight - 1), 0);

            for (int i = 0; i < Math.Min(Console.WindowWidth, Console.BufferWidth); i++)
            {
                SafeSetCursorPosition(i, seaFloorStart);
                Console.Write("~");
            }

            for (int i = 0; i < Math.Min(Console.WindowWidth, Console.BufferWidth); i++)
            {
                for (int j = seaFloorStart + 1; j < Math.Min(Console.WindowHeight, Console.BufferHeight); j++)
                {
                    // Make sure 'i' and 'j' are within the current console window width and height
                    int safeI = Math.Min(i, Console.WindowWidth - 1);
                    int safeJ = Math.Min(j, Console.WindowHeight - 1);

                    SafeSetCursorPosition(safeI, safeJ);
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
            SafeClearString(fish.X, fish.Y, fish.Symbol.Length);
            fish.Move(moveLeft);
            DrawFish(fish);
        }
    }

    static void UpdateAndRedrawBubbles(List<Bubble> bubbles)
    {
        foreach (Bubble bubble in bubbles)
        {
            SafeClearString(bubble.X, bubble.Y, bubble.Symbol.Length);
            bubble.Rise();
            DrawBubble(bubble);
        }
    }

    static void ClearAtPosition(int x, int y, int length)
    {
        SafeClearString(x, y, length);
    }

    static void DrawSeaPlants(List<SeaPlant> seaPlants)
    {
        foreach (SeaPlant seaPlant in seaPlants)
        {
            for (int j = 0; j < seaPlant.Height; j++)
            {
                SafeDrawString(j % 2 == 0 ? "(" : ")", seaPlant.X, seaPlant.Y - j, seaPlant.Color);
            }
        }
    }

    static void DrawFish(Fish fish)
    {
        SafeDrawString(fish.Symbol, fish.X, fish.Y, fish.Color);
    }

    static void DrawBubble(Bubble bubble)
    {
        SafeDrawString(bubble.Symbol, bubble.X, bubble.Y, bubble.Color);
    }

    static void DrawAquarium(List<Fish> fishesLeft, List<Fish> fishesRight, List<Bubble> bubbles, List<SeaPlant> seaPlants, Castle castle, List<Jellyfish> jellyfishList, List<Crab> crabs, TreasureChest treasureChest, Submarine submarine, List<Starfish> starfishList, Shark shark)
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
        DrawCastle(castle);
        DrawSeaPlants(seaPlants);
        
        foreach (var jelly in jellyfishList)
        {
             SafeDrawString(jelly.Symbol, jelly.X, jelly.Y, ConsoleColor.Magenta);
             SafeDrawString(jelly.Tentacles, jelly.X, jelly.Y + 1, ConsoleColor.Magenta);
        }

        foreach (var crab in crabs)
        {
            SafeDrawString(crab.Symbol, crab.X, crab.Y, ConsoleColor.Red);
        }

        treasureChest.Draw();

        for (int i = 0; i < submarine.Design.Length; i++)
        {
            SafeDrawString(submarine.Design[i], submarine.X, submarine.Y + i, submarine.Colors[i]);
        }

        foreach (var s in starfishList) s.Draw();
        shark.Draw();
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
            @"<°)))><",
            @"><>",
            @"<:::<",
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
            @"><(((°>",
            @"<><",
            @">:::>",
        };
    }

    static void UpdateFishes(List<Fish> fishes, bool moveLeft)
    {
        foreach (Fish fish in fishes)
        {
            fish.Move(moveLeft);
        }
    }

    static Castle GenerateCastle()
    {
        int x = Random.Next(10, Console.WindowWidth - 20);
        int y = Console.WindowHeight - BottomBuffer - 5; // Adjust based on castle height
        return new Castle(x, y);
    }

    static void DrawCastle(Castle castle)
    {
        if (castle == null) return;
        for (int i = 0; i < castle.Design.Length; i++)
        {
            SafeDrawString(castle.Design[i], castle.X, castle.Y + i, ConsoleColor.Gray);
        }
    }

    static List<Jellyfish> GenerateJellyfish()
    {
        List<Jellyfish> jellyfishList = new List<Jellyfish>();
        for (int i = 0; i < 3; i++)
        {
            int x = Random.Next(5, Console.WindowWidth - 5);
            int y = Random.Next(5, Console.WindowHeight - 10);
            jellyfishList.Add(new Jellyfish(x, y));
        }
        return jellyfishList;
    }

    static void UpdateAndRedrawJellyfish(List<Jellyfish> jellyfishList)
    {
        foreach (var jellyfish in jellyfishList)
        {
            jellyfish.Clear();
            jellyfish.Move();
            SafeDrawString(jellyfish.Symbol, jellyfish.X, jellyfish.Y, ConsoleColor.Magenta);
            SafeDrawString(jellyfish.Tentacles, jellyfish.X, jellyfish.Y + 1, ConsoleColor.Magenta);
        }
    }

    static List<Crab> GenerateCrabs(int bottomBuffer)
    {
        List<Crab> crabs = new List<Crab>();
        int y = Console.WindowHeight - bottomBuffer;
        for (int i = 0; i < 2; i++)
        {
            int x = Random.Next(2, Console.WindowWidth - 10);
            crabs.Add(new Crab(x, y));
        }
        return crabs;
    }

    static void UpdateAndRedrawCrabs(List<Crab> crabs)
    {
        foreach (var crab in crabs)
        {
            crab.Clear();
            crab.Move();
            SafeDrawString(crab.Symbol, crab.X, crab.Y, ConsoleColor.Red);
        }
    }

    static TreasureChest GenerateTreasureChest(int bottomBuffer)
    {
        int x = Random.Next(10, Console.WindowWidth - 20);
        int y = Console.WindowHeight - bottomBuffer; 
        return new TreasureChest(x, y);
    }

    static Submarine GenerateSubmarine()
    {
        return new Submarine(-10, 3);
    }

    static void UpdateAndRedrawSubmarine(Submarine sub)
    {
        sub.Clear();
        sub.Move();
        for (int i = 0; i < sub.Design.Length; i++)
        {
            SafeDrawString(sub.Design[i], sub.X, sub.Y + i, sub.Colors[i]);
        }
    }

    static List<Starfish> GenerateStarfish(int bottomBuffer)
    {
        List<Starfish> starfishList = new List<Starfish>();
        int y = Console.WindowHeight - bottomBuffer + 1;
        for (int i = 0; i < 5; i++)
        {
            int x = Random.Next(5, Console.WindowWidth - 5);
            int safeY = Random.Next(y, Console.WindowHeight - 1);
            starfishList.Add(new Starfish(x, safeY, ConsoleColor.Red));
        }
        return starfishList;
    }

    static Shark GenerateShark()
    {
        return new Shark(Console.WindowWidth, 10);
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
        ConsoleColor[] plantColors = { ConsoleColor.Green, ConsoleColor.DarkGreen, ConsoleColor.DarkCyan };

        for (int i = 0; i < Console.WindowWidth; i += Random.Next(2, 10))
        {
            int plantHeight = Random.Next(2, 7);
            ConsoleColor color = plantColors[Random.Next(plantColors.Length)];
            seaPlants.Add(new SeaPlant(i, seaFloorStart, plantHeight, color));
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
    public ConsoleColor Color { get; }

    public SeaPlant(int x, int y, int height, ConsoleColor color)
    {
        X = x;
        Y = y;
        Height = height;
        Color = color;
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

class Castle
{
    public int X { get; }
    public int Y { get; }
    public string[] Design { get; }

    public Castle(int x, int y)
    {
        X = x;
        Y = y;
        Design = new string[]
        {
            @"      /^\      ",
            @"     |   |     ",
            @"     |   |     ",
            @"    _|___|_    ",
            @"   [_______]   "
        };
    }
}

class Jellyfish
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public string Symbol { get; } = "(:)";
    public string Tentacles { get; } = " | ";

    private bool movingUp = true;
    private int moveCounter = 0;
    private readonly Random Random = new Random();

    public Jellyfish(int x, int y)
    {
        X = x;
        Y = y;
        movingUp = Random.Next(0, 2) == 0;
    }

    public void Move()
    {
        moveCounter++;
        if (moveCounter % 3 == 0) // Move slower than fish
        {
            if (movingUp)
            {
                Y--;
                if (Y < 5) movingUp = false;
            }
            else
            {
                Y++;
                if (Y > Console.WindowHeight - 10) movingUp = true;
            }
        }
    }

    public void Clear()
    {
        Program.SafeClearString(X, Y, 3);
        Program.SafeClearString(X, Y + 1, 3);
    }
}

class Crab
{
    public int X { get; private set; }
    public int Y { get; }
    public string Symbol { get; } = @"(\_/)";
    private bool movingLeft = true;
    private int moveCounter = 0;
    private readonly Random Random = new Random();

    public Crab(int x, int y)
    {
        X = x;
        Y = y;
        movingLeft = Random.Next(0, 2) == 0;
    }

    public void Move()
    {
        moveCounter++;
        if (moveCounter % 4 == 0) // Crabs are slow
        {
            if (movingLeft)
            {
                X--;
                if (X < 2) movingLeft = false;
            }
            else
            {
                X++;
                if (X > Console.WindowWidth - 7) movingLeft = true;
            }
        }
    }

    public void Clear()
    {
        Program.SafeClearString(X, Y, Symbol.Length + 1);
    }
}

class TreasureChest
{
    public int X { get; }
    public int Y { get; }
    public bool IsOpen { get; private set; }
    private int timer = 0;
    private readonly Random Random = new Random();

    public TreasureChest(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Update(List<Bubble> bubbles)
    {
        timer++;
        if (timer > 50 && Random.Next(0, 20) == 0)
        {
            IsOpen = !IsOpen;
            timer = 0;
        }

        if (IsOpen && timer % 5 == 0)
        {
            bubbles.Add(new Bubble(X + 3, Y, "o", ConsoleColor.Yellow, 7));
        }
    }

    public void Draw()
    {
        Program.SafeDrawString(IsOpen ? "[_###_]" : "[_____]", X, Y, ConsoleColor.DarkYellow);
    }
}

class Submarine
{
    public int X { get; private set; }
    public int Y { get; }
    public string[] Design { get; } = {
        @"   _|_   ",
        @"  |___|  ",
        @"<|_____|>",
        @"  (o)(o) "
    };
    public ConsoleColor[] Colors { get; } = {
        ConsoleColor.Cyan,
        ConsoleColor.Yellow,
        ConsoleColor.Yellow,
        ConsoleColor.DarkGray
    };
    private int moveCounter = 0;

    public Submarine(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Move()
    {
        moveCounter++;
        if (moveCounter % 5 == 0)
        {
            X++;
            if (X > Console.WindowWidth) X = -15;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < Design.Length; i++)
        {
            Program.SafeClearString(X, Y + i, Design[i].Length);
        }
    }
}

class Starfish
{
    public int X { get; }
    public int Y { get; }
    public ConsoleColor Color { get; }

    public Starfish(int x, int y, ConsoleColor color)
    {
        X = x;
        Y = y;
        Color = color;
    }

    public void Draw()
    {
        Program.SafeSetCursorPosition(X, Y);
        Console.ForegroundColor = Color;
        Console.Write("*");
    }
}

class Shark
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public string[] Design { get; } = {
        @"          /|            ",
        @"         / |            ",
        @"        /  |            ",
        @"  _____/   |__________  ",
        @" /                    \ ",
        @"/   O  |||             \",
        @"\_____________________  \",
        @"                      \_/"
    };
    public ConsoleColor[] Colors { get; } = {
        ConsoleColor.Gray,
        ConsoleColor.Gray,
        ConsoleColor.Gray,
        ConsoleColor.DarkGray,
        ConsoleColor.DarkGray,
        ConsoleColor.DarkGray,
        ConsoleColor.DarkGray,
        ConsoleColor.Gray
    };
    private bool active = false;
    private readonly Random Random = new Random();

    public Shark(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Update()
    {
        if (!active)
        {
            if (Random.Next(0, 200) == 0)
            {
                active = true;
                X = Console.WindowWidth;
                Y = Random.Next(2, Math.Max(2, Console.WindowHeight - 12));
            }
        }
        else
        {
            X -= 2;
            if (X < -30) active = false;
        }
    }

    public void Draw()
    {
        if (!active) return;
        for (int i = 0; i < Design.Length; i++)
        {
            Program.SafeDrawString(Design[i], X, Y + i, Colors[i]);
        }
    }

    public void Clear()
    {
        if (!active) return;
        for (int i = 0; i < Design.Length; i++)
        {
            // Clear a slightly larger area to handle the speed of 2
            Program.SafeClearString(X, Y + i, Design[i].Length + 2);
        }
    }
}
