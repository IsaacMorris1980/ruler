namespace Ruler.Shared
{
    /// <summary>
    /// Defines a contract for services that show dialogs.
    /// This allows the ViewModel to request a dialog without knowing
    /// about the specific UI implementation.
    /// </summary>
    public interface IDialogService
    {
        Size ShowSetSizeDialog(double width, double height);
        void ShowAboutDialog(string message);
    }
}
