using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TandC.RunIfYouWantToLive.Common;

namespace TandC.RunIfYouWantToLive
{
    public interface INotificationManager
    {
        event Action<Notification> DrawNotificationEvent;

        void DrawNotification(Enumerators.NotificationType type, string message);

    }
}