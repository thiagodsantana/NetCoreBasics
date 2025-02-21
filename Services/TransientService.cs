using NetCoreBasics.Interfaces;

namespace NetCoreBasics.Services
{
    public class TransientService : ITransientService
    {
        public string GetId() => Guid.NewGuid().ToString();
    }
}
