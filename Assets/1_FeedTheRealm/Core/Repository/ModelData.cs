using System;
using System.Collections.Generic;
using UnityEngine;

namespace FeedTheRealm.Core.Repository
{
    [Serializable]
    public class SerializedModelData
    {
        public List<ModelData> models = new();
    }

    [Serializable]
    public class ModelData
    {
        public string id;
        public string name;
        public string filePath;
        public Vector3 defaultRotation;
        public Vector3 defaultScale;
        public List<BoxColliderData> colliders = new();
    }

    [Serializable]
    public class BoxColliderData
    {
        public Vector3 center;
        public Vector3 size;
    }
}
