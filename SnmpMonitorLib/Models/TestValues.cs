using System;

namespace SnmpMonitorLib.Models
{
    public class TestValues
    {
        public int A { get; set; }
        public int B { get; set; }

        public string Serialize()
        {
            return A + "|" + B + "|";
        }

        public static TestValues Deserialize(string v)
        {
            string[] fields = v.Split('|');
            return new TestValues {A = Convert.ToInt32(fields[0]), B = Convert.ToInt32(fields[1])};
        }
    }
}