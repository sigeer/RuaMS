using Application.Core.Login.Services;
using Application.Utility.Tasks;

namespace Application.Core.Login.Tasks
{
    public class CharacterAutosaverTask : AbstractRunnable
    {
        readonly StorageService _storeSrv;

        public CharacterAutosaverTask(StorageService storeSrv)
        {
            _storeSrv = storeSrv;
        }

        public override void HandleRun()
        {
            _storeSrv.CommitAll();
        }
    }
}
