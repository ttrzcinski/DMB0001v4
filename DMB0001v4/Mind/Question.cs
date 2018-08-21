using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMB0001v4.Mind
{
    public class Question
    {
        public int id { get; set; }

        public string question { get; set; }

        public string[] answers { get; set; }

        public string[] responses { get; set; }

        public string FinalAnswer { get; set; }

        public bool Processed { get; set; }
    }
}
