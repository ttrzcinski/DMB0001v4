namespace DMB0001v4.Mind
{
    public class Ask
    {
        public int Id { get; set; }

        public string Question { get; set; }

        public string[] Answers { get; set; }

        public string[] Responses { get; set; }

        public string FinalAnswer { get; set; }

        public bool Processed { get; set; }
    }
}
