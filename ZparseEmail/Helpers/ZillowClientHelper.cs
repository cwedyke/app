using ZparseEmail.Models;

namespace ZparseEmail.Helpers
{
    public static class ZillowClientHelper
    {
        private static ZillowClientKeyModel z1 = new ZillowClientKeyModel() { key = "X1-ZWz19htcwah6h7_3e7gq", count = 0 };
        private static ZillowClientKeyModel z2 = new ZillowClientKeyModel() { key = "X1-ZWz1fgqqmgjaq3_376md", count = 0 };
        private static ZillowClientKeyModel z3 = new ZillowClientKeyModel() { key = "X1-ZWz1fip4255r0r_3csw9", count = 0 };
        private static ZillowClientKeyModel z4 = new ZillowClientKeyModel() { key = "X1-ZWz1fgqiqc32tn_31kch", count = 0 };
        private static ZillowClientKeyModel z5 = new ZillowClientKeyModel() { key = "X1-ZWz19ht8y892iz_3h0lo", count = 0 };

        public static string GetAvailableClientKey() // need to update with a timestamp to age count per 24 hours.
        {
            if (z1.count < 950)
                return z1.key;
            else if (z2.count < 950)
                return z2.key;
            else if (z3.count < 950)
                return z3.key;
            else if (z4.count < 950)
                return z4.key;
            else if (z5.count < 950)
                return z5.key;
            else
                return string.Empty;
        }

        public static void IncrementKeyCount(string key)
        {
            if (z1.key.Equals(key))
                z1.count++;
            else if (z2.key.Equals(key))
                z2.count++;
            else if (z3.key.Equals(key))
                z3.count++;
            else if (z4.key.Equals(key))
                z4.count++;
            else if (z5.key.Equals(key))
                z5.count++;
        }

    }
}