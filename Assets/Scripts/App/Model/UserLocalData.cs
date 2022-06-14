using TandC.RunIfYouWantToLive.Common;

namespace TandC.RunIfYouWantToLive
{
    public class UserLocalData
    {
        public Enumerators.Language appLanguage;


        public UserLocalData()
        {
            Reset();
        }

        public void Reset()
        {
            appLanguage = Enumerators.Language.NONE;
        }
    }
}