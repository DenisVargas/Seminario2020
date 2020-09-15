namespace Core.Interaction
{
    [System.Serializable]
    public enum OperationType : int
    {
        Take = 0,
        Ignite,
        Activate,
        Equip,
        Throw,
        inspect,
        Combine,
        Exchange,
        Drop,
        use,
    }
}
