namespace URLShortener.Common.Settings
{
    /// <summary>
    /// Type for common top level settings
    /// </summary>
    public class Settings
    {
        public string Base62Characters { get; set; }
        public int EncodingLength { get; set; }
    }
}