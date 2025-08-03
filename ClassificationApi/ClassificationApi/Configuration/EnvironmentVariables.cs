using EasyReasy.EnvironmentVariables;

namespace ClassificationApi.Configuration
{
    [EnvironmentVariableNameContainer]
    public static class EnvironmentVariable
    {
        [EnvironmentVariableName(minLength: 10)]
        public static readonly VariableName ByteShelfBaseUrl = new VariableName("BYTESHELF_BASE_URL");
        
        [EnvironmentVariableName(minLength: 20)]
        public static readonly VariableName ByteShelfApiKey = new VariableName("BYTESHELF_API_KEY");
        
        [EnvironmentVariableName]
        public static readonly VariableName ByteShelfCacheDir = new VariableName("BYTESHELF_CACHE_DIR");
    }
}