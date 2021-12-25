namespace DocumentMaker.Resources
{
    internal class ResourceInfo
    {
        public ResourceInfo(string name, string template = "")
        {
            ProjectName = name;
            TemplateName = template;
        }

        public string ProjectName { get; }
        public string TemplateName { get; }
    }
}
