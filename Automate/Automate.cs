using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;


namespace Automate
{
	internal class Automate
	{
		private List<char> signals = new List<char>();
		private List<char> states = new List<char>();
		private List<char> finals = new List<char>();
        private List<char> unreachable = new List<char> ();
		private List<char> deadlock = new List<char>();
        private List<List<char>> classes = new List<List<char>>();
		private char start;
        private Dictionary<char, Dictionary<char, char>> distinct = new Dictionary<char, Dictionary<char, char>>();
        private Dictionary<char, Dictionary<char, char>> delta = new Dictionary<char, Dictionary<char, char>>();

		public Automate(string inputFileName)
		{
			string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "/" + inputFileName, Encoding.UTF8);
			start = Convert.ToChar(lines[2]);
			char[] delimiters = {' '};
			var symbols = lines[3].Split(delimiters);
			for (int j = 1; j < symbols.Length; j++)
			{
				finals.Add(Convert.ToChar(symbols[j]));	
			}
			for (int i = 4; i < lines.Length; i++)
			{
				symbols = lines[i].Split(delimiters);
				char startState = Convert.ToChar(symbols[0]);
				char signal = Convert.ToChar(symbols[1]);
				char targetState = Convert.ToChar(symbols[2]);
				if (!states.Contains(startState))
					states.Add(startState);
				if (!states.Contains(targetState))
					states.Add(targetState);
                if (!signals.Contains(signal))
                    signals.Add(signal);
				if (delta.ContainsKey(startState))
					delta[startState].Add(signal, targetState);
				else
					delta.Add(startState, new Dictionary<char, char> {{signal, targetState}});
			}
            foreach (var state1 in states)
            {
                foreach (var state2 in states)
                {
                    if (!distinct.Keys.Contains(state1))
                    {
                        distinct.Add(state1, new Dictionary<char, char>());
                    }
                    distinct[state1].Add(state2, ' ');
                }
            }
            DefineUnreachableAndDeadlockStates();
		}

        public void Show ()
        {
            Console.Write("Signals: ");
            Program.Print(signals);
            Console.Write("\nStates: ");
            Program.Print(states);
            Console.Write("Start: {0}", start);
            Console.Write("\nFinal: ");
            Program.Print(finals);
            Console.Write("\nUnreachable: ");
            Program.Print(unreachable);
            Console.Write("Deadlock: ");
            Program.Print(deadlock);
        }

        private void DefineUnreachableAndDeadlockStates()
		{
			HashSet<char> marked = new HashSet<char>();
			Stack<char> stack = new Stack<char>();
			stack.Push(start);
			marked.Add(start);
			while (stack.Count != 0)
			{
                char currentState = stack.Pop();
				if (delta.ContainsKey(currentState))
				{
					foreach (var signalStatePair in delta[currentState])
					{
						if (!marked.Contains(signalStatePair.Value))
						{
							stack.Push(signalStatePair.Value);
							marked.Add(signalStatePair.Value);
						}
					}
				}
				else if (!finals.Contains(currentState))
				{
					deadlock.Add(currentState);
				}
			}
            unreachable = new List<char> (states);
            unreachable.RemoveAll(state => marked.Contains(state)); 
		}

        public void Minimize ()
        {
            foreach(var p in states)
            {
                foreach (var q in states)
                {
                    if (!p.Equals(q) && ((finals.Contains(p) && !finals.Contains(q)) || (!finals.Contains(p) && finals.Contains(q))))
                    {
                        distinct[p][q] = 'x';
                    }
                }
            }

            bool mayBeOptimized = true;
            
            while (mayBeOptimized)
            {
            label1:
                mayBeOptimized = false;
                foreach (var p in states)
                {
                    foreach (var q in states)
                    {
                        if (!p.Equals(q) && delta.Keys.Contains(p) && delta.Keys.Contains(q))
                        {
                            foreach (var signal in signals)
                            {
                                if (distinct[p][q] != 'x' &&
                                    delta[p].Keys.Contains(signal) && delta[q].Keys.Contains(signal) &&
                                    distinct[delta[p][signal]][delta[q][signal]] == 'x')
                                {
                                    distinct[p][q] = 'x';
                                    mayBeOptimized = true;
                                    goto label1;
                                }
                            }
                        }
                    }
                }
            }
            Console.Write("\n");
            Program.Print(distinct);
        }

        public void DeleteExcessStates ()
        {
            states.RemoveAll((state) => unreachable.Contains(state) || deadlock.Contains(state));
            foreach (var keyY in distinct.Keys)
            {
                bool isChecked = false;
                foreach (var stateClass in classes)
                {
                    if (stateClass.Contains(keyY))
                    {
                        isChecked = true;
                        break;
                    }
                } 
                if (!isChecked)
                {
                    List<char> statesClass = new List<char>();
                    statesClass.Add(keyY);
                    foreach (var keyX in distinct[keyY].Keys)
                    {
                        if (keyY != keyX && distinct[keyY][keyX] != 'x' && states.Contains(keyX) && states.Contains(keyY))
                        {
                            statesClass.Add(keyX);
                            states.Remove(keyX);
                        }
                    }
                    classes.Add(statesClass);
                }
            }
        }

        public void Write (int number)
        {
            List<string> lines = new List<string> ();
            lines.Add(signals.Count.ToString());
            lines.Add(states.Count.ToString());
            lines.Add(start.ToString());
            int finalsCount = 0;
            string finalsStr = " ";
            foreach (var final in finals)
            {
                if (states.Contains(final))
                {
                    finalsCount++;
                    finalsStr += final + " ";
                }
            }
            lines.Add(finalsCount + finalsStr);

            foreach (var key1 in delta.Keys)
            {
                if (!unreachable.Contains(key1) && !deadlock.Contains(key1))
                {
                    string record = "";
                    foreach (var stateClass in classes)
                    {
                        if (stateClass.Contains(key1))
                        {
                            record += stateClass[0].ToString();
                            break;
                        }
                    }

                    foreach (var key2 in delta[key1].Keys)
                    {
                        foreach (var stateClass in classes)
                        {
                            if (stateClass.Contains(delta[key1][key2]))
                            {
                                lines.Add(record + " " + key2 + " " + stateClass[0]);
                                break;
                            }
                        }
                    }
                }
            }

            lines = new List<string>(lines.Distinct());
            string path = Directory.GetCurrentDirectory() + "/input/1/output" + number + ".txt";
            string output = "";
            foreach (var line in lines)
            {
                output += line + "\n";
            }
            File.WriteAllText(path, output);
        }
	}
}
