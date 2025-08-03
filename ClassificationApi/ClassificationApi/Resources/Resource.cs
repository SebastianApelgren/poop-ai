namespace ClassificationApi.Resources
{
    public readonly struct Resource
    {
        public string Path { get; }

        private Resource(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Warning, this is dangerous and should not be used unless the string it's used with is from a serialized resource instance so that it's certain it exists!
        /// Should not be used unless it can be certain that the resource path is valid and exists since this can't be checked before hand!
        /// </summary>
        /// <param name="resourcePath">The verifyed resource path that is known to be of an existing resource that has then been serialized.</param>
        /// <returns>A resource instance.</returns>
        public static Resource Create(string resourcePath)
        {
            return new Resource(resourcePath);
        }

        public string GetFileName()
        {
            return System.IO.Path.GetFileName(Path);
        }

        public override string ToString() => Path;

        public static implicit operator string(Resource resourcePath) => resourcePath.Path;

        // Categories (no redundant "Resource" prefix)
        public static class Models
        {
            public static readonly Resource StoolModel = new Resource("Models/kf_sttool_classification_628.onnx");
        }

        public static class Frontend
        {
            public static readonly Resource StoolClassificationFrontend = new Resource("Frontend/StoolClassificationFrontend.html");
        }
    }
}
