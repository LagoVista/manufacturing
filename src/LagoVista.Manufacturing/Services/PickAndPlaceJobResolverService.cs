using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Models;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Services
{
    public class PickAndPlaceJobResolverService
    {
        IMachineManager _machineManager;
        IPickAndPlaceJobManager _jobManager;
        IStripFeederManager _stripFeederManager;
        IAutoFeederManager _autoFeederManager;
        ICircuitBoardManager _circuitBoardManager;
        ILogger _logger;

        public PickAndPlaceJobResolverService(IMachineManager machineManager, IStripFeederManager stripFeederManager, IAutoFeederManager autoFeeder, 
           ICircuitBoardManager circuitBoardManager, IPickAndPlaceJobManager jobManager, IAdminLogger adminLogger)
        {
            _machineManager = machineManager ?? throw new ArgumentNullException(nameof(machineManager));
            _jobManager = jobManager ?? throw new ArgumentNullException(nameof(jobManager));
            _logger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _stripFeederManager = stripFeederManager ?? throw new ArgumentNullException(nameof(stripFeederManager));
            _autoFeederManager = autoFeeder ?? throw new ArgumentNullException(nameof(autoFeeder));
            _circuitBoardManager = circuitBoardManager ?? throw new ArgumentNullException(nameof(circuitBoardManager));
        }

        public async Task<InvokeResult> ResolveJobAsync(string machineId, string jobId, EntityHeader org, EntityHeader user)
        {
            var machine = await _machineManager.GetMachineAsync(machineId, org, user);
            var job = await _jobManager.GetPickAndPlaceJobAsync(jobId, org, user);
            var stripFeedres = await _stripFeederManager.GetStripFeedersForMachineAsync(machineId, true, org, user);
            var autoFeeders = await _autoFeederManager.GetFeedersForMachineAsync(machineId, true, org, user);   

            var board = await _circuitBoardManager.GetCircuitBoardAsync(job.Board.Id, org, user);
            var revision = board.Revisions.Single(rev => rev.Id == job.BoardRevision.Id);
            job.BoardFiducials.AddRange( revision.PcbComponents.Where(pcbc => pcbc.Fiducial).Select(fid=>new BoardFiducial() { 
                Expected = new Point2D<double>() { X = fid.X.Value, Y = fid.Y.Value } }));

            if(job.BoardFiducials.Count < 2)
            {
                job.Warnings.Add($"Board should have at least 2 fiducials, had {job.BoardFiducials.Count}.");
            }

            foreach (var part in job.Parts)
            {

            }

            return InvokeResult.Success;
        }
    }
}
