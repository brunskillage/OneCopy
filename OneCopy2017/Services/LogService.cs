using System;

namespace OneCopy2017.Services
{
    public class LogService
    {
        public void Print(string s)
        {
            Console.WriteLine($"[Info] {s}");
        }
    }
}