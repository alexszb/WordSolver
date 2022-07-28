using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace WordSolver
{
    class Table
    {
        public Stopwatch sw { get; set; }

        public char[,] BonusPlaces { get; set; }

        public Dictionary<Word, int> WordsWorth = new Dictionary<Word, int>();

        public Dictionary<string, int> LetterValues = new Dictionary<string, int>();

        public List<List<int[]>> ValidWords = new List<List<int[]>>();


        public char[,] CurrentTable { get; set; }

        public List<string> WordsOnTable { get; set; }

        public Table()
        {
            this.CurrentTable = new char[6, 6];
            this.WordsOnTable = new List<string>();
            this.sw = new Stopwatch();
            this.BonusPlaces = new char[6, 6];
        }

        //Draws a word with a specific speed.
        public void Automate(Word w, int speed)
        {

            string solutionDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            var areaVals = File.ReadAllText(solutionDirectory + "/Config/Area.txt").Split(",");



            var arr = w.points.ToArray();

            int startx = 767;
            int starty = 512;
            int diff = 111;

            startx = int.Parse(areaVals[0]);
            starty = int.Parse(areaVals[1]);
            diff = int.Parse(areaVals[2]);

            SetCursorPos((startx + ((arr[0][1] - 1) * diff)), (starty + ((arr[0][0] - 1) * diff)));
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(Util.rnd.Next(15,26));
            for (int i = 0; i < arr.Length-1; i++)
            {
                int stx = startx + ((arr[i][1] - 1) * diff) /*+ Util.rnd.Next(-8,8)*/;
                int sty = starty + ((arr[i][0] - 1) * diff) /*+ Util.rnd.Next(-8, 8)*/;
                 int endx = startx + ((arr[i+1][1] - 1) * diff) /*+ Util.rnd.Next(-8, 8)*/;
                int endy = starty + ((arr[i + 1][0] - 1) * diff) /*+ Util.rnd.Next(-8, 8)*/;
                 LeftMouseClick(stx, sty, endx, endy, speed);
                Thread.Sleep(speed);
            }
            Thread.Sleep(Util.rnd.Next(15, 26));
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Thread.Sleep(Util.rnd.Next(15, 26));
        }

        //Finds all possible words score value on the table.
        public void CheckPoint(List<int[]> points)
        {
            int redcount = 0;
            int yellowcount = 0;
            double szum = 0;
            string word = "";
            foreach (var item in points)
            {
                word += CurrentTable[item[0], item[1]];

                if (BonusPlaces[item[0], item[1]] == 49)
                {
                    szum += LetterValues[CurrentTable[item[0], item[1]].ToString().ToLower()] * 2;
                }
                else if (BonusPlaces[item[0], item[1]] == 50)
                {
                    szum += LetterValues[CurrentTable[item[0], item[1]].ToString().ToLower()] * 3;
                }
                else if (BonusPlaces[item[0], item[1]] == 51)
                {
                    redcount++;
                    szum += LetterValues[CurrentTable[item[0], item[1]].ToString().ToLower()];
                }
                else if (BonusPlaces[item[0], item[1]] == 52)
                {
                    yellowcount++;
                    szum += LetterValues[CurrentTable[item[0], item[1]].ToString().ToLower()];
                }
                else
                {
                    szum += LetterValues[CurrentTable[item[0], item[1]].ToString().ToLower()];
                }
            }

            if (redcount != 0)
            {
                if (yellowcount != 0)
                {
                    szum = (redcount * 2) * (yellowcount * 3) * szum;
                }
                else
                {
                    szum = (redcount * 2) * szum;
                }
            }
            else
            {
                if (yellowcount != 0)
                {
                    szum = (yellowcount * 3) * szum;
                }
            }

            if (points.Count == 2)
            {
                szum += 3;
            }
            if (points.Count == 3)
            {
                szum += 4;
            }
            if (points.Count == 4)
            {
                szum += 6;
            }
            if (points.Count == 5)
            {
                szum += 9;
            }
            if (points.Count == 6)
            {
                szum += 11;
            }
            if (points.Count == 7)
            {
                szum += 14;
            }
            if (points.Count == 8)
            {
                szum += 17;
            }
            if (points.Count == 9)
            {
                szum += 19;
            }

            Word w = new Word(word.ToLower(), points);

            var q = WordsWorth.Any(x => x.Key.word == w.word);
            var same = WordsWorth.Where(x => x.Key.word == w.word).FirstOrDefault();

            if (!q)
            {
                WordsWorth.Add(w, (int)szum);
            }
            else
            {
                if (same.Value <= (int)szum)
                {
                    WordsWorth.Remove(same.Key);
                    WordsWorth.Add(w, (int)szum);
                }
            }
        }

        //Gets the letters and bonuses on the table.
        public void FillTable()
        {
            Console.WriteLine("Press enter to start...");
            Console.ReadLine();
            ReadImage.Reader.Read();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    CurrentTable[i + 1, j + 1] = ReadImage.Reader.chartable[j, i][0];

                    if (ReadImage.Reader.bonusColorTable[j,i] == 4)
                    {
                        BonusPlaces[i + 1, j + 1] = '4';
                    }
                    if (ReadImage.Reader.bonusColorTable[j, i] == 3)
                    {
                        BonusPlaces[i + 1, j + 1] = '3';
                    }
                    if (ReadImage.Reader.bonusColorTable[j, i] == 2)
                    {
                        BonusPlaces[i + 1, j + 1] = '2';
                    }
                    if (ReadImage.Reader.bonusColorTable[j, i] == 1)
                    {
                        BonusPlaces[i + 1, j + 1] = '1';
                    }
                    if (ReadImage.Reader.bonusColorTable[j, i] == 0)
                    {
                        BonusPlaces[i + 1, j + 1] = '0';
                    }
                }
            }

        }

        //Shows the read table.
        public void ShowTable(char[,] table)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            for (int i = 1; i < table.GetLength(0)-1; i++)
            {
                for (int j = 1; j < table.GetLength(1)-1; j++)
                {
                    Console.Write(table[i, j]);
                    Console.Write("\t");
                }
                Console.WriteLine("\n");
            }
        }

        //Creates a copy from current table.
        public char[,] CopyTable(char[,] Original)
        {
            char[,] CopyTable = new char[6, 6];
            for (int i = 1; i < Original.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < Original.GetLength(1) - 1; j++)
                {
                    CopyTable[i, j] = Original[i, j].ToString().ToLower()[0];
                }
            }
            return CopyTable;
        }

        //Adds found valid words positions to ValidWords.
        public void AddToList(List<int[]> points)
        {
            List<int[]> newpoints = new List<int[]>();
            foreach (var item in points)
            {
                newpoints.Add(item);
            }
            ValidWords.Add(newpoints);
        }

        //Draws a words path on the table.
        public void DrawPath(List<int[]> points)
        {
            char[,] table = new char[6, 6];
            for (int i = 1; i < table.GetLength(0)-1; i++)
            {
                for (int j = 1; j < table.GetLength(1)-1; j++)
                {
                    table[i, j] = 'X';
                }
            }
            int n = 0;
            foreach (var item in points)
            {
                table[item[0], item[1]] = n.ToString()[0];
                n++;
            }

            ShowTable(table);
        }

        //Searches for a word in the table.
        public void Searcher(string word)
        {
            char[,] Copied = CopyTable(CurrentTable);

            int i = 1;
            int j = 1;
            List<int[]> points = new List<int[]>();
            while (i< Copied.GetLength(0)-1)
            {
                j = 1;
                while(j<Copied.GetLength(1)-1)
                {
                    if (Copied[i, j] == word[0])
                    {
                        int[] p = new int[] { i, j };
                        FindNextChar(Copied, 1, word, p, points);
                        points.Remove(p);
                    }
                    points = new List<int[]>();
                    j++;
                }
                i++;
            }
        }

        public void FindNextChar(char[,] table, int index, string word, int[] Point, List<int[]> points)
        {
            points.Add(Point);
            char backup = table[Point[0], Point[1]];
            int nextindex = index;
            nextindex++;

            for (int i = Point[0] - 1; i < Point[0] + 2; i++)
            {
                for (int j = Point[1] - 1; j < Point[1] + 2; j++)
                {
                    table[Point[0], Point[1]] = '0';

                    if (table[i, j] == word[index])
                    {
                        if (nextindex < word.Length)
                        {
                            int[] p = new int[] { i, j };
                            FindNextChar(table, nextindex, word, p, points);
                        }
                        else if (index == word.Length - 1)
                        {
                            {
                                WordsOnTable.Add(word);
                                int[] lastpoint = new int[] { i, j };
                                points.Add(lastpoint);
                                AddToList(points);
                                points.Remove(points.Last());
                            }
                        }
                    }
                    table[Point[0], Point[1]] = backup;
                }
            }

            points.Remove(Point);

        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //Simulates a left click between two coordinates.
        public static void LeftMouseClick(int xpos, int ypos, int xend, int yend, int speed)
        {
            SetCursorPos(xpos, ypos);
            SetCursorPos(xend, yend);
        }
    }
}
