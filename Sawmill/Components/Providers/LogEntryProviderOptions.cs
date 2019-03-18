namespace Sawmill.Components.Providers
{
    public class LogEntryProviderOptions
    {
        /// <summary>
        /// Path of the input file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Maximum accepted line length in characters.
        /// </summary>
        public int MaxLineLength { get; set; } = 10024;
    }
}
