using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URLShortener.Common.Util
{
    /// <summary>
    /// Base 62 string encoder
    /// </summary>
    public class Base62Encoder : IEncoder
    {
        private readonly string Base62Characters;
        private readonly int EncodingLength;

        /// <summary>
        /// constructor init
        /// </summary>
        /// <param name="options"></param>
        public Base62Encoder(IOptions<Settings.Settings> options)
        {
            Base62Characters = options.Value.Base62Characters;
            EncodingLength = options.Value.EncodingLength;
        }

        /// <summary>
        /// encode to a base 62 string
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Encode(long id)
        {
            char[] buffer = new char[EncodingLength];
            for (int i = EncodingLength - 1; i >= 0; i--)
            {
                buffer[i] = Base62Characters[(int)(id % 62)];
                id /= 62;
            }
            return new string(buffer);
        }

        /// <summary>
        /// decode from a base 62 string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public long Decode(string input)
        {
            long id = 0;
            foreach (char c in input)
                id = id * 62 + Base62Characters.IndexOf(c);
            return id;
        }
    }
}