namespace Codenade.Inputbinder
{
    public static class ProcessorUtilities
    {
        public static string ExtractSingleProcessor(string input, string processorName)
        {
            if (!input.Contains(processorName) || input.IndexOf('(') < 0 || input.IndexOf(')') < 0)
                return processorName + "()";
            var start = input.IndexOf(processorName);
            var end = input.IndexOf(')', start);
            return input.Substring(start, end - start + 1);
        }

        public static bool ExtractSingleProcessor(string input, string processorName, out string output)
        {
            if (!input.Contains(processorName) || input.IndexOf('(') < 0 || input.IndexOf(')') < 0)
            {
                output = processorName + "()";
                return false;
            }
                
            var start = input.IndexOf(processorName);
            var end = input.IndexOf(')', start);
            output = input.Substring(start, end - start + 1);
            return true; 
        }
    }
}
