using System;
using UnityEngine;

namespace TandC.RunIfYouWantToLive
{
    public interface IGameplayManager
    {
        event Action GameplayStartedEvent;
        event Action GameplayEndedEvent;
        event Action ControllerInitEvent;
        bool IsGameplayStarted { get; }

        GameObject GameplayObject { get; }
        GameplayData GameplayData { get; }

        bool IsGamePaused { get; }

        Camera GameplayCamera { get; }

        T GetController<T>() where T : IController;

        void StartGameplay();
        void StopGameplay();
        void RestartGameplay();
        void PauseGame(bool enablePause);
    }
}