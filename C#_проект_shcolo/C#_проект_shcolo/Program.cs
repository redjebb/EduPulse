using System;
using System.IO;
using System.Text;
using C__проект_shcolo.Models;
using C__проект_shcolo.Services;
using C__проект_shcolo.UI;

namespace C__проект_shcolo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            JsonDataService storage = new JsonDataService();
            try
            {
                SchoolDatabase database = storage.Load();
                new ConsoleMenu(database, storage).Run();
            }
            catch (OperationCanceledException)
            {
                Console.ResetColor();
                Console.WriteLine("Входът е приключил. Изход от EduPulse.");
            }
            catch (InvalidDataException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                Console.WriteLine("Поправете school.json или възстановете резервно копие.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Неуспешно стартиране: " + ex.Message);
                Console.ResetColor();
            }
        }
    }
}
