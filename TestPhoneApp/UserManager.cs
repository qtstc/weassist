using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TestPhoneApp
{
    /// <summary>
    /// This class manages all the operations between the phone and the parse server.
    /// </summary>
    class UserManager
    {
        //private string _username;
        //private string _password;
        public enum loginState { UNCHECKED, WRONG_USERNAME, WRONG_PASSWORD, VALIDATED };
        
        private UserManager(String username, String password)
        {
            //this.username = username;
            //this.password = password;
        }

        /// <summary>
        /// Validate the user data with the server.
        /// It changes the value of currentState
        /// </summary>
        public static loginState validateUser(string username,string password)
        {
            Debug.WriteLine("validateing: " + username + " " + password);
            if(username.Equals("tao")&&password.Equals("p"))
                return loginState.VALIDATED;
            return loginState.WRONG_PASSWORD;
        }


        //public string username
        //{
        //    set { this._username = value;}
        //    get { return this._username; }
        //}

        //public string password
        //{
        //    set { this._password = value;}
        //    get { return this._password; }
        //}
    }
}
