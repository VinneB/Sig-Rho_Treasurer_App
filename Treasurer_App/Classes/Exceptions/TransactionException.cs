using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Treasurer_App.Classes.Exceptions {
    internal class TransactionException : Exception {
        public TransactionException() { }
        public TransactionException(string message) : base(message) { }
        public TransactionException(string message, Exception inner) : base(message, inner) { }
    }
}
