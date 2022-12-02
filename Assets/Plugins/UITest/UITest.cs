using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;

namespace UITest
{
    public class UISelectResult<T> : List<T> where T : MonoBehaviour
    {
        public UISelectResult(IEnumerable<T> objects) : base(objects) {}

        private T FirstElement {
            get {
                if (Count == 0) throw new System.IndexOutOfRangeException("UI selection result is empty");
                return this[0];
            }
        }

        #region Filter
        public UISelectResult<T> WithName(string name)
        {
            return new UISelectResult<T>(this.Where(obj => obj.name == name));
        }

        public UISelectResult<T> WithTag(string tag)
        {
            return new UISelectResult<T>(this.Where(obj => obj.CompareTag(tag)));
        }

        public UISelectResult<T> WithComponent(MonoBehaviour component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            return new UISelectResult<T>(this.Where(obj => obj.GetComponent<Component>() != null));
        }

        public UISelectResult<T> ThatIsActiveSelf(bool state)
        {
            return new UISelectResult<T>(this.Where(obj => obj.gameObject.activeSelf == state));
        }

        public UISelectResult<T> ThatIsActiveInHierarchy(bool state)
        {
            return new UISelectResult<T>(this.Where(obj => obj.gameObject.activeInHierarchy == state));
        }

        public UISelectResult<R> Children<R>() where R : MonoBehaviour
        {
            return new UISelectResult<R>(FirstElement.GetComponentsInChildren<R>());
        }
        #endregion

        #region Action
        public void Click() {}
        public void RightClick() {}
        public void MiddleClick() {}
        public void Type() {}
        public new void Clear() {}
        public void Check() {}
        #endregion
    }

    public class UI
    {
        public static UISelectResult<T> Get<T>() where T : MonoBehaviour
        {
            return new UISelectResult<T>(GameObject.FindObjectsOfType<T>());
        }
    }
}