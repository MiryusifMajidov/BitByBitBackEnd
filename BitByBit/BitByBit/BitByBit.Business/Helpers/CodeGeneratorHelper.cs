namespace BitByBit.Business.Helpers
{
    public static class CodeGeneratorHelper
    {
        public static string GenerateEmailConfirmationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6 rəqəm
        }

        public static string GeneratePasswordResetCode()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString(); // 4 rəqəm
        }

        public static string GenerateRandomCode(int length)
        {
            var random = new Random();
            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length) - 1;
            return random.Next(min, max).ToString();
        }
    }
}