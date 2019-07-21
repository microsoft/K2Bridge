namespace K2Bridge
{
    internal interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}
