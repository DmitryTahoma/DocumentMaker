namespace DocumentMaker.Resources
{
    internal class ResourceInfo
    {
        public ResourceInfo(ResourceType type, string name, string template = "")
        {
            Type = type;
            ProjectName = name;
            TemplateName = template;
        }

        public string ProjectName { get; }
        public string TemplateName { get; }
        public ResourceType Type { get; }
    }
}
