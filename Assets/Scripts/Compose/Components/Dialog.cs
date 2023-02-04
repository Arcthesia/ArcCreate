using UnityEngine;

namespace ArcCreate.Compose.Components
{
    public abstract class Dialog : MonoBehaviour
    {
        private static int dialogCount = 0;
        private static Dialog topmostDialog;
        private Dialog prevDialog;
        private Dialog nextDialog;

        public static bool IsAnyOpen => dialogCount > 0;

        public static void CloseTopmost()
        {
            if (topmostDialog != null)
            {
                topmostDialog.Close();
            }
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
            dialogCount += 1;
            prevDialog = topmostDialog;
            if (prevDialog != null)
            {
                prevDialog.nextDialog = this;
            }

            transform.SetAsLastSibling();

            topmostDialog = this;
        }

        public virtual void Close()
        {
            gameObject.SetActive(false);
            dialogCount -= 1;
            if (topmostDialog == this)
            {
                topmostDialog = prevDialog;
            }

            if (nextDialog != null)
            {
                nextDialog.prevDialog = prevDialog;
            }
        }
    }
}