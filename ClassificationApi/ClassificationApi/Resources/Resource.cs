using EasyReasy;
using EasyReasy.ByteShelfProvider;

namespace ClassificationApi.Resources
{
    [ResourceCollection(typeof(ByteShelfResourceProvider))]
    public static class Models
    {
        public static readonly Resource StoolModel = new Resource("StoolClassification/kf_stool_classification_628.onnx");
    }

    [ResourceCollection(typeof(EmbeddedResourceProvider))]
    public static class Frontend
    {
        public static readonly Resource StoolClassificationFrontend = new Resource("Frontend/StoolClassificationFrontend.html");
    }
}