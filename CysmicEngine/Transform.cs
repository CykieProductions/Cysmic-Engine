using System;

namespace CysmicEngine
{
    public abstract class Component : CE_Common
    {
        //bool _onlyOnePerGO = false;
        //public bool onlyOnePerGO { get { return _onlyOnePerGO; } protected set { _onlyOnePerGO = value; } }
        public GameObject gameObject;
        public Transform transform;

        internal void OnStart()
        {
            //transform = gameObject.transform;
            Start();
        }
        protected virtual void Start() { gameObject.StartComponents -= OnStart; }

        internal void OnUpdate() => Update();
        protected virtual void Update() { if ((gameObject != null && gameObject.wasDestroyed) || transform.wasDestroyed) gameObject = null; }

        //Called after this frame's Draw call
        internal void OnLateUpdate(/*System.Drawing.Graphics graphics*/) => LateUpdate();
        protected virtual void LateUpdate() { }

        internal void OnFixedUpdate()
        {
            if(gameObject != null && !gameObject.wasDestroyed)
                FixedUpdate();
        }
        protected virtual void FixedUpdate() { }

        [Obsolete("Use GameObject.GetComponent instead")]
        public bool TryGetComponent<T>(out T component) where T : Component
        {
            component = (T)gameObject.allComponents.Find(x => x is T);
            return component != null;
        }
        [Obsolete("Use GameObject.GetComponent instead")]
        public T GetComponent<T>() where T : Component
        {
            return (T)gameObject.allComponents.Find(x => x is T);
        }

        public virtual void OnTriggerEnter(Collider2D other) { }
        public virtual void OnTriggerStay(Collider2D other) { }
        public virtual void OnTriggerExit(Collider2D other) { }
        public virtual void OnCollisionEnter(Rigidbody2D.Collision collision) { }
        public virtual void OnCollisionStay(Collider2D self, Collider2D other) { }
        public virtual void OnCollisionExit(Collider2D self, Collider2D other) { }

        /// <summary> Override this to "return true;" in order to ensure that multiple instances of this component can't be added to this GameObject </summary>
        public virtual bool OnlyOnePerGO() { return false; }
    }

    /// <summary>
    /// A component for controlling the GameObject's position, rotation (WIP), and scale. There will always be just one on every GameObject.
    /// </summary>
    public class Transform : Component
    {
        public float lifespan;
        public float _aliveTimer;
        public float aliveTimer { get { return _aliveTimer; } }
        Vector2 _position = new Vector2();
        public Vector2 position { get { return _position; } set { _position = value; } }
        public Vector2 scale = new Vector2();
        public float rotation = 0;

        public bool isStatic = true;

        public Transform(Vector2 pos, Vector2 scl, float rot = 0)
        {
            position = pos;
            scale = scl;
            rotation = rot;
        }
        public Transform()
        {
            position = Vector2.zero;
            scale = (32, 32);
            rotation = 0;
        }

        public void Translate(Vector2 movement)
        {
            if (isStatic)
                return;

            _position.x += movement.x;
            _position.y -= movement.y;
        }
        public void Translate(float x, float y)
        {
            Translate((x, y));
        }

        public Transform SetPosition(Vector2 pos)
        {
            transform._position = pos;
            return transform;
        }
        public Transform SetScale(Vector2 scl)
        {
            transform.scale = scl;
            return transform;
        }
        public Transform SetRotation(float rot)
        {
            transform.rotation = rot;
            return transform;
        }

        public override bool OnlyOnePerGO()
        {
            //onlyOnePerGO = true;
            return true;
        }
        /*protected override void Start()
        {

        }*/

        bool _wasDestroyed = false;
        public bool wasDestroyed { get => _wasDestroyed; private set => _wasDestroyed = value; }
        protected override void Update()
        {
            base.Update();

            if (!wasDestroyed && lifespan > 0 && aliveTimer > lifespan)
            {
                gameObject._wasDestroyed = true;
                CysmicGame.Destroy(gameObject);
                wasDestroyed = true;
            }
            else if (wasDestroyed)
            {
                print("THIS OBJECT SHOULD'VE BEEN DESTROYED! aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                //CysmicGame.Destroy(gameObject);
                //CysmicGame.game.OnUpdate -= Update;
            }
            _aliveTimer += Time.deltaTime;
        }
    }
}
