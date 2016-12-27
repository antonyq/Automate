using System;
using System.Collections.Generic;

namespace Automate
{
    class Program
    {
		public static void Print(IEnumerable<char> symbols)
		{
			foreach (var symbol in symbols)
			{
				Console.Write(symbol + " ");
			}
            Console.Write("\n");
		}

        public static void Print (Dictionary<char, Dictionary<char, char>> dictDicts)
        {
            Console.Write("  ");
            foreach (var key in dictDicts.Keys)
            {
                Console.Write(key + " ");
            }
            Console.Write("\n");

            foreach (var outerKey in dictDicts.Keys)
            {
                Console.Write(outerKey + " ");
                foreach (var innerKey in dictDicts.Keys)
                {
                    Console.Write(dictDicts[outerKey][innerKey] + " ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }

	    static void Main(string[] args)
	    {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
			string[] inputs = { "in1", "in2", "in3", "in4", "in5", "in6", "in7" };
            foreach(var input in inputs){
                Automate automate = new Automate("input/1/" + input + ".txt");
                automate.Show();
                automate.Minimize();
                automate.DeleteExcessStates();
                automate.Write(Convert.ToInt32(input.Split('n')[1]));    
            }
            Console.Read();
        }
    }
}