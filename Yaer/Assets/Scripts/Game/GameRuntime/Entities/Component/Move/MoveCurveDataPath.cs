namespace Game.GameRuntime.Entities.Component.Move
{
    public static class MoveCurveDataPath
    {
        public static string GetPath(string curveName)
        {
            return "Assets/GameRes/Config/MoveCurveConfig/" + curveName + ".asset";
        }
    }
}