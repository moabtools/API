using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoabTools
{
    public enum TaskType
    {
        WordstatDeep = 0,
        DirectCheck = 1,
        Suggests = 2
    }

    public enum SuggestType
    {
        Phrase = 1,
        PhraseAndSpace = 2,
        PhraseAndRussianAlphabet = 3,
        PhraseAndEnglishAlphabet = 4,
        PhraseAndDigits = 5
    }

    public enum Syntax
    {
        NoQuotes = 1,
        Quotes = 2,
        QuotesAndExclamation = 3
    }

    public enum Db
    {
        All = 0,
        Desktop = 1,
        Mobile = 2,
        OnlyPhones = 3,
        OnlyTables = 4
    }

}
