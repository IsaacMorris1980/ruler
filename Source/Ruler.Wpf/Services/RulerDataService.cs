using Ruler.Shared.Models;
using Ruler.Shared.Strategies.Units;
using Ruler.Shared.Strategies;
using Ruler.Shared.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Services;

namespace Ruler.Wpf.Services
{
    /// <summary>
    /// Service responsible for the creation and sourcing of Ruler data.
    /// Incorporates the logic previously found in RulerFactory.
    /// </summary>
    public class RulerDataService : IRulerDataService
    {
        private readonly ISavingService _persistence;
        private readonly ILoggingService<RulerDataService> _logger;

        public RulerDataService(
            ISavingService persistence,
            ILoggingService<RulerDataService> logger)
        {
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<RulerInfo> GetInitialRulers(string[] args)
        {
            // Priority 1: Command Line Arguments
            if (args != null && args.Length > 0)
            {
                _logger.LogInfo("Command line arguments detected. Sourcing ruler from CLI.");
                var cliRuler = RulerFactory.CreateFromArgs(args);
                return new List<RulerInfo> { cliRuler };
            }

            // Priority 2: Saved Session
            try
            {
                var savedData = _persistence.Load<RulerInfo>();
                if (savedData != null && savedData.Any())
                {
                    _logger.LogInfo($"Successfully loaded {savedData.Count} rulers from persistence.");
                    return savedData;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load rulers from persistence.", ex);
            }

            // Priority 3: Default Fallback
            _logger.LogInfo("No arguments or saved data found. Creating default ruler.");
            return new List<RulerInfo> { RulerFactory.CreateDefault() };
        }

        public RulerInfo CreateNewRuler()
        {
            _logger.LogInfo("Creating a new default ruler instance.");
            return RulerFactory.CreateDefault();
        }

        public void SaveRulers(IEnumerable<RulerInfo> rulers)
        {
            if (rulers == null) return;

            var list = rulers.ToList();
            _logger.LogInfo($"Requesting persistence for {list.Count} rulers.");

            try
            {
                _persistence.Save(list);
            }
            catch (Exception ex)
            {
                _logger.LogError("Critical failure while saving rulers.", ex);
            }
        }
    }
}
