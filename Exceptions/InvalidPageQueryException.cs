using System.Text;

namespace FarmOrganizer.Exceptions
{
    internal class InvalidPageQueryException : Exception
    {
        public Dictionary<string, string> Queries { get; } = new();

        public InvalidPageQueryException(Dictionary<string, string> query) 
        {
            Queries = query;
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            foreach (var entry in Queries)
            {
                builder.AppendLine($"[{entry.Key},{entry.Value}]");
            }
            return builder.ToString();
        }
    }
}
