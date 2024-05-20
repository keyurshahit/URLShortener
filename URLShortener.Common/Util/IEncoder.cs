namespace URLShortener.Common.Util
{
    /// <summary>
    /// id to string encode/decode contract
    /// </summary>
    public interface IEncoder
    {
        string Encode(long id);
        long Decode(string input);
    }
}
