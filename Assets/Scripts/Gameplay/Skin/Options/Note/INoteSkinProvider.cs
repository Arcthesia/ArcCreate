using ArcCreate.Gameplay.Data;
using UnityEngine;

namespace ArcCreate.Gameplay.Skin
{
    public interface INoteSkinProvider
    {
        Sprite GetTapSkin(Tap note);

        (Sprite normal, Sprite highlight) GetHoldSkin(Hold note);

        (Mesh mesh, Material material) GetArcTapSkin(ArcTap note);
    }
}