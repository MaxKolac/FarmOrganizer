using System.Text;

namespace FarmOrganizer.Exceptions
{
    internal class InvalidPageQueryException : Exception
    {
        public IDictionary<string, object> Queries { get; }

        public InvalidPageQueryException(IDictionary<string, object> query) 
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
