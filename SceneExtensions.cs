using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rebalance;

public static class Extensions
{
    public static GameObject? FindGameObjectByPath(this Scene scene, string goPath)
    {
        var sepIndex = goPath.IndexOf('/');
        var rootName = (sepIndex == -1) ? goPath : goPath[..sepIndex];
        foreach (var obj in scene.GetRootGameObjects())
        {
            if (obj.name == rootName)
            {
                if (sepIndex == -1)
                    return obj;
                else
                {
                    var ts = obj.transform.Find(goPath[(sepIndex + 1)..]);
                    return ts.gameObject;
                }
            }
        }

        return null;
    }

    public static GameObject? FindChildByPath(this GameObject gameObject, string path)
    {
        return gameObject.transform.Find(path).gameObject;
    }
    
}