using Unity.Netcode;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if( _instance == null )
            {
                T[] gameObjects = FindObjectsOfType(typeof(T)) as T[];
                if (gameObjects.Length > 0)
                    _instance = gameObjects[0];
                if (_instance == null)
                {
                    GameObject gameObject = new GameObject();
                    gameObject.name = typeof(T).Name;

                    _instance = gameObject.AddComponent<T>();
                }
            }
            
            return _instance;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (_instance == this)
            _instance = null;
    }
}
