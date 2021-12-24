namespace DocumentMaker.Model
{
    class BackDataModel
    {
        public uint Id { get; set; }
        public BackType Type { get; set; }
        public string BackNumberText { get; set; }
        public string BackName { get; set; }
        public string BackCountRegionsText { get; set; }
        public bool IsRework { get; set; }
        public string SpentTimeText { get; set; }
    }
}
