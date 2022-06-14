using System.Collections.Generic;
using UnityEngine;

namespace TandC.RunIfYouWantToLive
{
    public interface IDataManager
    {
        UserLocalData CachedUserLocalData { get; set; }
        List<RecordItem> UserLocalRecords { get; set; }
        void StartLoadCache();
        void SaveAllCache();
        void AddRecord(RecordItem item);
        Sprite GetSpriteFromTexture(Texture2D texture);
    }
}