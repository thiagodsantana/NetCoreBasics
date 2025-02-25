using NetCoreBasics.Interfaces;

namespace NetCoreBasics.Services
{
    public class SingletonService : ISingletonService
    {
        private readonly string _id;
        public SingletonService() => _id = Guid.NewGuid().ToString();
        public string GetId() => _id;
    }
}
