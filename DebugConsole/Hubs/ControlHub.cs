using System.Threading.Tasks;
using DebugConsole;
using Microsoft.AspNetCore.SignalR;

namespace UChainDB.Example.Chain.DebugConsole.Hubs
{
    public class ControlHub : Hub
    {
        private readonly IHubContext<ControlHub> hubcontext;
        private readonly ControlService controlService;

        public ControlHub(IHubContext<ControlHub> hubcontext, ControlService controlService)
        {
            this.hubcontext = hubcontext;
            this.controlService = controlService;
        }

        public async Task Start()
        {
            await this.controlService.Start();
        }

        public async Task Stop()
        {
            await this.controlService.Stop();
        }

        public async Task AddNode()
        {
            await this.controlService.AddNode();
        }
    }
}