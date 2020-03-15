namespace cslox
{
    public interface IErrorReporter
    {
        void AddError(int line, string message);
        void AddError(Token token, string message);
    }
}
