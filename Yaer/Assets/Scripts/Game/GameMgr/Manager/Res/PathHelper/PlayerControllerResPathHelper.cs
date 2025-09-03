using Game.Static.Name.Clothes;

namespace Game.GameMgr.Manager.Res.PathHelper
{
    public class PlayerControllerResPathHelper
    {
        public string clothes;
        public string headWear;
        public string place;

        public string GetPath()
        {
            if (place == "Home")
            {
                if (clothes == ClothesName.Clothes.Dress)
                {
                    return $"Assets/GameRes/RuntimeController/Entity/Player/Home/Home_{clothes}_{headWear}.controller";
                }
                
                return $"Assets/GameRes/RuntimeController/Entity/Player/Home/Home_{clothes}_{headWear}.overrideController";
            }
            else
            {
                if (headWear == ClothesName.HeadWear.Crown)
                {
                    return $"Assets/GameRes/RuntimeController/Entity/Player/Combat/Combat_{clothes}_{headWear}.controller";
                }
                else
                {
                    return $"Assets/GameRes/RuntimeController/Entity/Player/Combat/Combat_{clothes}_{headWear}.overrideController";
                }
            }
                
        }
    }
}