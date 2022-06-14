using TandC.RunIfYouWantToLive.Common;
using UnityEngine.UI;

namespace TandC.RunIfYouWantToLive
{
    public interface IScreenOrientationManager
    {
        void SwitchOrientation(Enumerators.ScreenOrientationMode mode);
        Enumerators.ScreenOrientationMode GetOrientation();
    }
}