namespace Enemies.States
{
    public interface IBearState
    {
        void Enter();
        void Exit();
        void Update();
        void HandleMovement();
        void HandleCombat();
    }
} 