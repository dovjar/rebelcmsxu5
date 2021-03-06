﻿namespace Rebel.Framework.Localization.Processing
{
    public interface IFormatGroupExpander
    {        
        string Expand(string pattern, string content);                    
    }    

    public class HashTagFormatGroupExpander : IFormatGroupExpander
    {
        public string Expand(string pattern, string content)
        {
            return pattern.Replace("{#}", content);
        }
    }

}
