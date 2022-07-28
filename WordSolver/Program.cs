using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WordSolver
{

    struct Point
    {
        public int X;
        public int Y;
    }

    class Program
    {
        static string solutionDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;

        static string[] bWords = File.ReadAllLines(solutionDirectory + "/Words/words.txt", Encoding.Unicode);
        static string[] eWords = File.ReadAllLines(solutionDirectory + "/Words/extWords.txt", Encoding.Unicode);
        static List<string> triedWords = new List<string>();

        static int userDefinedPointToGet;

        static List<Dictionary<Word, int>> positions = new List<Dictionary<Word, int>>();


        static void Main(string[] args)
        {

            Console.WriteLine("---\tMENU\t---");
            Console.WriteLine("1 - Start to play");
            Console.WriteLine("2 - Game area configuration");

            int menuOpt;
            bool validNum = int.TryParse(Console.ReadLine(), out menuOpt);

            if (menuOpt == 2)
            {
                GetGameArea();
            }

            if (menuOpt == 1)
            {
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Enter how many points you want! (skip with ENTER for unlimited)");

                    positions = new List<Dictionary<Word, int>>();
                    int userDefinedPointToGetParsed;
                    bool success = int.TryParse(Console.ReadLine(), out userDefinedPointToGetParsed);
                    if (success)
                    {
                        userDefinedPointToGet = userDefinedPointToGetParsed;
                    }
                    else
                    {
                        userDefinedPointToGet = 0;
                    }

                    Table table = new Table();
                    StreamReader sr = new StreamReader(solutionDirectory + "/Words/letterValues.txt");

                    while (!sr.EndOfStream)
                    {
                        string[] s = sr.ReadLine().Split(',');
                        table.LetterValues.Add(s[0], int.Parse(s[1]));
                    }

                    sr.Close();
                    table.FillTable();
                    table.ShowTable(table.CurrentTable);
                    Console.WriteLine();
                    table.ShowTable(table.BonusPlaces);
                    table.sw.Start();

                    Thread.Sleep(500);
                    Solve(bWords, table, 20);
                    if (userDefinedPointToGet.ToString() == "")
                    {
                        Solve(eWords, table, 20);
                    }
                    Console.ReadLine();
                    triedWords = new List<string>();
                }
            }
        }

        //Searches for a word with a specific score in the table.
        private static void sum_up(Dictionary<Word, int> numbers, int target)
        {
            sum_up_recursive(numbers, target, new List<int>(), new Dictionary<Word, int>());
        }

        private static void sum_up_recursive(Dictionary<Word, int> numbers, int target, List<int> partial, Dictionary<Word, int> pos)
        {
            if (positions.Count == 0)
            {
                int s = 0;
                foreach (int x in partial) s += x;

                if (s == target)
                {
                    positions.Add(pos);
                }

                if (s >= target)
                    return;

                foreach (var item in numbers)
                {
                    Dictionary<Word, int> remaining = new Dictionary<Word, int>();
                    int n = item.Value;
                    foreach (var w in numbers)
                    {
                        remaining.Add(w.Key, w.Value);
                    }
                    remaining.Remove(item.Key);

                    List<int> partial_rec = new List<int>(partial);
                    Dictionary<Word, int> rec_pos = new Dictionary<Word, int>(pos);
                    partial_rec.Add(n);
                    rec_pos.Add(item.Key, item.Value);
                    sum_up_recursive(remaining, target, partial_rec, rec_pos);
                }
            }
        }

        //Draws all possible words from a dictionary.
        private static void Solve(string[] allwords, Table table, int speed)
        {
            foreach (var item in allwords)
            {
                if (item.Length > 1)
                {
                    table.Searcher(item.ToLower());
                }
            }

            foreach (var item in table.ValidWords)
            {
                //table.DrawPath(item);
                table.CheckPoint(item);
            }


            var w = table.WordsWorth.OrderByDescending(x => x.Value);
            int target = userDefinedPointToGet;

            if (userDefinedPointToGet != 0)
            {
                sum_up(table.WordsWorth, target);

                foreach (var v in positions)
                {
                    foreach (var item in v)
                    {
                        table.Automate(item.Key, speed);
                    }
                }
            }

            if (userDefinedPointToGet.ToString() == "")
            {
                foreach (var item in w)
                {
                    if (table.sw.ElapsedMilliseconds < 76000)
                    {
                        if (!(triedWords.Contains(item.Key.word)))
                        {
                            triedWords.Add(item.Key.word);
                            table.Automate(item.Key, speed);
                        }
                    }
                }
            }

            table.WordsWorth = new Dictionary<Word, int>();
            table.ValidWords = new List<List<int[]>>();
            table.WordsOnTable = new List<string>();
        }

        static void GetGameArea()
        {
            Console.WriteLine("Move the windows that way, that you'll be able to click on the game letter area.");
            Console.WriteLine("You'll have to click on the center of the top left letter, then wait 2 seconds and click on the center of the bottom right letter!");
            Console.WriteLine("Press ANY key to continue.");
            Console.ReadLine();
            int[] topLeft = GetClickCoordinates();

            Thread.Sleep(2000);
            int[] bottomRight = GetClickCoordinates();

            int diff = (bottomRight[0]-topLeft[0])/3;

            Console.WriteLine(topLeft[0] + "\t"  + topLeft[1] + "\t" + diff);
            File.WriteAllText(solutionDirectory + "/Config/Area.txt", topLeft[0] + "," + topLeft[1] + "," + diff);

        }

        static int[] GetClickCoordinates()
        {
            bool edgeClicked = false;
            int[] coordinates = new int[2];

            while (!edgeClicked)
            {
                [System.Runtime.InteropServices.DllImport("user32.dll")]
                static extern bool GetAsyncKeyState(UInt16 virtualKeyCode);
                if (GetAsyncKeyState(0x01))
                {
                    [System.Runtime.InteropServices.DllImport("user32.dll")]
                    static extern bool GetCursorPos(out Point point);
                    GetCursorPos(out Point point);
                    Console.WriteLine($"X:{point.X}; Y:{point.Y}");
                    coordinates[0] = point.X;
                    coordinates[1] = point.Y;
                    edgeClicked = true;
                }
                Thread.Sleep(50);
            }
            return coordinates;
        }
    }

    class Util
    {
        public static Random rnd = new Random();
    }
}
