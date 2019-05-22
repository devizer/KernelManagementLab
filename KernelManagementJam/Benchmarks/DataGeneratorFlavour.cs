namespace KernelManagementJam.Benchmarks
{
    public enum DataGeneratorFlavour
    {
        Random,
        StableRandom,
        
        // Text
        LoremIpsum,
        StableLoremIpsum,
        
        // 42 (maximum compression)
        FortyTwo,
        
        // An MS IL binary 
        ILCode
    }
}