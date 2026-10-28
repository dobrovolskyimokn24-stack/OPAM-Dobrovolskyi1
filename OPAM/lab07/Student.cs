namespace lab07
{
    public class Student
    {
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public int Score { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Group}) - {Score} балів";
        }
    }
}