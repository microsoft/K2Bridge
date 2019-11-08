using System;
namespace K2Bridge
{
    // Thrown when trying to visit a clause and it mandatory properties
    // are wrong/missing
    public class IllegalClauseException: Exception
    {
        public IllegalClauseException() :
            base("Clause is missing mandatory properties or has invalid values")
        {
        }

        public IllegalClauseException(string message) : base(message)
        {
        }

        public IllegalClauseException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
    }
}
