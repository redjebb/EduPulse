using System;

namespace C__проект_shcolo.UI
{
    public static class InputValidator
    {
        public static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(prompt);
                Console.ResetColor();
                string input = Console.ReadLine() ?? "";
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                {
                    return value;
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Моля, въведете число от {min} до {max}.");
                Console.ResetColor();
            }
        }

        public static void Pause()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine();
            Console.Write("Натиснете Enter, за да продължите...");
            Console.ResetColor();
            Console.ReadLine();
        }
    }
}
