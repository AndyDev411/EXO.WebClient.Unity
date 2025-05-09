using EXO.WebClient;
using UnityEngine;

public class ExoNetworkedMonoBehavior : MonoBehaviour
{
    protected virtual void Awake()
    {
        ExoNetworkManager.OnClientTickEvent += OnClientTick;
    }

    /// <summary>
    /// Override this function to do updates every time the server "Ticks".
    /// </summary>
    /// <param name="deltaTime"> The time between each tick. (Equivelant to DeltaTime in Update! Use this instead of Time.deltaTime!) </param>
    protected virtual void OnClientTick(float deltaTime)
    { 
        // Do Something...
    }
}
