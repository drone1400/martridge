using System;

namespace Martridge.Models.OnlineDmods {
    public class InvalidDmodUrlException : Exception{
        public InvalidDmodUrlException () : base() {
        }

        public InvalidDmodUrlException (string message) : base(message) {
        }
    }
}
