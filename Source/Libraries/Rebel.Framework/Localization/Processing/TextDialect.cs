﻿using Rebel.Framework.Localization.Parsing;

namespace Rebel.Framework.Localization.Processing
{
    public class TextDialect : PatternDialect
    {
        public TextDialect()
        {
            Parser = new TextParser();
            Encode = false;
        }

        private object _parseLock = new object();

        public override PatternEvaluator GetEvaluator(string pattern, TextManager manager)
        {
            lock (_parseLock)
            {
                var expr = Parser.Parse(pattern, manager);

                return new PatternEvaluator(expr);
            }
        }
    }
}
