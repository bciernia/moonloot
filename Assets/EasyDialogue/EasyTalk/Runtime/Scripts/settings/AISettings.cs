using System;
using UnityEngine;

namespace EasyTalk.Settings
{
    [Serializable]
    public class AISettings
    {
        [SerializeField]
        private int localPortNumer;

        [SerializeField]
        private string serverIPAddress;

        [SerializeField]
        private int serverPortNumber;

        public int LocalPortNumber 
        { 
            get { return this.localPortNumer; } 
        }

        public string ServerIPAddress 
        { 
            get { return serverIPAddress; } 
        }

        public int ServerPortNumber 
        { 
            get { return serverPortNumber; } 
        }
    }
}
