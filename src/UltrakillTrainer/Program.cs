using System;
using System.Threading;

namespace UltrakillTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "ULTRAKILL Trainer - Infinite Stamina";

            Memory mem = new Memory();
            Console.Write("Menunggu ULTRAKILL");
            while (!mem.OpenProcess("ULTRAKILL"))
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }
            Console.WriteLine("\n[OK] Game ketemu!");

            GameObjects objs = new GameObjects(mem);

            bool stamOn = false;

            Thread worker = new Thread(() =>
            {
                while (true)
                {
                    if (stamOn) objs.ApplyInfiniteStamina();
                    Thread.Sleep(50);
                }
            });
            worker.IsBackground = true;
            worker.Start();

            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("       ULTRAKILL TRAINER (STAMINA)      ");
                Console.WriteLine("========================================");
                Console.WriteLine(" [1] Infinite Stamina : " + (stamOn ? "ON" : "OFF"));
                Console.WriteLine("----------------------------------------");
                Console.WriteLine(" [0] Keluar");
                Console.WriteLine("========================================");

                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        stamOn = !stamOn;
                        break;
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        running = false;
                        break;
                }
            }
        }
    }
}