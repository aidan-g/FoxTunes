namespace FoxTunes.Interfaces
{
    public interface ISortParserProvider : IStandardComponent
    {
        bool TryParse(string sort, out ISortParserResultExpression expression);
    }
}
