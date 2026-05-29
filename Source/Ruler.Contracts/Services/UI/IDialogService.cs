using Ruler.Contracts.Models;
using Ruler.Contracts.Strategies;

namespace Ruler.Contracts.Services.UI
{
    /// <summary>
    /// Defines a contract for services that show dialogs.
    /// This allows the ViewModel to request a dialog without knowing
    /// about the specific UI implementation.
    /// </summary>
    public interface IDialogService
    {
        DialogSizeResult ShowSetSizeDialog(double width, double height, IUnitStrategy strategy);
        void ShowAboutDialog(string message);
        CalibrationResult? ShowCalibrationDialog(double defaultSliderValue);
    }
}
