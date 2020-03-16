namespace cslox
{
    public interface IErrorReporter
    {
        void AddScannerError(int line, string message);
        void AddParserError(Token token, string message);
        void AddRuntimeError(RuntimeError err);
    }
}
