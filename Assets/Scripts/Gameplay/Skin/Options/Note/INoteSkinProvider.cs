using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Skin
{
    public interface INoteSkinProvider
    {
        (Texture texture, Color connectionLineColor) GetTapSkin(Tap note);

        (Texture normal, Texture highlight) GetHoldSkin(Hold note);

        Texture GetArcTapSkin(ArcTap note);

        Texture GetArcCapSprite(Arc arc);
    }
}