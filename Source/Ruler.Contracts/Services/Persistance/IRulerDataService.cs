using System.Collections.Generic;

namespace Ruler.Shared
{
    public interface IRulerDataService
    {
        /// <summary>
        /// Retrieves the initial set of rulers based on CLI priority, then saved data, then defaults.
        /// </summary>
        IEnumerable<RulerInfo> GetInitialRulers(string[] args);

        /// <summary>
        /// Creates a fresh ruler instance with system defaults.
        /// </summary>
        RulerInfo CreateNewRuler();

        /// <summary>
        /// Persists the provided collection of rulers.
        /// </summary>
        void SaveRulers(IEnumerable<RulerInfo> rulers);
    }
}
