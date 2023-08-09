using CyTools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CysmicEngine
{
    /// <summary>
    /// An entity meant for debugging. Toggle the display through the static bool displayGizmos.
    /// </summary>
    public class GizmoObj
    {
        private Transform _transform;
        public Transform transform { get { return _transform; } }
        private Renderer _renderer;
        public Renderer renderer { get { return _renderer; } }

        public GizmoObj(Transform t, Renderer r)
        {
            _transform = t;
            _renderer = r;
            _renderer.transform = t;

            CysmicGame.allGizmos.Add(this);
        }
    }
    /// <summary>
    /// A GameObject is the base of any entity in the game world. They can contain componets that affect rendering and various other behaviours.
    /// </summary>
    public class GameObject
    {
        /*void OnTriggerEnter    (Collider2D other) { }
        void OnTriggerStay     (Collider2D other) { }
        void OnTriggerExit     (Collider2D other) { }
        void OnCollisionEnter  (Collider2D self, Collider2D other) { }
        void OnCollisionStay   (Collider2D self, Collider2D other) { }
        void OnCollisionExit   (Collider2D self, Collider2D other) { }*/

        public string name = "New Game Object";
        //public string name { get { return _name; } set { _uniqueID = _uniqueID.Replace(name, value); _name = value; } }
        string _uniqueID = "";
        public string uniqueID { get { return _uniqueID; } }

        List<Component> _allComponents;
        public List<Component> allComponents { get { return _allComponents; } private set { _allComponents = value; } }
        public Transform transform;

        public string layer = "";
        internal bool _wasDestroyed;
        internal Action StartComponents;
        bool isInitializing = true;

        public bool wasDestroyed { get { return _wasDestroyed; } }

        public T AddComponent<T>(T component) where T : Component
        {
            if (component.OnlyOnePerGO())
            {
                for (int i2 = 0; i2 < allComponents.Count; i2++)
                {
                    if (allComponents[i2].GetType() == component.GetType())
                    {
                        return null;
                    }
                }
            }

            allComponents.Add(component);
            int i = allComponents.IndexOf(component);
            if (transform == null)
                allComponents[i].transform = component as Transform;
            else
                allComponents[i].transform = transform;
            allComponents[i].gameObject = this;

            if (isInitializing)//wait to call Start
                StartComponents += allComponents[i].OnStart;
            else
                allComponents[i].OnStart();

            CysmicGame.game.OnUpdate += allComponents[i].OnUpdate;//() => { allComponents[i].Update(); CysmicGame.ClearNullGameObjects(); };
            CysmicGame.game.OnLateUpdate += allComponents[i].OnLateUpdate;//() => { allComponents[i].LateUpdate(); CysmicGame.ClearNullGameObjects(); };
            CysmicGame.game.OnFixedUpdate += allComponents[i].OnFixedUpdate;

            if (component is Renderer)
            {
                CysmicGame.allRenderers.Add(component as Renderer);
            }
            else if (allComponents[i] is Collider2D)
            {
                CysmicGame.allColliders.Add(component as Collider2D);
            }

            return component;
        }

        void Register()
        {
            if (CysmicGame.allGameObjects.Contains(this))
            {
                name += " (Clone)";
                CysmicGame.allGameObjects.Add(this);
            }
            else
            {
                CysmicGame.allGameObjects.Add(this);
            }

            for (int i = 0; i < allComponents.Count; i++)
            {
                if (allComponents[i] is Renderer)
                {
                    CysmicGame.allRenderers.Add(allComponents[i] as Renderer);
                }
                else if (allComponents[i] is Collider2D)
                {
                    CysmicGame.allColliders.Add(allComponents[i] as Collider2D);
                }
            }
        }

        /// <summary>
        /// Duplicates an existing GameObject
        /// </summary>
        /// <param name="gameObject">The GameObject to be duplicated</param>
        public GameObject(GameObject gameObject)
        {
            name += " (Clone)";
            _uniqueID = gameObject._uniqueID.Replace(gameObject.name, name);
            allComponents = gameObject.allComponents;
            transform = gameObject.transform;
        }

        /// <summary>
        /// Spawns a new GameObject. A default transform component is added if not transform is specified.
        /// </summary>
        public GameObject(string n = "[defaultName]", Transform trnfrm = null, List<Component> components = null, string lyr = "", string extraID = "")
        {
            allComponents = new List<Component>();
            if (n == "[defaultName]")
            {
                n = name;
            }
            name = n;
            layer = lyr;//replace with "find class by enum"

            //Initialize ID
            _uniqueID = "~" + name + "~" + DateTime.UtcNow.ToString() + DateTime.UtcNow.Millisecond + Basic.random.Next() + "|" + extraID;
            while (CysmicGame.allGameObjects.Exists(x => x != null && x.uniqueID == uniqueID))
                _uniqueID += "(C)";

            //Initialize Components
            //allComponents = components;
            if (components == null)
                components = new List<Component>();//If null, set it

            if (trnfrm != null)
            {
                components.Insert(0, trnfrm);
            }
            else//if doesn't have Transform, then add it
            {
                var t = components.Find(x => x is Transform);
                if (t == null)
                    components.Insert(0, new Transform());
                else
                {
                    components.RemoveAll(x => x is Transform);
                    components.Insert(0, t);//move it to the front
                }
            }

            transform = AddComponent(components.Find(x => x is Transform) as Transform);
            /*pos = pos ?? Vector2.Zero;//this is a condensed null check
            scl = scl ?? (1, 1);
            rot = rot != Vector2.Zero ? Vector2.Zero : ;*/

            for (int i = 0; i < components.Count; i++)
            {
                AddComponent(components[i]);
            }
            allComponents = allComponents.OrderBy(x => !(x is Collider2D)).ToList();

            isInitializing = false;
            StartComponents.Invoke();

            Register();
        }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            component = (T)allComponents.Find(x => x is T);
            return component != null;
        }
        public T GetComponent<T>() where T : Component
        {
            return (T)allComponents.Find(x => x is T);
        }
    }
}
