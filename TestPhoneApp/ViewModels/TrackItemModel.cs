﻿using CitySafe.Resources;
using Parse;
using ScheduledLocationAgent.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace CitySafe.ViewModels
{
    public class TrackItemModel
    {
        public ParseUser user;
        public ParseObject relation;

        public TrackItemModel(ParseUser user, ParseObject relation)
        {
            this.user = user;
            this.relation = relation;
        }

        [DataMember(Name = "email")]
        public string email
        {
            get { return user.Email; }
            set
            {
                user.Email = value;
                this.OnPropertyChanged(email);
            }
        }

        [DataMember(Name = "name")]
        public string name
        {
            get
            {
                string result = user.Get<string>(ParseContract.UserTable.FIRST_NAME);
                if (user.Get<bool>(ParseContract.UserTable.IN_DANGER))
                    result = result + " " + AppResources.Tracker_NeedHelp;
                return result;
            }
            set
            {
                user[ParseContract.UserTable.FIRST_NAME] = value;
                this.OnPropertyChanged(name);
            }
        }

        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
