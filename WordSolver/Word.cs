using System.Collections.Generic;

namespace WordSolver
{
    class Word
    {
        public List<int[]> points { get; set; }
        public string word { get; set; }

        public Word(string word, List<int[]> points)
        {
            this.points = points;
            this.word = word;
        }
    }
}
