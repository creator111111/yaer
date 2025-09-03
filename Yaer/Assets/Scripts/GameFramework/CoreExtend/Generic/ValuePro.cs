namespace GameFramework.CoreExtend.Generic
{
    public struct ValuePro
    {
        private readonly bool m_BoolValue;
        private readonly int m_IntValue;
        private readonly float m_FloatValue;
        private readonly string m_StringValue;

        public ValuePro(bool boolValue, int intValue, float floatValue, string stringValue)
        {
            this.m_BoolValue = boolValue;
            this.m_IntValue = intValue;
            this.m_FloatValue = floatValue;
            this.m_StringValue = stringValue;
        }

        public bool BoolValue => this.m_BoolValue;

        public int IntValue => this.m_IntValue;

        public float FloatValue => this.m_FloatValue;

        public string StringValue => this.m_StringValue;
    }
}