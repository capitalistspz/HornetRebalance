using System;
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
                    return ts == null ? null : ts.gameObject;
                }
            }
        }

        return null;
    }

    extension(GameObject gameObject)
    {
        public GameObject? FindChildByPath(string path)
        {
            var ts = gameObject.transform.Find(path);
            return ts == null ? null : ts.gameObject;
        }

        public string GetHierarchyPath()
        {
            if (!gameObject)
                return String.Empty;
            var transform = gameObject.transform;
            var path = gameObject.name;
            while (transform.parent)
            {
                transform = transform.parent;
                path = $"{transform.name}/{path}";
            }

            return path;
        }
    }
}