using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treasurer_App.Classes.Exceptions {
    internal class SettleException : Exception {
        public SettleException() { }
        public SettleException(string message) : base(message) { }
        public SettleException(string message, Exception inner) : base(message, inner) { }

    } 
}
