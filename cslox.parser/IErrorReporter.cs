namespace cslox
{
    public interface IErrorReporter
    {
        int Count { get; }

        void AddScannerError(int line, string message);
        void AddParserError(Token token, string message);
        void AddRuntimeError(RuntimeError err);
        void AddResolverError(Token name, string message);
    }
}
