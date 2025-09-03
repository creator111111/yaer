namespace Game.GameRuntime.Entities.Generic
{
    public struct LayerArea
    {
        public float yMax;
        public float yMin;

        public LayerArea(float yMax, float yMin)
        {
            this.yMax = yMax;
            this.yMin = yMin;
        }
    }
}