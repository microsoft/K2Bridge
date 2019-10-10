namespace K2Bridge.Models.Request
{
    using K2Bridge.Visitors;

    internal interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}
