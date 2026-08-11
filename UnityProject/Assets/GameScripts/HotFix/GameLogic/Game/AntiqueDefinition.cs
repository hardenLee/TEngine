namespace GameLogic
{
    /// <summary>
    /// 一件待鉴定藏品的静态定义。后续可由 Luban antique 表反序列化为此模型。
    /// </summary>
    public sealed class AntiqueDefinition
    {
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string ImageAddress { get; }
        public AntiqueVerdict CorrectVerdict { get; }
        public string CorrectLine { get; }
        public string WrongLine { get; }
        public int BaseScore { get; }

        public AntiqueDefinition(int id, string name, string description, string imageAddress,
            AntiqueVerdict correctVerdict, string correctLine, string wrongLine, int baseScore = 100)
        {
            Id = id;
            Name = name;
            Description = description;
            ImageAddress = imageAddress;
            CorrectVerdict = correctVerdict;
            CorrectLine = correctLine;
            WrongLine = wrongLine;
            BaseScore = baseScore;
        }
    }
}
