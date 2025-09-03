namespace Game.Static.Path
{
    public class SpriteAtlasPath
    {
        public static string GetPath(string path)
        {
            if (path.EndsWith(".spriteatlas"))
            {
                return "Assets/GameRes/Atlas/" + path;
            }
            return "Assets/GameRes/Atlas/" + path + ".spriteatlas";
        }
    }
}