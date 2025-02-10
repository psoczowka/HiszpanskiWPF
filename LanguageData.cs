using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiszpanskiWpf
{
    public class Word
    {
        public string Spanish { get; set; }
        public string Polish { get; set; }
    }

    public class Lesson
    {
        public string Title { get; set; }
        public List<Word> Words { get; set; }
    }

    public class Chapter
    {
        public string Title { get; set; }
        public List<Lesson> Lessons { get; set; }
    }

    public class LanguageData
    {
        public List<Chapter> Chapters { get; set; }
    }
}
