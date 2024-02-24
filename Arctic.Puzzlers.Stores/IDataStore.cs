namespace Arctic.Puzzlers.Stores
{
    public interface IDataStore : IDisposable
    {
        public bool SupportedStoreType(string storeType);        
    }
}
