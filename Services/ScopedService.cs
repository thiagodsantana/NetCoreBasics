using NetCoreBasics.Interfaces;

namespace NetCoreBasics.Services
{
    public class ScopedService : IScopedService
    {
        private readonly string _id;
        public ScopedService() => _id = Guid.NewGuid().ToString();
        public string GetId() => _id;
    }
}
