// Assets/Scripts/Items/Interfaces/ICollectible.cs
using UnityEngine;

namespace Items.Interfaces
{
    public interface ICollectible
    {
        void OnCollect(GameObject collector);
    }
}